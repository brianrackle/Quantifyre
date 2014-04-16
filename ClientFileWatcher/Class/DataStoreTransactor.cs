using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;

namespace Client.Class
{
    class DataStoreTransactor
    {
        private ActionDictionary _actionDictionary = new ActionDictionary();
        private readonly ServerTimes _serverTimes;
        private UserSettingsObject _knownUserSettings;

        private ConcurrentBag<string> _candidateProcesses = new ConcurrentBag<string>();
        private ConcurrentBag<string> _candidateExtensions = new ConcurrentBag<string>();
        private ConcurrentBag<string> _knownProcesses = new ConcurrentBag<string>();
        private ConcurrentBag<string> _knownExtensions = new ConcurrentBag<string>();

        private readonly List<IDataStore> _connectionList = new List<IDataStore>();

        private readonly Object _syncLock = new Object();
        private readonly Object _swapLock = new Object();

        private const ulong MaxMemory = 250000000;
        private const int MemoryCheckInterval = 10000;

        private readonly ulong _maxMemoryUsage;
        private readonly ulong _physicalMemory = new ComputerInfo().AvailablePhysicalMemory;

        private readonly string _installDir = string.Empty;
        private readonly string _appDataDir = string.Empty;

        private string _currentUser = string.Empty;
        private string _currentDomain = string.Empty;

        private readonly List<string> _ignoreDirs = new List<string>();

        public DataStoreTransactor(ServerTimes serverTimes)
        {
            _maxMemoryUsage = (_physicalMemory / 4) > MaxMemory ? MaxMemory : (_physicalMemory / 4);

            SetUserAndDomain(out _currentUser, out _currentDomain, UserAndDomain);

            var productDirKey = Registry.LocalMachine.OpenSubKey(@"Software\Quantifyre\Informant",false);
            if (productDirKey != null)
            {
                _installDir = productDirKey.GetValue("InstallDir") as String;
                _appDataDir = productDirKey.GetValue("AppDataDir") as String;

                if (_installDir != null) _ignoreDirs.Add(_installDir.ToUpper());
                if (_appDataDir != null) _ignoreDirs.Add(_appDataDir.ToUpper());

            }
            _serverTimes = serverTimes;

            CreateDatabases(_connectionList, _appDataDir);

            FilterRefresh();
            Sync();
        }

