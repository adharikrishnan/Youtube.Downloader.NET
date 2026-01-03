using System.IO.Compression;
using System.Net;
using System.Text.Json;
using Youtube.Downloader.NET.Common;
using static Youtube.Downloader.NET.Common.Constants;

namespace Youtube.Downloader.NET;

public class YoutubeDownloader
{
    private readonly Platform _platform = default;
    
    private readonly string _dependencyBasePath = Path.Combine(Directory.GetCurrentDirectory(), "dependencies");
    
    private readonly string _downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
    
    private readonly string _configPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
    
    private string _ffmpegPath = string.Empty;
    
    private string _ytdlpPath = string.Empty;
    
    public YoutubeDownloader(Platform platform)
    {
        _platform = platform;
        Directory.CreateDirectory(_downloadPath);
    }
    
    public YoutubeDownloader(Platform platform, string downloadPath)
    {
        _platform = platform;
        _downloadPath = downloadPath;
    }
    
    public YoutubeDownloader(Platform platform, string ffmpegPath, string ytdlpPath,  string downloadPath)
    {
        _platform = platform;
        _ffmpegPath = ffmpegPath;
        _ytdlpPath = ytdlpPath;
        _downloadPath = downloadPath;
    }
    
    /// <summary>
    /// Sets up the dependencies (ffmpeg and yt-dlp).
    /// If the executables are already present in the dependency folder
    /// </summary>
    /// <param name="ctx"></param>
    public async Task SetupDependencies(CancellationToken ctx = default)
    {
        var exe = Directory.GetFiles(_dependencyBasePath);
        
        // Check if the executables are already present in the dependency folder
        // If not downloads them.
        _ffmpegPath = exe.FirstOrDefault(x => Path.GetFileName(x).Contains("ffmpeg"), string.Empty);
        _ytdlpPath = exe.FirstOrDefault(x => Path.GetFileName(x).Contains("yt-dlp"), string.Empty);
        
        if(string.IsNullOrEmpty(_ffmpegPath))
            await DownloadFfmpeg(ctx: ctx);
        
        if(string.IsNullOrEmpty(_ytdlpPath))
            await DownloadYtdlp(ctx: ctx);
    }
    
    private async Task DownloadFfmpeg(string version = "latest", CancellationToken ctx = default)
    {
        using var client = new HttpClient();
        var versionsUrl = Path.Combine(FfmpegVersionsUrl, version);

        try
        {
            var response = await client.GetStringAsync(versionsUrl, ctx).ConfigureAwait(false);
            var ffmpeg = response.DeserializeTo<FfBinaryResponse>();
            var zipUrl = ffmpeg?.Bin?[_platform.ToFfmpegPlatformString()].Ffmpeg;

            var zipResponse = await client.GetAsync(zipUrl, ctx).ConfigureAwait(false);
            
            _ffmpegPath = await ZipAndExtract(zipResponse, _dependencyBasePath, ctx).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode is HttpStatusCode.NotFound)
                throw new YoutubeDownloaderException("Failed to download Ffmpeg: Could not find the specified FFmpeg Version.", e);

            throw new YoutubeDownloaderException(e.Message, e);
        }
        catch (Exception e)
        {
            throw new YoutubeDownloaderException(e.Message, e);
        }
    }

    private async Task DownloadYtdlp(string version = "latest", CancellationToken ctx = default)
    {
        var platformExecutable = _platform.ToYtdlpPlatformExeName();
        var filePath = Path.Combine(_dependencyBasePath, platformExecutable);
        var versionsUrl = Path.Combine(string.Format(YtdlpDownloadUrl, version), platformExecutable);
        
        try
        {
            using var client = new HttpClient();
                
            var response = await client.GetAsync(versionsUrl, ctx).ConfigureAwait(false);

            await using var fs = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920 * 4);
            
            await response.Content.CopyToAsync(fs, ctx).ConfigureAwait(false);
            _ytdlpPath = filePath;
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode is HttpStatusCode.NotFound)
                throw new YoutubeDownloaderException("Failed to download Ffmpeg: Could not find the specified FFmpeg Version.", e);

            throw new YoutubeDownloaderException(e.Message, e);
        }
        catch (Exception e)
        {
            throw new YoutubeDownloaderException(e.Message, e);
        }
    }
    
    private static async Task<string> ZipAndExtract(HttpResponseMessage responseMessage, string downloadPath, CancellationToken ctx = default)
    {
        var zipFilePath = Path.GetTempPath() + "/file.zip";
        
        Directory.CreateDirectory(downloadPath);
        
        // Write to temp Zip File and unzip to the specified download folder
        await using var fs = new FileStream(
            zipFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920 * 4);
        
        await responseMessage.Content.CopyToAsync(fs, ctx).ConfigureAwait(false);
        ZipFile.ExtractToDirectory(zipFilePath, downloadPath, overwriteFiles: true);
        
        // Delete the temp zip file
        File.Delete(zipFilePath);

        return Directory.GetFiles(downloadPath).FirstOrDefault()!;
    }
}

