using System.Net;
using ObiletJourneySearch.ApiClient;
using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Services
{
    public class SessionService : ISessionService
    {
        private readonly IObiletApiClient _apiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SessionService> _logger;
        private const string SessionKey = "ObiletDeviceSession";

        public SessionService(IObiletApiClient apiClient, IHttpContextAccessor httpContextAccessor, ILogger<SessionService> logger)
        {
            _apiClient = apiClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<DeviceSession> GetOrCreateSessionAsync()
        {
            // Try to get the session from the current HTTP context
            var session = GetCurrentSession();
            
            // If no session exists, create a new one
            if (session == null)
            {
                try
                {
                    var sessionRequest = CreateSessionRequest();
                    var response = await _apiClient.GetSessionAsync(sessionRequest);
                    
                    if (response.Status == "Success" && response.Data != null)
                    {
                        session = new DeviceSession
                        {
                            SessionId = response.Data.SessionId,
                            DeviceId = response.Data.DeviceId
                        };
                        
                        // Store the session in the HTTP context
                        StoreSession(session);
                        
                        _logger.LogInformation("Created new session with ID: {SessionId}", session.SessionId);
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
            }
            
            return session;
        }

        public DeviceSession GetCurrentSession()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Session.TryGetValue(SessionKey, out byte[] sessionData))
            {
                try
                {
                    var sessionJson = System.Text.Encoding.UTF8.GetString(sessionData);
                    return System.Text.Json.JsonSerializer.Deserialize<DeviceSession>(sessionJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving session data");
                }
            }
            
            return null;
        }

        private void StoreSession(DeviceSession session)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var sessionJson = System.Text.Json.JsonSerializer.Serialize(session);
                var sessionData = System.Text.Encoding.UTF8.GetBytes(sessionJson);
                context.Session.Set(SessionKey, sessionData);
            }
        }

        private SessionRequest CreateSessionRequest()
        {
            return new SessionRequest
            {
                Type = 7,
                Connection = new Connection
                {
                    IpAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Port = _httpContextAccessor?.HttpContext?.Connection?.RemotePort.ToString()
                },
                Browser = GetBrowserInfo(),
                Application = new Application
                {
                    Version = "1.0.0.0",
                    EquipmentId = "distribusion"
                }
            };
        }
        
        private Browser GetBrowserInfo()
        {
            try
            {
                var userAgent = _httpContextAccessor?.HttpContext?.Request?.Headers.UserAgent.ToString() ?? "";
                string browserName = "unknown";
                string browserVersion = "unknown";
                
                if (string.IsNullOrEmpty(userAgent))
                {
                    _logger.LogWarning("User agent is empty or null");
                    return new Browser { Name = browserName, Version = browserVersion };
                }
                
                // Extract browser name and version from user agent
                try
                {
                    if (userAgent.Contains("Firefox/"))
                    {
                        browserName = "Firefox";
                        browserVersion = ExtractVersion(userAgent, "Firefox/");
                    }
                    else if (userAgent.Contains("Edg/"))
                    {
                        browserName = "Edge";
                        browserVersion = ExtractVersion(userAgent, "Edg/");
                    }
                    else if (userAgent.Contains("Chrome/"))
                    {
                        browserName = "Chrome";
                        browserVersion = ExtractVersion(userAgent, "Chrome/");
                    }
                    else if (userAgent.Contains("Safari/") && userAgent.Contains("Version/"))
                    {
                        browserName = "Safari";
                        browserVersion = ExtractVersion(userAgent, "Version/");
                    }
                    else
                    {
                        _logger.LogInformation("Could not identify browser from user agent: {UserAgent}", userAgent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error determining browser from user agent");
                }
                
                return new Browser
                {
                    Name = browserName,
                    Version = browserVersion
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting browser information");
                return new Browser
                {
                    Name = "unknown",
                    Version = "unknown"
                };
            }
        }
        
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
}
