using System.Text.Json;

namespace Youtube.Downloader.NET;

public class YoutubeDownloader
{
    public YoutubeDownloader()
    {
    }


    public async Task<bool> DownloadFfmpeg()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://ffbinaries.com/api/v1/version/");
        
        var response = await client.GetStringAsync("6.1").ConfigureAwait(false);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var ffmpeg = JsonSerializer.Deserialize<FFBinaryResponse>(response, options);
        
        return await Task.FromResult(true);
    }

    public async Task<bool> DownloadClient(string downloadPath, string osVersion)
    {
        

        return await Task.FromResult(true);
    }
}

public class FFBinaryResponse
{
    public string Version { get; set; }
    public string Permalink { get; set; }
    public Dictionary<string, PlatformBinary> Bin { get; set; }
}

public class PlatformBinary
{
    public string Ffmpeg { get; set; }
    public string Ffprobe { get; set; }
}

