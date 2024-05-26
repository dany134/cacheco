using CacheCo.Console.Host.Services;
using CacheCo.Contracts;
using CacheCo.Provider.DependecyInjection;
using CacheCo.Provider.Extensions;
using CacheCo.Provider.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CacheCo.Console.Host
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load appsettings.json
             .Build();

            string redisConnectionString = configuration.GetSection("RedisConnection:ConnectionString").Value;
            string database = configuration.GetSection("RedisConnection:Database").Value;

            var configOptions = new CacheCoOptions
            {
                ConnectionString = redisConnectionString,
                Database = int.Parse(database)
            };
            IServiceCollection services = new ServiceCollection()              
                .AddCacheCo(configOptions)
                .AddSingleton<TestService>();           

            var provider = services.BuildServiceProvider();

            var test = provider.GetService<TestService>();
            var cache = provider.GetService<ICache>();
            var inMem = provider.GetService<IMemoryCache>();

            for (int i = 1; i < 6; i++)
            {
                var user = test.GetAbsoluteUser(i).GetAwaiter().GetResult();
                System.Console.WriteLine(user.FirstName);
            }           

            inMem.Remove(CacheKeyHelper.CreateCacheCoKey("testscopeA", "User", "2"));

            var user2 = cache.Get<User>(CacheKeyHelper.CreateCacheCoKey("testscopeA", "User", "2")).GetAwaiter().GetResult();


            System.Console.ReadKey();




        }

    }
}
