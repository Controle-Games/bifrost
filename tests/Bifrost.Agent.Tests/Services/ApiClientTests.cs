using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bifrost.Agent.Services;
using Bifrost.Shared.DTOs;
using Bifrost.Shared.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace Bifrost.Agent.Tests.Services;

// Handler customizado simples para capturar requisições HTTP sem depender de hacks de mock protegido
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _sendAsyncFunc;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsyncFunc)
    {
        _sendAsyncFunc = sendAsyncFunc;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _sendAsyncFunc(request);
    }
}

public class ApiClassTests
{
    [Fact]
    public async Task SendHistoryBatchAsync_WhenApiReturnsSuccess_ShouldReturnSuccessResponse()
    {
        // ARRANGE
        var expectedResponse = new SyncHistoryResponseDto { Success = true, ProcessedCount = 1, Message = "Lote processado" };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
        };

        var handler = new FakeHttpMessageHandler(req => Task.FromResult(httpResponse));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.bifrost.local/") };
        var apiClient = new ApiClient(httpClient, NullLogger<ApiClient>.Instance);

        var items = new[]
        {
            new BrowserHistoryItemDto(
                "https://success.return",
                "API-SUCCESS-RETURN",
                DateTime.UtcNow,
                BrowserType.Chrome,
                "Default"
            )
        };

        // ACT
        var result = await apiClient.SendHistoryBatchAsync(items);

        // ASSERT
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();
        result.ProcessedCount.ShouldBe(1);
    }

    [Fact]
    public async Task SendHistoryBatchAsync_WhenApiReturns500InternalServerError_ShouldReturnFailedResponse()
    {
        // ARRANGE
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Erro interno do servidor")
        };

        var handler = new FakeHttpMessageHandler(req => Task.FromResult(httpResponse));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.bifrost.local/") };
        var apiClient = new ApiClient(httpClient, NullLogger<ApiClient>.Instance);

        var items = new[]
        {
            new BrowserHistoryItemDto(
                "https://internal-server-error.return",
                "Internal Server Error",
                DateTime.UtcNow,
                BrowserType.Chrome,
                "Default"
            )
        };

        // ACT
        var result = await apiClient.SendHistoryBatchAsync(items);

        // ASSERT
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("InternalServerError");
    }

    [Fact]
    public async Task SendHistoryBatchAsync_WhenEmptyListPassed_ShouldReturnEarlyWithoutHttpCall()
    {
        // ARRANGE
        var httpCallMade = false;
        var handler = new FakeHttpMessageHandler(req =>
        {
            httpCallMade = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.bifrost.local/") };
        var apiClient = new ApiClient(httpClient, NullLogger<ApiClient>.Instance);

        // ACT
        var result = await apiClient.SendHistoryBatchAsync(Enumerable.Empty<BrowserHistoryItemDto>());

        // ASSERT
        result.Success.ShouldBeTrue();
        result.ProcessedCount.ShouldBe(0);
        httpCallMade.ShouldBeFalse(); // Garante que nenhuma chamada externa foi disparada
    }
}
