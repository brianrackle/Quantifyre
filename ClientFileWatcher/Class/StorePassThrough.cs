using System;
using Client.DataModel;

namespace Client.Class
{
    class StorePassThrough
    {
        private ServerTimer _serverTimer;
        private ServerTimes _serverTimes;
        private DataStoreTransactor _dataStoreTransactor;
        private bool _recording;

        public bool Recording
        {
            get
            {
                return _recording;
            }
            set
            {
                if (_recording != value)
                {
                    _recording = value;
                    if (!_recording)
                    {
                        Stop();
                    }
                    else
                    {
                        Start();
                    }
                }
            }
        }

        public StorePassThrough()
        {
            try
            {
                _serverTimes = new ServerTimes();
                _dataStoreTransactor = new DataStoreTransactor(_serverTimes);

                _serverTimer = new ServerTimer(_dataStoreTransactor.FilterRefresh, _dataStoreTransactor.Accumulation,
                                               _serverTimes);
            }
            catch
            {
                throw new InvalidOperationException("Unable to initialize pass through.");
            }
        }

        public void Start()
        {
            try
            {
                _serverTimer.Start();
                _dataStoreTransactor.Sync();
            }
            catch
            {
                throw new InvalidOperationException("Unable to start timer.");
            }
        }

        public void Stop()
        {
            try
            {
                _serverTimer.Stop();
                _dataStoreTransactor.Flush();
            }
            catch
            {
                throw new InvalidOperationException("Unable to stop timer.");
            }
        }

        public void AddMessage(string processName, string sourceFileName, string targetFileName, FILE_EVENT_TYPE_ENUM action)
        {
            if (!string.IsNullOrEmpty(processName) && !string.IsNullOrEmpty(sourceFileName) && !string.IsNullOrEmpty(targetFileName))
            {
                try
                {
                    _dataStoreTransactor.AddMessage(new Row(processName, sourceFileName, targetFileName, action));
                }
                catch
                {
                    throw new InvalidOperationException("Unable to add message.");
                }
            }
        }
    }
}
