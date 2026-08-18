using System;

namespace Chris_602473_Prg281_Proj
{
    public class OxygenSystem : StationResource
    {

        public OxygenSystem(double maxCapacity = 100, double startinglevel = 100)
            : base("Oxygen", startinglevel, maxCapacity)
        {
        }

        public override void CheckStatus()
        {
            if (PercentRemaining <= 0)
            {
                throw new CritSysFailExcep(
                    SystemName,
                    "Oxygen supply conpletely depleted! Life support failure!");
            }
            if (PercentRemaining <= 20)
            {
                OnAlertRaised(new StationAlertEventArgs(
                    AlertServerity.Emergency.ToString(),
                    $"Emergency: Oxygen level critically low at {PercentRemaining: F1}%!",
                    SystemName,
                    AlertServerity.Emergency,
                    DateTime.Now));
            }
            else if (PercentRemaining <= 40)
            {
                OnAlertRaised(new StationAlertEventArgs(
                    AlertServerity.Warning.ToString(),
                    $"Warning: Oxygen level reduced to {PercentRemaining: F1}%!.",
                    SystemName,
                    AlertServerity.Warning,
                    DateTime.Now));
            }
        }
    }
}

