using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Client.Class
{
    public class ActionDictionaryEnum : IEnumerator
    {
        private ConcurrentStack<ConcurrentDictionary<EventId, EventAction>> _list;
        private IEnumerator<ConcurrentDictionary<EventId, EventAction>> _listEnum;
        private IEnumerator<KeyValuePair<EventId, EventAction>> _dictionaryEnum;
        private bool _first = true;

        public ActionDictionaryEnum(ConcurrentStack<ConcurrentDictionary<EventId, EventAction>> list)
        {
            _list = list;
            _listEnum = _list.GetEnumerator();
            ConcurrentDictionary<EventId, EventAction> firstDictionary;
            if (_list.TryPeek(out firstDictionary))
            {
                _dictionaryEnum = firstDictionary.GetEnumerator();
            }
        }

        public bool MoveNext()
        {
            try
            {
                if (!_first || !_dictionaryEnum.MoveNext())
                {
                    if (!_listEnum.MoveNext())
                    {
                        return false;
                    }
                    if (_listEnum.Current != null)
                    {
                        _dictionaryEnum = _listEnum.Current.GetEnumerator();
                    }
                    _dictionaryEnum.MoveNext();
                    _first = false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Reset()
        {
            _listEnum = _list.GetEnumerator();
            _first = true;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public KeyValuePair<EventId, EventAction> Current
        {
            get
            {
                try
                {
                    return _dictionaryEnum.Current;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}