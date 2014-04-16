using System;
using System.Runtime.Serialization;
using Client.DataModel;

namespace Client.Class
{
    [DataContract]
    public class EventId : Tuple<string, string, string, FILE_EVENT_TYPE_ENUM>
    {
        public EventId(string sourceFileName, string targetFileName, string processName, FILE_EVENT_TYPE_ENUM action)
            : base(sourceFileName, targetFileName, processName, action)
        {
        }

        public string SourceFileName
        {
            get { return Item1; }
        }

        public string TargetFileName
        {
            get { return Item2;  }
        }

        public string ProcessName
        {
            get { return Item3; }
        }

        public FILE_EVENT_TYPE_ENUM Action
        {
            get { return Item4; }
        }
    }
}