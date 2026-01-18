namespace Youtube.Downloader.NET.Common;

public static class Constants
{
    public const string FfmpegVersionsUrl = "https://ffbinaries.com/api/v1/version";
    
    public const string YtdlpDownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/{0}/download";

    public const string Ffmpeg = "ffmpeg";
    
    public const string Ytdlp = "yt-dlp";

    public const string SingleDownloadMp3Template = "-x --audio-format {0} -o \"%(title)s.%(ext)s\" --progress --newline --ffmpeg-location {1} -P {2} {3} {4}";
    
    public const string MultiDownloadMp3Template = "-x --audio-format {0} -o \"%(title)s.%(ext)s\" --progress --newline --ffmpeg-location {1} -P {2} {3} ";

    public const string GetPlaylistTemplate = "--flat-playlist --print url {0}";
}