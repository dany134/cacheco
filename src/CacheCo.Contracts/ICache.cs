using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheCo.Contracts
{
    public interface ICache
    {
        Task<T?> Get<T>(string key);
        Task Refresh(string key);
        Task Remove(string key);
        Task SetWithSlidingExpiration<T>(string key, T value, TimeSpan? slidingExpiration = null);
        Task SetWithAbsoluteExpiration<T>(string key, T value, DateTimeOffset? absoluteExpiration = null);

    }
}
