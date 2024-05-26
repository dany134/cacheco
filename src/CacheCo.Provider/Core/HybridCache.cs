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
                   var value = JsonConvert.DeserializeObject<T>(distributedValue);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                    _memoryCache.Set(key, value, cacheEntryOptions);
                   return value;
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

                //according to Microsoft just by accessing cached item should refresh the chached data
                object cachedValue = _memoryCache.Get(key);

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
            try
            {
                if (absoluteExpiration.HasValue)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions();
                    cacheEntryOptions.SetAbsoluteExpiration(absoluteExpiration.Value);

                    _memoryCache.Set(key, value, cacheEntryOptions);
                    var absoluteExpirationOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = absoluteExpiration
                    };

                    var serialized = JsonConvert.SerializeObject(value);
                    await _distributedCache.SetStringAsync(key, serialized, absoluteExpirationOptions);
                }
                else
                {
                    _memoryCache.Set(key, value);
                    var serialized = JsonConvert.SerializeObject(value);
                    await _distributedCache.SetStringAsync(key, serialized);
                }



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
            try
            {
                if(slidingExpiration.HasValue)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(slidingExpiration.Value);

                    _memoryCache.Set(key, value, cacheEntryOptions);

                    var slidingExpirationOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = slidingExpiration 
                    };
                    var serialized = JsonConvert.SerializeObject(value);
                    await _distributedCache.SetStringAsync(key, serialized, slidingExpirationOptions);

                }
                else
                {
                    _memoryCache.Set(key, value);
                    var serialized = JsonConvert.SerializeObject(value);
                    await _distributedCache.SetStringAsync(key, serialized);
                }


            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
