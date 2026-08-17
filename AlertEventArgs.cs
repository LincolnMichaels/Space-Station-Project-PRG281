using System;

namespace Chris_602473_Prg281_Proj
{
    public class AlertEventArgs : EventArgs
    {
        public string Severity { get; }   // "WARNING" or "CRITICAL"
        public string Message { get; }
        public DateTime Timestamp { get; }

        public AlertEventArgs(string severity, string message)
        {
            Severity = severity;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }

}
