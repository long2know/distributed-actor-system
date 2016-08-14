//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using Common.Messages;
//using Common.Services;

//namespace Common.Controllers.Node
//{
//    [Route("api/[controller]")]
//    public class WorkerController : Controller
//    {
//        private IWorkerStatus<WorkerStatusMessage> _status;

//        public WorkerController(IWorkerStatus<WorkerStatusMessage> status)
//        {
//            _status = status;
//        }

//        /// <summary>
//        /// Receive a general message
//        /// </summary>
//        /// <param name="message"></param>
//        /// <returns></returns>
//        [HttpGet("{message}")]
//        public string Get(string message)
//        {
//            var workerMessage = new WorkerStatusMessage();
//            workerMessage.Message = "Received!";
//            _status.AddStatus(workerMessage);

//            return "Message received";
//        }

//        /// <summary>
//        /// Start job
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="startJob"></param>
//        /// <returns></returns>
//        [HttpPost("startJob")]
//        public string GetStartJob(string message, [FromBody]StartJob startJob)
//        {
//            var workerMessage = new WorkerStatusMessage();
//            workerMessage.Message = string.Format("StartJob: {0}", message);
//            workerMessage.Source = "cluster";
//            workerMessage.PayLoadType = typeof(StartJob);
//            workerMessage.PayLoad = startJob;
//            _status.AddStatus(workerMessage);
//            return "Ok";
//        }

//        /// <summary>
//        /// Receive process market message
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="market"></param>
//        /// <returns></returns>
//        [HttpPost("processMarket/{message}")]
//        public string PostProcessMarket(string message, [FromBody]ProcessMarket market)
//        {
//            var workerMessage = new WorkerStatusMessage();
//            workerMessage.Message = string.Format("StartJob: {0}", message);
//            workerMessage.Source = "cluster";
//            workerMessage.PayLoadType = typeof(ProcessMarket);
//            workerMessage.PayLoad = market;
//            _status.AddStatus(workerMessage);
//            return "Ok";
//        }        
//    }
//}
