namespace Bifrost.Shared.DTOs;

public record IngestionPayloadDto(
    string MachineName,
    string UserName,
    IReadOnlyCollection<BrowserHistoryItemDto> Items
);
