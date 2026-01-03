using System.Collections.Concurrent;
using System.Linq;
using mg3.foundry.Features.FoundryCore.Models;
using Microsoft.Extensions.Caching.Memory;

namespace mg3.foundry.Features.FoundryCore.Services
{
    public class ModelCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, bool> _cacheKeys;
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(30);

        public ModelCacheService()
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1024, // Max 1024 items
                CompactionPercentage = 0.25,
                ExpirationScanFrequency = TimeSpan.FromMinutes(5)
            });
            _cacheKeys = new ConcurrentDictionary<string, bool>();
        }

        public void CacheModel(FoundryModelInfo model)
        {
            if (model == null) return;

            var cacheKey = GetCacheKey(model.Name);
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _defaultCacheDuration,
                Size = 1,
                PostEvictionCallbacks = {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            _cacheKeys.TryRemove(key.ToString(), out _);
                        }
                    }
                }
            };

            _cache.Set(cacheKey, model, cacheOptions);
            _cacheKeys.TryAdd(cacheKey, true);
        }

        public FoundryModelInfo? GetCachedModel(string modelName)
        {
            var cacheKey = GetCacheKey(modelName);
            return _cache.TryGetValue(cacheKey, out FoundryModelInfo? model) ? model : null;
        }

        public void RemoveModel(string modelName)
        {
            var cacheKey = GetCacheKey(modelName);
            _cache.Remove(cacheKey);
            _cacheKeys.TryRemove(cacheKey, out _);
        }

        public void ClearCache()
        {
            // Get all cache keys from our tracking dictionary
            var keys = _cacheKeys.Keys.ToList();
            
            foreach (var key in keys)
            {
                _cache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }
        }

        public int GetCacheCount()
        {
            return _cacheKeys.Count;
        }

        private string GetCacheKey(string modelName)
        {
            return $"model_cache_{modelName.ToLower().Replace(" ", "_")}";
        }
    }
}