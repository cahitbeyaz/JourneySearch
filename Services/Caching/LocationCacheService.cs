using ObiletJourneySearch.ApiClient;
using ObiletJourneySearch.Models.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ObiletJourneySearch.Services.Caching
{
    /// <summary>
    /// Service to handle caching for bus location data
    /// </summary>
    public class LocationCacheService : ILocationCacheService
    {
        private readonly ICacheService _cacheService;
        private readonly IObiletApiClient _apiClient;
        private readonly ILogger<LocationCacheService> _logger;
        private const string LocationCacheKey = "AllBusLocations";
        
        // Default cache duration for locations (1 hour)
        private static readonly TimeSpan DefaultLocationCacheDuration = TimeSpan.FromHours(1);

        public LocationCacheService(
            ICacheService cacheService,
            IObiletApiClient apiClient,
            ILogger<LocationCacheService> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<BusLocationResponse> GetAllLocationsAsync(BusLocationRequest request)
        {
            return await _cacheService.GetOrCreateAsync(
                LocationCacheKey,
                async () => {
                    _logger.LogInformation("Fetching all bus locations from API");
                    return await _apiClient.GetBusLocationsAsync(request);
                },
                DefaultLocationCacheDuration);
        }

        /// <inheritdoc />
        public void InvalidateLocationsCache()
        {
            _logger.LogInformation("Invalidating bus locations cache");
            _cacheService.Remove(LocationCacheKey);
        }
    }
}
