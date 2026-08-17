using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Chris_602473_Prg281_Proj
{
    public class Equipment : StationEntity
    {
        public EquipmentType Type { get; }
        public bool IsOperational { get; set; }
        public StationModule AssignedModule { get; set; }
        public Equipment(string name, EquipmentType type) : base(name)
        {
            Type = type;
            IsOperational = true;
            AssignedModule = null;
        }
        public override string GetDetails()
        {
            return $"Equipment {Name} (Id: {Id}) - Type: {Type}, Operational: {IsOperational}, Assigned Module: {(AssignedModule != null ? AssignedModule.Name : "None")}";
        }
    }
}