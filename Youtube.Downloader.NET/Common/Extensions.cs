using System.Text.Json;

namespace Youtube.Downloader.NET.Common;

public static class Extensions
{
    private static readonly Dictionary<Platform, string> FfmpegPlatformMappings = new ()
    {
        { Platform.WindowsX64, "windows-64"},
        { Platform.Linux32 , "linux-32"},
        { Platform.Linux64, "linux-64"},
        { Platform.LinuxARM64 , "linux-arm64"},
        { Platform.MacOSX64, "osx-64"}
    };
    
    private static readonly Dictionary<Platform, string> YtdlpPlatformExeMappings = new ()
    {
        { Platform.WindowsX64, "yt-dlp.exe"},
        { Platform.Linux32 , "yt-dlp_linux"},
        { Platform.Linux64, "yt-dlp_linux"},
        { Platform.LinuxARM64 , "yt-dlp_linux_aarch64"},
        { Platform.MacOSX64, "yt-dlp_macos"}
    };
    
    public static string ToFfmpegPlatformString(this Platform platform) 
        => FfmpegPlatformMappings[platform];
    
    public static string ToYtdlpPlatformExeName(this Platform platform) 
        => YtdlpPlatformExeMappings[platform];
    
    public static T? DeserializeTo<T>(this string response)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<T>(response, options);
    }
}

    
