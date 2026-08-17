using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public class StationAlertEventArgs : AlertEventArgs
    {
        public string systemName { get; }
        public string message { get; }
        public AlertServerity serverity { get; }
        public DateTime timeStamp { get; }

        public StationAlertEventArgs(string severity, string message ,string systemName, AlertServerity serverity, DateTime timeStamp) 
            : base( severity, message)
        {
            this.systemName = systemName;
            this.message = message;
            this.serverity = serverity;
            this.timeStamp = timeStamp;
        }

        public override string ToString() =>
            $"[{timeStamp:HH:mm:ss}] ({serverity}) {systemName}: {message}";

    }
}
