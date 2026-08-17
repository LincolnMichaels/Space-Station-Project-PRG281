using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{

    //thrown When an operator or system tries to consume/allocate more of 
    // a resource than what is currently available on the space station
    public class InsuffResExcep : Exception
    {
        private string resourceName;
        private double requested;
        private double available;

    
        public string ResourceName { get => resourceName; set => resourceName = value; }
        public double Requested { get => requested; set => requested = value; }
        public double Available { get => available; set => available = value; }

        public InsuffResExcep() 
            : base("Insufficient resource available on the space station")
        {

        }

        public InsuffResExcep(string message) : base(message)
        {

        }

        public InsuffResExcep(string message, Exception innerException) : base(message, innerException)
        {

        }

        public InsuffResExcep(string resourceName, double requested, double available)
            : base($"Insufficient {resourceName}: requested {requested:F1}, but only {available:F1} available.")
        {
            ResourceName = resourceName;
            Requested = requested;
            Available = available;
        }

        public InsuffResExcep(string resourceName, double requested, double available, Exception inner)
           : base($"Insufficient {resourceName}: requested {requested:F1}, but only {available:F1} available.", inner)
        {
            ResourceName = resourceName;
            Requested = requested;
            Available = available;
        }


    }

    //This is Thrown when a station system enters a state so unsafe 
    //that normal operation cannot continue
    public class CritSysFailExcep : Exception
    {
        public string SystemName { get; }

        public CritSysFailExcep()
            : base("A critical system failure occurred.") 
        { 

        }

        public CritSysFailExcep(string message)
            : base(message) 
        { 

        }
        public CritSysFailExcep(string systemName, string message) 
            : base($"CRITICAL FAILURE [{systemName}]: {message}")
        {
            SystemName = systemName;
        }

        public CritSysFailExcep(string systemName, string message, Exception inner)
            : base($"CRITICAL FAILURE [{systemName}]: {message}", inner)
        {
            SystemName = systemName;
        }
    }
}
