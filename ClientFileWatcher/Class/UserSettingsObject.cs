using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Class
{
    [DataContract]
    public class UserSettingsObject
    {
        public UserSettingsObject(List<string> groupProcesses, List<string> groupExtensions, GroupSettingsObject groupSettings, string user, string domain)
        {
            GroupProcesses = groupProcesses;
            GroupExtensions = groupExtensions;
            GroupSettings = groupSettings;
            User = user;
            Domain = domain;
        }

        [DataMember] public string User;

        [DataMember] public string Domain;

        [DataMember] public List<string> GroupProcesses;

        [DataMember] public List<string> GroupExtensions;

        [DataMember] public GroupSettingsObject GroupSettings;
    }
}
