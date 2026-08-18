using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    class Program
    {
        private static Station station = new Station();
        private static StationResourceManager resourceManager = new StationResourceManager();
        private static StationSimulation simulation = new StationSimulation(station, resourceManager);

        static void Main(string[] args)
        {
            Console.WriteLine("=== SPACE STATION OPERATIONS — Person 1 Test Harness ===\n");

            // === ADDITION START ===
            // Starts running immediately and independently of the menu loop below, per the brief's requirement that background operations must not wait on user input. 
            // The console line here is just a minimal demo hook. 
            // Person 4 should replace this with proper console formatting/logging once that's built.
            simulation.AlertRaised += (s, e) =>
                Console.WriteLine($"\n[{e.Timestamp:HH:mm:ss}] {e.Severity}: {e.Message}");
            simulation.Start();
            // === ADDITION END ===

            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1. Add Astronaut");
                Console.WriteLine("2. Add Module");
                Console.WriteLine("3. Add Equipment");
                Console.WriteLine("4. Assign Astronaut to Module");
                Console.WriteLine("5. Show Crew Information");
                Console.WriteLine("6. Show Station Status");
                Console.WriteLine("7. Show Module Details");
                Console.WriteLine("8. Remove Astronaut (by ID)");
                Console.WriteLine("9. Polymorphism Demo (list of StationEntity)");
                Console.WriteLine("10. Show Live Resource Levels (Simulation)");
                Console.WriteLine("11. Pause Background Simulation");
                Console.WriteLine("12. Resume Background Simulation");
                Console.WriteLine("13. Restart Simulation (reset to defaults)");
                Console.WriteLine("0. Exit");
                Console.Write("Choose: \n\n");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddAstronaut();
                        break;
                    case "2":
                        AddModule();
                        break;
                    case "3":
                        AddEquipment();
                        break;
                    case "4":
                        AssignAstronaut();
                        break;
                    case "5":
                        Console.WriteLine("\n" + station.GetCrewInformation());
                        Console.WriteLine(station.GetCrewStatusSummary());
                        break;
                    case "6":
                        Console.WriteLine("\n" + station.GetStationStatus());
                        break;
                    case "7":
                        ShowModules();
                        break;
                    case "8":
                        RemoveAstronaut();
                        break;
                    case "9":
                        PolymorphismDemo();
                        break;
                    case "10":
                        ShowResourceLevels();
                        break;
                    case "11":
                        simulation.Stop();
                        Console.WriteLine("Simulation paused.");
                        break;
                    case "12":
                        simulation.Start();
                        Console.WriteLine("Simulation resumed.");
                        break;
                    case "13":
                        simulation.Restart();
                        Console.WriteLine("Simulation restarted from defaults.");
                        Console.WriteLine("Simulation resumed.");
                        break;
                    case "0":
                        simulation.Stop();
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("\nPress any Key to continue...");
                    Console.ReadKey();
                }

            }
        }

        static void AddAstronaut()
        {
            Console.Write("Name: ");
            string name = Console.ReadLine();
            Console.Write("Specialization: ");
            string spec = Console.ReadLine();
            try
            {
                var ast = new Astronaut(name, spec);
                station.AddAstronaut(ast);
                Console.WriteLine($"Astronaut added with ID: {ast.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void AddModule()
        {
            Console.Write("Module Name: ");
            string name = Console.ReadLine();
            Console.WriteLine("Module Types: Laboratory, LivingQuarters, DockingBay, ControlCenter, Storage, Medical");
            Console.Write("Type: ");
            string typeStr = Console.ReadLine();
            if (!Enum.TryParse<ModuleType>(typeStr, true, out ModuleType type))
            {
                Console.WriteLine("Invalid type.");
                return;
            }
            Console.Write("Capacity (int): ");
            if (!int.TryParse(Console.ReadLine(), out int capacity))
            {
                Console.WriteLine("Invalid capacity.");
                return;
            }
            try
            {
                var mod = new StationModule(name, type, capacity);
                station.AddModule(mod);
                Console.WriteLine($"Module added with ID: {mod.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void AddEquipment()
        {
            Console.Write("Equipment Name: ");
            string name = Console.ReadLine();
            Console.WriteLine("Equipment Types: Tool, MedicalKit, Communication, LifeSupport, ScienceInstrument");
            Console.Write("Type: ");
            string typeStr = Console.ReadLine();
            if (!Enum.TryParse<EquipmentType>(typeStr, true, out EquipmentType type))
            {
                Console.WriteLine("Invalid type.");
                return;
            }
            try
            {
                var eq = new Equipment(name, type);
                station.AddEquipment(eq);
                Console.WriteLine($"Equipment added with ID: {eq.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void AssignAstronaut()
        {
            Console.Write("Astronaut ID: ");
            string astId = Console.ReadLine();
            Console.Write("Module ID: ");
            string modId = Console.ReadLine();
            bool success = station.AssignAstronautToModule(astId, modId);
            Console.WriteLine(success ? "Assignment successful." : "Assignment failed (check IDs or capacity).");
        }

        static void ShowModules()
        {
            Console.WriteLine("\n=== Modules ===");
            foreach (var mod in station.Modules)
            {
                Console.WriteLine(mod.GetDetails());
                foreach (var ast in mod.AssignedAstronauts)
                {
                    Console.WriteLine($"  -> {ast.Name} ({ast.Specialization})");
                }
            }
        }

        static void RemoveAstronaut()
        {
            Console.Write("Astronaut ID to remove: ");
            string id = Console.ReadLine();
            bool removed = station.RemoveAstronaut(id);
            Console.WriteLine(removed ? "Removed." : "Not found.");
        }

        static void PolymorphismDemo()
        {
            Console.WriteLine("\n=== Polymorphism: List<StationEntity> ===");
            var list = new System.Collections.Generic.List<StationEntity>();
            // Add some existing entities (if any)
            foreach (var a in station.Astronauts) list.Add(a);
            foreach (var m in station.Modules) list.Add(m);
            foreach (var e in station.EquipmentList) list.Add(e);
            if (list.Count == 0)
            {
                Console.WriteLine("No entities to show. Add some first.");
                return;
            }
            foreach (var entity in list)
            {
                Console.WriteLine(entity.GetDetails());
            }
        }

        // Displays the simulation engine's current resource snapshot.
        // Reads whatever the engine currently tracks.
        static void ShowResourceLevels()
        {
            Console.WriteLine("\n=== Live Station Resources (Simulation Engine) ===");
            Console.WriteLine($"Oxygen:      {simulation.Oxygen}%");
            Console.WriteLine($"Water:       {simulation.Water}%");
            Console.WriteLine($"Power:       {simulation.Power}%");
            Console.WriteLine($"Temperature: {simulation.Temperature:F1}\u00b0C");
            Console.WriteLine($"Simulation running: {simulation.IsRunning}");
        }
    }
}