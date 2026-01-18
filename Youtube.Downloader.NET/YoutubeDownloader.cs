using System.Net;
using Youtube.Downloader.NET.Common;
using static Youtube.Downloader.NET.Common.Helpers;
using static Youtube.Downloader.NET.Common.Constants;

namespace Youtube.Downloader.NET;

public class YoutubeDownloader
{
    private readonly Platform _platform = default;

    private readonly string _dependencyBasePath = Path.Combine(Directory.GetCurrentDirectory(), "dependencies");

    private readonly string _downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");

    private string _ffmpegPath = string.Empty;

    private string _ytdlpPath = string.Empty;

    public YoutubeDownloader(Platform platform)
    {
        _platform = platform;
    }

    public YoutubeDownloader(Platform platform, string downloadPath)
    {
        _platform = platform;
        _downloadPath = downloadPath;
    }

    public YoutubeDownloader(Platform platform, string ffmpegPath, string ytdlpPath, string downloadPath)
    {
        _platform = platform;
        _downloadPath = downloadPath;
        _ffmpegPath = ffmpegPath.ValidatePath(Ffmpeg);
        _ytdlpPath = ytdlpPath.ValidatePath(Ytdlp);
    }

    public async Task DownloadAudioAsync(
        string url,
        AudioFormat format = AudioFormat.MP3,
        string additionalFlags = "",
        CancellationToken ctx = default)
    {
        if(string.IsNullOrEmpty(_ffmpegPath) || string.IsNullOrEmpty(_ytdlpPath))
            await ValidateDependencies(ctx).ConfigureAwait(false);

        var output = await ProcessRunner
            .RunAsync (
                _ytdlpPath,
                CreateCommand(SingleDownloadMp3Template, format.ToString().ToLower(), _ffmpegPath, _downloadPath, additionalFlags, url), 
                Console.WriteLine,
                ctx: ctx)
            .ConfigureAwait(false);

        if (output.ProcessStatus is ProcessStatus.Error)
            throw new YoutubeDownloaderException($"An error occured while trying to process download: ${output.Error}");
        
    }
    
    public async Task<IEnumerable<MultiDownloadResult>> DownloadAudioFromPlayList(
        string url,
        AudioFormat format = AudioFormat.MP3,
        string additionalFlags = "",
        CancellationToken ctx = default)
    {
        if(string.IsNullOrEmpty(_ffmpegPath) || string.IsNullOrEmpty(_ytdlpPath))
            await ValidateDependencies(ctx).ConfigureAwait(false);

        var playlistUrls = await GetPlayListUrls(url, ctx).ConfigureAwait(false);

        var command = 
            CreateCommand(SingleDownloadMp3Template, format.ToString().ToLower(), _ffmpegPath, _downloadPath,
            additionalFlags);

        var tasks = new List<Task<ProcessOutput>>();
        var results = new List<MultiDownloadResult>();

        foreach (var playlistUrl in playlistUrls)
            tasks
                .Add(ProcessRunner.RunAsync(_ytdlpPath, command + playlistUrl, Console.WriteLine, ctx: ctx));
        
        var taskResults = await Task.WhenAll(tasks).ConfigureAwait(false);
        

        for (int i = 0; i < playlistUrls.Length; i++)
        {
            results.Add(new MultiDownloadResult()
            {
                Url = playlistUrls[i],
                IsDownloaded = taskResults[i].ProcessStatus is ProcessStatus.Success, 
                Message = taskResults[i].Output,
                ErrorMessage = taskResults[i].Error,
            });
        }

        return results;
    }
    
