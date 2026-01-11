namespace Youtube.Downloader.NET.Common;

public static class Constants
{
    public const string FfmpegVersionsUrl = "https://ffbinaries.com/api/v1/version";
    
    public const string YtdlpDownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/{0}/download";

    public const string Ffmpeg = "ffmpeg";
    
    public const string Ytdlp = "yt-dlp";

    public const string Mp3Template = "-x --audio-format mp3 -o \"%(title)s.%(ext)s\" --progress --newline --ffmpeg-location {0} -P {1} {2}";
}