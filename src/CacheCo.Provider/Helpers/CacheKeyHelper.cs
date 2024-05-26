using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheCo.Provider.Helpers
{
    public static class CacheKeyHelper
    {
        public static string CreateCacheCoKey(string scope, string type, string id)
        {
            return $"{scope}-{type}-{id}";
        }
    }
}
