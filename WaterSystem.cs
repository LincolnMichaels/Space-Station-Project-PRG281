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
           : base("Water", maxCapacity, startinglevel)
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
                OnAlertRaised(new StationAlertEventArgs(SystemName,
                    $"Emergency: Water supply critically low at {PercentRemaining: F1}%!",
                    AlertServerity.Emergency,
                    DateTime.Now));
            }
            else if (PercentRemaining <= 35)
            {
                OnAlertRaised(new StationAlertEventArgs(SystemName,
                    $"Warning: Water supply reduced to {PercentRemaining: F1}%!.",
                    AlertServerity.Warning,
                    DateTime.Now));
            }
        }
    }
}
