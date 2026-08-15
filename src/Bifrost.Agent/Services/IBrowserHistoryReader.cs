using Bifrost.Shared.DTOs;

namespace Bifrost.Agent.Services;

public interface IBrowserHistoryReader
{
    Task<IEnumerable<BrowserHistoryItemDto>> ReadHistoryAsync(CancellationToken cancellationToken = default);
}
