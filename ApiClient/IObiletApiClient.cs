using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.ApiClient
{
    public interface IObiletApiClient
    {
        /// <summary>
        /// Creates a session with the obilet.com API
        /// </summary>
        /// <param name="request">The session request information</param>
        /// <returns>Session response with session ID and device ID</returns>
        Task<SessionResponse> GetSessionAsync(SessionRequest request);
        
        /// <summary>
        /// Gets all bus locations or searches for locations based on the provided query
        /// </summary>
        /// <param name="request">The bus location request with optional search query</param>
        /// <returns>List of bus locations</returns>
        Task<BusLocationResponse> GetBusLocationsAsync(BusLocationRequest request);
        
        /// <summary>
        /// Gets available journeys between the origin and destination on the specified date
        /// </summary>
        /// <param name="request">The journey request with origin, destination, and date information</param>
        /// <returns>List of available journeys</returns>
        Task<JourneyResponse> GetBusJourneysAsync(JourneyRequest request);
    }
}
