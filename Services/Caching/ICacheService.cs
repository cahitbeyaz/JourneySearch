using System;

namespace ObiletJourneySearch.Services.Caching
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null);
        bool TryGetValue<T>(string key, out T value);
        void Set<T>(string key, T value, TimeSpan? expirationTime = null);
        void Remove(string key);
    }
}
