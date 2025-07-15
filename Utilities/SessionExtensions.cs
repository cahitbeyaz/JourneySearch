using Microsoft.AspNetCore.Http;
using ObiletJourneySearch.Middleware;
using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Utilities
{
    public static class SessionExtensions
    {
        public static DeviceSession GetObiletSession(this HttpContext context)
        {
            if (context.Items.TryGetValue(ObiletSessionMiddleware.ObiletSessionKey, out var sessionObj) && sessionObj is DeviceSession session)
            {
                return session;
            }
            return null;
        }
    }
}
