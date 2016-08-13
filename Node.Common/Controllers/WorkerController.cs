using Common;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Node.Common.Controllers
{
    [Route("api/[controller]")]
    public class WorkerController : Controller
    {
        private IWorkerSatus<WorkerStatusMessage> _status;

        public WorkerController(IWorkerSatus<WorkerStatusMessage> status)
        {
            _status = status;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
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

        // GET api/values/5
        [HttpGet("startJob/{message}")]
        public string GetStartJob(string message)
        {
            var workerMessage = new WorkerStatusMessage();
            workerMessage.Message = string.Format("StartJob: {0}", message);
            workerMessage.Source = "cluster";
            _status.AddStatus(workerMessage);
            return "Ok";
        }

        /// <summary>
        /// Receive process market message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="market"></param>
        /// <returns></returns>
        [HttpPost("processMarket/{message}")]
        public string PostProcessMarket(string message, [FromBody]ProcessMarket market)
        {
            var workerMessage = new WorkerStatusMessage();
            workerMessage.Message = string.Format("StartJob: {0}", message);
            workerMessage.Source = "cluster";
            workerMessage.PayLoadType = typeof(ProcessMarket);
            workerMessage.PayLoad = market;
            _status.AddStatus(workerMessage);
            return "Ok";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
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
