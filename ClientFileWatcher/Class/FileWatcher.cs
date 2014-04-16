using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using Client.DataModel;
using CbFlt;

namespace Client.Class
{
    class FileWatcher
    {
        private CallbackFilter _filter;
        private bool _filterAttached = false;

        //filter notifications
        private CbFltCreateFileEventN _createFileN;
        private CbFltOpenFileEventN _openFileN;
        private CbFltReadFileEventN _readFileN;
        private CbFltRenameOrMoveFileEventN _renameFileN;
        private CbFltWriteFileEventN _writeFileN;

        private CbFltCallbackFlags _callbackFlags = CbFltCallbackFlags.CreateNotify
                                                    | CbFltCallbackFlags.ReadNotify
                                                    | CbFltCallbackFlags.RenameNotify
                                                    | CbFltCallbackFlags.WriteNotify
                                                    | CbFltCallbackFlags.OpenNotify;

        private HashSet<string> _filtersApplied = new HashSet<string>();
        private ManagementEventWatcher _deviceAdded = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2"));
        private ManagementEventWatcher _deviceRemoved = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3"));

        private const string ProductName = "0A47665F-E2C5-4B05-9702-8D8B4D869166";

        /// <summary>
        /// Returns the file watcher's messages.
        /// </summary>
        private readonly StorePassThrough _passThrough = new StorePassThrough();
     
        /// <summary>
        /// Returns the filter attached value.
        /// </summary>
        public bool FilterAttached
        {
            get
            {
                return _filterAttached;
            }
            set
            {
                if (_filterAttached != value)
                {
                    _filterAttached = value;
                    _passThrough.Recording = value;
                }
            }
        }

        //this class needs to detect when new drives are added and automatically track them
        public FileWatcher()
        {
            InitNotificationDelegates();

            //Set Eldos Callback Filter key here
            //CallbackFilter.SetRegistrationKey("...");

            bool driverInstalled;
            int fileVersionHigh;
            int fileVersionLow;
            SERVICE_STATUS serviceStatus;
            CallbackFilter.GetDriverStatus(ProductName, out driverInstalled, out fileVersionHigh, out fileVersionLow, out serviceStatus);
            if (!driverInstalled)
            {
                bool reboot = false;
                CallbackFilter.InstallDriver(@"lib\cbfltfs.sys", ProductName, ref reboot);
            }
            else
            {
                //CbFlt.CallbackFilter.UninstallDriver(productName, ref reboot);
            }

            _filter = new CallbackFilter
                {
                    OnCreateFileN = _createFileN,
                    OnOpenFileN = _openFileN,
                    OnReadFileN = _readFileN,
                    OnRenameOrMoveFileN = _renameFileN,
                    OnWriteFileN = _writeFileN
                };


            _deviceAdded.EventArrived += (sender, args) => UpdateFilters();
            _deviceAdded.Start();

            _deviceRemoved.EventArrived += (sender, args) => RemoveUnusedFilters();
            _deviceRemoved.Start();
        }

        private void UpdateFilters()
        {
            try
            {
                var driveInfo = DriveInfo.GetDrives();

                foreach (var info in driveInfo.Where(i => i.DriveType == DriveType.Fixed))
                {
                    if (!_filtersApplied.Contains(info.Name))
                    {
                        _filtersApplied.Add(info.Name);
                        _filter.AddFilterCallbackRule(info.Name + @"*.*", _callbackFlags);
                    }
                }

                RemoveUnusedFilters(driveInfo);
            }
            catch
            {
                throw new InvalidOperationException("Unable to update filters.");
            }
        }

        private void RemoveFilter(string filter)
        {
            try
            {
                _filter.DeleteFilterRule(filter + @"*.*", CbFltAccessFlags.ClearAccessFlags, _callbackFlags);
                _filtersApplied.Remove(filter);
            }
            catch
            {
                throw new InvalidOperationException("Unable to remove filter.");
            }
        }

        private void RemoveUnusedFilters(DriveInfo[] driveInfo = null)
        {
            try
            {
                if(driveInfo == null)
                    driveInfo = DriveInfo.GetDrives();

                var result = _filtersApplied.Where(f => !driveInfo.Any(info => info.Name == f));
            
                foreach (var r in result.ToList())
                {
                    RemoveFilter(r);
                }
            }
            catch
            {
                throw new InvalidOperationException("Unable to remove filters.");
            }
        }

