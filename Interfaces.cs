using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    //serverity levels for alerts
    public enum AlertServerity
    {
        Warning,
        Critical,
        Emergency
    }

    //Custom EventArgs carring strustured alert data

    public class StationAlertEventArgs : EventArgs
    {
        public string systemName { get; }
        public string message { get; }
        public AlertServerity serverity { get; }
        public DateTime timeStamp { get; }

        public StationAlertEventArgs(string systemName, string message, AlertServerity serverity, DateTime timeStamp)
        {
            this.systemName = systemName;
            this.message = message;
            this.serverity = serverity;
            this.timeStamp = timeStamp;
        }

        public override string ToString() =>
            $"[{Timestamp:HH:mm:ss}] ({Severity}) {systemName}: {message}";

    }

    public delegate void ResourceAlertHandler(object sender, StationAlertEventArgs e);

    //Interface 1:  behaviour contract for anything that draws down a
    /// finite station resource (oxygen, water, food, power...).
    
    public interface IResourceConsumer
    {
        string SystemName { get; }
        double CurrentLevel { get; }
        double MaxCapacity { get; }
        double ComsumptionRatePerTick { get; }


        //comsume specific amount
        //must throw InsufficcientResouirceExeption
        //if not enough remains
        void Consume(double amount);

        //Replenish up to MaxCapacity
        void Replenish(double amount);

        //Percentage of Capacity remaing, 0-100
        double PercentRemaining { get; }


    }

    //Interface 2: bahaviour contract for anything capable of generating station alerts.

    public interface IAlertSource
    {
        event ResourceAlertHandler AlertRaised;

        //Evaluate current state and raise AlertRaised if thresholds are breached.
        void CheckStatus();
    }




}
