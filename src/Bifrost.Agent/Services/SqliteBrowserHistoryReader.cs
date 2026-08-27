using System.Data;
using Bifrost.Agent.Configuration;
using Bifrost.Shared.DTOs;
using Bifrost.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Bifrost.Agent.Services;

public class SqliteBrowserHistoryReader : IBrowserHistoryReader
{
    private readonly BrowserOptions _options;
    private readonly ILogger<SqliteBrowserHistoryReader> _logger;

    public SqliteBrowserHistoryReader(
        IOptions<BrowserOptions> options,
        ILogger<SqliteBrowserHistoryReader> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<BrowserHistoryItemDto>> ReadHistoryAsync(CancellationToken cancellationToken = default)
    {
        var items = new List<BrowserHistoryItemDto>();

        // 1. Leitura de navegadores Chromium (Chrome e Edge)
        items.AddRange(await ReadChromiumHistoryAsync(BrowserType.Chrome, _options.Chrome, cancellationToken));
        items.AddRange(await ReadChromiumHistoryAsync(BrowserType.Edge, _options.Edge, cancellationToken));
        items.AddRange(await ReadChromiumHistoryAsync(BrowserType.Brave, _options.Brave, cancellationToken));

        // 2. Leitura do Firefox
        items.AddRange(await ReadFirefoxHistoryAsync(_options.Firefox, cancellationToken));

        return items;
    }
    
    private async Task<IEnumerable<BrowserHistoryItemDto>> ReadChromiumHistoryAsync(
        BrowserType browserType,
        BrowserProfileConfig config,
        CancellationToken cancellationToken)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var profilesPath = Path.Combine(appData, config.RelativePath);

        if (Directory.Exists(profilesPath) == false)
        {
            _logger.LogWarning("Diretório dos perfis do {Browser} não encontrado {Path}", browserType, profilesPath);
            return Enumerable.Empty<BrowserHistoryItemDto>();
        }

        // Procura em todas as pastas de perfil dos navegadores pelo arquivo History
        var historyFiles = Directory.GetFiles(profilesPath, config.FileName, SearchOption.AllDirectories);
        if (historyFiles.Length == 0)
        {
            return Enumerable.Empty<BrowserHistoryItemDto>();
        }

        var items = new List<BrowserHistoryItemDto>();
        foreach (var sourcePath in historyFiles)
        {
            var profileName = Path.GetFileName(Path.GetDirectoryName(sourcePath)) ?? "Unknown";
            var tempFilePath = GetTempCopyPath(browserType);

            try
            {
                // Shadow Copy do banco SQLite do navegador
                File.Copy(sourcePath, tempFilePath, overwrite: true);
    
                using var conn = new SqliteConnection($"Data Source={tempFilePath};Mode=ReadOnly;Cache=Shared");
                await conn.OpenAsync(cancellationToken);
    
                // Chrome Epoch: Microssegundos desde 01/01/1601
                // Convertido via SQL para Unix Timestamp em segundos
                var query = @"
                    SELECT url, title, (last_visit_time / 10000000 - 11644473600) AS visit_unix
                    FROM urls
                    WHERE url LIKE 'http%'
                    ORDER BY last_visit_time DESC
                    LIMIT 100";
    
                using var cmd = conn.CreateCommand();
                cmd.CommandText = query;
    
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var url = reader.GetString("url");
                    var title = reader.IsDBNull(reader.GetOrdinal("title")) ? null : reader.GetString("title");
                    var unixSeconds = reader.GetInt64("visit_unix");
                    var visitedAtUtc = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
    
                    items.Add(new BrowserHistoryItemDto(
                        url,
                        title,
                        visitedAtUtc,
                        browserType,
                        profileName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ler o histórico do {Browser}", browserType);
                return Enumerable.Empty<BrowserHistoryItemDto>();
            }
            finally
            {
                // Limpa o Shadow Copy
                CleanTempFile(tempFilePath);
            }
        }
        
        return items;
    }

    private async Task<IEnumerable<BrowserHistoryItemDto>> ReadFirefoxHistoryAsync(
        BrowserProfileConfig config,
        CancellationToken cancellationToken)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var profilesPath = Path.Combine(appData, config.RelativePath);

        if (Directory.Exists(profilesPath) == false)
        {
            _logger.LogWarning("Diretório de perfis do Firefox não encontrado: {Path}", profilesPath);
            return Enumerable.Empty<BrowserHistoryItemDto>();
        }

        // Procura em todas as pastas de perfil do Firefox pelo arquivo places.sqlite
        var historyFiles = Directory.GetFiles(profilesPath, config.FileName, SearchOption.AllDirectories);
        if (historyFiles.Length == 0)
        {
            _logger.LogWarning("Arquivo de histórico não encontrado para Firefox: places.sqlite");
            return Enumerable.Empty<BrowserHistoryItemDto>();
        }

        var items = new List<BrowserHistoryItemDto>();
        foreach (var sourcePath in historyFiles)
        {
            var profileName = Path.GetFileName(Path.GetDirectoryName(sourcePath)) ?? "Unknown";
            var tempFilePath = GetTempCopyPath(BrowserType.Firefox);

            try
            {
                // Shadow Copy do banco SQLite do navegador
                // File.Copy(sourcePath, tempFilePath, overwrite: true);
                //
                // Foi alterado para FileStream com FileShare para prevenir travamentos
                using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var destinationStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    await sourceStream.CopyToAsync(destinationStream, cancellationToken);
                }

                using var conn = new SqliteConnection($"Data Source={tempFilePath};Mode=ReadOnly;Cache=Shared");
                await conn.OpenAsync(cancellationToken);

                // Firefox PRTime: Microssegundos desde a era Unix (01/01/1970)
                var query = @"
                    SELECT h.url, h.title, (v.visit_date / 1000000) AS visit_unix
                    FROM moz_places h
                    INNER JOIN moz_historyvisits v on h.id = v.place_id
                    WHERE h.url LIKE 'http%'
                    ORDER BY v.visit_date DESC
                    LIMIT 100";

                using var cmd = conn.CreateCommand();
                cmd.CommandText = query;

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var url = reader.GetString("url");
                    var title = reader.IsDBNull(reader.GetOrdinal("title")) ? null : reader.GetString("title");
                    var unixSeconds = reader.GetInt64("visit_unix");
                    var visitedAtUtc = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;

                    items.Add(new BrowserHistoryItemDto(
                        url,
                        title,
                        visitedAtUtc,
                        BrowserType.Firefox,
                        profileName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ler o histórico do perfil {Profile} no Firefox", profileName);
                return Enumerable.Empty<BrowserHistoryItemDto>();
            }
            finally
            {
                // Limpa o Shadow Copy
                CleanTempFile(tempFilePath);
            }
        }

        return items;
    }

    private static string GetTempCopyPath(BrowserType browser) =>
        Path.Combine(Path.GetTempPath(), $"bifrost_history_{browser}_{Guid.NewGuid():N}.tmp");

    private void CleanTempFile(string filePath)
    {
        if (File.Exists(filePath) == false) return;

        try
        {
            // Limpa os pools de conexões mantidos pelo Microsoft.Data.Sqlite
            SqliteConnection.ClearAllPools();

            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível remover o aquivo temporário {Path}", filePath);
        }
    }
}
