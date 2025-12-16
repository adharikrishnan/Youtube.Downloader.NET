using System.Text.Json;

namespace Youtube.Downloader.NET.Common;

public static class Extensions
{
    public static T? DeserializeTo<T>(this string response)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<T>(response, options);
    }
    
    private static readonly Dictionary<Platform, string> OperatingSystemsMappings = new ()
    {
        { Platform.WindowsX64, "windows-64"},
        { Platform.Linux32 , "linux-32"},
        { Platform.Linux64, "linux-64"},
        { Platform.LinuxARM64 , "linux-arm64"},
        { Platform.LinuxARMhf  , "linux-armhf"},
        { Platform.LinuxARMel,  "linux-armel"},
        { Platform.MacOSX64, "osx-64"}
    };

    public static string ToPlatformString(this Platform platform) 
        => OperatingSystemsMappings[platform];
}

    
