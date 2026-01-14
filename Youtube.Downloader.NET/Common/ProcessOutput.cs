using System.Diagnostics;

namespace Youtube.Downloader.NET.Common;

/// <summary>
/// Model class to return the output from a process in a standard form. 
/// </summary>
public sealed class ProcessOutput
{
    /// <summary>
    /// Creates a new instance of the <see cref="ProcessOutput"/> Class with a given Process Instance.
    /// </summary>
    public ProcessOutput(string standardOutput)
    {
        ProcessStatus = ProcessStatus.Success;
        Output = standardOutput;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ProcessOutput"/> Class with a given Process Instance.
    /// </summary>
    public ProcessOutput(string errorOutput, Exception? exception)
    {
        ProcessStatus = ProcessStatus.Error;
        Error = errorOutput;
        Exception = exception;
    }

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