namespace Youtube.Downloader.NET.Common;

public class YoutubeDownloaderException(string message, Exception innerException)
    : Exception(message, innerException);