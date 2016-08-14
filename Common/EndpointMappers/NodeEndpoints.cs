using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Common.Models;
using Common.Messages;
using Common.Utilities;
using Common.Services;

namespace Common.EndpointMappers
{
    public class NodeEndpoints : IEndpoints
    {
        private IWorkerStatus<WorkerStatusMessage> _workerStatus;
        private string _workerRoute = "api/worker";

        public NodeEndpoints(IWorkerStatus<WorkerStatusMessage> workerStatus)
        {
            _workerStatus = workerStatus;
        }
        
        public void MapEndpoints(IApplicationBuilder app)
        {
            // We need a routeBuilder in order to utilize MapGet and MapPost
            var routeBuilder = new RouteBuilder(app);

            // Listen node ready and node left
            var startJobUrl = $"{_workerRoute}/startJob";
            var processMarketUrl = $"{_workerRoute}/processMarket/{{message}}";
            var getMessageUrl = $"{_workerRoute}/{{message}}";

            routeBuilder.MapPost(startJobUrl, StartJob);
            routeBuilder.MapPost(processMarketUrl, ProcessMarket);
            routeBuilder.MapGet(getMessageUrl, GetMessage);

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }

        public RequestDelegate StartJob
        {
            get
            {
                RequestDelegate requestDelegate = context =>
                {
                    var startJob = context.Request.ReadAsAsync<StartJob>().Result;
                    var workerMessage = new WorkerStatusMessage()
                    {
                        Message = $"StartJob: {startJob.TaskId}",
                        Source = "cluster",
                        PayLoadType = typeof(StartJob),
                        PayLoad = startJob
                    };
                    _workerStatus.AddStatus(workerMessage);
                    return context.Response.WriteAsync($"OK, {startJob.TaskId}!");
                };
                return requestDelegate;
            }
        }

        public RequestDelegate ProcessMarket
        {
            get
            {
                RequestDelegate requestDelegate = context =>
                {
                    var message = (string)context.GetRouteValue("message");
                    var processMarket = context.Request.ReadAsAsync<ProcessMarket>().Result;
                    var workerMessage = new WorkerStatusMessage()
                    {
                        Message = $"ProcessMarket: {message}",
                        Source = "cluster",
                        PayLoadType = typeof(ProcessMarket),
                        PayLoad = processMarket
                    };
                    _workerStatus.AddStatus(workerMessage);
                    return context.Response.WriteAsync($"OK, {message}!");
                };
                return requestDelegate;
            }
        }

        public RequestDelegate GetMessage
        {
            get
            {
                RequestDelegate requestDelegate = context =>
                {
                    var message = (string)context.GetRouteValue("message");
                    var workerMessage = new WorkerStatusMessage();
                    workerMessage.Message = $"Receive message, {message}!";
                    _workerStatus.AddStatus(workerMessage);
                    return context.Response.WriteAsync($"Receive message, {message}!");
                };
                return requestDelegate;
            }
        }
    }
}