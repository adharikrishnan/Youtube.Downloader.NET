using System.Diagnostics;
using System.Text;

namespace Youtube.Downloader.NET.Common;

/// <summary>
/// Static Process Runner Class.
/// </summary>
public static class ProcessRunner
{
    /// <summary>
    /// Runs the Process with the given path to application path and supplied arguments.
    /// </summary>
    /// <param name="filePath">The file path to the application.</param>
    /// <param name="arguments">The Arguments to be passed to the Application.</param>
    /// <returns>The Process Output Object containing Process Data.</returns>
    public static ProcessOutput Run(string filePath, string arguments)
    {
        Process process = new Process();
        process.StartInfo = SetStartInfo(filePath, arguments);
        return RunProcess(process);
    }
    
    /// <summary>
    /// Runs the Process with the given path to application path and supplied arguments asynchronously.
    /// </summary>
    /// <param name="filePath">The file path to the application.</param>
    /// <param name="arguments">The Arguments to be passed to the Application.</param>
    /// <param name="cancellationToken">The Optional Cancellation Token.</param>
    /// <returns>The Process Output Object containing Process Data.</returns>

    public static async Task<ProcessOutput> RunAsync(string filePath, string? arguments = null,
        CancellationToken cancellationToken = default)
    {
        Process process = new Process();
        process.StartInfo = SetStartInfo(filePath, arguments);
        return await RunProcessAsync(process, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets a ProcessStartInfo Object with the given arguments.
    /// </summary>
    /// <param name="filePath">The file path to the application.</param>
    /// <param name="arguments">The Arguments.</param>
    /// <returns>The Process Start Info Data.</returns>
    private static ProcessStartInfo SetStartInfo(string filePath, string? arguments = null)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = filePath;
        startInfo.Arguments = arguments ?? null;
        startInfo.UseShellExecute = false;
        startInfo.StandardOutputEncoding = Encoding.UTF8;
        startInfo.StandardErrorEncoding = Encoding.UTF8;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        return startInfo;
    }

    /// <summary>
    /// Runs the provided process.
    /// </summary>
    /// <param name="process">The Process Object.</param>
    /// <returns>The Process Output.</returns>
    private static ProcessOutput RunProcess(Process process)
    {
        try
        {
            process.Start();
            process.WaitForExit();

            return new ProcessOutput(process, ProcessStatus.Success);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occured when trying to start process: {ex.Message}");
            return new ProcessOutput(process, ProcessStatus.Error, ex);
        }
        finally
        {
            process?.Dispose();
        }
    }

    /// <summary>
    /// Runs the provided process asynchronously.
    /// </summary>
    /// <param name="process">The Process Object.</param>
    /// <returns>The Process Output.</returns>
    private static async Task<ProcessOutput> RunProcessAsync(Process process, CancellationToken cancellationToken)
    {
        try
        {
            process.Start();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            return new ProcessOutput(process, ProcessStatus.Success);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occured when trying to start process: {ex.Message}");
            return new ProcessOutput(process, ProcessStatus.Error, ex);
        }
        finally
        {
            process?.Dispose();
        }
    }
}