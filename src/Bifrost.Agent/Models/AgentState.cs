namespace Bifrost.Agent.Models;

public class AgentState
{
    /// <summary>
    /// Mapeia o identificador do perfil (ex. "Chrome_Default", "Edge_Profile 1")
    /// para o timestamp UTC do último registro processado com sucesso
    /// </summary>
    public Dictionary<string, DateTime> LastSyncedPerProfile { get; set; } = new();
}
