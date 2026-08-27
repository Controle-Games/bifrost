using System.Net;
using System.Text.Json;
using Bifrost.Agent.Services;
using Bifrost.Shared.DTOs;
using Bifrost.Shared.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Shouldly;

namespace Bifrost.Agent.Tests.Services;

public class ApiClassTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

    private HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        return new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.bifrost.local/")
        };
    }

    [Fact]
    public async Task SendHistoryBatchAsync_WhenApiReturnsSuccess_ShouldReturnSuccessResponse()
    {
        // Arrange
        var expectedResponse = new SyncHistoryResponseDto
        {
            Success = true,
            ProcessedCount = 1,
            Message = "Lote processado"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
        };

        var httpClient = CreateMockHttpClient(httpResponse);
        var apiClient = new ApiClient(httpClient, NullLogger<ApiClient>.Instance);

        var items = new[]
        {
            new BrowserHistoryItemDto(
                Url: "https://success.return",
                Title: "API-SUCCESS-RETURN",
                VisitedAtUtc: DateTime.UtcNow,
                Browser: BrowserType.Chrome,
                ProfileName: "Default"
            )
        };

        // Act
        var result = await apiClient.SendHistoryBatchAsync(items);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();
        result.ProcessedCount.ShouldBe(1);
    }

    [Fact]
    public async Task SendHistoryBatchAsync_WhenApiReturns500InternalServerError_ShouldReturnFailedResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Erro interno do servidor")
        };

        var httpClient = CreateMockHttpClient(httpResponse);
        var apiClient = new ApiClient(httpClient, NullLogger<ApiClient>.Instance);

        var items = new[]
        {
            new BrowserHistoryItemDto(
                Url: "https://internal-server-error.return",
                Title: "Intertal Server Error",
                VisitedAtUtc: DateTime.UtcNow,
                Browser: BrowserType.Chrome,
                ProfileName: "Default"
            )
        };

        // Act
        var result = await apiClient.SendHistoryBatchAsync(items);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("InternalServerError");
    }

    [Fact]
    public async Task SendHistoryBatchAsync_WhenEmptyListPassed_ShouldReturnEarlyWithoutHttpCall()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var apiClient = new ApiClient(httpClient, NullLogger<ApiClient>.Instance);

        // Act
        var result = await apiClient.SendHistoryBatchAsync(Enumerable.Empty<BrowserHistoryItemDto>());

        // Assert
        result.Success.ShouldBeTrue();
        result.ProcessedCount.ShouldBe(0);

        // Garante que o handler HTTP nem foi chamado
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
