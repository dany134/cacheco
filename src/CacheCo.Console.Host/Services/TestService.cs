using CacheCo.Contracts;
using CacheCo.Provider.Extensions;
using CacheCo.Provider.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheCo.Console.Host.Services
{
    public class TestService
    {

        private static List<User> _users;

        private readonly ICache cache;
        private const string cacheScopeA = "testscopeA";
        private const string cacheScopeB = "testscopeB";

        public TestService(ICache cache)
        {
            this.cache = cache;
            _users = new List<User>
            {
                new User
                {
                    Id = 1,
                    FirstName = "User 1",
                    LastName = "User 1"
                },
                new User
                {
                    Id = 2,
                    FirstName = "User 2",
                    LastName = "User 2"
                },
                new User
                {
                    Id = 3,
                    FirstName = "User 3",
                    LastName = "User 3"
                },
                new User
                {
                    Id = 4,
                    FirstName = "User 4",
                    LastName = "User 4"
                },
                new User
                {
                    Id = 5,
                    FirstName = "User 5",
                    LastName = "User 5"
                },
        };
        }
        public async Task<User> GetAbsoluteUser(int id)
        {
            var key = CacheKeyHelper.CreateCacheCoKey(cacheScopeA, nameof(User), id.ToString());
            var user = await cache.GetOrAddAbsoluteAsync<User>(key, async () =>
            {
                //this mimcis the database call
                return GetUser(id);
            }, DateTimeOffset.Now.AddMinutes(5));

            return user;

        }
        public async Task<User> GetSlidingUser(int id)
        {
            var key = CacheKeyHelper.CreateCacheCoKey(cacheScopeB, nameof(User), id.ToString());
            var user = await cache.GetOrAddSlidingAsync<User>(key, async () =>
            {
                //this mimcis the database call
                return GetUser(id);
            }, TimeSpan.FromMinutes(10));

            return user;

        }
        private User GetUser(int id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
