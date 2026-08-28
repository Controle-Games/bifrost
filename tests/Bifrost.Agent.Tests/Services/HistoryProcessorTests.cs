using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bifrost.Agent.Models;
using Bifrost.Agent.Services;
using Bifrost.Shared.DTOs;
using Bifrost.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Bifrost.Agent.Tests.Services;

public class HistoryProcessorTest
{
    private readonly IBrowserHistoryReader _readerMock;
    private readonly IStateRepository _stateRepositoryMock;
    private readonly ILogger<HistoryProcessor> _loggerMock;
    private readonly HistoryProcessor _processor;

    public HistoryProcessorTest()
    {
        // Criação de Substitutos (mocks) usando NSubstitute
        _readerMock = Substitute.For<IBrowserHistoryReader>();
        _stateRepositoryMock = Substitute.For<IStateRepository>();
        _loggerMock = Substitute.For<ILogger<HistoryProcessor>>();
        
        // Construtor real de 3 argumentos
        _processor = new HistoryProcessor(_readerMock, _stateRepositoryMock, _loggerMock);
    }

    [Fact]
    public async Task ProcessPendingHistoryAsync_ShouldFilterItemsOlderThanLastSyncedAndSaveNewState()
    {
        // ARRANGE
        var profileKey = "Chrome_Default";
        var lastSyncedDate = new DateTime(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc);
        
        var existingState = new AgentState();
        existingState.LastSyncedPerProfile[profileKey] = lastSyncedDate;
        
        _stateRepositoryMock
            .LoadStateAsync(Arg.Any<CancellationToken>())
            .Returns(existingState);

        var oldItem = new BrowserHistoryItemDto(
            "https://site-antigo.dev",
            "Antigo",
            lastSyncedDate.AddMinutes(-10), // Mais antigo que a última sincronização
            BrowserType.Chrome,
            "Default"
        );

        var newItem = new BrowserHistoryItemDto(
            "https://teste.dev",
            "Page Title",
            lastSyncedDate.AddMinutes(10), // Novo registro válido
            BrowserType.Chrome,
            "Default"
        );

        _readerMock
            .ReadHistoryAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BrowserHistoryItemDto> { oldItem, newItem });

        // ACT
        var result = (await _processor.ProcessPendingHistoryAsync()).ToList();

        // ASSERT
        result.Count.ShouldBe(1);
        result.First().Url.ShouldBe("https://teste.dev");

        // Verifica se o estado local foi atualizado para o timestamp do item mais recente
        await _stateRepositoryMock.Received(1).SaveStateAsync(
            Arg.Is<AgentState>(s => s.LastSyncedPerProfile[profileKey] == newItem.VisitedAtUtc),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ProcessPendingHistoryAsync_WhenNoNewRecords_ShouldNotSaveStateOrReturnItems()
    {
        // ARRANGE
        var profileKey = "Chrome_Default";
        var lastSyncedDate = new DateTime(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc);
        
        var existingState = new AgentState();
        existingState.LastSyncedPerProfile[profileKey] = lastSyncedDate;
        
        _stateRepositoryMock
            .LoadStateAsync(Arg.Any<CancellationToken>())
            .Returns(existingState);

        var oldItem = new BrowserHistoryItemDto(
            "https://site-antigo.dev",
            "Antigo",
            lastSyncedDate.AddMinutes(-10),
            BrowserType.Chrome,
            "Default"
        );

        _readerMock
            .ReadHistoryAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BrowserHistoryItemDto> { oldItem });

        // ACT
        var result = (await _processor.ProcessPendingHistoryAsync()).ToList();

        // ASSERT
        result.ShouldBeEmpty();

        // Verifica que o SaveStateAsync NÃO foi chamado visto que nenhum registro novo foi processado
        await _stateRepositoryMock.DidNotReceiveWithAnyArgs().SaveStateAsync(default!, default);
    }

