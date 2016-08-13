using Microsoft.AspNetCore.Mvc;
using Common.Messages;
using Common.Services;

[Route("api/[controller]")]
public class StatusController : Common.Controllers.Cluster.StatusController
{
    public StatusController(IWorkerSatus<WorkerStatusMessage> status) : base(status) { }
}