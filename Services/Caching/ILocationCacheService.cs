using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Services.Caching
{
    public interface ILocationCacheService
    {
        Task<BusLocationResponse> GetAllLocationsAsync(BusLocationRequest request);
    }
}
