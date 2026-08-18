using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public class ConsoleMenu
    {
        private readonly Station _station;
        private readonly StationResourceManager _resources;
        private readonly StationSimulation _simulation;

        private bool _running;

        // Stores background alerts so they can be displayed safely
        // by the console without allowing background threads to
        // interfere with user input.
        private readonly Queue<string> _pendingAlerts = new Queue<string>();
        private readonly object _alertLock = new object();

        public ConsoleMenu(Station station, StationResourceManager resources, StationSimulation simulation)
        {
            _station = station;
            _resources = resources;
            _simulation = simulation;

            // Person 3's simulation events.
            _simulation.AlertRaised += OnSimulationAlert;
            _simulation.Tick += OnSimulationTick;

            // Person 2's resource manager event.
            _resources.AlertRaised += OnResourceAlert;
        }

        public void Run()
        {
            _running = true;

            SystemLogger.Log("Space Station Operations Console started.");
            _simulation.Start();

            while (_running)
            {
                DisplayMainMenu();
                DisplayPendingAlerts();

                string choice =InputHandler.GetString("Enter option: ");

                try
                {
                    HandleMenuChoice(choice);
                }
                catch (InsuffResExcep ex)
                {
                    DisplayError("Insufficient resource: " + ex.Message);
                    SystemLogger.Log("RESOURCE ERROR: " + ex.Message);
                }
                catch (CritSysFailExcep ex)
                {
                    DisplayError("Critical system failure: " + ex.Message);
                    SystemLogger.Log("CRITICAL SYSTEM FAILURE: " + ex.Message);
                }
                catch (ArgumentException ex)
                {
                    DisplayError(ex.Message);
                    SystemLogger.Log("ARGUMENT ERROR: " + ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    DisplayError(ex.Message);
                    SystemLogger.Log("INVALID OPERATION: " + ex.Message);
                }
                catch (Exception ex)
                {
                    DisplayError("Unexpected system error: " + ex.Message);
                    SystemLogger.Log("UNEXPECTED ERROR: " + ex.Message);
                }

                if (_running)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press ENTER to continue...");
                    Console.ReadLine();
                }
            }
        }
        //Main Menu
        private void DisplayMainMenu()
        {
            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("             SPACE STATION OPERATIONS CONSOLE");
            Console.WriteLine("============================================================");
            Console.WriteLine();

            DisplayQuickStatus();

            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("OPERATIONS");
            Console.WriteLine("------------------------------------------------------------");

            Console.WriteLine("1. Crew Management");
            Console.WriteLine("2. Station Module Management");
            Console.WriteLine("3. Equipment Management");
            Console.WriteLine("4. Resource Management");
            Console.WriteLine("5. Emergency Management");
            Console.WriteLine("6. Full Station Status");
            Console.WriteLine("7. Polymorphism / Entity Overview");
            Console.WriteLine("8. System Logs");
            Console.WriteLine("9. Pause Background Simulation");
            Console.WriteLine("10. Resume Background Simulation");
            Console.WriteLine("11. Restart Simulation");
            Console.WriteLine("0. Shutdown Console");
            Console.WriteLine();
        }

        private void DisplayQuickStatus()
        {
            Console.WriteLine("SYSTEM STATUS");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Simulation : " + (_simulation.IsRunning ? "RUNNING" : "PAUSED"));
            Console.WriteLine("Crew       : " + _station.Astronauts.Count);
            Console.WriteLine("Modules    : " + _station.Modules.Count);
            Console.WriteLine("Equipment  : " + _station.EquipmentList.Count);
            Console.WriteLine("Oxygen     : " + _resources.OxygenPercent + "%");
            Console.WriteLine("Water      : " + _resources.WaterPercent + "%");
            Console.WriteLine("Power      : " + _resources.PowerPercent + "%");
            Console.WriteLine("Temperature: " + _resources.TemperatureCelsius.ToString("F1") + " C");
        }

        private void HandleMenuChoice(string choice)
        {
            switch (choice)
            {
                case "1":
                    CrewManagement();
                    break;

                case "2":
                    ModuleManagement();
                    break;

                case "3":
                    EquipmentManagement();
                    break;

                case "4":
                    ResourceManagement();
                    break;

                case "5":
                    EmergencyManagement();
                    break;

                case "6":
                    DisplayStationStatus();
                    break;

                case "7":
                    PolymorphismDemo();
                    break;

                case "8":
                    SystemLogger.ShowLogs();
                    break;

                case "9":
                    PauseSimulation();
                    break;

                case "10":
                    ResumeSimulation();
                    break;

                case "11":
                    RestartSimulation();
                    break;

                case "0":
                    Shutdown();
                    break;

                default:
                    Console.WriteLine(
                        "Invalid option. Please choose a valid menu option."
                    );
                    break;
            }
        }

        // CREW MANAGEMENT
        private void CrewManagement()
        {
            Console.Clear();

            Console.WriteLine("============================================================");
            Console.WriteLine("                       CREW MANAGEMENT");
            Console.WriteLine("============================================================");

            Console.WriteLine();

            Console.WriteLine("1. Add Astronaut");
            Console.WriteLine("2. Remove Astronaut");
            Console.WriteLine("3. View Crew");
            Console.WriteLine("4. Assign Astronaut to Module");
            Console.WriteLine("5. Return");

            Console.WriteLine();
            string choice = InputHandler.GetString("Choose option: ");

            switch (choice)
            {
                case "1":
                    AddAstronaut();
                    break;

                case "2":
                    RemoveAstronaut();
                    break;

                case "3":
                    ViewCrew();
                    break;

                case "4":
                    AssignAstronaut();
                    break;

                case "5":
                    return;

                default:
                    Console.WriteLine(
                        "Invalid option."
                    );
                    break;
            }
        }

        private void AddAstronaut()
        {
            string name = InputHandler.GetString("Astronaut name: ");
            string specialization = InputHandler.GetString("Specialization: ");

            Astronaut astronaut = new Astronaut(name, specialization);
            _station.AddAstronaut(astronaut);

            Console.WriteLine();
            Console.WriteLine("Astronaut successfully added.");
            Console.WriteLine("Astronaut ID: " +astronaut.Id);
            SystemLogger.Log("Astronaut added: " + astronaut.Name + " (" + astronaut.Id + ")");
        }

        private void RemoveAstronaut()
        {
            string id = InputHandler.GetString("Astronaut ID: ");

            Astronaut astronaut = _station.GetAstronaut(id);

            if (astronaut == null)
            {
                Console.WriteLine("Astronaut not found.");
                return;
            }

            bool removed =_station.RemoveAstronaut(id);
            if (removed)
            {
                Console.WriteLine("Astronaut removed successfully.");
                SystemLogger.Log("Astronaut removed: " + astronaut.Name + " (" + id + ")");
            }
        }

        private void ViewCrew()
        {
            Console.WriteLine();
            Console.WriteLine(_station.GetCrewInformation());

            Console.WriteLine();
            Console.WriteLine(_station.GetCrewStatusSummary());
        }

        private void AssignAstronaut()
        {
            string astronautId = InputHandler.GetString("Astronaut ID: ");
            string moduleId = InputHandler.GetString("Module ID: ");

            bool success = _station.AssignAstronautToModule(astronautId, moduleId);
            if (success)
            {
                Console.WriteLine("Astronaut successfully assigned.");
                SystemLogger.Log("Astronaut " + astronautId + " assigned to module " + moduleId);
            }
            else
            {
                Console.WriteLine("Assignment failed. Check the IDs or module capacity.");
            }
        }

        // MODULE MANAGEMENT
        private void ModuleManagement()
        {
            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("                 STATION MODULE MANAGEMENT");
            Console.WriteLine("============================================================");
            Console.WriteLine();

            Console.WriteLine("1. Add Module");
            Console.WriteLine("2. Remove Module");
            Console.WriteLine("3. View Modules");
            Console.WriteLine("4. Return");

            string choice = InputHandler.GetString("Choose option: ");
            switch (choice)
            {
                case "1":
                    AddModule();
                    break;

                case "2":
                    RemoveModule();
                    break;

                case "3":
                    ShowModules();
                    break;

                case "4":
                    return;

                default:
                    Console.WriteLine(
                        "Invalid option."
                    );
                    break;
            }
        }

        private void AddModule()
        {
            string name = InputHandler.GetString("Module name: ");

            Console.WriteLine();
            Console.WriteLine("Available Module Types:");

            foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
            {
            Console.WriteLine("- " + type);
            }

            string typeInput = InputHandler.GetString("Module type: ");

            ModuleType moduleType;

            if (!Enum.TryParse(typeInput, true, out moduleType))
            {
                Console.WriteLine("Invalid module type.");
                return;
            }

            int capacity = InputHandler.GetPositiveInt("Capacity: ");

            StationModule module = new StationModule(name, moduleType, capacity);
            _station.AddModule(module);

            Console.WriteLine();
            Console.WriteLine("Module successfully added.");
            Console.WriteLine("Module ID: " + module.Id);
            SystemLogger.Log("Module added: " + module.Name +" (" + module.Id + ")");
        }

        private void RemoveModule()
        {
            string id =InputHandler.GetString("Module ID: ");
            StationModule module = _station.GetModule(id);

            if (module == null)
            {
                Console.WriteLine("Module not found.");
                return;
            }

            bool removed =_station.RemoveModule(id);
            if (removed)
            {
                Console.WriteLine("Module removed successfully.");
                SystemLogger.Log("Module removed: " + module.Name + " (" + id + ")");
            }
        }

        private void ShowModules()
        {
            Console.WriteLine();

            if (_station.Modules.Count == 0)
            {
                Console.WriteLine("No station modules currently exist.");
                return;
            }

            foreach (StationModule module in _station.Modules)
            {
                Console.WriteLine(module.GetDetails());

                foreach (Astronaut astronaut in module.AssignedAstronauts)
                {
                    Console.WriteLine("  Astronaut: " + astronaut.Name + " (" + astronaut.Id + ")");
                }

                foreach (Equipment equipment in module.EquipmentList)
                {
                    Console.WriteLine("  Equipment: " + equipment.Name + " (" + equipment.Id + ")");
                }

                Console.WriteLine();
            }
        }

        // EQUIPMENT MANAGEMENT
        private void EquipmentManagement()
        {
            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("                   EQUIPMENT MANAGEMENT");
            Console.WriteLine("============================================================");

            Console.WriteLine();

            Console.WriteLine("1. Add Equipment");
            Console.WriteLine("2. Remove Equipment");
            Console.WriteLine("3. View Equipment");
            Console.WriteLine("4. Assign Equipment to Module");
            Console.WriteLine("5. Return");

            string choice = InputHandler.GetString("Choose option: ");
            switch (choice)
            {
                case "1":
                    AddEquipment();
                    break;

                case "2":
                    RemoveEquipment();
                    break;

                case "3":
                    ViewEquipment();
                    break;

                case "4":
                    AssignEquipment();
                    break;

                case "5":
                    return;

                default:
                    Console.WriteLine(
                        "Invalid option."
                    );
                    break;
            }
        }

        private void AddEquipment()
        {
            string name = InputHandler.GetString("Equipment name: ");

            Console.WriteLine();
            Console.WriteLine("Available Equipment Types:");

            foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
            {
                Console.WriteLine("- " + type);
            }

            string typeInput = InputHandler.GetString("Equipment type: ");

            EquipmentType equipmentType;

            if (!Enum.TryParse(typeInput, true, out equipmentType))
            {
                Console.WriteLine("Invalid equipment type.");
                return;
            }

            Equipment equipment = new Equipment(name, equipmentType);
            _station.AddEquipment(equipment);

            Console.WriteLine();
            Console.WriteLine("Equipment successfully added.");
            Console.WriteLine("Equipment ID: " + equipment.Id);
            SystemLogger.Log("Equipment added: " + equipment.Name + " (" + equipment.Id + ")");
        }

        private void RemoveEquipment()
        {
            string id = InputHandler.GetString("Equipment ID: ");

            Equipment equipment = _station.GetEquipment(id);
            if (equipment == null)
            {
                Console.WriteLine("Equipment not found.");
                return;
            }

            bool removed = _station.RemoveEquipment(id);
            if (removed)
            {
                Console.WriteLine("Equipment removed successfully.");
                SystemLogger.Log("Equipment removed: " + equipment.Name + " (" + id + ")");
            }
        }

        private void ViewEquipment()
        {
            Console.WriteLine();

            if (_station.EquipmentList.Count == 0)
            {
                Console.WriteLine("No equipment currently exists.");
                return;
            }

            foreach (Equipment equipment in _station.EquipmentList)
            {
                Console.WriteLine(equipment.GetDetails());
            }
        }

        private void AssignEquipment()
        {
            string equipmentId = InputHandler.GetString("Equipment ID: ");
            string moduleId = InputHandler.GetString("Module ID: ");

            bool success = _station.AssignEquipmentToModule(equipmentId, moduleId);
            if (success)
            {
                Console.WriteLine("Equipment successfully assigned.");
                SystemLogger.Log("Equipment " + equipmentId + " assigned to module " + moduleId);
            }
            else
            {
                Console.WriteLine("Assignment failed. Check the IDs.");
            }
        }

        // RESOURCE MANAGEMENT
        private void ResourceManagement()
        {
            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("                    RESOURCE MANAGEMENT");
            Console.WriteLine("============================================================");

            Console.WriteLine();
            DisplayResources();
            Console.WriteLine();

            Console.WriteLine("1. Consume Resource");
            Console.WriteLine("2. Replenish Resource");
            Console.WriteLine("3. Adjust Temperature");
            Console.WriteLine("4. Check Resource Status");
            Console.WriteLine("5. Reset Resources");
            Console.WriteLine("6. Return");

            string choice = InputHandler.GetString("Choose option: ");
            switch (choice)
            {
                case "1":
                    ConsumeResource();
                    break;

                case "2":
                    ReplenishResource();
                    break;

                case "3":
                    AdjustTemperature();
                    break;

                case "4":
                    _resources.CheckStatus();
                    Console.WriteLine(
                        "All resource systems are within operating limits."
                    );
                    break;

                case "5":
                    _resources.Reset();

                    Console.WriteLine(
                        "Resources reset to their starting values."
                    );

                    SystemLogger.Log(
                        "Resource systems reset by operator."
                    );

                    break;

                case "6":
                    return;

                default:
                    Console.WriteLine(
                        "Invalid option."
                    );
                    break;
            }
        }

        private void DisplayResources()
        {
            Console.WriteLine("Oxygen      : " + _resources.OxygenPercent + "%");
            Console.WriteLine("Water       : " + _resources.WaterPercent + "%");
            Console.WriteLine("Power       : " + _resources.PowerPercent + "%");
            Console.WriteLine("Temperature : " + _resources.TemperatureCelsius.ToString("F1") + " C");
        }

        private string GetResourceName()
        {
            Console.WriteLine();
            Console.WriteLine("Available resources:");
            Console.WriteLine("1. Oxygen");
            Console.WriteLine("2. Water");
            Console.WriteLine("3. Power"); 
             
            string choice = InputHandler.GetString("Choose resource: ");
            switch (choice)
            {
                case "1":
                    return "Oxygen";

                case "2":
                    return "Water";

                case "3":
                    return "Power";

                default:
                    throw new ArgumentException(
                        "Invalid resource selection."
                    );
            }
        }

        private void ConsumeResource()
        {
            string resource = GetResourceName();
            double amount = InputHandler.GetPositiveDouble("Amount to consume: ");
            _resources.Consume(resource, amount);

            Console.WriteLine(amount.ToString("F1") + "% of " + resource + " consumed.");
            SystemLogger.Log("Resource consumed: " + amount.ToString("F1") + " units of " + resource);
        }

        private void ReplenishResource()
        {
            string resource = GetResourceName();
            double amount = InputHandler.GetPositiveDouble("Amount to replenish: ");
            _resources.Replenish(resource, amount);

            Console.WriteLine(resource + " replenished.");
            SystemLogger.Log( "Resource replenished: " + amount.ToString("F1") + " units of " + resource);
        }

        private void AdjustTemperature()
        {
            double amount = InputHandler.GetDouble("Temperature change (+/- C): ");
            _resources.AdjustTemperature(amount);

            Console.WriteLine("Temperature is now " + _resources.TemperatureCelsius.ToString("F1") + " C");
            SystemLogger.Log("Temperature adjusted by " + amount.ToString("F1") + " C");
        }


        // EMERGENCY MANAGEMENT
        private void EmergencyManagement()
        {
            Console.Clear();

            Console.WriteLine("============================================================");
            Console.WriteLine("                   EMERGENCY MANAGEMENT");
            Console.WriteLine("============================================================");

            Console.WriteLine();
            DisplayResources();
            Console.WriteLine();

            Console.WriteLine("The background simulation automatically monitors");
            Console.WriteLine("station systems and generates alerts when thresholds");
            Console.WriteLine("are exceeded.");

            Console.WriteLine();
            DisplayPendingAlerts();
        }

        // STATION STATUS
        private void DisplayStationStatus()
        {
            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("                    FULL STATION STATUS");
            Console.WriteLine("============================================================");

            Console.WriteLine();

            Console.WriteLine("SIMULATION");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Status: " + (_simulation.IsRunning ? "RUNNING" : "PAUSED"));

            Console.WriteLine();

            Console.WriteLine("CREW");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Astronauts: " + _station.Astronauts.Count);
            Console.WriteLine(_station.GetCrewStatusSummary());

            Console.WriteLine();

            Console.WriteLine("RESOURCES");
            Console.WriteLine("------------------------------------------------------------");
            DisplayResources();

            Console.WriteLine();
            Console.WriteLine("STATION");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine(_station.GetStationStatus());

            Console.WriteLine();
            DisplayPendingAlerts();
        }


        // POLYMORPHISM
        private void PolymorphismDemo()
        {
            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("             POLYMORPHISM / ENTITY OVERVIEW");
            Console.WriteLine("============================================================");

            List<StationEntity> entities =
                new List<StationEntity>();

            foreach (Astronaut astronaut in _station.Astronauts)
            {
                entities.Add(astronaut);
            }

            foreach (StationModule module in _station.Modules)
            {
                entities.Add(module);
            }

            foreach (Equipment equipment in _station.EquipmentList)
            {
                entities.Add(equipment);
            }

            if (entities.Count == 0)
            {
                Console.WriteLine("No station entities currently exist.");

                return;
            }

            foreach (StationEntity entity in entities)
            {
                // Polymorphism: the overridden GetDetails()
                // method of each derived class is called.
                Console.WriteLine(entity.GetDetails());
            }
        }


        // SIMULATION CONTROLS       
        private void PauseSimulation()
        {
            _simulation.Stop();

            Console.WriteLine("Background simulation pause requested.");

            SystemLogger.Log("Background simulation paused by operator.");
        }

        private void ResumeSimulation()
        {
            _simulation.Start();

            Console.WriteLine("Background simulation resumed.");

            SystemLogger.Log("Background simulation resumed by operator.");
        }

        private void RestartSimulation()
        {
            _simulation.Restart();

            Console.WriteLine("Background simulation restarted.");

            SystemLogger.Log("Background simulation restarted by operator.");
        }

        // EVENTS
        private void OnSimulationAlert(object sender, AlertEventArgs e)
        {
            string alert = "[" + e.Timestamp.ToString("HH:mm:ss") + "] " + e.Severity + ": " + e.Message;

            lock (_alertLock)
            {
                _pendingAlerts.Enqueue(alert);
            }

            SystemLogger.Log("SIMULATION ALERT: " + e.Severity + " - " + e.Message);
        }

        private void OnResourceAlert(object sender, AlertEventArgs e)
        {
            string alert = "[" + e.Timestamp.ToString("HH:mm:ss") + "] RESOURCE " + e.Severity + ": " + e.Message;

            lock (_alertLock)
            {
                _pendingAlerts.Enqueue(alert);
            }

            SystemLogger.Log("RESOURCE ALERT: " + e.Severity + " - " + e.Message);
        }

        private void OnSimulationTick(
            object sender,
            SimulationTickEventArgs e)
        {
            // Person 3's Tick event provides a snapshot of the
            // station resource levels on every background tick.
            //
            // Person 4 does not write directly to the console here
            // because this event is raised from a background task.
            // The main console reads the current resource manager
            // values when displaying the station status.
        }

        private void DisplayPendingAlerts()
        {
            lock (_alertLock)
            {
                if (_pendingAlerts.Count == 0)
                {
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("------------------------------------------------------------");
                Console.WriteLine("                       ACTIVE ALERTS");
                Console.WriteLine("------------------------------------------------------------");

                while (_pendingAlerts.Count > 0)
                {
                    Console.WriteLine(_pendingAlerts.Dequeue());
                }
            }
        }

       
        // ERROR HANDLING
        private void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine();
            Console.WriteLine("!!! SYSTEM ERROR !!!");

            Console.WriteLine(message);

            Console.ResetColor();
        }

        
        // SHUTDOWN
        private void Shutdown()
        {
            Console.WriteLine();

            bool confirm = InputHandler.GetYesNo("Are you sure you want to shut down?");

            if (!confirm)
            {
                return;
            }

            _simulation.Stop();

            SystemLogger.Log("Space Station Operations Console shut down.");

            Console.WriteLine();
            Console.WriteLine("Background simulation stopped.");

            Console.WriteLine("Space Station Operations Console shutting down...");

            _running = false;
        }
    }
}

