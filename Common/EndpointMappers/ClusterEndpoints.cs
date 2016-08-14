using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Common.Models;
using Common.Messages;
using Common.Utilities;

namespace Common.EndpointMappers
{
    public class ClusterEndpoints : IEndpoints
    {
        private Cluster _cluster;
        private string _workerRoute = "api/worker";

        public ClusterEndpoints(Cluster cluster)
        {
            _cluster = cluster;
        }

        //[HttpGet("ready/{ip}")]
        //public string GetWorkerReady(string ip)
        //{
        //    var nodeReady = new NodeReady() { IpAddress = ip };
        //    _cluster.Tell(nodeReady);
        //    return "OK";
        //}

        //[HttpGet("left/{ip}")]
        //public string GetWorkerLeft(string ip)
        //{
        //    var workerMessage = new WorkerStatusMessage();
        //    workerMessage.Message = string.Format("WorkerLeft: {0}", ip);
        //    _status.AddStatus(workerMessage);
        //    return "OK";
        //}

        public void MapEndpoints(IApplicationBuilder app)
        {
            //// We need a route builder in order to get the query/post values
            //var trackPackageRouteHandler = new RouteHandler(context =>
            //{
            //    var routeValues = context.GetRouteData().Values;
            //    return null;
            //    //context.Response.WriteAsync($"Hello! Route values: {string.Join(", ", routeValues)}");
            //});

            //var routeBuilder = new RouteBuilder(app, trackPackageRouteHandler);

            var routeBuilder = new RouteBuilder(app);

            // Listen node ready
            //_app.Map("/ready/{ip}", builder =>
            //{
            //    builder.Run(async context =>
            //    {
            //        await context.Response.WriteAsync("Map Test Successful");
            //    });
            //});

            // Listen node ready and node left
            var readyUrl = $"{_workerRoute}/ready/{{ip}}";
            var leftUrl = $"{_workerRoute}/left/{{ip}}";
            var taskCompleteUrl = $"{_workerRoute}/taskComplete";

            routeBuilder.MapGet(readyUrl, WorkerNodeReady);
            routeBuilder.MapGet(readyUrl, WorkerNodeLeft);
            routeBuilder.MapPost(taskCompleteUrl, TaskComplete);

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }

        public RequestDelegate WorkerNodeReady
        {
            get
            {
                RequestDelegate requestDelegate = context =>
                {
                    var ip = (string)context.GetRouteValue("ip");
                    var nodeReady = new NodeReady() { IpAddress = ip };
                    _cluster.Tell(nodeReady);
                    return context.Response.WriteAsync($"OK, {ip}!");
                };
                return requestDelegate;
            }
        }

        public RequestDelegate WorkerNodeLeft
        {
            get
            {
                RequestDelegate requestDelegate = context =>
                {
                    var ip = (string)context.GetRouteValue("ip");
                    var nodeLeft = new NodeLeft() { IpAddress = ip };
                    _cluster.Tell(nodeLeft);
                    return context.Response.WriteAsync($"OK, {ip}!");
                };
                return requestDelegate;
            }
        }

        public RequestDelegate TaskComplete
        {
            get
            {
                RequestDelegate requestDelegate = context =>
                {
                    string taskId = context.Request.ReadAsAsync<string>().Result;
                    var taskComplete = new TaskComplete() { TaskId = taskId };
                    _cluster.Tell(taskComplete);
                    return context.Response.WriteAsync($"OK, {taskId}!");
                };
                return requestDelegate;
            }
        }
    }
}