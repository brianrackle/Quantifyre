using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Client.DataModel;

namespace Client.Class
{
    public class ServerDataStore : IDataStore
    {

        private readonly Type _databaseType;

        private readonly Object _storeLock = new Object();

        public ServerDataStore()
        {
            _databaseType = typeof (fgServerDb);
        }

        public DbContext DataContext
        {
            get
            {
                var obj = Activator.CreateInstance(_databaseType);
                if (obj is DbContext)
                {
                    var context = obj as DbContext;
                    context.Configuration.ValidateOnSaveEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                    return context;
                }
                throw new ArgumentException("Incorrect database type.");
            }
        }

        public SqlConnection Connection
        {
            get
            {
                if (DataContext != null && DataContext.Database != null && DataContext.Database.Connection != null)
                {
                    return new SqlConnection(DataContext.Database.Connection.ConnectionString);
                }
                return new SqlConnection();
            }
        }

        #region Database Manipulation

        public bool ForceDbSyncAction(IDataStore store)
        {
            throw new NotImplementedException();
        }

        public UserSettingsObject ForceDbFilterRefresh(string user, string domain)
        {
            lock (_storeLock)
            {
                return DbFilterRefresh(user,domain);
            }
        }

        public void Flush()
        {
        }

        public void ForceDbAccumulation(StoreObject storeObject)
        {
            lock (_storeLock)
            {
                DbStore(storeObject);
            }
        }

        public void ForceDbAccumulation(UserSettingsObject settingsObject)
        {
            throw new NotImplementedException();
        }

        public UserSettingsObject DbFilterRefresh(string user, string domain)
        {
            try
            {
                return new UserSettingsObject(new List<string>(DbRetrieveGroupProcesses(user, domain)),
                                              new List<string>(DbRetrieveGroupExtensions(user, domain)),
                                              DbRetrieveGroupSettings(user, domain), user, domain);
            }
            catch (EntityException)
            {
                return null;
            }
        }

        public void DbStore(StoreObject storeObject)
        {
            DbInsertProcesses(storeObject.Processes);
            DbInsertExtensions(storeObject.Extensions);

            var files = ((from KeyValuePair<EventId, EventAction> kvp in storeObject.Dictionary
                            select kvp.Key.SourceFileName).Union
                (from KeyValuePair<EventId, EventAction> kvp in storeObject.Dictionary
                    select kvp.Key.TargetFileName)).Distinct().AsEnumerable();

            DbInsertFiles(files, storeObject.User, storeObject.Computer, storeObject.Domain);
            DbInsertFileEvents(storeObject.Dictionary);
        }

        private GroupSettingsObject DbRetrieveGroupSettings(string user, string domain)
        {
            using (var dataContext = DataContext as fgServerDb)
            {
                var settings = DbRetrieveUser(dataContext, user, DbRetrieveDomain(dataContext, domain).ID)
                        .USER_GROUP.GROUP_SETTING;
                return new GroupSettingsObject(settings.FILTER_UPDATE_INTERVAL,
                                               settings.ACCUMULATION_INTERVAL, 
                                               settings.GRANULARITY,
                                               settings.COLLECT_PROCESSES,
                                               settings.COLLECT_EXTENSIONS);

            }
        }

        private IEnumerable<string> DbRetrieveGroupProcesses(string user, string domain)
        {
            using (var dataContext = DataContext as fgServerDb)
            {
                return from p in DbRetrieveUser(dataContext, user, DbRetrieveDomain(dataContext, domain).ID).USER_GROUP.PROCESS 
                        select p.NAME;
            }
        }

        private IEnumerable<string> DbRetrieveGroupExtensions(string user, string domain)
        {
            using (var dataContext = DataContext as fgServerDb)
            {
                return from e in DbRetrieveUser(dataContext, user, DbRetrieveDomain(dataContext, domain).ID).USER_GROUP.EXTENSION 
                        select e.NAME;
            }
        }

        private void DbInsertProcesses(IEnumerable<string> processes)
        {
            using (var dataContext = DataContext as fgServerDb)
            {
                var unstoredProcesses = from p in processes
                                        where !dataContext.PROCESS.Any(n => n.NAME == p)
                                        select new PROCESS {NAME = p};

                foreach (var process in unstoredProcesses)
                {
                    dataContext.PROCESS.Add(process);
                }

                dataContext.SaveChanges();
            }
        }

