using ObiletJourneySearch.ApiClient;
using ObiletJourneySearch.Models.DTOs;
using ObiletJourneySearch.Utilities;
using System.Text.Json;

namespace ObiletJourneySearch.Middleware
{
    /// <summary>
    /// Middleware responsible for managing Obilet API sessions across the application.
    /// This centralizes session management and ensures all requests have access to a valid session.
    /// </summary>
    public class ObiletSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IObiletApiClient _apiClient;
        private readonly ILogger<ObiletSessionMiddleware> _logger;
        private const string SessionKey = "ObiletDeviceSession";

        public ObiletSessionMiddleware(
            RequestDelegate next,
            IObiletApiClient apiClient,
            ILogger<ObiletSessionMiddleware> logger)
        {
            _next = next;
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip session creation for static files and error pages
            if (!RequiresSession(context.Request.Path))
            {
                await _next(context);
                return;
            }

            try
            {
                // Try to get an existing session from HTTP context
                var session = GetSessionFromContext(context);

                // If no session exists, create a new one
                if (session == null)
                {
                    // Create new session using injected API client
                    session = await CreateNewSessionAsync(context);
                }

                if (session != null)
                {
                    // Store session in HttpContext.Items for easy access in controllers
                    context.Items["ObiletSession"] = session;
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue with the request pipeline
                // This ensures error pages work even if session creation fails
                _logger.LogError(ex, "Failed to create or retrieve Obilet session");
            }

            // Continue with the request pipeline
            await _next(context);
        }

        /// <summary>
        /// Determines if a request path requires a session
        /// </summary>
        private bool RequiresSession(PathString path)
        {
            return RequestPathHelper.RequiresSession(path);
        }

        /// <summary>
        /// Get the current session from the HTTP context if available
        /// </summary>
        private DeviceSession GetSessionFromContext(HttpContext context)
        {
            if (context.Session.TryGetValue(SessionKey, out byte[] sessionData))
            {
                try
                {
                    var sessionJson = System.Text.Encoding.UTF8.GetString(sessionData);
                    return JsonSerializer.Deserialize<DeviceSession>(sessionJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing session data");
                }
            }
            
            return null;
        }

        /// <summary>
        /// Creates a new session by calling the Obilet API
        /// </summary>
        /// <param name="context">The HttpContext for the current request</param>
        private async Task<DeviceSession> CreateNewSessionAsync(HttpContext context)
        {
            try
            {
                var sessionRequest = new SessionRequest
                {
                    Type = 7,
                    Connection = new Connection
                    {
                        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        Port = context.Connection.RemotePort.ToString()
                    },
                    Browser = GetBrowserInfo(context)
                };

                var response = await _apiClient.GetSessionAsync(sessionRequest);
                
                if (response.Status == "Success" && response.Data != null)
                {
                    var session = new DeviceSession
                    {
                        SessionId = response.Data.SessionId,
                        DeviceId = response.Data.DeviceId
                    };
                    
                    // Store the session in HTTP context
                    StoreSession(context, session);
                    
                    _logger.LogInformation("Created new session with ID: {SessionId}", session.SessionId);
                    return session;
                }
                else
                {
                    _logger.LogError("Failed to create session. Status: {Status}, Message: {Message}", 
                        response.Status, response.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session");
            }
            
            return null;
        }

        /// <summary>
        /// Stores session in the HTTP session
        /// </summary>
        private void StoreSession(HttpContext context, DeviceSession session)
        {
            var sessionJson = JsonSerializer.Serialize(session);
            var sessionData = System.Text.Encoding.UTF8.GetBytes(sessionJson);
            context.Session.Set(SessionKey, sessionData);
        }

        /// <summary>
        /// Extracts browser information from the user agent
        /// </summary>
        private Browser GetBrowserInfo(HttpContext context)
        {
            try
            {
                return BrowserInfoExtractor.GetBrowserInfo(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting browser information");
                return new Browser { Name = "unknown", Version = "unknown" };
            }
        }

        /// <summary>
        /// Extracts version information from a user agent string
        /// </summary>
        private string ExtractVersion(string userAgent, string marker)
        {
            if (string.IsNullOrEmpty(userAgent) || string.IsNullOrEmpty(marker))
            {
                _logger.LogWarning("User agent or marker is empty when extracting version");
                return "unknown";
            }
            
            try
            {
                int markerIndex = userAgent.IndexOf(marker);
                if (markerIndex < 0)
                {
                    _logger.LogWarning("Marker '{Marker}' not found in user agent", marker);
                    return "unknown";
                }
                
                int index = markerIndex + marker.Length;
                int endIndex = index;
                
                // Find the end of the version string
                while (endIndex < userAgent.Length && 
                       (char.IsDigit(userAgent[endIndex]) || userAgent[endIndex] == '.'))
                {
                    endIndex++;
                }
                
                if (endIndex > index)
                {
                    string version = userAgent.Substring(index, endIndex - index);
                    return string.IsNullOrEmpty(version) ? "unknown" : version;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting browser version from user agent with marker {Marker}", marker);
            }
            
            return "unknown"; // Fallback version if extraction fails
        }
    }

    // Extension method for registering the middleware
    public static class ObiletSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseObiletSession(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ObiletSessionMiddleware>();
        }
    }
}
