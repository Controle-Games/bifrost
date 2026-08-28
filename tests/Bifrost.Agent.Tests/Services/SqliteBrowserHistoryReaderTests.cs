using Bifrost.Agent.Configuration;
using Bifrost.Agent.Services;
using Bifrost.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace Bifrost.Agent.Tests.Services;

public class SqliteBrowserHistoryReaderTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly ILogger<SqliteBrowserHistoryReader> _loggerMock;

    public SqliteBrowserHistoryReaderTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"bifrost_sqlite_reader_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
        
        // Substituído Moq por NSubstitute
        _loggerMock = Substitute.For<ILogger<SqliteBrowserHistoryReader>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
            catch
            {
                /* Ignora se houver lock temporário do SO */
            }
        }
    }

    [Fact]
    public async Task ReadHistoryAsync_WhenDirectoriesDoNotExist_ShouldReturnEmptyList()
    {
        // ARRANGE
        var options = Options.Create(new BrowserOptions
        {
            Chrome = new BrowserProfileConfig { RelativePath = "Chrome", FileName = "History" },
            Edge = new BrowserProfileConfig { RelativePath = "Edge", FileName = "History" },
            Brave = new BrowserProfileConfig { RelativePath = "Brave", FileName = "History" },
            Firefox = new BrowserProfileConfig { RelativePath = "Firefox", FileName = "places.sqlite" },
        });
        
        var reader = new SqliteBrowserHistoryReader(options, _loggerMock);

        // ACT
        var result = await reader.ReadHistoryAsync();

        // ASSERT
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReadChromiumHistoryAsync_WithValidSqliteDatabase_ShouldMapToDtosCorrectly()
    {
        // ARRANGE
        var chromeUserData = Path.Combine(_tempDirectory, "ChromeUserData");
        var defaultProfile = Path.Combine(chromeUserData, "Default");
        Directory.CreateDirectory(defaultProfile);
        
        var sqlitePath = Path.Combine(defaultProfile, "History");
        await CreateFakeChromiumDatabaseAsync(sqlitePath);

        var options = Options.Create(new BrowserOptions
        {
            Chrome = new BrowserProfileConfig { RelativePath = chromeUserData, FileName = "History" },
            Edge = new BrowserProfileConfig { RelativePath = "EmptyPath", FileName = "History" },
            Brave = new BrowserProfileConfig { RelativePath = "EmptyPath", FileName = "History" },
            Firefox = new BrowserProfileConfig { RelativePath = "EmptyPath", FileName = "places.sqlite" },
        });
        
        var reader = new SqliteBrowserHistoryReader(options, _loggerMock);

        // ACT
        var result = await reader.ReadHistoryAsync();

        // ASSERT
        var historyList = result.ToList();
        historyList.ShouldNotBeEmpty();
        
        var item = historyList.FirstOrDefault(x => x.Browser == BrowserType.Chrome);
        item.ShouldNotBeNull();
        item!.Url.ShouldBe("https://teste.dev");
        item.Title.ShouldBe("Page Title");
        item.ProfileName.ShouldBe("Default");
        item.VisitedAtUtc.Year.ShouldBeGreaterThan(2020);
    }

    private static async Task CreateFakeChromiumDatabaseAsync(string dbPath)
    {
        using var conn = new SqliteConnection($"Data Source={dbPath}");
        await conn.OpenAsync();
        
        var createTableQuery = @"
            CREATE TABLE urls (
                id INTEGER PRIMARY KEY,
                url TEXT NOT NULL,
                title TEXT,
                last_visit_time INTEGER NOT NULL
            );";
            
        using var cmd = conn.CreateCommand();
        cmd.CommandText = createTableQuery;
        await cmd.ExecuteNonQueryAsync();

        var insertQuery = @"
            INSERT INTO urls (url, title, last_visit_time) 
            VALUES ('https://teste.dev', 'Page Title', 133550000000000000);";
            
        cmd.CommandText = insertQuery;
        await cmd.ExecuteNonQueryAsync();
    }
}
