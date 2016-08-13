using Microsoft.AspNetCore.Mvc;
using Common.Services;
using Common.Messages;

namespace Worker1.Controllers
{
    [Route("api/[controller]")]
    public class WorkerController : Common.Controllers.Node.WorkerController
    {
        public WorkerController(IWorkerSatus<WorkerStatusMessage> status) : base(status) { }
    }
}