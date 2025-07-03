using Microsoft.AspNetCore.Http;

namespace ObiletJourneySearch.Utilities
{
    /// <summary>
    /// Helper class for request path operations
    /// </summary>
    public static class RequestPathHelper
    {
        /// <summary>
        /// Determines if a request path requires a session
        /// </summary>
        /// <param name="path">The request path to check</param>
        /// <returns>True if the path requires a session, false if not (static file, error page, etc.)</returns>
        public static bool RequiresSession(PathString path)
        {
            // Get the path as a lowercase string for case-insensitive comparison
            string pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
            
            // Skip session for static files, error pages, and favicon
            return !pathValue.StartsWith("/css") &&
                   !pathValue.StartsWith("/js") &&
                   !pathValue.StartsWith("/lib") &&
                   !pathValue.StartsWith("/images") &&
                   !pathValue.StartsWith("/error") &&
                   !pathValue.StartsWith("/home/error") &&
                   !pathValue.EndsWith("favicon.ico");
        }
    }
}
