using System;
using System.Web;
using System.Web.Caching;
using System.Collections.Generic;

namespace ClassLibrary1.Caching
{
    public sealed class Cache
    {
        private static Cache instance = new Cache();
        private object lockObjCKDS = new object();
        private Dictionary<string, string> _cacheKeyDataStore;
        private int _defaultCacheExpirationMinutes = 60;
        private int _timerInterval;

        public static Cache Instance
        {
            get { return instance; }
        }

        private Cache()
        {
            _timerInterval = 30000;
            _cacheKeyDataStore = new Dictionary<string, string>();
        }

        public bool CacheKeyDataStoreValueChanged(string cacheKey, string timestamp)
        {
            bool result = false;

            lock (lockObjCKDS)
                result = !_cacheKeyDataStore.ContainsKey(cacheKey) || (_cacheKeyDataStore.ContainsKey(cacheKey) && !_cacheKeyDataStore[cacheKey].Equals(timestamp));

            return result;
        }

        private void UpdateHttpRuntime(CacheKey cacheKey)
        {
            int cacheExpriationMinutes = cacheKey.CacheExpirationTime;
            if (cacheExpriationMinutes == 0)
                cacheExpriationMinutes = _defaultCacheExpirationMinutes;

            DateTime cacheDependencyTimestamp = GetCacheDependencyTimestamp(cacheKey);
            cacheKey.Timestamp = cacheDependencyTimestamp;
            if (cacheDependencyTimestamp == DateTime.MinValue)
            {
                // no cachekey in DB found
                cacheKey.Timestamp = DateTime.UtcNow;
                UpdateCacheDependency(cacheKey);
                cacheDependencyTimestamp = cacheKey.Timestamp;
            }

            RemoveHttpKey(cacheKey.Name);
            HttpRuntime.Cache.Insert(cacheKey.Name, cacheKey.Name, 
                new TimerCacheDependency(cacheKey.Name, cacheDependencyTimestamp.ToString(), _timerInterval),
                System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(cacheExpriationMinutes),
                System.Web.Caching.CacheItemPriority.Default, RemoveCallback);
        }

        private void RemoveCallback(string key, object value, CacheItemRemovedReason reason)
        {
            throw new NotImplementedException();
        }

        private void RemoveHttpKey(object name)
        {
            throw new NotImplementedException();
        }

        private void UpdateCacheDependency(CacheKey cacheKey)
        {
            throw new NotImplementedException();
        }

        private DateTime GetCacheDependencyTimestamp(CacheKey cacheKey)
        {
            throw new NotImplementedException();
        }
    }
}
