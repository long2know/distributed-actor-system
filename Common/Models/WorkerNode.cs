﻿using System;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.EndpointMappers;
using Common.Services;
using Common.Utilities;
using Common.Messages;

namespace Common.Models
{
    public static class NodeExtentions
    {
        /// <summary>
        /// Register our Middleware
        /// </summary>
        /// <param name="app"></param>
        public static void UserWorkerNode(this IApplicationBuilder app)
        {
            var endpointMapper = app.ApplicationServices.GetRequiredService<IEndpoints>();
            endpointMapper.MapEndpoints(app);
        }

        /// <summary>
        /// Add all worker node dependencies
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static WorkerNode AddWorkerNode(this IServiceCollection services)
        {
            var workerStatus = new WorkerStatus<WorkerStatusMessage>();
            var workerNode = new WorkerNode(workerStatus);
            services.AddSingleton<WorkerNode>(workerNode);
            services.AddSingleton<IWorkerStatus<WorkerStatusMessage>>(workerStatus);
            services.AddTransient<IApiService, ApiService>();
            services.AddTransient<IEndpoints, NodeEndpoints>();

            var serviceProvider = workerNode.ServiceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(@"Properties/launchSettings.json", optional: false, reloadOnChange: true);
            var launchConfig = builder.Build();
            var ssslPort = launchConfig.GetValue<int>("iisSettings:iisExpress:sslPort");
            var hostUrl = launchConfig.GetValue<string>("iisSettings:iisExpress:applicationUrl");
            workerNode.HostUrl = hostUrl;

            var lifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
            lifetime.ApplicationStarted.Register(workerNode.Started);
            lifetime.ApplicationStopping.Register(workerNode.Stopping);
            lifetime.ApplicationStopped.Register(workerNode.Stopped);
            return workerNode;
        }
    }

    public class WorkerNode
    {
        private IWorkerStatus<WorkerStatusMessage> _workerStatus;
        private IServiceProvider _serviceProvider { get; set; }
        private string _hostUrl;
        private string _clusterUrl = "http://localhost:9000";
        public IServiceProvider ServiceProvider { set { _serviceProvider = value; } }
        public string HostUrl { set { _hostUrl = value; } }
        public string ClusterUrl { get; set; }

        public WorkerNode(IWorkerStatus<WorkerStatusMessage> workerStatus)
        {
            _workerStatus = workerStatus;

            var maxActors = 5;
            for (var i = 0; i < maxActors; i++)
            {
                var actor = new BasicActor();
                actor.AddCallback("node", message =>
                {
                    var apiService = _serviceProvider.GetRequiredService<IApiService>();
                    var url = string.Format("{0}/api/worker/taskComplete", _clusterUrl);
                    apiService.PutOrPostToApi<string, string>(url, message.TaskId);
                });
                _workerStatus.AddActor(Guid.NewGuid().ToString(), actor);
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
                    var encodedHostUrl = WebUtility.UrlEncode(_hostUrl.Replace("http://", string.Empty));
                    apiService.GetFromApi<string>(string.Format("{0}/api/worker/ready/{1}", _clusterUrl, encodedHostUrl));
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
                apiService.GetFromApi<string>(string.Format(string.Format("{0}/api/worker/left/{1}", _clusterUrl, WebUtility.HtmlEncode(_hostUrl))));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't join cluster.  Keep trying?");
                Console.WriteLine(ex);
            }
        }
    }
}
