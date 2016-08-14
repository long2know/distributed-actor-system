using System;

namespace Common.Messages
{
    public class WorkerStatusMessage
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public string TaskId { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public long Ticks { get; set; } = DateTime.UtcNow.Ticks;

        public Type PayLoadType { get; set; }
        public object PayLoad { get; set; }

        public WorkerStatusMessage()
        {
        }
    }
}
