using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Class
{
    [DataContract]
    public class StoreObject
    {
        public StoreObject()
        {
            
        }

        public StoreObject(ActionDictionary dictionary, IEnumerable<string> processes, IEnumerable<string> extensions, string user, string computer, string domain)
        {
            Dictionary = dictionary;
            Processes = processes;
            Extensions = extensions;
            User = user;
            Computer = computer;
            Domain = domain;
        }

        [DataMember] public string User;

        [DataMember] public string Computer;

        [DataMember] public string Domain;

        [DataMember] public ActionDictionary Dictionary;

        [DataMember] public IEnumerable<string> Processes;

        [DataMember] public IEnumerable<string> Extensions;
    }
}
