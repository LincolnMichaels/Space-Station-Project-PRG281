using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Chris_602473_Prg281_Proj
{
    public abstract class StationEntity
    {
        public string Id { get; }
        public string Name { get; }
        public Status Status { get; set; }
        protected StationEntity(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Status = Status.Active;
        }
        public abstract string GetDetails();
        public override string ToString()
        {
            return $"{GetType().Name}[Id={Id}, Name={Name}, Status={Status}]";
        }
    }
}