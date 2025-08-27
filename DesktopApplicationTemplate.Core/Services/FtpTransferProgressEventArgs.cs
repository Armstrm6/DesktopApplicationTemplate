using System;

namespace DesktopApplicationTemplate.Core.Services
{
    /// <summary>
    /// Provides progress information for an FTP file transfer.
    /// </summary>
    public sealed class FtpTransferProgressEventArgs : FtpTransferEventArgs
    {
        public FtpTransferProgressEventArgs(string path, long size, long bytesTransferred, bool isUpload)
            : base(path, size, isUpload)
        {
            BytesTransferred = bytesTransferred;
        }

        /// <summary>Gets the number of bytes transferred so far.</summary>
        public long BytesTransferred { get; }

        /// <summary>Gets the fractional progress between 0 and 1.</summary>
        public double Progress => Size == 0 ? 0 : (double)BytesTransferred / Size;
    }
}
