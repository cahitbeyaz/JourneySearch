using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Services
{
    public interface ISessionService
    {
        /// <summary>
        /// Gets or creates a session for the current user
        /// </summary>
        /// <returns>The device session information</returns>
        Task<DeviceSession> GetOrCreateSessionAsync();
        
        /// <summary>
        /// Gets the current device session if it exists
        /// </summary>
        /// <returns>The device session or null if no session exists</returns>
        DeviceSession GetCurrentSession();
    }
}
