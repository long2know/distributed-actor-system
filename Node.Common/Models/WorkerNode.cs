using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Common;
using Common.Services;
using Common.Utilities;
using Microsoft.AspNetCore.Hosting;

namespace Node.Common.Models
{
    public static class NodeExtention
    {
        public static WorkerNode AddWorkerNode(this IServiceCollection services)
        {
            var workerStatus = new WorkerStatus<WorkerStatusMessage>();
            var workerNode = new WorkerNode(workerStatus);
            services.AddSingleton<WorkerNode>(workerNode);
            services.AddSingleton<IWorkerSatus<WorkerStatusMessage>>(workerStatus);
            services.AddTransient<IApiService, ApiService>();

            var serviceProvider = workerNode.ServiceProvider = services.BuildServiceProvider();

            var lifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
            lifetime.ApplicationStarted.Register(workerNode.Started);
            lifetime.ApplicationStopping.Register(workerNode.Stopping);
            lifetime.ApplicationStopped.Register(workerNode.Stopped);
            return workerNode;
        }
    }

    public class WorkerNode
    {
        private IWorkerSatus<WorkerStatusMessage> _workerStatus;
        private IServiceProvider _serviceProvider { get; set; }
        public IServiceProvider ServiceProvider { set { _serviceProvider = value; } }

        public WorkerNode(IWorkerSatus<WorkerStatusMessage> workerStatus)
        {
            _workerStatus = workerStatus;

            var maxActors = 2;
            for (var i = 0; i < maxActors; i++)
            {
                var actor = new BasicActor<WorkerStatusMessage>();
                actor.AddCallback("node", message =>
                {
                    Console.WriteLine();
                });
                _workerStatus.AddActor(new Guid().ToString(), new BasicActor<WorkerStatusMessage>());
            }
        }

        public void Started()
        {
            // Tell "cluster" I'm ready.
            _workerStatus.AddCallback("test", action =>
            {
                if (action.PayLoad == null)
                {
                    _workerStatus.Next.Tell(action.Message);
                }
                else
                {
                    _workerStatus.Next.Tell(action.PayLoad);
                }
            });

            var cancellationTokenSource = new CancellationTokenSource();
            TaskRepeater.Interval(TimeSpan.FromSeconds(30), () =>
            {
                try
                {
                    var apiService = _serviceProvider.GetRequiredService<IApiService>();
                    apiService.GetFromApi<string>("http://localhost:9000/api/worker/ready/localhost%3A9001");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Couldn't join cluster.  Keep trying?");
                    Console.WriteLine(ex);
                }
            }, cancellationTokenSource.Token, true);
        }

        public void Stopping()
        {
            _workerStatus.AddCallback("test", action =>
            {
                Console.WriteLine(action.TimeStamp);
            });
            Console.WriteLine();
        }

        public void Stopped()
        {
            // Tell "cluster" I'm gone!.
            try
            {
                var apiService = _serviceProvider.GetRequiredService<IApiService>();
                apiService.GetFromApi<string>("http://localhost:9000/api/worker/left/localhost%3A9001");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't join cluster.  Keep trying?");
                Console.WriteLine(ex);
            }
        }
    }
}
