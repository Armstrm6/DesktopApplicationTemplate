using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services
{
    /// <summary>
    /// Provides methods for searching files on disk with caching.
    /// </summary>
    public interface IFileSearchService
    {
        /// <summary>
        /// Asynchronously retrieves files in the specified directory matching the search pattern.
        /// Results may be cached for subsequent calls.
        /// </summary>
        /// <param name="directoryPath">The directory to search.</param>
        /// <param name="searchPattern">The search pattern, e.g. "*.txt".</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A read-only collection of file paths.</returns>
        Task<IReadOnlyCollection<string>> GetFilesAsync(string directoryPath, string searchPattern, CancellationToken cancellationToken = default);
    }
}
