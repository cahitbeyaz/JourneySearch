using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace ObiletJourneySearch.Services.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheService> _logger;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

        public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null)
        {
            if (_cache.TryGetValue<T>(key, out var cachedValue))
            {
                _logger.LogInformation("Cache hit for key: {CacheKey}", key);
                return cachedValue;
            }

            _logger.LogInformation("Cache miss for key: {CacheKey}", key);
            var newValue = await factory();
            
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime ?? DefaultExpiration
            };
            
            _cache.Set(key, newValue, options);
            return newValue;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var result = _cache.TryGetValue<T>(key, out value);
            
            if (result)
            {
                _logger.LogInformation("Cache hit for key: {CacheKey}", key);
            }
            else
            {
                _logger.LogInformation("Cache miss for key: {CacheKey}", key);
            }
            
            return result;
        }

        public void Set<T>(string key, T value, TimeSpan? expirationTime = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime ?? DefaultExpiration
            };
            
            _cache.Set(key, value, options);
            _logger.LogInformation("Item added to cache with key: {CacheKey}", key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            _logger.LogInformation("Item removed from cache with key: {CacheKey}", key);
        }
    }
}
