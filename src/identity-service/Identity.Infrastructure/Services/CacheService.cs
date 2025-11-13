using Identity.Domain.Abtractions;
using Microsoft.Extensions.Caching.Memory;

namespace Identity.Infrastructure.Services
{
    public class CacheService: ICacheService
    {
        private readonly IMemoryCache _cache;
        public CacheService(IMemoryCache cache) {
            _cache = cache;
        }
        public void Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiry != null)
                options.AbsoluteExpirationRelativeToNow = expiry;
            _cache.Set(key, value, options);
        }

        public T? Get<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
