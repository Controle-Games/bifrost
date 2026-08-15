using Bifrost.Shared.DTOs;

namespace Bifrost.Agent.Services;

public class HistoryProcessor : IHistoryProcessor
{
    private readonly IBrowserHistoryReader _reader;
    private readonly IStateRepository _stateRepository;
    private readonly ILogger<HistoryProcessor> _logger;

    public HistoryProcessor(
        IBrowserHistoryReader reader,
        IStateRepository stateRepository,
        ILogger<HistoryProcessor> logger)
    {
        _reader = reader;
        _stateRepository = stateRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<BrowserHistoryItemDto>> ProcessPendingHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var state = await _stateRepository.LoadStateAsync(cancellationToken);
        var rawHistory = await _reader.ReadHistoryAsync(cancellationToken);

        var pendingItems = new List<BrowserHistoryItemDto>();

        // Agrupa os itens por chave de perfil (ex. "Chrome_Default", "Firefox_Profile 1")
        var groupedByProfile = rawHistory.GroupBy(x => $"{x.Browser}_{x.ProfileName}");

        foreach (var group in groupedByProfile)
        {
            var profileKey = group.Key;

            // Busca o último timestamp gravado para esse perfil (ou DateTime.MinValue se for a 1ª vez)
            state.LastSyncedPerProfile.TryGetValue(profileKey, out var lastSyncedUtc);

            // APLICAÇÃO DO DELTA
            // Filtra apenas visitas estritamente posteriores ao último envio
            var deltaItems = group
                .Where(x => x.VisitedAtUtc > lastSyncedUtc)
                .OrderBy(x => x.VisitedAtUtc)
                .ToList();

            if (deltaItems.Any() == false)
            {
                _logger.LogInformation("Nenhum registro novo para o perfil {ProfileKey}.", profileKey);
                continue;
            }

            _logger.LogInformation("Encontrados {Count} novos registros para o perfil {ProfileKey}.", deltaItems.Count, profileKey);
            pendingItems.AddRange(deltaItems);

            // Atualiza o estado com a data e hora da última visita processada
            var latestVisit = deltaItems.Max(x => x.VisitedAtUtc);
            state.LastSyncedPerProfile[profileKey] = latestVisit;
        }

        if (pendingItems.Any())
        {
            await _stateRepository.SaveStateAsync(state, cancellationToken);
        }

        return pendingItems;
    }
}
