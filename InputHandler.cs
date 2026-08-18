using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public static class InputHandler
    {
        public static string GetString(string message) // Centralises user input validation so that invalid console input
        {                                               // does not crash the application.
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input.Trim();
                }

                Console.WriteLine("Input cannot be empty.");
            }
        }

        public static int GetInt(string message)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine();

                int value;

                if (int.TryParse(input, out value))
                {
                    return value;
                }

                Console.WriteLine("Invalid input. Please enter a whole number.");
            }
        }

        public static int GetPositiveInt(string message)
        {
            while (true)
            {
                int value = GetInt(message);

                if (value > 0)
                {
                    return value;
                }

                Console.WriteLine("Please enter a number greater than zero.");
            }
        }

        public static double GetDouble(string message)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine();

                double value;

                if (double.TryParse(input, out value))
                {
                    return value;
                }

                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }

        public static double GetPositiveDouble(string message)
        {
            while (true)
            {
                double value = GetDouble(message);

                if (value > 0)
                {
                    return value;
                }

                Console.WriteLine("Please enter a number greater than zero.");
            }
        }

        public static bool GetYesNo(string message)
        {
            while (true)
            {
                Console.Write(message + " (Y/N): ");

                string input = Console.ReadLine();

                if (input.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (input.Equals("N", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                Console.WriteLine( "Please enter Y or N.");
            }
        }
    }
}
