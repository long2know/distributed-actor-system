using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Common.Services;
using Common.Messages;
using Common.Models;
using Microsoft.AspNetCore.Http.Features;

namespace Common.Controllers.Cluster
{
    [Route("api/[controller]")]
    public class WorkerController : Controller
    {
        private IWorkerSatus<WorkerStatusMessage> _status;
        private Models.Cluster _cluster;

        public WorkerController(IWorkerSatus<WorkerStatusMessage> status, Models.Cluster cluster)
        {
            _status = status;
            _cluster = cluster;
        }

        [HttpGet("ready/{ip}")]
        public string GetWorkerReady(string ip)
        {
            //var remoteAddress = string.Format("{0}:{1}", Request.HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort);
            var nodeReady = new NodeReady() { IpAddress = ip };
            _cluster.Tell(nodeReady);
            return "OK";
        }

        [HttpGet("left/{ip}")]
        public string GetWorkerLeft(string ip)
        {
            var workerMessage = new WorkerStatusMessage();
            workerMessage.Message = string.Format("WorkerLeft: {0}", ip);
            _status.AddStatus(workerMessage);
            return "OK";
        }

        // GET api/values/5
        [HttpGet("{message}")]
        public string Get(string message)
        {
            var workerMessage = new WorkerStatusMessage();
            workerMessage.Message = "Received!";
            _status.AddStatus(workerMessage);
            return "Message received";
        }

        // POST api/values
        [HttpPost("taskComplete")]
        public void Post([FromBody]string taskId)
        {
            var taskComplete = new TaskComplete() { TaskId = taskId };
            _cluster.Tell(taskComplete);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
