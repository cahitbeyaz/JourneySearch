using Microsoft.AspNetCore.Http;
using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Utilities
{
    /// <summary>
    /// Extension methods to easily access the Obilet session
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Gets the Obilet device session from HttpContext
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <returns>The DeviceSession if available, otherwise null</returns>
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
