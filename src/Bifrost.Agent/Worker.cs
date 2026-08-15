using Bifrost.Agent.Services;

namespace Bifrost.Agent;

public class Worker(ILogger<Worker> logger, IBrowserHistoryReader historyReader) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IBrowserHistoryReader _historyReader = historyReader;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Iniciando leitura do histórico de navegadores...");

        var items = await _historyReader.ReadHistoryAsync(stoppingToken);

        foreach (var item in items.Take(10))
        {
            _logger.LogInformation("[{Browser}] {Title} - {Url} ({VisitedAt})",
                item.Browser, item.Title ?? "Sem título", item.Url, item.VisitedAtUtc);
        }

        _logger.LogInformation("✅ Total de registros lidos: {Count}", items.Count());
        
       // while (!stoppingToken.IsCancellationRequested)
       // {
       //     if (logger.IsEnabled(LogLevel.Information))
       //     {
       //         logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
       //     }
       //     await Task.Delay(1000, stoppingToken);
       // }
    }
}
