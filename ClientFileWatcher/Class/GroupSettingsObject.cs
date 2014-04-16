using System;
using System.Runtime.Serialization;

namespace Client.Class
{
    [DataContract]
    public class GroupSettingsObject
    {
        public GroupSettingsObject(TimeSpan filterInterval, TimeSpan accumulationInterval, TimeSpan granularity,
                           bool collectProcesses, bool collectExtensions)
        {
            FilterInterval = filterInterval;
            AccumulationInterval = accumulationInterval;
            Granularity = granularity;
            CollectProcesses = collectProcesses;
            CollectExtensions = collectExtensions;
        }

        [DataMember]
        public TimeSpan FilterInterval { get; set; }

        [DataMember]
        public TimeSpan AccumulationInterval { get; set; }

        [DataMember]
        public TimeSpan Granularity { get; set; }

        [DataMember]
        public bool CollectProcesses { get; set; }

        [DataMember]
        public bool CollectExtensions { get; set; }
    }
}
