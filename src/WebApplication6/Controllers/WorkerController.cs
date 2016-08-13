using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Common.Services;
using Common.Messages;
using Common.Models;

namespace ClusterMaster.Controllers
{
    [Route("api/[controller]")]
    public class WorkerController : Common.Controllers.Cluster.WorkerController
    {
        public WorkerController(IWorkerSatus<WorkerStatusMessage> status, Cluster cluster) : base(status, cluster) { }
    }
}