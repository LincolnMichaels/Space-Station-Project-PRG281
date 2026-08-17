using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Chris_602473_Prg281_Proj
{
    public class Station
    {
        private readonly List<Astronaut> _astronauts = new List<Astronaut>();
        private readonly List<StationModule> _modules = new List<StationModule>();
        private readonly List<Equipment> _equipment = new List<Equipment>();

        public IReadOnlyList<Astronaut> Astronauts => _astronauts.AsReadOnly();
        public IReadOnlyList<StationModule> Modules => _modules.AsReadOnly();
        public IReadOnlyList<Equipment> EquipmentList => _equipment.AsReadOnly();

        // Astronaut CRUD
        public void AddAstronaut(Astronaut astronaut)
        {
            if (_astronauts.Any(a => a.Id == astronaut.Id))
                throw new InvalidOperationException($"Astronaut with ID {astronaut.Id} already exists.");
            _astronauts.Add(astronaut);
        }

        public bool RemoveAstronaut(string astronautId)
        {
            var astronaut = _astronauts.FirstOrDefault(a => a.Id == astronautId);
            if (astronaut == null) return false;
            if (astronaut.AssignedModule != null)
                astronaut.AssignedModule.RemoveAstronaut(astronaut);
            return _astronauts.Remove(astronaut);
        }

        public Astronaut GetAstronaut(string id) => _astronauts.FirstOrDefault(a => a.Id == id);

        // Module CRUD
        public void AddModule(StationModule module)
        {
            if (_modules.Any(m => m.Id == module.Id))
                throw new InvalidOperationException($"Module with ID {module.Id} already exists.");
            _modules.Add(module);
        }

        public bool RemoveModule(string moduleId)
        {
            var module = _modules.FirstOrDefault(m => m.Id == moduleId);
            if (module == null) return false;
            foreach (var ast in module.AssignedAstronauts.ToList())
                module.RemoveAstronaut(ast);
            foreach (var eq in module.EquipmentList.ToList())
                module.RemoveEquipment(eq);
            return _modules.Remove(module);
        }

        public StationModule GetModule(string id) => _modules.FirstOrDefault(m => m.Id == id);

        // Equipment CRUD
        public void AddEquipment(Equipment equipment)
        {
            if (_equipment.Any(e => e.Id == equipment.Id))
                throw new InvalidOperationException($"Equipment with ID {equipment.Id} already exists.");
            _equipment.Add(equipment);
        }

        public bool RemoveEquipment(string equipmentId)
        {
            var eq = _equipment.FirstOrDefault(e => e.Id == equipmentId);
            if (eq == null) return false;
            if (eq.AssignedModule != null)
                eq.AssignedModule.RemoveEquipment(eq);
            return _equipment.Remove(eq);
        }

        public Equipment GetEquipment(string id) => _equipment.FirstOrDefault(e => e.Id == id);

        // Assignment
        public bool AssignAstronautToModule(string astronautId, string moduleId)
        {
            var astronaut = GetAstronaut(astronautId);
            var module = GetModule(moduleId);
            if (astronaut == null || module == null) return false;
            return module.AddAstronaut(astronaut);
        }

        // Info methods
        public string GetCrewInformation()
        {
            if (_astronauts.Count == 0) return "No astronauts on station.";
            return string.Join(Environment.NewLine, _astronauts.Select(a => a.GetDetails()));
        }

        public string GetCrewStatusSummary()
        {
            int active = _astronauts.Count(a => a.Status == Status.Active);
            int inactive = _astronauts.Count(a => a.Status == Status.Inactive);
            int deceased = _astronauts.Count(a => a.Status == Status.Deceased);
            int offDuty = _astronauts.Count(a => a.Status == Status.OffDuty);
            // double avgHealth = _astronauts.Count > 0 ? _astronauts.Average(a => a.Health) : 0;  <-- Made it so it doesnt factor in the deceased. if this is preferred, remove the two lines below.
            var living = _astronauts.Where(a => a.Status != Status.Deceased).ToList();
            double avgHealth = living.Count > 0 ? living.Average(a => a.Health) : 0;
            // ^ These 2
            return $"Crew Status: Active: {active}, Inactive: {inactive}, Deceased: {deceased}, OffDuty: {offDuty}, Average Health: {avgHealth:F1}%";
        }

        public string GetStationStatus()
        {
            int totalModules = _modules.Count;
            int totalCapacity = _modules.Sum(m => m.Capacity);
            int totalOccupancy = _modules.Sum(m => m.CurrentOccupancy);
            int totalEquipment = _equipment.Count;
            int operationalEquipment = _equipment.Count(e => e.IsOperational);
            return $"Station Status: Modules: {totalModules}, Occupancy: {totalOccupancy}/{totalCapacity}, Equipment: {operationalEquipment}/{totalEquipment} operational.";
        }
    }
}