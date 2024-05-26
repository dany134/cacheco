using CacheCo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheCo.Provider.Extensions
{
    public static  class CacheCoExtensions
    {
        public static async Task<T?> GetOrAddAbsoluteAsync<T>(this ICache cache, string key, Func<Task<T>> valueFactory,  DateTimeOffset? expiration = null)
        {
            var cachedItem = await cache.Get<T>(key);
            if (cachedItem == null)
            {
                var data = await valueFactory();
                cache.SetWithAbsoluteExpiration(key, data, expiration);
                return data;
            }
            return cachedItem;
        }
        public static async Task<T?> GetOrAddSlidingAsync<T>(this ICache cache, string key, Func<Task<T>> valueFactory, TimeSpan? expiration = null)
        {
            var cachedItem = await cache.Get<T>(key);
            if (cachedItem == null)
            {
                var data = await valueFactory();
                cache.SetWithSlidingExpiration(key, data, expiration);
                return data;
            }
            return cachedItem;
        }
    }
}
