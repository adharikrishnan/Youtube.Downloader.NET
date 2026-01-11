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
        var process = new Process();
        process.StartInfo = SetStartInfo(filePath, arguments);
        return RunProcess(process);
    }

    /// <summary>
    /// Runs the Process with the given path to application path and supplied arguments asynchronously.
    /// </summary>
    /// <param name="filePath">The file path to the application.</param>
    /// <param name="arguments">The Arguments to be passed to the Application.</param>
    /// <param name="outputCallBack">The optional output call back.</param>
    /// <param name="ctx">The Optional Cancellation Token.</param>
    /// <returns>The Process Output Object containing Process Data.</returns>
    public static async Task<ProcessOutput> RunAsync(string filePath, string? arguments = null,
        Action<string>? outputCallBack = null, CancellationToken ctx = default)
    {
        var process = new Process();

        process.StartInfo = SetStartInfo(filePath, arguments);
        return await RunProcessAsync(process, outputCallBack, ctx).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets a ProcessStartInfo Object with the given arguments.
    /// </summary>
    /// <param name="filePath">The file path to the application.</param>
    /// <param name="arguments">The Arguments.</param>
    /// <returns>The Process Start Info Data.</returns>
    private static ProcessStartInfo SetStartInfo(string filePath, string? arguments = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = arguments ?? null,
            UseShellExecute = false,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

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

            return new ProcessOutput(process.ExitCode, process.StandardOutput.ReadToEnd());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occured when trying to start process: {ex.Message}");
            return new ProcessOutput(process.ExitCode, process.StandardError.ReadToEnd(), ex);
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
    /// <param name="ctx">The Cancellation Token</param>
    /// <returns>The Process Output.</returns>
    private static async Task<ProcessOutput> RunProcessAsync(Process process, Action<string>? outputCallback = null,
        CancellationToken ctx = default)
    {
        var standardOutput = new StringBuilder();
        var standardError = new StringBuilder();
        
        try
        {
            process.Start();

            process.OutputDataReceived += (s, e) =>
            {
                standardOutput.AppendLine(e.Data);
                outputCallback?.Invoke(e.Data);
            };
            
            process.ErrorDataReceived += (s, e) =>
            {
                standardError.AppendLine(e.Data);
                outputCallback?.Invoke(e.Data);
            };
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync(ctx).ConfigureAwait(false);
            return new ProcessOutput(process.ExitCode, standardOutput.ToString());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occured when trying to start process: {ex.Message}");
            return new ProcessOutput(process.ExitCode, standardError.ToString(), ex);
        }
        finally
        {
            process?.Dispose();
        }
    }
}