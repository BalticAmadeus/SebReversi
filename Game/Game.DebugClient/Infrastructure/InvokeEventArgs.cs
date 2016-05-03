using System;

namespace Game.DebugClient.Infrastructure
{
    public class InvokeEventArgs : EventArgs
    {
        public string Request { get; set; }
        public string Response { get; set; }
        public long OperationTime { get; set; }
    }
}