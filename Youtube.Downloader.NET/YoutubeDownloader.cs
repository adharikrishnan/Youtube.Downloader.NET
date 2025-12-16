using System.IO.Compression;
using System.Net;
using Youtube.Downloader.NET.Common;
using static Youtube.Downloader.NET.Common.Constants;

namespace Youtube.Downloader.NET;

public class YoutubeDownloader
{
    private Platform _platform = default;
    
    private string _version = string.Empty;
    
    private string _ffmpegPath = string.Empty;
    
    private readonly string _downloadPath = Path.Combine(Directory.GetCurrentDirectory());
    
    public YoutubeDownloader()
    {
    }
    
    public YoutubeDownloader(string downloadPath)
    {
        _downloadPath = downloadPath;
    }

    /// <summary>
    /// Download a specific version of Ffmpeg.
    /// </summary>
    /// <param name="version">The Ffmpeg version.</param>
    /// <param name="platform">The Platform the application is running on.</param>
    /// <returns>True/False</returns>
    public async Task<bool> DownloadFfmpeg(string version = "latest", Platform platform = Platform.WindowsX64)
    {
        using var client = new HttpClient();
        var versionsUrl = string.Format(FfmpegVersionsUrl, version);
        
        try
        {
            var response = await client.GetStringAsync(versionsUrl).ConfigureAwait(false);
            var ffmpeg = response.DeserializeTo<FfBinaryResponse>();
            var zipUrl = ffmpeg?.Bin?[platform.ToPlatformString()].Ffmpeg;

            var zipResponse = await client.GetAsync(zipUrl).ConfigureAwait(false);

            var ffmpegFolderPath = Path.Combine(_downloadPath, "ffmpeg");
            
            _ffmpegPath = await ZipAndExtract(zipResponse, Path.Combine(ffmpegFolderPath, "ffmpeg")).ConfigureAwait(false);
            
            _platform = platform;
            _version = version;
            
            return true;
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode is HttpStatusCode.NotFound)
                throw new Exception("Could not find FFmpeg Version.");

            throw new Exception(e.Message, e);
        }

    }

    private async Task<string> ZipAndExtract(HttpResponseMessage responseMessage, string downloadPath)
    {
        var zipFilePath = Path.GetTempPath() + "/file.zip";
        
        Directory.CreateDirectory(downloadPath);
        
        // Write to temp Zip File and unzip to the specified download folder
        await using var fs = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write);
        await responseMessage.Content.CopyToAsync(fs).ConfigureAwait(false);
        ZipFile.ExtractToDirectory(zipFilePath, downloadPath, overwriteFiles: true);
        
        // Delete the temp zip file
        File.Delete(zipFilePath);

        return Directory.GetFiles(downloadPath).FirstOrDefault()!;
    }
}

