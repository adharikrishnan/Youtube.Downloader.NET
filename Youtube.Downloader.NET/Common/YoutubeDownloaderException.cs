namespace Youtube.Downloader.NET.Common;

public class YoutubeDownloaderException : Exception
{
    public YoutubeDownloaderException(string message): base(message)
    {}
    
    public YoutubeDownloaderException(string message, Exception innerException) 
        : base(message, innerException)
    {}
}