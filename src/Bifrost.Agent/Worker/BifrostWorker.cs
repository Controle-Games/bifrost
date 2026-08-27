using Bifrost.Agent.Configuration;
using Bifrost.Agent.Services;
using Microsoft.Extensions.Options;

namespace Bifrost.Agent.Worker;

/// <summary>
/// Serviço em segundo plano responsável por gerenciar o ciclo de coleta,
/// processamento de delta e envio de histórico de navegadores para a API.
/// </summary>
public class BifrostWorker : BackgroundService
{
    private readonly ILogger<BifrostWorker> _logger;
    private readonly IHistoryProcessor _historyProcessor;
    private readonly IStateRepository _stateRepository;
    private readonly IApiClient _apiClient;
    private readonly BrowserOptions _options;

    // Intervalo de execução (por padrão 5 min)
    private readonly TimeSpan _executionInterval = TimeSpan.FromMinutes(5);

    public BifrostWorker(
        ILogger<BifrostWorker> logger,
        IHistoryProcessor historyProcessor,
        IStateRepository stateRepository,
        IApiClient apiClient,
        IOptions<BrowserOptions> options)
    {
        _logger = logger;
        _historyProcessor = historyProcessor;
        _stateRepository = stateRepository;
        _apiClient = apiClient;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Garante que o intervalo seja de pelo menos 1 min para evitar loop infinito.
        var minutosConfigurados = _options.IntervalInMinutes <= 0 ? 1 : _options.IntervalInMinutes;
        var delayInterval = TimeSpan.FromMinutes(minutosConfigurados);

        _logger.LogInformation("Serviço BifrostWorker inicializado. Monitoramento configurado para rodar a cada {Minutes} min.", minutosConfigurados);

        while (cancellationToken.IsCancellationRequested == false)
        {
            _logger.LogInformation("Iniciando ciclo de coleta do histórico...");

            try
            {
                // 1. Carrega o estado atual de sincronização do arquivo JSON local
                var estadoLocal = await _stateRepository.LoadStateAsync(cancellationToken);

                // 2. Extrai e filtra o delta de histórico
                var novosItens = await _historyProcessor.ProcessPendingHistoryAsync(cancellationToken);

                // 3. Se encontrar algum registro
                if (novosItens != null && novosItens.Any())
                {
                    _logger.LogInformation("Encontrados {Count} novos registros de histórico. Enviando para API...", novosItens.Count());

                    // 4. Envia os registros coletados para API via ApiClient
                    await _apiClient.SendHistoryBatchAsync(novosItens, cancellationToken);

                    // 5. Atualiza e persiste o novo estado local
                    await _stateRepository.SaveStateAsync(estadoLocal, cancellationToken);

                    _logger.LogInformation("Sincronização de histórico realizada com sucesso!");
                }
                else
                {
                    _logger.LogInformation("Nenhum registro novo encontrado neste ciclo.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Falha na execução deste ciclo. Aguarde {minutosConfigurados} min. para o novo ciclo.");
            }

            // 6. Aguarda 5 minutos para iniciar o próximo ciclo
            await Task.Delay(delayInterval, cancellationToken);
        }
    }
}
