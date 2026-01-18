using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Youtube.Downloader.NET.Common;

public enum Platform
{
    WindowsX64,
    Linux32,
    Linux64,
    LinuxARM64,
    MacOSX64
}


[JsonConverter(typeof(EnumConverter))]
public enum AudioFormat
{
    MP3,
    AAC,
    BEST,
    FLAC,
    M4A,
    OPUS,
    VORBIS,
    WAV
}

public enum VideoFormat
{
    
}