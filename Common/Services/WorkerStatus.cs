using System;
using System.Collections.Concurrent;
using System.Linq;
using Common.Models;

namespace Common.Services
{
    public interface IWorkerSatus<T> : IDisposable where T : class
    {
        T CurrentStatus { get; }
        DateTime Date { get; set; }
        ConcurrentQueue<T> StatusMessages { get; }
        TimeSpan TimeSpanSinceLastStatus { get; }
        string TimeSinceLastStatus { get; }
        int MaxMessages { get; set; }

        void AddStatus(T message);
        void AddCallback(string id, Action<T> action);
        void RemoveCallback(string id);

        void RemoveActor(string id);
        void AddActor(string id, IActor<T> actor);
        void AddActorTell(string id, IActor<T> action);

        IActor<T> Next { get; }
    }

    public class WorkerStatus<T> : IWorkerSatus<T> where T : class
    {
        private DateTime _lastTimeReceived;
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private ConcurrentDictionary<string, IActor<T>> _actors = new ConcurrentDictionary<string, IActor<T>>();
        private ConcurrentDictionary<string, Action<T>> _actions = new ConcurrentDictionary<string, Action<T>>();
        private int _lastIndex = 0;

        public IActor<T> Next
        {
            get
            {
                if ((_actors?.Count ?? 0) == 0)
                {
                    return null;
                }
                
                if (_lastIndex == _actors.Count - 1)
                {
                    _lastIndex = 0;
                }
                else
                {
                    _lastIndex++;
                }

                return _actors.ElementAt(_lastIndex).Value;
            }
        }

        public string TimeSinceLastStatus
        {
            get
            {
                var timespan = DateTime.UtcNow - _lastTimeReceived;
                return timespan.ToString(@"mm\:ss\.ffff");
            }
        }

        public TimeSpan TimeSpanSinceLastStatus { get { return DateTime.UtcNow - _lastTimeReceived; } }
        public T CurrentStatus { get { return StatusMessages.LastOrDefault(); } }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public ConcurrentQueue<T> StatusMessages { get { return _queue; } }
        public int MaxMessages { get; set; } = 20;

        public WorkerStatus()
        {

        }

        public void AddStatus(T message)
        {
            _lastTimeReceived = DateTime.UtcNow;
            //_queue.Enqueue(new StatusMessage() { Message = message.Replace(@"{", @"{{").Replace(@"}", @"}}") });
            _queue.Enqueue(message);

            T outItem;
            while (_queue.Count > this.MaxMessages)
            {
                _queue.TryDequeue(out outItem);
            }

            T dequeuedMessage = this.CurrentStatus;

            if (_actions.Count > 0)
            {
                foreach (var action in _actions)
                {
                    action.Value.Invoke(dequeuedMessage);
                }
            }
        }

        public void AddCallback(string id, Action<T> action)
        {
            RemoveCallback(id);
            _actions.TryAdd(id, action);
        }

        public void RemoveCallback(string id)
        {
            if (_actions.ContainsKey(id))
            {
                Action<T> outAction;
                _actions.TryRemove(id, out outAction);
            }
        }

        public void Dispose()
        {

        }

        public void RemoveActor(string id)
        {
            if (_actors.ContainsKey(id))
            {
                IActor<T> outActor;
                if (_actors.TryRemove(id, out outActor))
                {
                    //outActor.Tell<string>(x => "Hello");
                }
            }
        }

        public void AddActor(string id, IActor<T> actor)
        {
            RemoveActor(id);
            _actors.TryAdd(id, actor);
        }

        public void AddActorTell(string id, IActor<T> action)
        {
            throw new NotImplementedException();
        }
    }
}