        private void RemoveAllFilters()
        {
            try
            {
                foreach (var filter in _filtersApplied)
                {
                    RemoveFilter(filter);
                }
            }
            catch
            {
                throw new InvalidOperationException("Unable to remove all filters.");
            }
        }

        /// <summary>
        /// Begin monitoring file system activity
        /// </summary>
        public void StartWatching()
        {
            try
            {
                if (!FilterAttached)
                {
                    if (!_filter.Active)
                    {
                        _filter.AttachFilter(30000);
                        UpdateFilters();
                        _filter.ReadWriteFileInPreCreatePath = false;
                        _filter.FlushFilesOnClose = false;
                        _filter.FlushFilesOnOpen = false;
                        _filter.OwnProcessFiltered = false;
                    }
                    FilterAttached = true;
                }
            }
            catch
            {
                FilterAttached = _filter.Active;
            }
        }

        /// <summary>
        /// End monitoring file system activity
        /// </summary>
        public void EndWatching()
        {
            try
            {
                if (FilterAttached)
                {
                    if (_filter.Active)
                    {
                        _filter.DetachFilter();
                        _deviceAdded.Stop();
                        _deviceRemoved.Stop();
                    }
                    FilterAttached = false;
                }
            }
            catch
            {
                FilterAttached = _filter.Active;
            }
        }

        public void StopWatching()
        {
            try
            {
                RemoveAllFilters();
                _passThrough.Recording = false;
            }
            catch
            {
                throw new InvalidOperationException("Unable to stop watching.");
            }
        }

        /// <summary>
        /// Initialize file system events
        /// </summary>
        private void InitNotificationDelegates()
        {
            _createFileN = OnCreateFileEventN;
            _openFileN = OnOpenFileEventN;
            _readFileN = OnReadFileEventN;
            _renameFileN = OnRenameOrMoveFileEventN;
            _writeFileN = OnWriteFileEventN;
        }

        private string ProcessName(CallbackFilter sender)
        {
            string processName;
            sender.GetOriginatorProcessName(out processName);
            return processName;
        }

        //FILE_EVENT_TYPE_ENUM.Unknown
        //FILE_EVENT_TYPE_ENUM.CreateFile
        //FILE_EVENT_TYPE_ENUM.ReadFile
        //FILE_EVENT_TYPE_ENUM.RenameMoveFile
        //FILE_EVENT_TYPE_ENUM.WriteFile
        //FILE_EVENT_TYPE_ENUM.DeleteFile
 
        private void OnCreateFileEventN(CallbackFilter sender, string fileName, uint desiredAccess, ushort fileAttributes, ushort shareMode, uint createOptions, ushort createDisposition)
        {
            _passThrough.AddMessage(ProcessName(sender), fileName, fileName, FILE_EVENT_TYPE_ENUM.CreateFile);
        }

        private void OnOpenFileEventN(CallbackFilter sender, string fileName, uint desiredAccess, ushort fileAttributes, ushort shareMode, uint createOptions, ushort createDisposition)
        {
            _passThrough.AddMessage(ProcessName(sender), fileName, fileName, FILE_EVENT_TYPE_ENUM.ReadFile);
        }

        private void OnReadFileEventN(CallbackFilter sender, string fileName, long position, int bytesToRead)
        {
            _passThrough.AddMessage(ProcessName(sender), fileName, fileName, FILE_EVENT_TYPE_ENUM.ReadFile);
        }

        private void OnRenameOrMoveFileEventN(CallbackFilter sender, string fileName, string newFileName)
        {
            try
            {
                if (newFileName.Contains(@"\??\"))
                {
                    int indexOf = newFileName.IndexOf(@"\??\", 0, StringComparison.Ordinal);
                    newFileName = newFileName.Remove(indexOf, 4);
                }
            }
            catch
            {
                throw new InvalidOperationException("Unable to record rename/move.");
            }

            _passThrough.AddMessage(ProcessName(sender), fileName, newFileName, FILE_EVENT_TYPE_ENUM.RenameMoveFile);
        }

        private void OnWriteFileEventN(CallbackFilter sender, string fileName, long position, int bytesToWrite)
        {
            _passThrough.AddMessage(ProcessName(sender), fileName, fileName, FILE_EVENT_TYPE_ENUM.WriteFile);
        }
    }
}