        private void DbInsertExtensions(IEnumerable<string> extensions)
        {
            using (var dataContext = DataContext as fgServerDb)
            {
                var unstoredExtensions = from e in extensions
                                            where !dataContext.EXTENSION.Any(n => n.NAME == e)
                                            select new EXTENSION {NAME = e};

                foreach (var extension in unstoredExtensions)
                {
                    dataContext.EXTENSION.Add(extension);
                }

                dataContext.SaveChanges();
            }
        }

        private void DbInsertFiles(IEnumerable<string> files, string user, string computer, string domain)
        {
            using (var dataContext = DataContext as fgServerDb)
            {
                var retrieveUser = DbRetrieveUser(dataContext, user, DbRetrieveDomain(dataContext, domain).ID);
                var retrieveComputer = DbRetrieveComputer(dataContext, computer);

                var unstoredFiles = from f in files.AsEnumerable()
                                    where !dataContext.FILE.Any(n => n.NAME == f)
                                    select f;
                if (unstoredFiles != null)
                {
                    foreach (var name in unstoredFiles)
                    {
                        var extension = Path.GetExtension(name).ToUpper();
                        var file = new FILE
                            {
                                NAME = name,
                                COMPUTER_ID = retrieveComputer.ID,
                                EXTENSION_ID = dataContext.EXTENSION.First(e => e.NAME == extension).ID,
                                USER_ID = retrieveUser.ID
                            };
                        dataContext.FILE.Add(file);
                    }


                    dataContext.SaveChanges();
                }
            }
        }

        private void DbInsertFileEvents(ActionDictionary actionDictionary)
        {
            using (var dataContext = DataContext as fgServerDb)
            {
                //remove computer and user.ID from the queries because its from a local db
                var events = from KeyValuePair<EventId, EventAction> kvp in actionDictionary
                                select new FILE_EVENT
                                    {
                                        PROCESS_ID =
                                            dataContext.PROCESS.First(p => p.NAME == kvp.Key.ProcessName).ID,
                                        SOURCE_FILE_ID =
                                            dataContext.FILE.First(f => f.NAME == kvp.Key.SourceFileName).ID,
                                        TARGET_FILE_ID =
                                            dataContext.FILE.First(f => f.NAME == kvp.Key.TargetFileName).ID,
                                        START_TIME = kvp.Value.StartTime,
                                        END_TIME = kvp.Value.EndTime,
                                        COUNT = kvp.Value.Count,
                                        TYPE = kvp.Key.Action
                                    };
                //use SQL bulk copy http://cgeers.com/2011/05/19/entity-framework-bulk-copy/
                foreach (var fileEvent in events)
                {
                    dataContext.FILE_EVENT.Add(fileEvent);
                }

                dataContext.SaveChanges();
            }
        }

        private DOMAIN DbRetrieveDomain(fgServerDb dataContext, string domain)
        {
            DOMAIN retrieveDomain;
            try
            {
                retrieveDomain = dataContext.DOMAIN.First(b => b.NAME == domain);
            }
            catch
            {
                retrieveDomain = new DOMAIN { NAME = domain };
                dataContext.DOMAIN.Add(retrieveDomain);
                dataContext.SaveChanges();
            }

            return retrieveDomain;
        }

        private USER DbRetrieveUser(fgServerDb dataContext, string user, long domainId)
        {
            USER retrieveUser;
            try
            {
                retrieveUser = dataContext.USER.First(b => b.NAME == user && b.DOMAIN_ID == domainId);
            }
            catch
            {
                retrieveUser = new USER {NAME = user, DOMAIN_ID = domainId };
                dataContext.USER.Add(retrieveUser);
                dataContext.SaveChanges();
            }

            return retrieveUser;
        }

        private COMPUTER DbRetrieveComputer(fgServerDb dataContext, string computer)
        {
            COMPUTER retrieveComputer;
            try
            {
                retrieveComputer = dataContext.COMPUTER.First(b => b.NAME == computer);
            }
            catch
            {
                retrieveComputer = new COMPUTER {NAME = computer };
                dataContext.COMPUTER.Add(retrieveComputer);
                dataContext.SaveChanges();
            }

            return retrieveComputer;
        }
        #endregion
    }
}
