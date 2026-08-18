using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public class PowerSystem : StationResource
    {
        public PowerSystem(double maxCapacity = 100, double startinglevel = 100)
           : base("Power", maxCapacity, startinglevel)
        {

        }

        public override void CheckStatus()
        {
            if (PercentRemaining <= 0)
            {
                throw new CritSysFailExcep(
                    SystemName,
                    "Power supply completely depleted!"
                );
            }

            if (PercentRemaining <= 20)
            {
                OnAlertRaised(
                    new StationAlertEventArgs(
                        AlertServerity.Emergency.ToString(),
                        $"Emergency: Power level critically low at {PercentRemaining:F1}%!",
                        SystemName,
                        AlertServerity.Emergency,
                        DateTime.Now
                    )
                );
            }
            else if (PercentRemaining <= 40)
            {
                OnAlertRaised(
                    new StationAlertEventArgs(
                        AlertServerity.Warning.ToString(),
                        $"Warning: Power level reduced to {PercentRemaining:F1}%!",
                        SystemName,
                        AlertServerity.Warning,
                        DateTime.Now
                    )
                );
            }
        }
    }
}
