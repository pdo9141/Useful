using System;

namespace ClassLibrary1.Caching
{
    public class CacheKey
    {
        public int CacheExpirationTime { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
