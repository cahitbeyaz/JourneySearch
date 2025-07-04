using Microsoft.AspNetCore.Http;

namespace ObiletJourneySearch.Utilities
{
    public static class RequestPathHelper
    {
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
