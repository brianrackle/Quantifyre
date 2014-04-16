using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;

namespace Client.Class
{
    public class FileDataStore : IDataStore
    {
        /// <summary>
        /// Handles StoreObject and SettingsObject transaction with AppData folder files
        /// </summary>
        /// <param name="rootPath">AppData path</param>
        public FileDataStore(string rootPath)
        {
            _rootPath = rootPath;
            foreach (var domains in Directory.EnumerateDirectories(CreateDirectory(FullPath(DataDir))))
            {
                foreach (var users in Directory.EnumerateDirectories(domains))
                {
                    foreach (var file in Directory.EnumerateFiles(users, "*.xml").OrderByDescending(d => new FileInfo(d).CreationTime))
                    {
                        _fileBacklog.Enqueue(file);
                    }
                }
            }
        }

        /// <summary>
        /// Perfromance syncronization between store and the unsynchronized memory and files.
        /// </summary>
        /// <param name="store">The IDataStore to syncronize the StoreObjects with.</param>
        /// <returns>If further syncronization is required.</returns>
        public bool ForceDbSyncAction(IDataStore store)
        {
            return SyncMemory(store) || SyncFile(store);
        }

        public UserSettingsObject ForceDbFilterRefresh(string user, string domain)
        {
            lock (_filterLock)
            {
                try
                {
                    var serializer = new DataContractSerializer(typeof (UserSettingsObject));
                    var obj = DecryptDataFromStream(serializer, DataProtectionScope.CurrentUser, SettingStore(user,domain));
                    return (UserSettingsObject)obj;
                }
                catch
                {
                    return new UserSettingsObject(new List<string>(), new List<string>(),
                                                  new GroupSettingsObject(_defaultRefreshInterval, _defaultAccumulationInterval, _defaultGranularity, false, false),
                                                  user, domain);
                }
            }
        }

        public void Flush()
        {
            foreach (var s in _memoryBacklog)
            {
                WriteToFile(s);
            }
        }

        public void ForceDbAccumulation(StoreObject storeObject)
        {
            _memoryBacklog.Enqueue(storeObject);
        }
        
        public void ForceDbAccumulation(UserSettingsObject settingsObject)
        {
            lock (_filterLock)
            {
                string settingsFile = NewSettingStore(settingsObject.User,settingsObject.Domain);
                var serializer = new DataContractSerializer(typeof (UserSettingsObject));
                EncryptDataToFile(serializer, settingsObject, DataProtectionScope.CurrentUser, settingsFile);
            }
        }

        #region Private

        private const string DataDir = "data";
        private const string SettingsDir = "settings";
        private readonly string _rootPath;

        private readonly TimeSpan _defaultRefreshInterval = new TimeSpan(TimeSpan.TicksPerMinute);
        private readonly TimeSpan _defaultAccumulationInterval = new TimeSpan(0, 0, 0, 0, -1);
        private readonly TimeSpan _defaultGranularity = new TimeSpan(0, 0, 0, 0, -1);

        private readonly ConcurrentQueue<StoreObject> _memoryBacklog = new ConcurrentQueue<StoreObject>();
        private readonly ConcurrentQueue<string> _fileBacklog = new ConcurrentQueue<string>();

        private readonly Object _filterLock = new Object();

        private string LocalStorage(string user, string domain)
        {
            return Path.Combine(domain, user);
        }

        private string NewDataStore(string user, string domain)
        {
            string dataStore = Path.Combine(
                CreateDirectory(FullPath(Path.Combine(DataDir, LocalStorage(user, domain)))),
                Guid.NewGuid().ToString() + @".xml");
            if (!File.Exists(dataStore))
            {
                var file = File.Create(dataStore);
                file.Close();
            }
            return dataStore;
        }

        private string NewSettingStore(string user, string domain)
        {
            string settingStore =
                Path.Combine(CreateDirectory(FullPath(Path.Combine(SettingsDir, LocalStorage(user, domain)))),
                             "settings" + @".xml");
            if (File.Exists(settingStore))
            {
                File.Delete(settingStore);
            }

            var file = File.Create(settingStore);
            file.Close();

            return settingStore;
        }

