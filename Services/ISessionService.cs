using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Services
{
    public interface ISessionService
    {
        Task<DeviceSession> GetOrCreateSessionAsync();
    
        DeviceSession GetCurrentSession();
    }
}
