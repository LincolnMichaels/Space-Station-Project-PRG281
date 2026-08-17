using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public class SimulationTickEventArgs : EventArgs
    {
        public int Oxygen { get; }
        public int Water { get; }
        public int Power { get; }
        public double Temperature { get; }

        public SimulationTickEventArgs(int oxygen, int water, int power, double temperature)
        {
            Oxygen = oxygen;
            Water = water;
            Power = power;
            Temperature = temperature;
        }
    }
}
