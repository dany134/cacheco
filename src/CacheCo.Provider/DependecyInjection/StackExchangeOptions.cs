using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheCo.Provider.DependecyInjection
{
    public class CacheCoOptions
    {
        public string ConnectionString { get; set; }
        public int Database { get; set; }
    }
}
