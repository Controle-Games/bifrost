namespace Bifrost.Agent.Configuration;

public class BrowserOptions
{
    public const string SectionName = "BrowserOptions";

    public int IntervalInMinutes { get; set; } = 5;
    public BrowserProfileConfig Chrome { get; set; } = new();
    public BrowserProfileConfig Edge { get; set; } = new();
    public BrowserProfileConfig Firefox { get; set; } = new();
    public BrowserProfileConfig Brave { get; set; } = new();
    public BrowserProfileConfig Opera { get; set; } = new();
}

public class BrowserProfileConfig
{
    public string RelativePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
