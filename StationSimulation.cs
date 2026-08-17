using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    // ====================================================================
    // PERSON 3 — Background Operations & Simulation Engine
    // ====================================================================
    //
    // Design intent (so it doesn't fight with Person 2's work later):
    //   - Oxygen/Water/Power/Temperature below are a TEMPORARY stand-in.
    //     Person 2 owns the real resource manager + IResourceConsumer /
    //     IAlertSource interfaces. Once that exists, SimulateOneTick()
    //     should call into it (e.g. resourceManager.Consume(...)) instead
    //     of mutating these fields directly. Everything else (the loop,
    //     threading, events) stays the same. This is the one remaining
    //     placeholder in this file.
    //   - This engine raises its own events (Tick / AlertRaised) rather
    //     than writing to Console directly, so Person 4 can subscribe and
    //     decide how it's displayed/logged, and Person 2's own events
    //     (LowOxygenAlert, PowerFailure, etc.) can coexist independently.
    // ====================================================================

    // Raised whenever the simulation detects a warning/critical condition.

    // Raised on every simulation tick with a snapshot of resource levels.
    
    public class StationSimulation
    {
        private readonly Station _station;
        private readonly Random _rng = new Random();
        private CancellationTokenSource _cts;
        private Task _loopTask;
        private readonly object _stateLock = new object();

        // TODO: Once Person 2's resource manager / IResourceConsumer implementation exists, replace these four fields with reads/writes against that instead. 
        // Kept here so the engine is runnable and demoable on its own right now.
        public int Oxygen { get; private set; } = 100;
        public int Water { get; private set; } = 100;
        public int Power { get; private set; } = 100;
        public double Temperature { get; private set; } = 21.0;

        public bool IsRunning => _loopTask != null && !_loopTask.IsCompleted;

        // Tracks whether each alert is currently "active" so RaiseAlert only and fires once when a threshold is crossed, instead of every tick the condition remains true.
        // See SimulateOneTick for the crossing logic.
        private bool _oxygenWarningActive = false;
        private bool _oxygenCriticalActive = false;
        private bool _powerCriticalActive = false;
        private bool _waterCriticalActive = false;

        // Remembers each astronaut's last-reported 25% health band (keyed by ID)
        private readonly Dictionary<string, int> _lastHealthBand = new Dictionary<string, int>();

        // Fires every tick with a resource snapshot. Person 4 can subscribe for live display.
        public event EventHandler<SimulationTickEventArgs> Tick;

        // Fires when a warning/critical condition is detected.
        public event EventHandler<AlertEventArgs> AlertRaised;

        public StationSimulation(Station station)
        {
            _station = station ?? throw new ArgumentNullException(nameof(station));
        }

        // Starts the background loop. Safe to call if already stopped/never started.
        public void Start()
        {
            if (IsRunning) return;
            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => RunLoop(_cts.Token));
        }

        // Signals the loop to stop after its current tick.
        public void Stop()
        {
            _cts?.Cancel();
        }

        // Resets all tracked values back to their starting defaults.
        // Does not start or stop the loop by itself: see Restart() below.
        public void Reset()
        {
            lock (_stateLock)
            {
                Oxygen = 100;
                Water = 100;
                Power = 100;
                Temperature = 21.0;
                _oxygenWarningActive = false;
                _oxygenCriticalActive = false;
                _powerCriticalActive = false;
                _waterCriticalActive = false;
                _lastHealthBand.Clear();
            }
        }

        // Stops the current run, waits for it to actually finish (Stop() only requests cancellation, doesn't block it), resets values to defaults, then starts a fresh run.
        public void Restart()
        {
            Stop();
            try { _loopTask?.Wait(); } catch { /* expected: task ends via cancellation */ }
            Reset();
            Start();
        }

        private async Task RunLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    SimulateOneTick();
                }
                catch (Exception ex)
                {
                    // A background thread must never take the console down so report and keep looping.
                    Console.WriteLine($"[SIMULATION ERROR] {ex.Message}");
                }

                try
                {
                    await Task.Delay(5000, token); // "every 5 seconds" per the brief
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private void SimulateOneTick()
        {
            int oxygenSnapshot, waterSnapshot, powerSnapshot;
            double tempSnapshot;
            bool powerCriticalThisTick;

            lock (_stateLock)
            {
                // Power depletes on its own, independent of everything else.
                // TODO (Person 2): Water below, and Temperature still have placeholder functionality: same status as Oxygen, pending their real resource manager / IResourceConsumer.
                Power = Clamp(Power - _rng.Next(1, 5));
                powerCriticalThisTick = Power < 15;

                // Oxygen ALWAYS drains at this baseline rate.
                // ADDITIONAL drain on top of the baseline, so oxygen loss accelerates when power is critical. 
                // TODO (Person 2): same as above, swap for the real resource manager once it exists.
                int oxygenDraw = _rng.Next(1, 4); // baseline, always applied
                if (powerCriticalThisTick)
                    oxygenDraw += _rng.Next(2, 5); // extra strain, power loss only
                Oxygen = Clamp(Oxygen - oxygenDraw);

                Water = Clamp(Water - _rng.Next(0, 3));
                Temperature += (_rng.NextDouble() - 0.5);

                oxygenSnapshot = Oxygen;
                waterSnapshot = Water;
                powerSnapshot = Power;
                tempSnapshot = Temperature;
            }

            Tick?.Invoke(this, new SimulationTickEventArgs(oxygenSnapshot, waterSnapshot, powerSnapshot, tempSnapshot));

            // Threshold alerts. Long term this logic (and the exception for over allocation) belongs to Person 2's resource manager. this is a working stand in so the engine is event driven right now.
            // Made edge-triggered below: each alert fires once when the level crosses into a threshold, and once more when it recovers back out of it.
            if (oxygenSnapshot < 20)
            {
                if (!_oxygenCriticalActive)
                {
                    RaiseAlert("CRITICAL", $"Oxygen critically low: {oxygenSnapshot}%");
                    _oxygenCriticalActive = true;
                }
                _oxygenWarningActive = true;
            }
            else if (oxygenSnapshot < 40)
            {
                if (!_oxygenWarningActive)
                    RaiseAlert("WARNING", $"Oxygen level dropping: {oxygenSnapshot}%");
                if (_oxygenCriticalActive)
                    RaiseAlert("OK", $"Oxygen back above critical threshold: {oxygenSnapshot}%");
                _oxygenWarningActive = true;
                _oxygenCriticalActive = false;
            }
            else
            {
                if (_oxygenWarningActive || _oxygenCriticalActive)
                    RaiseAlert("OK", $"Oxygen levels back to normal: {oxygenSnapshot}%");
                _oxygenWarningActive = false;
                _oxygenCriticalActive = false;
            }

            if (powerSnapshot < 15)
            {
                if (!_powerCriticalActive)
                {
                    RaiseAlert("CRITICAL", $"Power failure risk: {powerSnapshot}%");
                    RaiseAlert("WARNING", "Oxygen consumption is increasing due to power failure.");
                    _powerCriticalActive = true;
                }
            }
            else if (_powerCriticalActive)
            {
                RaiseAlert("OK", $"Power levels back to normal: {powerSnapshot}%");
                _powerCriticalActive = false;
            }

            // Water critical, same edge triggered pattern as oxygen/power.
            if (waterSnapshot < 15)
            {
                if (!_waterCriticalActive)
                {
                    RaiseAlert("CRITICAL", $"Water reserves critically low: {waterSnapshot}%");
                    _waterCriticalActive = true;
                }
            }
            else if (_waterCriticalActive)
            {
                RaiseAlert("OK", $"Water reserves back to normal: {waterSnapshot}%");
                _waterCriticalActive = false;
            }

            // While oxygen is critical, all astronauts take damage each tick; if health hits 0 they're marked Deceased.
            // Alerts are made to fire once per 25% lost health per person
            if (oxygenSnapshot < 20)
            {
                var crew = _station.Astronauts?.Where(a => a.Status == Status.Active).ToList();
                if (crew != null)
                {
                    foreach (var astronaut in crew)
                        ApplyHealthDamage(astronaut, _rng.Next(1, 4), "oxygen deprivation");
                }
            }

            // Water depletion is a much slower death than oxygen: smaller
            // damage, and only a chance per tick rather than guaranteed.
            if (waterSnapshot < 15)
            {
                var crew = _station.Astronauts?.Where(a => a.Status == Status.Active).ToList();
                if (crew != null)
                {
                    foreach (var astronaut in crew)
                    {
                        if (_rng.Next(0, 100) < 50) // ~50% chance per tick, vs. guaranteed for oxygen
                            ApplyHealthDamage(astronaut, _rng.Next(1, 3), "dehydration");
                    }
                }
            }

            // Random equipment failure now wired to Equipment.IsOperational
            // (confirmed real property, from Equipement.cs). Picks a random currently-operational item and actually takes it offline, rather than just reporting it.
            if (_rng.Next(0, 100) < 8) // ~8% chance per tick
            {
                var operational = _station.EquipmentList?.Where(e => e.IsOperational).ToList();
                if (operational != null && operational.Count > 0)
                {
                    var failed = operational[_rng.Next(operational.Count)];
                    failed.IsOperational = false;
                    RaiseAlert("CRITICAL", $"Equipment failure: {failed.GetDetails()}");
                }
            }

            // Occasional repair, so the station doesn't just grind down to zero operational equipment over a long-running demo. Same property, opposite direction.
            if (_rng.Next(0, 100) < 4) // ~4% chance per tick
            {
                var broken = _station.EquipmentList?.Where(e => !e.IsOperational).ToList();
                if (broken != null && broken.Count > 0)
                {
                    var repaired = broken[_rng.Next(broken.Count)];
                    repaired.IsOperational = true;
                    RaiseAlert("WARNING", $"Equipment restored to service: {repaired.GetDetails()}");
                }
            }
        }

        private void RaiseAlert(string severity, string message)
        {
            AlertRaised?.Invoke(this, new AlertEventArgs(severity, message));
        }

        private void ApplyHealthDamage(Astronaut astronaut, int damage, string cause)
        {
            astronaut.Health = Math.Max(0, astronaut.Health - damage);
            int band = astronaut.Health / 25;

            if (!_lastHealthBand.TryGetValue(astronaut.Id, out int lastBand) || band < lastBand)
            {
                _lastHealthBand[astronaut.Id] = band;
                RaiseAlert("CRITICAL", $"{astronaut.Name} health dropping ({cause}): {astronaut.Health}%");
            }

            if (astronaut.Health <= 0 && astronaut.Status != Status.Deceased)
            {
                astronaut.Status = Status.Deceased;
                RaiseAlert("CRITICAL", $"{astronaut.Name} has died due to {cause}.");
            }
        }

        private static int Clamp(int value) => Math.Max(0, Math.Min(100, value));
    }
}