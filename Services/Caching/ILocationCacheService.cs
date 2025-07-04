using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Services.Caching
{
    /// <summary>
    /// Interface for caching bus locations
    /// </summary>
    public interface ILocationCacheService
    {
        /// <summary>
        /// Gets all bus locations from cache or loads them if not cached
        /// </summary>
        /// <param name="request">The bus location request</param>
        /// <returns>The bus location response</returns>
        Task<BusLocationResponse> GetAllLocationsAsync(BusLocationRequest request);
        
        /// <summary>
        /// Removes locations from the cache
        /// </summary>
        void InvalidateLocationsCache();
    }
}
