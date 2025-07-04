using Microsoft.AspNetCore.Http;
using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Utilities
{
    public static class SessionExtensions
    {
        public static DeviceSession GetObiletSession(this HttpContext context)
        {
            if (context.Items.TryGetValue("ObiletSession", out var sessionObj) && sessionObj is DeviceSession session)
            {
                return session;
            }
            return null;
        }
    }
}
