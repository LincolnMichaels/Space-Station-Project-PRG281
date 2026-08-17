using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public abstract class StationResource : IResourceConsumer, IAlertSource
    {
        public string SystemName { get; protected set; }
        public double CurrentLevel { get; protected set; }
        public double MaxCapacity { get; protected set; }
        public double ComsumptionRatePerTick { get; protected set; }

        public event ResourceAlertHandler AlertRaised;

        protected StationResource(string systemName, double currentLevel, double maxCapacity)
        {
            SystemName = systemName;
            CurrentLevel = currentLevel;
            MaxCapacity = maxCapacity;
           
        }

        public double PercentRemaining => MaxCapacity > 0 ? (CurrentLevel / MaxCapacity) * 100.0 : 0.0;

        //Comsumes a specifc amount of the resource
        public virtual void Consume(double amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative");

            if (CurrentLevel < amount )
            {
                throw new InsuffResExcep(SystemName, amount, CurrentLevel);
            }

            CurrentLevel -= amount;
        }


        //Replenishes resource up to MaxCapacity
        public virtual void Replenish(double amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative");

            CurrentLevel = Math.Min(MaxCapacity, CurrentLevel + amount);
        }

        //Evaluates resouce level
        public abstract void CheckStatus();

        protected virtual void OnAlertRaised(StationAlertEventArgs e)
        {
            AlertRaised?.Invoke(this, e);
        }


    }
}
