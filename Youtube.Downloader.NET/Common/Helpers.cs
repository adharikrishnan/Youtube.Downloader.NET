using System.IO.Compression;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Youtube.Downloader.NET.Common;

public static class Helpers
{
    /// <summary>
    /// Set the file permissions to be executable by the current user. 
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="platform">The platform.</param>
    public static void SetExecutablePermissionByPlatform(string filePath, Platform platform)
    {
        // Set the Executable Permission by platform
        switch (platform)
        {
            case Platform.Linux32 or Platform.Linux64 or Platform.LinuxARM64:
                SetExecutablePermissionLinux(filePath);
                break;
            case Platform.WindowsX64:
                SetExecutablePermissionWindows(filePath);
                break;
            case Platform.MacOSX64:
                // TODO: Set MAC OS implementation as required.
                break;
        }
    }

    /// <summary>
    /// Downloads a zip file from an HTTP Response message and extracts the files to the specified folder.
    /// </summary>
    /// <param name="responseMessage">The HTTP Response Message to download the ZIP from.</param>
    /// <param name="downloadPath">The destination path for the extracted files.</param>
    /// <param name="ctx">The Cancellation Token.</param>
    /// <returns>The path to the extracted zip files </returns>
    public static async Task<string> ZipAndExtract(HttpResponseMessage responseMessage, string downloadPath,
        CancellationToken ctx = default)
    {
        var zipFilePath = Path.Combine(downloadPath, "file.zip");

        try
        {
            // Write to a temp zip File and unzip to the specified download folder
            await using (var fs = new FileStream(
                zipFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920 * 4))
            {
                // Copy to zip file to stream.
                await responseMessage.Content.CopyToAsync(fs, ctx).ConfigureAwait(false);
            }
            
            ZipFile.ExtractToDirectory(zipFilePath, downloadPath, overwriteFiles: true);
            
            return Directory.GetFiles(downloadPath).FirstOrDefault()!;
        }
        finally
        {
            // Delete the temp zip file
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);
        }
    }
    
    /// <summary>
    /// Set the Executable Permission in Linux for the specified file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    private static void SetExecutablePermissionLinux(string filePath)
    {
        ProcessRunner.Run("chmod", $"u+x {filePath}");
    }
    
    /// <summary>
    /// Set the Executable Permission in Windows for the specified file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    private static void SetExecutablePermissionWindows(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var fileSecurity = FileSystemAclExtensions.GetAccessControl(fileInfo);
        var user = WindowsIdentity.GetCurrent().Name;
        
        var rule = new FileSystemAccessRule(user, FileSystemRights.ExecuteFile, AccessControlType.Allow);
        fileSecurity.AddAccessRule(rule);
        FileSystemAclExtensions.SetAccessControl(fileInfo, fileSecurity);
    }
}