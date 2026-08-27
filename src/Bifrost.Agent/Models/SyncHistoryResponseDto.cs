namespace Bifrost.Shared.DTOs;

public class SyncHistoryResponseDto
{
    public int ProcessedCount { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}
