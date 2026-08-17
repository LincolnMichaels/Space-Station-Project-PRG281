using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public class TemperatureSystem : StationResource
    {
        public TemperatureSystem(double startingCelsius = 21.0)
           : base("Temperature", 50.0, startingCelsius)
        {

        }
        public void Adjust(double deltaCelsius)
        {
            CurrentLevel += deltaCelsius;
        }

        public override void CheckStatus()
        {
            if (CurrentLevel < 0.0 || CurrentLevel > 45.0)
            {
                throw new CritSysFailExcep(SystemName,
                    $"Extreme temperature failure! Current level: {CurrentLevel:F1}°C.");
            }
            if (CurrentLevel <= 10 ||  CurrentLevel > 35.0)
            {
                OnAlertRaised(new StationAlertEventArgs(
                    AlertServerity.Emergency.ToString(),
                    $"Critical Temperature Threshold: Station environment is {CurrentLevel:F1}°C!",
                    SystemName,
                    AlertServerity.Emergency,
                    DateTime.Now));
            }
            else if (CurrentLevel <= 15 || CurrentLevel > 28.0)
            {
                OnAlertRaised(new StationAlertEventArgs(
                    AlertServerity.Warning.ToString(),
                    $"Temperature Warning: Environment reading {CurrentLevel:F1}°C.",
                    SystemName,
                    AlertServerity.Warning,
                    DateTime.Now));
            }
        }
    }
}
