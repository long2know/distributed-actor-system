using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Common.Messages;
using Common.Services;

namespace Common.Controllers.Node
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        private IWorkerSatus<WorkerStatusMessage> _status;

        public StatusController(IWorkerSatus<WorkerStatusMessage> status)
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
