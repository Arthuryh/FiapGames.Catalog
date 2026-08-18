using System.Text.Json;
using Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Services
{
    public class CatalogCacheService : ICatalogCacheService
    {
        private readonly IDistributedCache _cache;

        public CatalogCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedValue = await _cache.GetStringAsync(key);
            if (string.IsNullOrWhiteSpace(cachedValue))
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var serialized = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
