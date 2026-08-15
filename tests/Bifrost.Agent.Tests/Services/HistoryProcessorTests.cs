using Bifrost.Agent.Models;
using Bifrost.Agent.Services;
using Bifrost.Shared.DTOs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Bifrost.Agent.Tests.Services;

public class HistoryProcessorTest
{
    private readonly Mock<IBrowserHistoryReader> _readerMock = new();
    private readonly Mock<IStateRepository> _stateRepositoryMock = new();

    [Fact]
    public async Task ProcessPendingHistoryAsync_ShouldFilterItemsOlderThanLastSyncedAndSaveNewState()
    {
        // ARRANGE
        var profileKey = "Chrome_Default";
        var lastSyncedDate = new DateTime(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc);

        var existingState = new AgentState();
        existingState.LastSyncedPerProfile[profileKey] = lastSyncedDate;

        _stateRepositoryMock
            .Setup(x => x.LoadStateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingState);

        var oldItem = new BrowserHistoryItemDto
        (
            "https://site-antigo.dev",
            "Antigo",
            lastSyncedDate.AddMinutes(-10), // Mais antigo que a última sincronização
            BrowserType.Chrome,
            "Default"
        );

        var newItem = new BrowserHistoryItemDto
        (
            "https://teste.dev",
            "Page Title",
            lastSyncedDate.AddMinutes(10), // Novo
            BrowserType.Chrome,
            "Default"
        );

        _readerMock
            .Setup(x => x.ReadHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { oldItem, newItem });

        var processor = new HistoryProcessor(
            _readerMock.Object,
            _stateRepositoryMock.Object,
            NullLogger<HistoryProcessor>.Instance
        );

        // ACT
        var result = (await processor.ProcessPendingHistoryAsync()).ToList();

        // ASSERT
        result.Count.ShouldBe(1);
        result.First().Url.ShouldBe("https://teste.dev");

        // Verifica se o estado local foi atualizado para o timestamp do item mais recente
        _stateRepositoryMock.Verify(x => x.SaveStateAsync(
            It.Is<AgentState>(s => s.LastSyncedPerProfile[profileKey] == newItem.VisitedAtUtc),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
