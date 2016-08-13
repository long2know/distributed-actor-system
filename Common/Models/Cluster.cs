using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Common.Services;
using Common.Utilities;
using Common.Messages;
using Microsoft.AspNetCore.Hosting;

namespace Common.Models
{
    public static class ClusterExtensions
    {
        public static Cluster AddCluster(this IServiceCollection services)
        {
            var workerStatus = new WorkerStatus<WorkerStatusMessage>();
            var cluster = new Cluster(workerStatus);
            services.AddSingleton<Cluster>(cluster);
            services.AddSingleton<IWorkerSatus<WorkerStatusMessage>>(workerStatus);
            services.AddTransient<IApiService, ApiService>();

            var serviceProvider = cluster.ServiceProvider = services.BuildServiceProvider();

            var lifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
            lifetime.ApplicationStarted.Register(cluster.Started);
            lifetime.ApplicationStopping.Register(cluster.Stopping);
            lifetime.ApplicationStopped.Register(cluster.Stopped);
            return cluster;
        }
    }

    /// <summary>
    /// Basic cluster - inherit from BasicActor so that we can receive type messages (Tell)
    /// </summary>
    public class Cluster : BasicActor
    {
        private IWorkerSatus<WorkerStatusMessage> _workerStatus;
        private IServiceProvider _serviceProvider { get; set; }

        private ConcurrentDictionary<string, string> _availableNodes = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<Guid, object> _outbox = new ConcurrentDictionary<Guid, object>();
        public IServiceProvider ServiceProvider { set { _serviceProvider = value; } }
        private int _lastIndex = 0;

        public Cluster(IWorkerSatus<WorkerStatusMessage> workerStatus)
        {
            _workerStatus = workerStatus;

            Receive<NodeReady>(n =>
                _availableNodes.TryAdd(n.IpAddress, n.IpAddress)
            );

            Receive<NodeLeft>(n =>
            {
                string node;
                _availableNodes.TryRemove(n.IpAddress, out node);
            });

            Receive<TaskComplete>(t =>
            {
                object task;
                var success = _outbox.TryRemove(new Guid(t.TaskId), out task);
                if (task == null)
                {
                    // Why wasn't it found?
                    Console.WriteLine();
                }
            });
        }

        /// <summary>
        /// Simple round-robin to get next node
        /// </summary>
        public string NextNode
        {
            get
            {
                if ((_availableNodes?.Count ?? 0) == 0)
                {
                    return null;
                }

                if (_lastIndex == _availableNodes.Count - 1)
                {
                    _lastIndex = 0;
                }
                else
                {
                    _lastIndex++;
                }

                return _availableNodes.ElementAt(_lastIndex).Value;
            }
        }

        public void Started()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            TaskRepeater.Interval(TimeSpan.FromMilliseconds(50), () =>
            {
                if ((_availableNodes?.Count ?? 0) > 0)
                {
                    var random = new Random();
                    var nodeHost = this.NextNode;

                    // Randomly choose what to send to the node(s)
                    if (!string.IsNullOrWhiteSpace(nodeHost))
                    {
                        // Randomly choose what to send to the waiting nodes.
                        if (random.Next(0, 1000) > 500)
                        {
                            TellNodeToProcessMarket(nodeHost);
                        }
                        else
                        {
                            TellNodeToStart(nodeHost);
                        }
                    }
                }
            }, cancellationTokenSource.Token, true);
        }

        public void Stopping()
        {
            // Perform some cleanup?
        }

        public void Stopped()
        {
            // Perform some cleanup?
        }

        //private void ReceivedNodeReady(NodeReady nodeReady)
        //{
        //    _workerStatus.AddCallback("cluster", action =>
        //    {
        //        if (action.Message.Contains("WorkerReady:"))
        //        {
        //            var ipAddress = action.Message.Replace("WorkerReady: ", string.Empty);
        //            _availableNodes.TryAdd(ipAddress, ipAddress);
        //        }
        //        Console.WriteLine(action.TimeStamp);
        //    });
        //}

        private void TellNodeToStart(string nodeHost)
        {
            try
            {
                var taskId = Guid.NewGuid().ToString();
                var message = "start please";
                var apiService = _serviceProvider.GetRequiredService<IApiService>();
                var url = string.Format("http://{0}/api/worker/startJob/", nodeHost);
                var startJob = new StartJob() { TaskId = taskId, MarketIdentity = message };
                _outbox.TryAdd(new Guid(taskId), startJob);
                apiService.PutOrPostToApi<StartJob, string>(url, startJob);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void TellNodeToProcessMarket(string nodeHost)
        {
            try
            {
                var taskId = Guid.NewGuid().ToString();
                var apiService = _serviceProvider.GetRequiredService<IApiService>();
                var url = string.Format("http://{0}/api/worker/processMarket/{1}", nodeHost, "process");
                var processMarket = new ProcessMarket() { MarketIdentity = RandomHelper.RandMarket(), TaskId = taskId };
                _outbox.TryAdd(new Guid(taskId), processMarket);
                apiService.PutOrPostToApi<ProcessMarket, string>(url, processMarket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
