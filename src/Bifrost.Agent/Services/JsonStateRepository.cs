using System.Text.Json;
using Bifrost.Agent.Models;

namespace Bifrost.Agent.Services;

public class JsonStateRepository : IStateRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonStateRepository> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public JsonStateRepository(
        ILogger<JsonStateRepository> logger,
        string? customDirectory = null)
    {
        _logger = logger;

        // Se nenhum diretório customizado for passado,
        // usa a pasta AppData (Windows) ou ~/.config (Linux/MacOS)
        var baseDir = customDirectory ??
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Bifrost"
            );

        Directory.CreateDirectory(baseDir);
        _filePath = Path.Combine(baseDir, "agent_state.json");
    }

    public async Task<AgentState> LoadStateAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_filePath) == false)
        {
            _logger.LogInformation("Arquivo de estado local não encontrado em {FilePath}. Criando um novo arquivo de estado inicial.", _filePath);
            return new AgentState();
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var state = await JsonSerializer.DeserializeAsync<AgentState>(stream, JsonOptions, cancellationToken);
            return state ?? new AgentState();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ler o arquivo de estado em {FilePath}. Criando um novo arquivo de estado inicial.", _filePath);
            return new AgentState();
        }
    }

    public async Task SaveStateAsync(AgentState state, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
            _logger.LogDebug("Estado do agente salvo com sucesso em {FilePath}.", _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar o arquivo de estado em {FilePath}.", _filePath);
            throw;
        }
    }
}