    /// <summary>
    /// Get the List of URL for each video from a playlist
    /// </summary>
    /// <param name="url">The playlist url.</param>
    /// <param name="ctx">The Cancellation Token.</param>
    /// <returns>A list of urls.</returns>
    /// <exception cref="YoutubeDownloaderException">Throws this exception when the retrieving the urls fail.</exception>
    private async Task<string[]> GetPlayListUrls(string url, CancellationToken ctx = default)
    {
        var res = await ProcessRunner
            .RunAsync (
                _ytdlpPath,
                CreateCommand(GetPlaylistTemplate ,url), 
                Console.WriteLine,
                ctx: ctx)
            .ConfigureAwait(false);
        
        return res.ProcessStatus is ProcessStatus.Error 
            ? throw new YoutubeDownloaderException($"An error occured while trying to get the playlist urls: ${res.Error}") 
            : res!.Output!.Split('\n');
    }
    
    /// <summary>
    /// Sets up the dependencies (ffmpeg and yt-dlp).
    /// If the executables are already present in the dependency folder, the downloads are skipped.
    /// </summary>
    /// <param name="ctx">The Cancellation Token.</param>
    private async Task ValidateDependencies(CancellationToken ctx = default)
    {
        // Creates the dependency directory if it does not exist
        Directory.CreateDirectory(_dependencyBasePath);

        // Check if the executables are already present in the dependency folder
        // If not, downloads them.
        var dependencies = Directory.GetFiles(_dependencyBasePath);

        _ffmpegPath = dependencies.FirstOrDefault(x => Path.GetFileName(x).Contains(Ffmpeg), string.Empty);
        _ytdlpPath = dependencies.FirstOrDefault(x => Path.GetFileName(x).Contains(_platform.ToYtdlpPlatformExeName()),
            string.Empty);

        if (string.IsNullOrEmpty(_ffmpegPath))
            await DownloadFfmpeg(ctx: ctx).ConfigureAwait(false);

        if (string.IsNullOrEmpty(_ytdlpPath))
            await DownloadYtdlp(ctx: ctx).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads ffmpeg to the dependency folder, based on the specified platform.
    /// </summary>
    /// <param name="version">The version to download, defaults to the latest.</param>
    /// <param name="ctx">The Cancellation Token</param>
    /// <exception cref="YoutubeDownloaderException">Throws this exception when the Download fails.</exception>
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

            // Set the executable permissions for the downloaded binary.
            SetExecutablePermissionByPlatform(_ffmpegPath, _platform);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode is HttpStatusCode.NotFound)
                throw new YoutubeDownloaderException(
                    "Failed to download Ffmpeg: Could not find the specified FFmpeg Version.", e);

            throw new YoutubeDownloaderException(e.Message, e);
        }
        catch (Exception e)
        {
            throw new YoutubeDownloaderException(e.Message, e);
        }
    }

    /// <summary>
    /// Downloads yt-dlp to the dependency folder, based on the specified platform.
    /// </summary>
    /// <param name="version">The version to download, defaults to the latest.</param>
    /// <param name="ctx">The Cancellation Token</param>
    /// <exception cref="YoutubeDownloaderException">Throws this exception when the Download fails.</exception>
    private async Task DownloadYtdlp(string version = "latest", CancellationToken ctx = default)
    {
        var platformExecutable = _platform.ToYtdlpPlatformExeName();
        var filePath = Path.Combine(_dependencyBasePath, platformExecutable);
        var versionsUrl = Path.Combine(string.Format(YtdlpDownloadUrl, version), platformExecutable);

        try
        {
            using var client = new HttpClient();

            var response = await client.GetAsync(versionsUrl, ctx).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var fs = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920 * 4);

            await response.Content.CopyToAsync(fs, ctx).ConfigureAwait(false);
            _ytdlpPath = filePath;

            // Set the executable permissions for the downloaded binary.
            Helpers.SetExecutablePermissionByPlatform(_ytdlpPath, _platform);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode is HttpStatusCode.NotFound)
                throw new YoutubeDownloaderException(
                    "Failed to download Ffmpeg: Could not find the specified FFmpeg Version.", e);

            throw new YoutubeDownloaderException(e.Message, e);
        }
        catch (Exception e)
        {
            throw new YoutubeDownloaderException(e.Message, e);
        }
    }
}