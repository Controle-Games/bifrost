using System.Net.Http.Json;
using System.Text.Json;
using Bifrost.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace Bifrost.Agent.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new () { PropertyNameCaseInsensitive = true };

    public ApiClient (HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SyncHistoryResponseDto> SendHistoryBatchAsync(
        IEnumerable<BrowserHistoryItemDto> items,
        CancellationToken cancellationToken = default)
    {
        var itemsList = items.ToList();

        if (!itemsList.Any())
        {
            _logger.LogDebug("Nenhum item para enviar para a API.");
            return new SyncHistoryResponseDto
            {
                Success = true,
                ProcessedCount = 0,
                Message = "Nenhum item enviado."
            };
        }

        try
        {
            _logger.LogInformation("Enviando lote de {Count} registros para a API...", itemsList.Count);

            var response = await _httpClient.PostAsJsonAsync("api/v1/history/batch", itemsList, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SyncHistoryResponseDto>(JsonOptions, cancellationToken);
                _logger.LogInformation("Lote enviado com sucesso. Processados pela API: {Count}", result?.ProcessedCount ?? 0);
                return result ?? new SyncHistoryResponseDto
                {
                    Success = true,
                    ProcessedCount = itemsList.Count
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Falha ao enviar lote para a API. Status {StatusCode}, Resposta {ErrorContent}", response.StatusCode, errorContent);

            return new SyncHistoryResponseDto
            {
                Success = false,
                Message = $"Erro HTTP {response.StatusCode}: {errorContent}"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha de rede/conexão ao tentar se comunicar com a API em {BaseAddress}.", _httpClient.BaseAddress);
            return new SyncHistoryResponseDto
            {
                Success = false,
                Message = $"Falha de conexão: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar lote de histórico");
            throw;
        }
    }
}
