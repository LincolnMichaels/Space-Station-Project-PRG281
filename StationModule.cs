using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Chris_602473_Prg281_Proj
{
    public class StationModule : StationEntity
    {
        public ModuleType Type { get; }
        public int Capacity { get; }
        private readonly List<Astronaut> _assignedAstronauts = new List<Astronaut>();
        private readonly List<Equipment> _equipmentList = new List<Equipment>();
        public IReadOnlyList<Astronaut> AssignedAstronauts => _assignedAstronauts.AsReadOnly();
        public IReadOnlyList<Equipment> EquipmentList => _equipmentList.AsReadOnly();
        public int CurrentOccupancy => _assignedAstronauts.Count;
        public StationModule(string name, ModuleType type, int capacity) : base(name)
        {
            Type = type;
            Capacity = capacity;
        }
        // Domain rule: capacity check
        public bool AddAstronaut(Astronaut astronaut)
        {
            if (_assignedAstronauts.Count >= Capacity)
                return false;
            if (astronaut.AssignedModule != null)
                astronaut.AssignedModule.RemoveAstronaut(astronaut);
            _assignedAstronauts.Add(astronaut);
            astronaut.AssignedModule = this;
            return true;
        }
        public bool RemoveAstronaut(Astronaut astronaut)
        {
            if (_assignedAstronauts.Remove(astronaut))
            {
                astronaut.AssignedModule = null;
                return true;
            }
            return false;
        }
        public void AddEquipment(Equipment equipment)
        {
            _equipmentList.Add(equipment);
            equipment.AssignedModule = this;
        }
        public bool RemoveEquipment(Equipment equipment)
        {
            if (_equipmentList.Remove(equipment))
            {
                equipment.AssignedModule = null;
                return true;
            }
            return false;
        }
        public override string GetDetails()
        {
            return $"StationModule {Name} (Id: {Id}) - Type: {Type}, Occupancy: {_assignedAstronauts.Count}/{Capacity}, Equipment count: {_equipmentList.Count}";
        }
    }
}