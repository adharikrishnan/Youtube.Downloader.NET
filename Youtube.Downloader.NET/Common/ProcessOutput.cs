using System.Diagnostics;

namespace Youtube.Downloader.NET.Common;

/// <summary>
/// Model class to return the output from a process in a standard form. 
/// </summary>
public class ProcessOutput
{
    /// <summary>
    /// Default Constructor
    /// </summary>
    public ProcessOutput()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ProcessOutput"/> Class with a given Process Instance.
    /// </summary>
    /// <param name="process">The Process Instance.</param>
    public ProcessOutput(Process process, ProcessStatus status)
    {
        this.ExitCode = process.ExitCode;
        this.Output = process?.StandardOutput?.ReadToEnd();
        this.Error = process?.StandardError?.ReadToEnd();
        this.ProcessStatus = status;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ProcessOutput"/> Class with a given Process Instance.
    /// </summary>
    /// <param name="process">The Process Instance.</param>
    public ProcessOutput(Process process, ProcessStatus status, Exception exception) 
    : this(process, status)
    {
        this.Exception = exception;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ProcessOutput"/> Class with an Exit Code.
    /// </summary>
    /// <param name="exitCode">The Process Exit Code.</param>
    /// <param name="status">The Process Status.</param>
    public ProcessOutput(int exitCode, ProcessStatus status)
    {
        this.ExitCode = exitCode;
        this.ProcessStatus = status;
    }

    /// <summary>
    /// The Exit Code as an Integer.
    /// </summary>
    public int ExitCode { get;}

    /// <summary>
    /// The Output from a process as a string.
    /// </summary>
    public string? Output { get; }

    /// <summary>
    /// The Error from a process as a string.
    /// </summary>
    public string? Error { get; }
    
    /// <summary>
    /// The Process Status.
    /// </summary>
    public ProcessStatus ProcessStatus { get; }

    /// <summary>
    /// The Exception that occured during process execution, if any.
    /// </summary>
    public Exception? Exception { get; }
}

/// <summary>
/// The Process Status Enum.
/// </summary>
public enum ProcessStatus
{
    Success = 0,
    Error = 1
}