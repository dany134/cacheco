using CacheCo.Contracts;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheCo.Provider.DependecyInjection
{
    public static  class ServiceColletionExtensions
    {
        public static IServiceCollection AddCacheCo(this IServiceCollection services, CacheCoOptions config)
        {
            services.AddMemoryCache();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"{config.ConnectionString},defaultDatabase={config.Database}"; ;
             
            });
            services.AddSingleton<ICache, HybridCache>();
            return services;
        }
    }
}