    [Fact]
    public async Task ProcessPendingHistoryAsync_OnFirstRun_ShouldReturnAllItemsAndInitializeState()
    {
        // ARRANGE
        var existingState = new AgentState(); // Sem chaves cadastradas (primeira execução)
        
        _stateRepositoryMock
            .LoadStateAsync(Arg.Any<CancellationToken>())
            .Returns(existingState);

        var item1 = new BrowserHistoryItemDto(
            "https://dev.to",
            "Dev.to",
            new DateTime(2026, 8, 28, 8, 0, 0, DateTimeKind.Utc),
            BrowserType.Chrome,
            "Default"
        );

        var item2 = new BrowserHistoryItemDto(
            "https://github.com",
            "GitHub",
            new DateTime(2026, 8, 28, 8, 5, 0, DateTimeKind.Utc),
            BrowserType.Chrome,
            "Default"
        );

        _readerMock
            .ReadHistoryAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BrowserHistoryItemDto> { item1, item2 });

        // ACT
        var result = (await _processor.ProcessPendingHistoryAsync()).ToList();

        // ASSERT
        result.Count.ShouldBe(2);
        result.First().Url.ShouldBe("https://dev.to");
        result.Last().Url.ShouldBe("https://github.com");

        // Verifica se o estado local foi criado e atualizado com o timestamp mais recente
        await _stateRepositoryMock.Received(1).SaveStateAsync(
            Arg.Is<AgentState>(s => s.LastSyncedPerProfile["Chrome_Default"] == item2.VisitedAtUtc),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ProcessPendingHistoryAsync_WithMultipleProfiles_ShouldFilterAndTrackIndependently()
    {
        // ARRANGE
        var chromeProfile = "Chrome_Default";
        var firefoxProfile = "Firefox_Default";
        var chromeLastSynced = new DateTime(2026, 8, 28, 7, 0, 0, DateTimeKind.Utc);
        var firefoxLastSynced = new DateTime(2026, 8, 28, 7, 30, 0, DateTimeKind.Utc);

        var existingState = new AgentState();
        existingState.LastSyncedPerProfile[chromeProfile] = chromeLastSynced;
        existingState.LastSyncedPerProfile[firefoxProfile] = firefoxLastSynced;

        _stateRepositoryMock
            .LoadStateAsync(Arg.Any<CancellationToken>())
            .Returns(existingState);

        var chromeOld = new BrowserHistoryItemDto(
            "https://chrome.old",
            "Chrome Old",
            chromeLastSynced.AddMinutes(-5),
            BrowserType.Chrome,
            "Default"
        );
        var chromeNew = new BrowserHistoryItemDto(
            "https://chrome.new",
            "Chrome New",
            chromeLastSynced.AddMinutes(10), // Novo para o Chrome
            BrowserType.Chrome,
            "Default"
        );

        var firefoxOld = new BrowserHistoryItemDto(
            "https://firefox.old",
            "Firefox Old",
            firefoxLastSynced.AddMinutes(-5),
            BrowserType.Firefox,
            "Default"
        );
        var firefoxNew = new BrowserHistoryItemDto(
            "https://firefox.new",
            "Firefox New",
            firefoxLastSynced.AddMinutes(5), // Novo para o Firefox
            BrowserType.Firefox,
            "Default"
        );

        _readerMock
            .ReadHistoryAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BrowserHistoryItemDto> { chromeOld, chromeNew, firefoxOld, firefoxNew });

        // ACT
        var result = (await _processor.ProcessPendingHistoryAsync()).ToList();

        // ASSERT
        result.Count.ShouldBe(2);
        result.ShouldContain(x => x.Url == "https://chrome.new");
        result.ShouldContain(x => x.Url == "https://firefox.new");

        // Verifica se ambos os perfis foram atualizados com seus respectivos timestamps de forma isolada
        await _stateRepositoryMock.Received(1).SaveStateAsync(
            Arg.Is<AgentState>(s => 
                s.LastSyncedPerProfile[chromeProfile] == chromeNew.VisitedAtUtc &&
                s.LastSyncedPerProfile[firefoxProfile] == firefoxNew.VisitedAtUtc
            ),
            Arg.Any<CancellationToken>()
        );
    }
}
