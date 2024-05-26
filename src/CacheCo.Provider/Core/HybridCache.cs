using CacheCo.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheCo.Provider
{
    public class HybridCache : ICache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;

        public HybridCache(IDistributedCache distributedCache, IMemoryCache memoryCache)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
        }
        public async Task<T?> Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default(T);
            }
            try
            {
                var cachedValue = _memoryCache.Get<T>(key);
                if (cachedValue != null)
                {
                    return cachedValue;
                }
                var distributedValue = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrWhiteSpace(distributedValue))
                {
                    return JsonConvert.DeserializeObject<T>(distributedValue);
                }

            }
            catch (Exception ex)
            {
                throw;
            }
           
            return default(T);

        }

        public async Task Refresh(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }
            try
            {
                await _distributedCache.RefreshAsync(key);

                object cachedValue = _memoryCache.Get(key);

                if (cachedValue != null)
                {
                    if (_memoryCache.TryGetValue(key + "-options", out MemoryCacheEntryOptions existingOptions))
                    {
                        _memoryCache.Set(key, cachedValue, existingOptions);
                    }
                    else
                    {
                        _memoryCache.Set(key, cachedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }
            try
            {
                await _distributedCache.RemoveAsync(key);
                _memoryCache.Remove(key);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task SetWithAbsoluteExpiration<T>(string key, T value, DateTimeOffset? absoluteExpiration = null)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
            {
                return;
            }
            var expiration = absoluteExpiration != null ? absoluteExpiration.Value - DateTimeOffset.Now : TimeSpan.FromMinutes(10);
            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(expiration);

                _memoryCache.Set(key, value, cacheEntryOptions);

                var absoluteExpirationOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = absoluteExpiration != null ? absoluteExpiration.Value : DateTimeOffset.Now.AddMinutes(10)
                };
                
                var serialized = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, serialized, absoluteExpirationOptions);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task SetWithSlidingExpiration<T>(string key, T value, TimeSpan? slidingExpiration = null)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
            {
                return;
            }
            var defaultExpiration = TimeSpan.FromMinutes(10);
            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(slidingExpiration != null ? slidingExpiration.Value : defaultExpiration);

                _memoryCache.Set(key, value, cacheEntryOptions);

                var slidingExpirationOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = slidingExpiration != null ? slidingExpiration.Value : defaultExpiration
                };
                var serialized = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, serialized, slidingExpirationOptions);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
