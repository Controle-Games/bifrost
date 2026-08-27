using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bifrost.Agent.Configuration;
using Bifrost.Agent.Models;
using Bifrost.Agent.Services;
using Bifrost.Agent.Worker;
using Bifrost.Shared.DTOs;
using Bifrost.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Bifrost.Agent.Tests.Worker;

public class BifrostWorkerTests
{
    private readonly ILogger<BifrostWorker> _logger = Substitute.For<ILogger<BifrostWorker>>();
    private readonly IHistoryProcessor _history = Substitute.For<IHistoryProcessor>();
    private readonly IStateRepository _state = Substitute.For<IStateRepository>();
    private readonly IApiClient _api = Substitute.For<IApiClient>();
    private readonly IOptions<BrowserOptions> _options = Substitute.For<IOptions<BrowserOptions>>();

    public BifrostWorkerTests()
    {
        // Define uma configuração padrão de intervalo para os testes (1 minuto)
        _options.Value.Returns(new BrowserOptions { IntervalInMinutes = 1 });
    }

    [Fact]
    public async Task Deve_ProcessarEEnviarHistorico_Quando_ExistiremNovosItems()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var estadoFake = new AgentState();
        var itensFake = new List<BrowserHistoryItemDto>
        {
            new("https://github.com", "GitHub", DateTime.UtcNow, BrowserType.Chrome, "Default")
        };

        _state.LoadStateAsync(Arg.Any<CancellationToken>()).Returns(estadoFake);
        _history.ProcessPendingHistoryAsync(Arg.Any<CancellationToken>()).Returns(itensFake);

        var worker = new BifrostWorker(_logger, _history, _state, _api, _options);

        // Act - Executa e cancela para não entrar em loop infinito
        var task = worker.StartAsync(cts.Token);
        await Task.Delay(50); // Aguarda um pequeno intervalo para execução do ciclo
        await cts.CancelAsync();
        await worker.StopAsync(CancellationToken.None);

        // Assert
        await _state.Received(1).LoadStateAsync(Arg.Any<CancellationToken>());
        await _history.Received(1).ProcessPendingHistoryAsync(Arg.Any<CancellationToken>());
        await _api.Received(1).SendHistoryBatchAsync(Arg.Any<IEnumerable<BrowserHistoryItemDto>>(), Arg.Any<CancellationToken>());
        await _state.Received(1).SaveStateAsync(estadoFake, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NaoDeve_EnviarDadosParaAPI_SeNaoHouverNovosItens()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var estadoFake = new AgentState();
        var itensFake = new List<BrowserHistoryItemDto>(); // Sem novos itens

        _state.LoadStateAsync(Arg.Any<CancellationToken>()).Returns(estadoFake);
        _history.ProcessPendingHistoryAsync(Arg.Any<CancellationToken>()).Returns(itensFake);

        var worker = new BifrostWorker(_logger, _history, _state, _api, _options);

        // Act - Executa e cancela para não entrar em loop infinito
        var task = worker.StartAsync(cts.Token);
        await Task.Delay(50); // Aguarda um pequeno intervalo para execução do ciclo
        await cts.CancelAsync();
        await worker.StopAsync(CancellationToken.None);

        // Assert
        await _state.Received(1).LoadStateAsync(Arg.Any<CancellationToken>());
        await _history.Received(1).ProcessPendingHistoryAsync(Arg.Any<CancellationToken>());

        // Garante que a SendHistoryAsync e o SaveStateAsync NÃO foram chamados
        await _api.DidNotReceive().SendHistoryBatchAsync(Arg.Any<IEnumerable<BrowserHistoryItemDto>>(), Arg.Any<CancellationToken>());
        await _state.DidNotReceive().SaveStateAsync(Arg.Any<AgentState>(), Arg.Any<CancellationToken>());
     }
}
