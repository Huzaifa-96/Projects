using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS_Assess1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EmergencyQueue queue = new EmergencyQueue(); // make new queue
            bool running = true;

            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("===== Emergency Queue System ====="); // main menu text
                Console.WriteLine("1. Add a new call");
                Console.WriteLine("2. Dequeue and show oldest");
                Console.WriteLine("3. Peek next call");
                Console.WriteLine("4. Show all calls");
                Console.WriteLine("5. Show highest severity");
                Console.WriteLine("6. Dequeue first k calls");
                Console.WriteLine("7. Exit");
                Console.Write("Choose option: ");

                string option = Console.ReadLine();
                Console.WriteLine();

                switch (option)
                {
                    case "1": AddNewCall(queue); break; // add one call
                    case "2": DequeueAndShow(queue); break;
                    case "3": PeekCall(queue); break; // check next call
                    case "4": ShowQueue(queue); break;
                    case "5": ShowHighest(queue); break; // show most serious
                    case "6": DequeueK(queue); break;
                    case "7": running = false; break; // stop loop
                    default: Console.WriteLine("Invalid option"); break;


                }
            }
        }
        static void AddNewCall(EmergencyQueue queue)
        {
            if (queue.IsFull()) { Console.WriteLine("Queue is full"); return; }

            Console.Write("Caller name: ");
            string name = Console.ReadLine();
            Console.Write("Emergency type: ");
            string type = Console.ReadLine();

            int severity;
            while (true)
            {
                Console.Write("Severity (1-5): ");
                if (int.TryParse(Console.ReadLine(), out severity)) break; // wait for a number
                Console.WriteLine("Please enter a number.");
            }

            EmergencyCall call = new EmergencyCall(name, type, severity);
            queue.Enqueue(call); // put call in queue
            Console.WriteLine("Call added.");
        }
        static void DequeueAndShow(EmergencyQueue queue)
        {
            if (queue.IsEmpty()) { Console.WriteLine("Queue is empty"); return; }
            EmergencyCall call = queue.Dequeue(); // remove first call
            Console.WriteLine("Dequeued call:");
            Console.WriteLine(call);
        }
        static void PeekCall(EmergencyQueue queue)
        {
            EmergencyCall call = queue.Peek();
            if (call == null) Console.WriteLine("Queue is empty");
            else Console.WriteLine($"Next call:\n{call}"); // show next in line
        }
        static void ShowQueue(EmergencyQueue queue)
        {
            Console.WriteLine(queue.ToString()); // print all calls
        }
        static void ShowHighest(EmergencyQueue queue)
        {
            EmergencyCall call = queue.PeekHighestSeverity();
            if (call == null) Console.WriteLine("Queue is empty");
            else Console.WriteLine($"Most serious call:\n{call}"); // show highest level
        }
        static void DequeueK(EmergencyQueue queue)
        {
            Console.Write("Enter k: ");
            if (!int.TryParse(Console.ReadLine(), out int k)) { Console.WriteLine("Bad number"); return; }
            queue.DequeueFirstKCalls(k); // remove k calls
            Console.WriteLine($"Tried to remove {k} call(s)."); // simple message
        }
    }
}
