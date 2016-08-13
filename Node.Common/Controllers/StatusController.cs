using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Common;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Node.Common.Controllers
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
