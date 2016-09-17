using System;
using System.Web.Caching;
using System.Threading;

namespace ClassLibrary1.Caching
{
    public class TimerCacheDependency : CacheDependency
    {
        private System.Threading.Timer _timer;
        private string _timestamp;
        private string _cacheKey;

        public TimerCacheDependency(string cacheKey, string timestamp, int timerInterval)
        {
            _cacheKey = cacheKey;
            _timestamp = timestamp;

            if (_timer == null)
            {
                TimerCallback timerCallback = new TimerCallback(CheckDependencyCallback);
                _timer = new Timer(timerCallback, this, timerInterval, timerInterval);
            }
        }

        protected override void DependencyDispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
            base.DependencyDispose();
        }

        private void CheckDependencyCallback(object sender)
        {
            TimerCacheDependency timerCacheDependency = (TimerCacheDependency)sender;

            if (DependencyChanged())
                timerCacheDependency.NotifyDependencyChanged(timerCacheDependency, EventArgs.Empty);
        }

        private bool DependencyChanged()
        {
           return Cache.Instance.CacheKeyDataStoreValueChanged(_cacheKey, _timestamp);
        }
    }
}
