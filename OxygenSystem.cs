using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public class OxygenSystem : StationResource
    {
        
        public OxygenSystem(double maxCapacity = 100, double startinglevel = 100)
            : base("Oxygen", maxCapacity, startinglevel)
        {
        }

        public override void CheckStatus()
        {
            if (PercentRemaining <= 0)
            {
                throw new CritSysFailExcep(SystemName,
                    "Oxygen supply conpletely depleted! Life support failure!");
            }
            if (PercentRemaining <= 20)
            {
                OnAlertRaised(new StationAlertEventArgs(SystemName,
                    $"Emergency: Oxygen level critically low at {PercentRemaining: F1}%!",
                    AlertServerity.Emergency,
                    DateTime.Now));
            }
            else if (PercentRemaining <= 40)
            {
                OnAlertRaised(new StationAlertEventArgs(SystemName,
                    $"Warning: Oxygen level reduced to {PercentRemaining: F1}%!.",
                    AlertServerity.Warning,
                    DateTime.Now));
            }
        }
    }
}

