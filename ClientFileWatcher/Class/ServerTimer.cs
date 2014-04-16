using System;
using System.Threading;

namespace Client.Class
{
    /// <summary>
    /// Maintains timers for calling database stores and updates.
    /// </summary>
    class ServerTimer
    {
        public delegate void DbFilter();
        public delegate void DbAccumulation(bool swap, bool force, bool sync);

        private readonly TimeSpan _zeroTime = new TimeSpan(0);

        private readonly Object _filterLock = new Object();
        private readonly Object _accumulationLock = new Object();

        private Timer _filterRefreshTimer;
        private Timer _accumulationTimer;

        private DbFilter _dbFilterRefresh;
        private DbAccumulation _dbAccumulation;
       
        private ServerTimes _serverTimes;

        //for conditional expression look at
        //Flee http://flee.codeplex.com/
        //CodeDom  http://msdn.microsoft.com/en-us/library/y2k85ax6.aspx
        public ServerTimer(DbFilter filterRefresh, DbAccumulation accumulation, ServerTimes serverTimes)
        {
            _dbFilterRefresh = filterRefresh;
            _dbAccumulation = accumulation;
            _serverTimes = serverTimes;
        }

        public void Start()
        {
            lock (_accumulationLock) lock (_filterLock)
            {
                if (_filterRefreshTimer == null)
                    _filterRefreshTimer = new Timer(FilterRefresh, null, _zeroTime,
                                                    _serverTimes.FilterRefreshInterval);
                if (_accumulationTimer == null)
                    _accumulationTimer = new Timer(Accumulation, null, _zeroTime,
                                                    _serverTimes.AccumulationInterval);
            }
        }

        public void Stop()
        {
            lock (_accumulationLock) lock (_filterLock)
            {
                if (_filterRefreshTimer != null) _filterRefreshTimer.Dispose();
                if (_accumulationTimer != null) _accumulationTimer.Dispose();
            }
        }

        private void FilterRefresh(Object state)
        {
            if (Monitor.TryEnter(_filterLock))
            {
                try
                {
                    if (_filterRefreshTimer != null)
                    {
                        var elapsed = (DateTime.Now - _serverTimes.LastFilterRefresh).TotalSeconds;
                        if (elapsed >= _serverTimes.FilterRefreshInterval.TotalSeconds)
                        {
                            _dbFilterRefresh();
                        }
                        _filterRefreshTimer.Change(
                            _serverTimes.FilterRefreshInterval,
                            _serverTimes.FilterRefreshInterval);
                        _accumulationTimer.Change(
                            _serverTimes.AccumulationInterval,
                            _serverTimes.AccumulationInterval);
                    }
                }
                catch
                {
                    //throw new InvalidOperationException("Unable to refresh filter.");
                }
                finally
                {
                    Monitor.Exit(_filterLock);
                }
            }
        }

        private void Accumulation(Object state)
        {
            if (Monitor.TryEnter(_accumulationLock))
            {
                try
                {
                    if (_accumulationTimer != null)
                    {
                        var elapsed = (DateTime.Now - _serverTimes.LastAccumulation).TotalSeconds;
                        if (elapsed >= _serverTimes.AccumulationInterval.TotalSeconds)
                        {
                            _dbAccumulation(true,true,true);
                        }
                    }
                }
                catch
                {
                    //throw new InvalidOperationException("Unable to accumulate messages.");
                }
                finally
                {
                    Monitor.Exit(_accumulationLock);                    
                }
            }
        }
    }
}
