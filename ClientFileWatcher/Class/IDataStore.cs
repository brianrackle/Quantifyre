namespace Client.Class
{
    public interface IDataStore
    {
        /// <summary>
        /// Sync the next unsynced StoreObject with store from unsynced queue
        /// </summary>
        /// <param name="store">IDataStore to save StoreObjects to</param>
        /// <returns>Returns if more StoreObjects need to be synced</returns>
        bool ForceDbSyncAction(IDataStore store);

        /// <summary>
        /// Retried the saved UserSettingsObject associated with the user and domain
        /// </summary>
        /// <param name="user">the user who's settings to retrieve</param>
        /// <param name="domain">the domain of the user</param>
        /// <returns>UserSettingsObject of the specified user</returns>
        UserSettingsObject ForceDbFilterRefresh(string user, string domain);

        /// <summary>
        /// Accumulate any enqueued items to this IDataStore
        /// </summary>
        void Flush();

        /// <summary>
        /// Save storeObject to this DataStore
        /// </summary>
        /// <param name="storeObject">storeObject to be saved</param>
        void ForceDbAccumulation(StoreObject storeObject);

        /// <summary>
        /// Save settingsObject to this DataStore
        /// </summary>
        /// <param name="settingsObject">settingsObject to be saved</param>
        void ForceDbAccumulation(UserSettingsObject settingsObject);
    }
}
