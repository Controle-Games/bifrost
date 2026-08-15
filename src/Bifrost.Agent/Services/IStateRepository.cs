using Bifrost.Agent.Models;

namespace Bifrost.Agent.Services;

public interface IStateRepository
{
    Task<AgentState> LoadStateAsync(CancellationToken cancellationToken = default);
    Task SaveStateAsync(AgentState state, CancellationToken cancellationToken = default);
}
