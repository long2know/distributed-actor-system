using Common.Messages;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Models
{
    public interface IActor<T>
    {
        string Id { get; set; }
        CancellationToken CancellationToken { get; set; }

        // Track my own status
        void AddStatus(T message);

        // For now, actor callbacks are used to pass completion
        void Callback(T message);

        // For cross actor/object communication
        void AddCallback(string id, Action<T> action);

        // For removing tells
        void RemoveCallback(string id);

        // Incoming message
        void Tell<V>(V message);
    }

    public abstract class BaseActor<T> : IActor<T> where T : new()
    {
        private string _id;
        private bool _isBusy;

        public string Id { get { return _id; } set { _id = value; } }
        public CancellationToken CancellationToken { get { return _cancellationToken; } set { _cancellationToken = value; } }
        public int MaxMessages { get; set; } = 20;
        public ConcurrentQueue<T> StatusMessages { get { return _queue; } }
        public TimeSpan TimeSpanSinceLastStatus { get { return DateTime.UtcNow - _lastTimeReceived; } }
        public T CurrentStatus { get { return StatusMessages.LastOrDefault(); } }

        protected CancellationToken _cancellationToken;
        private DateTime _lastTimeReceived;
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private ConcurrentDictionary<Type, Action<object>> _handlers = new ConcurrentDictionary<Type, Action<object>>();
        private ConcurrentDictionary<string, Action<T>> _actions = new ConcurrentDictionary<string, Action<T>>();
        private ConcurrentQueue<object> _mailbox = new ConcurrentQueue<object>();
        private object _lockObj = new object();
        private object _lockCheck = new object();

        /// <summary>
        /// Thread-safe busy indicator
        /// </summary>
        public bool IsBusy
        {
            get { lock (_lockObj) { return _isBusy; } }
            set { lock (_lockObj) { _isBusy = value; } }
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
        }

        public void Callback(T message)
        {
            if (_actions.Count > 0)
            {
                foreach (var action in _actions)
                {
                    action.Value.Invoke(message);
                }
            }
        }

        protected void Receive<V>(Action<V> handler, Predicate<V> execute = null)
        {
            var key = typeof(V);
            // We have to wrap the action order for it to be Action<object>!
            var handlerObj = new Action<object>(obj => { var castObj = (V)Convert.ChangeType(obj, typeof(V)); handler(castObj); });
            _handlers.TryAdd(typeof(V), handlerObj);
        }

        public void Tell<V>(V message)
        {
            _mailbox.Enqueue(message);
            MessageFinished();
        }

        private void MessageFinished()
        {
            Action finished = () => MessageFinished();
            ProcessNextMessage(finished);
        }

        private void ProcessNextMessage(Action finished)
        {
            // Using the lockcheck so that multiple messages don't get processed
            // We should come out of the lock quickly since the working code is a seperate
            // task.
            lock (_lockCheck)
            {
                if (_mailbox.Count > 0 && _handlers.Count > 0 && !IsBusy)
                {
                    IsBusy = true;
                    object message = null;
                    _mailbox.TryDequeue(out message);
                    Action<object> handler = null;
                    var key = message.GetType();
                    _handlers.TryGetValue(key, out handler);

                    // Spawn task to perform the wait.
                    var task = new Task(() =>
                    {
                        try
                        {
                            handler(message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            IsBusy = false;
                            finished();
                        }
                    }, _cancellationToken);
                    task.Start();
                }
            }
        }
    }

    public class BasicActor : BaseActor<WorkerStatusMessage>
    {
        public BasicActor()
        {
            Receive<string>(s =>
                Console.WriteLine("Received string: " + s)
            );

            Receive<StartJob>(job =>
            {
                var task = Task.Run(() =>
                {
                    StartJob(job);
                    WorkerStatusMessage message = new WorkerStatusMessage() { TaskId = job.TaskId, Message = "Complete" };
                    AddStatus(message);
                    Callback(message);
                }, _cancellationToken);

                // Wait for the task to finish so that the calling
                // task (ProcessMessage) will not continue processing other messages
                // until this work is done.
                task.Wait();
            });

            Receive<ProcessMarket>(job =>
            {
                var task = Task.Run(() =>
                {
                    ProcessMarket(job);
                    WorkerStatusMessage message = new WorkerStatusMessage() { TaskId = job.TaskId, Message = "Complete" };
                    AddStatus(message);
                    Callback(message);
                }, _cancellationToken);

                // Wait for the task to finish so that the calling
                // task (ProcessMessage) will not continue processing other messages
                // until this work is done.
                task.Wait();
            });
        }

        void StartJob(StartJob job)
        {
            Console.WriteLine("Starting job.. ");
        }

        void ProcessMarket(ProcessMarket market)
        {
            Console.WriteLine("Processing market.. ");
        }
    }
}
