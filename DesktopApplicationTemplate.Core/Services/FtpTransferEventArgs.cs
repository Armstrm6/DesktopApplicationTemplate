using System;

namespace DesktopApplicationTemplate.Core.Services
{
    /// <summary>
    /// Provides information about an FTP file transfer.
    /// </summary>
    public class FtpTransferEventArgs : EventArgs
    {
        public FtpTransferEventArgs(string path, long size, bool isUpload)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Size = size;
            IsUpload = isUpload;
        }

        /// <summary>
        /// Gets the file path relative to the FTP root.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the size of the transferred file in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Gets a value indicating whether the transfer was an upload (<c>true</c>) or download (<c>false</c>).
        /// </summary>
        public bool IsUpload { get; }
    }
}
