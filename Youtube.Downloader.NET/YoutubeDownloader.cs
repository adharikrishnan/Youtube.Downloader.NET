using System.Net;
using System.Text;
using Youtube.Downloader.NET.Common;
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
        // If not downloads them.
        var dependencies = Directory.GetFiles(_dependencyBasePath);

        _ffmpegPath = dependencies.FirstOrDefault(x => Path.GetFileName(x).Contains(Ffmpeg), string.Empty);
        _ytdlpPath = dependencies.FirstOrDefault(x => Path.GetFileName(x).Contains(_platform.ToYtdlpPlatformExeName()),
            string.Empty);

        if (string.IsNullOrEmpty(_ffmpegPath))
            await DownloadFfmpeg(ctx: ctx).ConfigureAwait(false);

        if (string.IsNullOrEmpty(_ytdlpPath))
            await DownloadYtdlp(ctx: ctx).ConfigureAwait(false);
    }

    public async Task DownloadVideoAsMp3Async(string url, CancellationToken ctx  = default)
    {
        if(string.IsNullOrEmpty(_ffmpegPath) && string.IsNullOrEmpty(_ytdlpPath))
            await ValidateDependencies(ctx).ConfigureAwait(false);

        var output = await ProcessRunner
            .RunAsync (
                _ytdlpPath,
                Helpers.CreateCommand(Mp3Template, _ffmpegPath, _downloadPath, url)
                , null,
                ctx)
            .ConfigureAwait(false);
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

            _ffmpegPath = await Helpers.ZipAndExtract(zipResponse, _dependencyBasePath, ctx).ConfigureAwait(false);

            // Set the executable permissions for the downloaded binary.
            Helpers.SetExecutablePermissionByPlatform(_ffmpegPath, _platform);
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