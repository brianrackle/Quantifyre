using System;
using System.Runtime.Serialization;

namespace Client.Class
{
    [DataContract]
    public class EventAction
    {
        [DataMember]
        public DateTime StartTime = DateTime.MinValue;
        [DataMember]
        public DateTime EndTime = DateTime.MinValue;
        [DataMember]
        public long Count = 0;
    }
}
