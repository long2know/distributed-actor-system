//using Microsoft.AspNetCore.Mvc;
//using Common.Services;
//using Common.Messages;

//namespace Common.Controllers.Cluster
//{
//    [Route("api/[controller]")]
//    public class WorkerController : Controller
//    {
//        private IWorkerStatus<WorkerStatusMessage> _status;
//        private Models.Cluster _cluster;

//        public WorkerController(IWorkerStatus<WorkerStatusMessage> status, Models.Cluster cluster)
//        {
//            _status = status;
//            _cluster = cluster;
//        }

//        [HttpGet("ready/{ip}")]
//        public string GetWorkerReady(string ip)
//        {
//            var nodeReady = new NodeReady() { IpAddress = ip };
//            _cluster.Tell(nodeReady);
//            return "OK";
//        }

//        [HttpGet("left/{ip}")]
//        public string GetWorkerLeft(string ip)
//        {
//            var workerMessage = new WorkerStatusMessage();
//            workerMessage.Message = string.Format("WorkerLeft: {0}", ip);
//            _status.AddStatus(workerMessage);
//            return "OK";
//        }

//        // GET api/values/5
//        [HttpGet("{message}")]
//        public string Get(string message)
//        {
//            var workerMessage = new WorkerStatusMessage();
//            workerMessage.Message = "Received!";
//            _status.AddStatus(workerMessage);
//            return "Message received";
//        }

//        // POST api/values
//        [HttpPost("taskComplete")]
//        public void Post([FromBody]string taskId)
//        {
//            var taskComplete = new TaskComplete() { TaskId = taskId };
//            _cluster.Tell(taskComplete);
//        }
//    }
//}
