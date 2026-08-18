using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    class Program
    {
        static void Main(string[] args)
        {
           
            // Creates the main station systems and passes them into the
            // operator console. The console then integrates the work
            // completed by Persons 1, 2 and 3.
            Station station = new Station();
            StationResourceManager resources = new StationResourceManager();
            StationSimulation simulation =new StationSimulation(station, resources);
            ConsoleMenu menu =new ConsoleMenu(station, resources, simulation);

            menu.Run();
        }
    }
}