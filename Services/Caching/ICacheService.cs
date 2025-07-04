using System;

namespace ObiletJourneySearch.Services.Caching
{
    /// <summary>
    /// Interface for general caching operations
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a cached item. If not found, add it via the factory and return.
        /// </summary>
        /// <typeparam name="T">The type of the cached item</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="factory">Factory method to create the item if not found in cache</param>
        /// <param name="expirationTime">Optional expiration timespan</param>
        /// <returns>The cached or newly added item</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null);

        /// <summary>
        /// Gets a cached item if it exists
        /// </summary>
        /// <typeparam name="T">The type of the cached item</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The cached value if found</param>
        /// <returns>True if item was found in cache, false otherwise</returns>
        bool TryGetValue<T>(string key, out T value);

        /// <summary>
        /// Adds or updates an item in the cache
        /// </summary>
        /// <typeparam name="T">The type of the item to cache</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The value to cache</param>
        /// <param name="expirationTime">Optional expiration timespan</param>
        void Set<T>(string key, T value, TimeSpan? expirationTime = null);

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">The cache key to remove</param>
        void Remove(string key);
    }
}
