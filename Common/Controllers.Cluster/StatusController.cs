//using System.Collections.Generic;
//using Microsoft.AspNetCore.Mvc;
//using Common.Services;
//using Common.Messages;

//namespace Common.Controllers.Cluster
//{
//    [Route("api/[controller]")]
//    public class StatusController : Controller
//    {
//        private IWorkerStatus<WorkerStatusMessage> _status;

//        public StatusController(IWorkerStatus<WorkerStatusMessage> status)
//        {
//            _status = status;
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
//    }
//}
