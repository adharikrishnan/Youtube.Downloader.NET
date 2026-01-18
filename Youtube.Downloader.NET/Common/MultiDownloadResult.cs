namespace Youtube.Downloader.NET.Common;

public sealed class MultiDownloadResult
{
    public required string Url { get; set; }
    public bool IsDownloaded { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}