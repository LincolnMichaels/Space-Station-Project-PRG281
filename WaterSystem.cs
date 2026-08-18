using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public class WaterSystem : StationResource
    {
        public WaterSystem(double maxCapacity = 100, double startinglevel = 100)
           : base("Water", startinglevel, maxCapacity)
        {
        }

        public override void CheckStatus()
        {
            if (PercentRemaining <= 0)
            {
                throw new CritSysFailExcep(SystemName,
                    "Water supply conpletely depleted!");
            }
            if (PercentRemaining <= 15)
            {
                OnAlertRaised(new StationAlertEventArgs(
                    AlertServerity.Emergency.ToString(),
                    $"Emergency: Water supply critically low at {PercentRemaining: F1}%!",
                    SystemName,
                    AlertServerity.Emergency,
                    DateTime.Now));
            }
            else if (PercentRemaining <= 35)
            {
                OnAlertRaised(new StationAlertEventArgs(
                    AlertServerity.Warning.ToString(),
                    $"Warning: Water supply reduced to {PercentRemaining: F1}%!.",
                    SystemName,
                    AlertServerity.Warning,
                    DateTime.Now));
            }
        }
    }
}
