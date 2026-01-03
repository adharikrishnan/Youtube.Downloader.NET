// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable NotAccessedPositionalProperty.Global

namespace Youtube.Downloader.NET.Common;

public sealed record PlatformBinary(string Ffmpeg, string Ffprobe);

public sealed class FfBinaryResponse
{
    public required string Version { get; init; }
    
    public required string Permalink { get; init; }
    
    public Dictionary<string, PlatformBinary>? Bin { get; init; }
}