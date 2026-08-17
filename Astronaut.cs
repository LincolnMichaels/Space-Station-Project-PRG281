using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Chris_602473_Prg281_Proj
{
    public class Astronaut : StationEntity
    {
        public string Specialization { get; }
        public int Health { get; set; }
        public StationModule AssignedModule { get; set; }
        public Astronaut(string name, string specialization) : base(name)
        {
            Specialization = specialization;
            Health = 100;
            AssignedModule = null;
        }
        public override string GetDetails()
        {
            return $"Astronaut {Name} (Id: {Id}) - Specialization: {Specialization}, Health: {Health}, Assigned Module: {(AssignedModule != null ? AssignedModule.Name : "None")}";
        }
    }
}