using Bifrost.Agent.Models;
using Bifrost.Agent.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Bifrost.Agent.Tests.Services;

public class JsonStateRepositoryTests : IDisposable
{
    private readonly string _tempDirectory;

    public JsonStateRepositoryTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"bifrost_state_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try { Directory.Delete(_tempDirectory, recursive: true); }
            catch { /* Ignora se houve algum arquivo temporário em liberação pelo SO */ }
        }
    }

    [Fact]
    public async Task LoadStateAsync_WhenFileDoesNotExists_ShouldReturnEmptyState()
    {
        var repository = new JsonStateRepository(NullLogger<JsonStateRepository>.Instance, _tempDirectory);

        var state = await repository.LoadStateAsync();

        state.ShouldNotBeNull();
        state.LastSyncedPerProfile.ShouldBeEmpty();
    }

    [Fact]
    public async Task SaveStateAsync_And_LoadStateAync_ShouldPersistAndRetrieveStateCorrectly()
    {
        var repository = new JsonStateRepository(NullLogger<JsonStateRepository>.Instance, _tempDirectory);
        var expectedDate = new DateTime(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc);

        var stateToSave = new AgentState();
        stateToSave.LastSyncedPerProfile["Chrome_Default"] = expectedDate;

        await repository.SaveStateAsync(stateToSave);
        var loadedState = await repository.LoadStateAsync();

        loadedState.ShouldNotBeNull();
        loadedState.LastSyncedPerProfile.ShouldContainKey("Chrome_Default");
        loadedState.LastSyncedPerProfile["Chrome_Default"].ShouldBe(expectedDate);
    }
}
