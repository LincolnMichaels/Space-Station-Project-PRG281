using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    /// Central resource manager for the station's life-support and power
    /// systems (Oxygen, Water, Power, Temperature).
    public class StationResourceManager : IAlertSource
    {
        public  OxygenSystem Oxygen { get; private set; }
        public  WaterSystem Water{ get; private set; }
        public  PowerSystem Power{ get; private set; }
        public TemperatureSystem Temperature { get; private set; }

        // Initial settings preserved for Reset operations
        private readonly double _oxygenMax;
        private readonly double _waterMax;
        private readonly double _foodMax;
        private readonly double _powerMax;
        private readonly double _startingCelsius;

        // Fires whenever ANY underlying system raises an alert.
        public event ResourceAlertHandler AlertRaised;

        public StationResourceManager(
            double oxygenMax = 100, double oxygenStart = 100,
            double waterMax = 100, double waterStart = 100,
            double powerMax = 100, double powerStart = 100,
            double startingCelsius = 21.0)
        {
            _oxygenMax = oxygenMax;
            _waterMax = waterMax;
            _powerMax = powerMax;
            _startingCelsius = startingCelsius;

            InitializeSystems(oxygenStart, waterStart, powerStart, startingCelsius);
        }

        private void InitializeSystems(double oxygenStart, double waterStart, double powerStart, double startingCelsius)
        {
            Oxygen = new OxygenSystem(_oxygenMax, oxygenStart);
            Water = new WaterSystem(_waterMax, waterStart);
            Power = new PowerSystem(_powerMax, powerStart);
            Temperature = new TemperatureSystem(startingCelsius);

            // Bubble every system's own AlertRaised up into our single aggregated event.
            Oxygen.AlertRaised += (s, e) => OnAlertRaised(e);
            Water.AlertRaised += (s, e) => OnAlertRaised(e);
            Power.AlertRaised += (s, e) => OnAlertRaised(e);
            Temperature.AlertRaised += (s, e) => OnAlertRaised(e);
        }

      
        /// Resets all underlying life support systems to 100% capacity and initial temperature.
       
        public void Reset()
        {
            InitializeSystems(_oxygenMax, _waterMax, _powerMax, _startingCelsius);
        }
        /// Consume from a named system. Lets InsufficientResourceException
        /// propagate to the caller.
        
        public void Consume(string systemName, double amount) => GetSystem(systemName).Consume(amount);

        public void Replenish(string systemName, double amount) => GetSystem(systemName).Replenish(amount);

        public void AdjustTemperature(double deltaCelsius) => Temperature.Adjust(deltaCelsius);

        
        /// Runs CheckStatus() on every system (call once per simulation tick).
       
        public void CheckStatus()
        {
            CritSysFailExcep firstFailure = null;

            void SafeCheck(Action checkAction)
            {
                try { checkAction();
            }
                catch (CritSysFailExcep ex)
                {
                    if (firstFailure == null) firstFailure = ex;
                }
            }

            SafeCheck(() => Oxygen.CheckStatus());
            SafeCheck(() => Water.CheckStatus());
            SafeCheck(() => Power.CheckStatus());
            SafeCheck(() => Temperature.CheckStatus());

            if (firstFailure != null) throw firstFailure;
        }

        // Convenience percent/value accessors matching Person 3's expectations
        public int OxygenPercent => (int)Math.Round(Oxygen.PercentRemaining);
        public int WaterPercent => (int)Math.Round(Water.PercentRemaining);
        public int PowerPercent => (int)Math.Round(Power.PercentRemaining);
        public double TemperatureCelsius => Temperature.CurrentLevel;

        private StationResource GetSystem(string systemName)
        {
            switch (systemName)
            {
                case "Oxygen": return Oxygen;
                case "Water": return Water;
                case "Power": return Power;
                case "Temperature": return Temperature;
                default:
                    throw new ArgumentException($"Unknown resource system: {systemName}", nameof(systemName));
            }
        }

        protected virtual void OnAlertRaised(StationAlertEventArgs e)
        {
            AlertRaised?.Invoke(this, e);
        }
    }
}