        /// <summary>
        /// Adds the given message to IDataStores given the right conditions are met
        /// </summary>
        /// <param name="message">The Row to store</param>
        public void AddMessage(Row message)
        {
            if (!_ignoreDirs.Any(sf => message.SourceFileName.StartsWith(sf)))
            {
                var userSettings = _knownUserSettings;
                if (ProcessValid(userSettings, message.ProcessName) &&
                    ExtensionValid(userSettings, Row.GetExtension(message.SourceFileName)))
                {
                    string[] userAndDomain = UserAndDomain;
                    string user, domain;
                    SetUserAndDomain(out user, out domain, userAndDomain);
                    if (!String.IsNullOrEmpty(user) && !String.IsNullOrEmpty(domain))
                    {
                        bool usersMatched = true;
                        if (_currentUser != user || _currentDomain != domain)
                        {
                            lock (_swapLock)
                            {
                                if (_currentUser != user || _currentDomain != domain)
                                {
                                    Accumulation(false, true, true);
                                    SetUserAndDomain(out _currentUser, out _currentDomain, userAndDomain);
                                    _actionDictionary.Add(message.SourceFileName, message.TargetFileName,
                                                          message.ProcessName,
                                                          message.Action, message.Time);
                                    FilterRefresh();
                                    usersMatched = false;
                                }
                            }
                        }

                        if (usersMatched)
                        {
                            _actionDictionary.Add(message.SourceFileName, message.TargetFileName, message.ProcessName,
                                                  message.Action, message.Time);
                            Accumulation(true, false, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the _knownUserSettings from the highest priority IDataStore to the lowest priority IDatastore
        /// </summary>
        public void FilterRefresh()
        {
            bool success = false;
            for (int i = _connectionList.Count-1; i >= 0 && !success; --i)
            {
                UserSettingsObject newSettings = _connectionList[i].ForceDbFilterRefresh(_currentUser, _currentDomain);
                if (newSettings != null)
                {
                    _serverTimes.AccumulationInterval = newSettings.GroupSettings.AccumulationInterval;
                    _serverTimes.FilterRefreshInterval = newSettings.GroupSettings.FilterInterval;
                    _serverTimes.AccumulationGranularity = newSettings.GroupSettings.Granularity;

                    _actionDictionary.Interval = _serverTimes.AccumulationGranularity;

                    if(i != 0)
                        _connectionList.First().ForceDbAccumulation(newSettings);

                    _knownUserSettings = newSettings;
                    success = true;
                }
            }
        }

        //flush should kill any database syncs and return StoreObjects????
        //look at sync... go through all IDataStores and flush them
        /// <summary>
        /// Flush any unstored data from the IDataStores
        /// </summary>
        public void Flush()
        {
            Accumulation(true, true, false);
            foreach (var s in _connectionList)
            {
                s.Flush();
            }
        }

        /// <summary>
        /// Performs an store of the current settings if the right parameters are met.
        /// </summary>
        /// <param name="swap">Swap the existing dictionary with a new one</param>
        /// <param name="force">Force a store</param>
        /// <param name="sync">Perform a sync after store</param>
        public void Accumulation(bool swap, bool force, bool sync)
        {
            var storeObject = SwapDictionaries(swap, force);

            if(storeObject != null)
            {
                _connectionList.First().ForceDbAccumulation(storeObject);
                if(sync)
                    Sync();
            }
        }

        /// <summary>
        /// Syncs data with the lowest priority IDataStore to the highest priority IDataStore
        /// </summary>
        public void Sync()
        {
            if (Monitor.TryEnter(_syncLock))
            {
                try
                {
                    for (int i = 1; i < _connectionList.Count; i++)
                    {
                        while (_connectionList[i - 1].ForceDbSyncAction(_connectionList[i]))
                        {
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_syncLock);
                }
            }
        }

        /// <summary>
        /// Checks if the process should be monitored
        /// </summary>
        /// <param name="userSettings">The UserSettingsObject that the process is checked with</param>
        /// <param name="process">The process name</param>
        /// <returns>If the process should be monitored</returns>
        private bool ProcessValid(UserSettingsObject userSettings, string process)
        {
            bool valid = true;
            if (!_knownUserSettings.GroupProcesses.Contains(process))
            {
                if(userSettings.GroupSettings.CollectProcesses && !_candidateProcesses.Contains(process))
                {
                    _candidateProcesses.Add(process);
                }
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Checks if the extension should be monitored
        /// </summary>
        /// <param name="userSettings">The UserSettingsObject that the process is checked with</param>
        /// <param name="extension">The extension</param>
        /// <returns>If the extension should be monitored</returns>
        private bool ExtensionValid(UserSettingsObject userSettings, string extension)
        {
            bool valid = true;
            if (!_knownUserSettings.GroupExtensions.Contains(extension))
            {
                if (userSettings.GroupSettings.CollectExtensions && !_candidateExtensions.Contains(extension))
                {
                    _candidateExtensions.Add(Row.GetExtension(extension));
                }
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Turns the _actionDictionary into a StoreObject given the right conditions
        /// </summary>
        /// <param name="rollOverDictionary">True if the current dictionary should be included</param>
        /// <param name="force">If the swap should be forced</param>
        /// <returns>The StoreObject made from the _actionDictionary, or null if conditions were not met</returns>
        private StoreObject SwapDictionaries(bool rollOverDictionary, bool force)
        {
            bool lockedAcquired = false;
            try
            {
                if ((force || ((_actionDictionary.Count%MemoryCheckInterval) == 0 && (ulong) GC.GetTotalMemory(true) > _maxMemoryUsage))
                    && (Monitor.TryEnter(_swapLock) && (force || _actionDictionary.Count > MemoryCheckInterval)))
                {
                    lockedAcquired = true;

                    var currentDictionary = _actionDictionary;
                    if (rollOverDictionary && _actionDictionary.Dictionaries.Count > 1)
                    {
                        var newDictionary = new ActionDictionary { Interval = _actionDictionary.Interval };
                        ConcurrentDictionary<EventId, EventAction> firstDictionary;
                        _actionDictionary.Dictionaries.TryPop(out firstDictionary);

                        if (firstDictionary != null)
                            newDictionary.Dictionaries.Push(firstDictionary);

                        _actionDictionary = newDictionary;
                    }
                    else
                    {
                        _actionDictionary = new ActionDictionary { Interval = _actionDictionary.Interval };
                    }

                    var currentProcesses = _candidateProcesses.Except(_knownProcesses).ToList();
                    var currentExtensions = _candidateExtensions.Except(_knownExtensions).ToList();

                    _candidateProcesses = new ConcurrentBag<string>();
                    _candidateExtensions = new ConcurrentBag<string>();

                    _knownProcesses = new ConcurrentBag<string>(_knownProcesses.Union(currentProcesses));
                    _knownExtensions = new ConcurrentBag<string>(_knownExtensions.Union(currentExtensions));


                    return new StoreObject(currentDictionary, currentProcesses, currentExtensions,
                                           _currentUser, Environment.MachineName.ToUpper(), _currentDomain);
                }
            }
            finally
            {
                if (lockedAcquired)
                {
                    Monitor.Exit(_swapLock);
                }
            }

            return null;
        }

        /// <summary>
        /// The currently logged on user and domain in format [0] = DOMAIN, [1] = USER
        /// </summary>
        private string[] UserAndDomain
        {
            get
            {
                var searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
                var collection = searcher.Get();
                var name = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
                return name != null ? name.ToUpper().Split('\\') : new string[2]{String.Empty, String.Empty};
            }
        }

        /// <summary>
        /// Sets user and domain given userAndDomain
        /// </summary>
        /// <param name="user">The user to set</param>
        /// <param name="domain">The domain to set</param>
        /// <param name="userAndDomain">The userAndDomain to set with</param>
        private void SetUserAndDomain(out string user,out string domain, string[] userAndDomain)
        {
            domain = userAndDomain[0];
            user = userAndDomain[1];       
        }

        /// <summary>
        /// Creates databases and adds them to databases.
        /// </summary>
        /// <param name="databases">The ICollection to add the databases to</param>
        /// <param name="storageDir">The directory for file storage</param>
        private void CreateDatabases(ICollection<IDataStore> databases, string storageDir)
        {
            databases.Add(new FileDataStore(storageDir));
            databases.Add(new ServerDataStore());
        }
 
    }
}
