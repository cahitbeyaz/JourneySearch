using ObiletJourneySearch.ApiClient;
using ObiletJourneySearch.Models.DTOs;
using ObiletJourneySearch.Utilities;
using System.Text.Json;

namespace ObiletJourneySearch.Middleware
{
    // Middleware responsible for managing Obilet API sessions across the application.
    public class ObiletSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IObiletApiClient _apiClient;
        private readonly ILogger<ObiletSessionMiddleware> _logger;
        public const string ObiletSessionKey = "ObiletDeviceSession";
        private const string ObDeviceIdCookiKey = "ob_deviceId";
        private const string ObSessionKeyCookiKey = "ob_sessionKey";
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
            try
            {
                var session = GetSession(context);
                if (session == null)
                {
                    session = await CreateNewSessionAsync(context);
                    TimeSpan sessionExpireTimeSpan = TimeSpan.FromDays(1);
                    var cookieOpts = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.Add(sessionExpireTimeSpan),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };
                    context.Response.Cookies.Append(ObDeviceIdCookiKey, session.DeviceId, cookieOpts);
                    context.Response.Cookies.Append(ObSessionKeyCookiKey, session.SessionId, cookieOpts);
                }
                context.Items[ObiletSessionKey] = session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or retrieve Obilet session");
            }

            // Continue with the request pipeline
            await _next(context);
        }
        private DeviceSession? GetSession(HttpContext context)
        {

            string obDeviceId = context.Request.Cookies[ObDeviceIdCookiKey];
            string obSessionKey = context.Request.Cookies[ObSessionKeyCookiKey];
            if (!string.IsNullOrEmpty(obDeviceId) && !string.IsNullOrEmpty(obSessionKey))
            {
                return new DeviceSession
                {
                    DeviceId = obDeviceId,
                    SessionId = obSessionKey
                };
            }

            return null;
        }

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
