using System;
using System.Threading;

namespace Client.Class
{
    class ServerTimes
    {
        public TimeSpan FilterRefreshInterval
        {
            get { return new TimeSpan(Interlocked.Read(ref _filterRefreshInterval)); }
            set { Interlocked.Exchange(ref _filterRefreshInterval, value.Ticks); }
        }

        public TimeSpan AccumulationInterval
        {
            get { return new TimeSpan(Interlocked.Read(ref _accumulationInterval)); }
            set { Interlocked.Exchange(ref _accumulationInterval, value.Ticks); } }

        public TimeSpan AccumulationGranularity
        {
            get { return new TimeSpan(Interlocked.Read(ref _accumulationGranularity)); }
            set { Interlocked.Exchange(ref _accumulationGranularity, value.Ticks); }
        }

        public DateTime LastSyncAction
        {
            get { return new DateTime(Interlocked.Read(ref _lastSyncAction)); }
            set { Interlocked.Exchange(ref _lastSyncAction, value.Ticks); }
        }

        public DateTime LastFilterRefresh
        {
            get { return new DateTime(Interlocked.Read(ref _lastFilterRefresh)); }
            set { Interlocked.Exchange(ref _lastFilterRefresh, value.Ticks); }
        }

        public DateTime LastAccumulation
        {
            get { return new DateTime(Interlocked.Read(ref _lastAccumulation)); }
            set { Interlocked.Exchange(ref _lastAccumulation, value.Ticks); }
        }

        private readonly TimeSpan _defaultTime = TimeSpan.Zero;
        private readonly DateTime _defaultDate = DateTime.MinValue;
        
        private long _filterRefreshInterval;
        private long _accumulationInterval;
        private long _accumulationGranularity;
        private long _lastSyncAction;
        private long _lastFilterRefresh;
        private long _lastAccumulation;

        public ServerTimes()
        {
            FilterRefreshInterval = _defaultTime;
            AccumulationInterval = _defaultTime;
            AccumulationGranularity = _defaultTime;

            LastAccumulation = _defaultDate;
            LastFilterRefresh = _defaultDate;
            LastSyncAction = _defaultDate;
        }
    }
}
