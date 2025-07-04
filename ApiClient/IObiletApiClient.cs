using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.ApiClient
{
    public interface IObiletApiClient
    {
        Task<SessionResponse> GetSessionAsync(SessionRequest request);
        Task<BusLocationResponse> GetBusLocationsAsync(BusLocationRequest request);
        Task<JourneyResponse> GetBusJourneysAsync(JourneyRequest request);
    }
}
