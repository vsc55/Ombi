using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ombi.Store.SQL
{
    public class SqliteQueryService : ISqliteQueryService
    {
        private const string SqlQueriesCacheKey = "queries";
        private readonly object _lock = new object();
        private readonly IMemoryCache _cache;
        public SqliteQueryService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string GetQuery(string resourceId)
        {
            var cache = _cache.Get(SqlQueriesCacheKey) as Dictionary<string, string>;
            if (cache == null || !cache.ContainsKey(resourceId))
            {
                lock (_lock)
                {
                    cache = _cache.Get(SqlQueriesCacheKey) as Dictionary<string, string>;
                    if (cache == null || !cache.ContainsKey(resourceId))
                    {
                        cache = cache ?? new Dictionary<string, string>();
                        var assembly = Assembly.GetCallingAssembly();
                        var resource = assembly.GetManifestResourceStream(resourceId);
                        string query;
                        using (var reader = new StreamReader(resource))
                        {
                            query = reader.ReadToEnd();
                        }
                        cache.Add(resourceId, query);
                        _cache.Set(SqlQueriesCacheKey, cache);
                    }
                }
            }
            return cache[resourceId];
        }
    }
}