        private string SettingStore(string user, string domain)
        {
            string settingsStore =
                Path.Combine(CreateDirectory(FullPath(Path.Combine(SettingsDir, LocalStorage(user, domain)))),
                             "settings" + ".xml");
            if (!File.Exists(settingsStore))
            {
                NewSettingStore(user, domain);
            }
            return settingsStore;
        }

        private string FullPath(string localPath)
        {
            string fullPath = Path.Combine(_rootPath, localPath);
            return fullPath;
        }

        private string CreateDirectory(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }


        /// <summary>
        /// Retrieves the next StoreObject from the memoryBackLog and writes it to the store if available, else to a file.
        /// </summary>
        /// <param name="store">The data store this object should write the backlogged store objects to</param>
        /// <returns>true if backlog isn't empty</returns>
        private bool SyncMemory(IDataStore store)
        {
            StoreObject storeObject = null;
            
            try
            {
                if (_memoryBacklog.Count != 0
                    && _memoryBacklog.TryDequeue(out storeObject)
                    && storeObject != null)
                    store.ForceDbAccumulation(storeObject);
            }
            catch (EntityException)
            {
                WriteToFile(storeObject);
            }
            catch
            {
            }

            return _memoryBacklog.Count != 0;
        }

        private void WriteToFile(StoreObject storeObject)
        {
            if (storeObject != null)
            {
                string storeFile = NewDataStore(storeObject.User, storeObject.Domain);
                var serializer = new DataContractSerializer(typeof (StoreObject));
                EncryptDataToFile(serializer, storeObject, DataProtectionScope.CurrentUser, storeFile);
                _fileBacklog.Enqueue(storeFile);
            }
        }

        private bool SyncFile(IDataStore store)
        {
            bool success = false;
            string frontFile = String.Empty;
            try
            {
                if (_fileBacklog.Count != 0
                    && _fileBacklog.TryDequeue(out frontFile)
                    && !String.IsNullOrEmpty(frontFile))
                {
                    var serializer = new DataContractSerializer(typeof (StoreObject));
                    var obj = DecryptDataFromStream(serializer, DataProtectionScope.CurrentUser, frontFile);
                    store.ForceDbAccumulation(obj as StoreObject);
                    File.Delete(frontFile);
                    success = true;
                }
            }
            catch (EntityException)
            {
                if (!String.IsNullOrEmpty(frontFile))
                    _fileBacklog.Enqueue(frontFile);
            }
            catch
            {
                //need to resolve action dictionary problem of returning null ref to dictionary and remove the nullref catch
                if (!String.IsNullOrEmpty(frontFile))
                {
                    File.Delete(frontFile);
                }
                success = true;
            }

            return success && _fileBacklog.Count != 0;
        }

        private void EncryptDataToFile(DataContractSerializer serializer, object data, DataProtectionScope scope, string file)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (XmlWriter memWriter = XmlWriter.Create(memStream))
                {
                    serializer.WriteObject(memWriter, data);
                    memWriter.Flush();
                    memWriter.Close();
                }

                byte[] b = memStream.ToArray();
                memStream.SetLength(0);
                memStream.Close();

                byte[] encrptedData = ProtectedData.Protect(b, null, scope);
                File.WriteAllBytes(file, encrptedData);
            }
        }

        private object DecryptDataFromStream(DataContractSerializer serializer, DataProtectionScope scope, string file)
        {
            using (FileStream fileStream = new FileStream(file, FileMode.Open))
            {
                byte[] b = ReadFully(fileStream);
                byte[] outBuffer = ProtectedData.Unprotect(b, null, scope);

                using (MemoryStream memStream = new MemoryStream(outBuffer))
                {
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(memStream, new XmlDictionaryReaderQuotas());
                    object obj = serializer.ReadObject(reader);

                    memStream.Close();
                    fileStream.Close();

                    return obj;
                }
            }
        }

        private byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        #endregion
    }
}
