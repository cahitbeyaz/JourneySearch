using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.ApiClient
{
    public class ObiletApiClient : IObiletApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiClientToken;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<ObiletApiClient> _logger;

        public ObiletApiClient(ILogger<ObiletApiClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public ObiletApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiClientToken = configuration["ObiletApi:ApiClientToken"];
            _logger.LogInformation("ObiletApiClient initialized with token: {Token}", _apiClientToken);
            // Configure base address and default headers
            _httpClient.BaseAddress = new Uri("https://v2-api.obilet.com/api/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _apiClientToken);

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<SessionResponse> GetSessionAsync(SessionRequest request)
        {
            return await SendRequestAsync<SessionRequest, SessionResponse>(request, "client/getsession");
        }

        public async Task<BusLocationResponse> GetBusLocationsAsync(BusLocationRequest request)
        {
            return await SendRequestAsync<BusLocationRequest, BusLocationResponse>(request, "location/getbuslocations");
        }

        public async Task<JourneyResponse> GetBusJourneysAsync(JourneyRequest request)
        {
            return await SendRequestAsync<JourneyRequest, JourneyResponse>(request, "journey/getbusjourneys");
        }

        private async Task<TResponse> SendRequestAsync<TRequest, TResponse>(TRequest request, string endpoint)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Check for HTTP errors but don't throw

                var result = JsonSerializer.Deserialize<TResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result;
            }
            catch
            {
                throw; // Rethrow to be handled by caller
            }
        }
    }
}
