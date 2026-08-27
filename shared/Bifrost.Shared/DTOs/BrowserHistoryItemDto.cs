using Bifrost.Shared.Enums;

namespace Bifrost.Shared.DTOs;

public record BrowserHistoryItemDto(
    string Url,
    string? Title,
    DateTime VisitedAtUtc,
    BrowserType Browser,
    string? ProfileName
);
