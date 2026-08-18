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

        private readonly StationResourceManager _resources;
        public int Oxygen => _resources.OxygenPercent;
        public int Water => _resources.WaterPercent;
        public int Power => _resources.PowerPercent;
        public double Temperature => _resources.TemperatureCelsius;

        public bool IsRunning => _loopTask != null && !_loopTask.IsCompleted;

        // Tracks whether the alert is currently "active" so RaiseAlert only and fires once when a threshold is crossed, instead of every tick the condition remains true.
        // See SimulateOneTick for the crossing logic.
        private bool _powerCriticalActive = false;

        private bool _waterCriticalActive = false;

        private readonly HashSet<string> _powerShutdownEquipmentIds = new HashSet<string>();
        private readonly HashSet<string> _waterShutdownEquipmentIds = new HashSet<string>();

        // Remembers each astronaut's last-reported 25% health band (keyed by ID).
        private readonly Dictionary<string, int> _lastHealthBand = new Dictionary<string, int>();

        // Fires every tick with a resource snapshot. Person 4 can subscribe for live display.
        public event EventHandler<SimulationTickEventArgs> Tick;

        // Fires when a warning/critical condition is detected.
        public event EventHandler<AlertEventArgs> AlertRaised;

        public StationSimulation(Station station, StationResourceManager resources)
        {
            _station = station ?? throw new ArgumentNullException(nameof(station));
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));

            _resources.AlertRaised += (s, e) => AlertRaised?.Invoke(this, e);
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
                _resources.Reset();
                _powerCriticalActive = false;
                _waterCriticalActive = false;
                _powerShutdownEquipmentIds.Clear();
                _waterShutdownEquipmentIds.Clear();
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
                    await Task.Delay(5000, token); // "Every 5 seconds" per the brief.
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
            bool powerCriticalThisTick = false;

            // Consumption goes through Person 2's Consume() calls.
            // Consume() throws InsuffResExcep if the draw exceeds what's left, which would fire every single tick once a resource nears 0 so each draw is clamped to what's actually available first.
            // The try/catch below is for the genuinely abnormal case (a draw request larger than what clamping accounts for), not for routine depletion.
            try
            {
                lock (_stateLock)
                {
                    // Power depletes on its own, independent of everything else.
                    double powerDraw = Math.Min(_resources.Power.CurrentLevel, _rng.Next(1, 5));
                    if (powerDraw > 0) _resources.Power.Consume(powerDraw);
                    powerCriticalThisTick = _resources.Power.PercentRemaining < 15;

                    // Oxygen always drains at this baseline rate.
                    // Additional drain on top of the baseline when power is critical, so oxygen loss accelerates rather than only happening when power is out.
                    double oxygenDraw = _rng.Next(1, 4); // Baseline, always applied.
                    if (powerCriticalThisTick)
                        oxygenDraw += _rng.Next(2, 5); // Extra strain, power loss only.
                    oxygenDraw = Math.Min(_resources.Oxygen.CurrentLevel, oxygenDraw);
                    if (oxygenDraw > 0) _resources.Oxygen.Consume(oxygenDraw);

                    double waterDraw = Math.Min(_resources.Water.CurrentLevel, _rng.Next(0, 3));
                    if (waterDraw > 0) _resources.Water.Consume(waterDraw);

                    _resources.AdjustTemperature(_rng.NextDouble() - 0.5);
                }
            }
            catch (InsuffResExcep ex)
            {
                RaiseAlert("CRITICAL", $"Resource draw failed during simulation: {ex.Message}");
            }

            // Lets each system evaluate itself and raise its own alerts (forwarded to our AlertRaised via the subscription set up in the constructor). 
            // CheckStatus() can throw CritSysFailExcep if a system is fully depleted / out of safe range.
            try
            {
                _resources.CheckStatus();
            }
            catch (CritSysFailExcep ex)
            {
                RaiseAlert("CRITICAL", ex.Message);
            }

            oxygenSnapshot = Oxygen;
            waterSnapshot = Water;
            powerSnapshot = Power;
            tempSnapshot = Temperature;

            Tick?.Invoke(this, new SimulationTickEventArgs(oxygenSnapshot, waterSnapshot, powerSnapshot, tempSnapshot));

            if (powerCriticalThisTick && !_powerCriticalActive)
            {
                RaiseAlert("WARNING", "Oxygen consumption is increasing due to power failure.");
                ShutdownEquipment(_powerShutdownEquipmentIds, "power failure", e => true);
            }
            else if (!powerCriticalThisTick && _powerCriticalActive)
            {
                RestoreEquipment(_powerShutdownEquipmentIds, "power restored");
            }
            _powerCriticalActive = powerCriticalThisTick;

            // While oxygen is critical, all astronauts take damage each tick; if health hits 0 they're marked Deceased.
            // Alerts are made to fire once per 25% lost health per person.
            if (oxygenSnapshot < 20)
            {
                var crew = _station.Astronauts?.Where(a => a.Status == Status.Active).ToList();
                if (crew != null)
                {
                    foreach (var astronaut in crew)
                        ApplyHealthDamage(astronaut, _rng.Next(1, 4), "oxygen deprivation");
                }
            }

            // Water depletion is a much slower death than oxygen: smaller damage, and only a chance per tick rather than guaranteed.
            if (waterSnapshot < 15)
            {
                var crew = _station.Astronauts?.Where(a => a.Status == Status.Active).ToList();
                if (crew != null)
                {
                    foreach (var astronaut in crew)
                    {
                        if (_rng.Next(0, 100) < 50) // ~50% Chance per tick, vs guaranteed for oxygen.
                            ApplyHealthDamage(astronaut, _rng.Next(1, 3), "dehydration");
                    }
                }
            }

            // Same pattern for water, but scoped to LifeSupport equipment only so water shortage plausibly affects life-support-related systems specifically.
            bool waterCriticalThisTick = waterSnapshot < 15;
            if (waterCriticalThisTick && !_waterCriticalActive)
            {
                ShutdownEquipment(_waterShutdownEquipmentIds, "water shortage", e => e.Type == EquipmentType.LifeSupport);
            }
            else if (!waterCriticalThisTick && _waterCriticalActive)
            {
                RestoreEquipment(_waterShutdownEquipmentIds, "water restored");
            }
            _waterCriticalActive = waterCriticalThisTick;

            // Random equipment failure now wired to Equipment.IsOperational
            // (confirmed real property, from Equipement.cs). Picks a random currently-operational item and actually takes it offline, rather than just reporting it.
            if (_rng.Next(0, 100) < 8) // ~8% Chance per tick.
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
            if (_rng.Next(0, 100) < 4) // ~4% Chance per tick.
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

        // Shuts down roughly half of currently operational equipment that matches `eligible` (e.g. "all of it" for power, "LifeSupport only" for water), and remembers which items it touched in `tracker` so RestoreEquipment can bring back exactly those items later but not anything the random failure mechanic separately took offline.
        private void ShutdownEquipment(HashSet<string> tracker, string causeLabel, Func<Equipment, bool> eligible)
        {
            var candidates = _station.EquipmentList?.Where(e => e.IsOperational && eligible(e)).ToList();
            if (candidates == null || candidates.Count == 0) return;

            int toShutDown = Math.Max(1, (candidates.Count + 1) / 2); // Roughly half, at least one.
            var chosen = candidates.OrderBy(_ => _rng.Next()).Take(toShutDown).ToList();

            foreach (var eq in chosen)
            {
                eq.IsOperational = false;
                tracker.Add(eq.Id);
            }

            RaiseAlert("CRITICAL", $"{chosen.Count} equipment unit(s) shut down due to {causeLabel}.");
        }

        // Restores exactly the equipment this mechanism shut down (not equipment the random failure/repair cycle separately touched), then clears the tracker.
        private void RestoreEquipment(HashSet<string> tracker, string reason)
        {
            if (tracker.Count == 0) return;

            var restored = _station.EquipmentList?.Where(e => tracker.Contains(e.Id)).ToList();
            if (restored != null)
            {
                foreach (var eq in restored)
                    eq.IsOperational = true;

                if (restored.Count > 0)
                    RaiseAlert("OK", $"{restored.Count} equipment unit(s) back online — {reason}.");
            }

            tracker.Clear();
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

                astronaut.AssignedModule?.RemoveAstronaut(astronaut);
            }
        }
    }
}