using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Client.DataModel;

namespace Client.Class
{
    [DataContract]
    public class ActionDictionary : IEnumerable
    {
        [DataMember]
        public TimeSpan Interval;

        [DataMember]
        public long Count { get; private set; }

        public ConcurrentStack<ConcurrentDictionary<EventId, EventAction>> Dictionaries = new ConcurrentStack<ConcurrentDictionary<EventId, EventAction>>();
        private ConcurrentDictionary<EventId, EventAction> CurrentDictionary
        {
            get
            {
                ConcurrentDictionary<EventId, EventAction> dict;
                return Dictionaries.TryPeek(out dict) ? dict : null;
            }
        }

        [DataMember]
        public Stack<Dictionary<EventId, EventAction>> GetDictionaryStack
        {
            get
            {
                return new Stack<Dictionary<EventId, EventAction>>
                    (Dictionaries.Select(dict => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
            }
           
            set
            {
                Dictionaries = new ConcurrentStack<ConcurrentDictionary<EventId, EventAction>>
                    (value.Select(dict => new ConcurrentDictionary<EventId, EventAction>(dict)));
            }
        }

        [DataMember]
        private DateTime _startTime = DateTime.Now;

        public ActionDictionary()
        {
            Dictionaries.Push(new ConcurrentDictionary<EventId, EventAction>());
        }

        public void Add(string sourceFileName, string targetFileName, string processName, FILE_EVENT_TYPE_ENUM action, DateTime time)
        {
            if ((time - _startTime).TotalSeconds > Interval.TotalSeconds)
            {
                _startTime = DateTime.Now;
                Dictionaries.Push(new ConcurrentDictionary<EventId, EventAction>());
            }

            //Create, Read, Write, Rename/Move, Delete
            var eventId = new EventId(sourceFileName, targetFileName, processName, action);

            if (!CurrentDictionary.ContainsKey(eventId))
            {
                var eventAction = new EventAction { StartTime = time, EndTime = time, Count = 1 };
                CurrentDictionary[eventId] = eventAction;
                Count++;
            }
            else
            {
                var entry = CurrentDictionary[eventId];
                entry.EndTime = time;
                entry.Count++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ActionDictionaryEnum GetEnumerator()
        {
            return new ActionDictionaryEnum(Dictionaries);
        }
    }
}