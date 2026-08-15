using Bifrost.Shared.DTOs;

namespace Bifrost.Agent.Services;

public interface IHistoryProcessor
{
    Task<IEnumerable<BrowserHistoryItemDto>> ProcessPendingHistoryAsync(CancellationToken cancellationToken = default);
}
