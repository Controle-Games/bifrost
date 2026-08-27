using Bifrost.Shared.DTOs;

namespace Bifrost.Agent.Services;

public interface IApiClient
{
    /// <summary>
    /// Envia um lote de itens de histórico para a API do Bifrost.
    /// </summary>
    /// <param name="items">Lista de itens a serem sincronizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Objeto contendo o status de processamento da API.</returns>
    Task<SyncHistoryResponseDto> SendHistoryBatchAsync (
            IEnumerable<BrowserHistoryItemDto> items,
            CancellationToken cancellationToken = default);
}
