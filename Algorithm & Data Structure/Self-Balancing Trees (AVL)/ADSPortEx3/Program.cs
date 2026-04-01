using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx3
{
    internal class Program
    {
        static void Main(string[] args)
        {


            //Create a Menu driven interface here so a user can interact with your implementations

            //I.e. while(true){
            // print to user - "Select an option"
            // "1. Demo sort..."
            // "2. Add item for greedy algorithm... ect
            //}

            //You won't need to create either of the utils classes, both use static methods.

            //I.e. 
            //SortUtils.InsertSort();
            //GreedyUtils.GetGreedyManifesto();
            List<AidItem> items = new List<AidItem>(); // Task C: holds all user-created AidItems
            ComplexityFunctions cf = new ComplexityFunctions(); // Task A: contains Algorithm1 and Algorithm2 for Big O

            bool running = true; // controls menu loop
            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("===== Assessed Exercise 3 Menu ====="); // single menu for Task A, B, C
                Console.WriteLine("1. Task A: Run Algorithm1 and show Big O result");
                Console.WriteLine("2. Task A: Run Algorithm2 and show Big O result");
                Console.WriteLine("3. Task B: QuickSortGen<int> demo (sort integers)");
                Console.WriteLine("4. Task B: QuickSortGen<string> demo (sort strings)");
                Console.WriteLine("5. Task C: Add AidItem (name, weight, priority)");
                Console.WriteLine("6. Task C: View all AidItems + their PriorityRatio");
                Console.WriteLine("7. Task C: Run greedy selection with 75kg limit");
                Console.WriteLine("8. Exit");
                Console.Write("Choose option: ");

                string option = Console.ReadLine(); // read user choice
                Console.WriteLine();

                switch (option)
                {
                    case "1":
                        RunAlgorithm1(cf); // Task A: executes Algorithm1 and prints Big O statement
                        break;

                    case "2":
                        RunAlgorithm2(cf); // Task A: executes Algorithm2 and prints Big O statement
                        break;

                    case "3":
                        DemoSortInts(); // Task B: proves QuickSortGen works for int
                        break;

                    case "4":
                        DemoSortStrings(); // Task B: proves QuickSortGen works for string
                        break;

                    case "5":
                        AddAidItem(items); // Task C: create an AidItem object from user input
                        break;

                    case "6":
                        ViewAidItems(items); // Task C: show all items with ratio so user can see ranking
                        break;

                    case "7":
                        RunGreedy(items); // Task C: build best loadout under 75kg using greedy rule
                        break;

                    case "8":
                        running = false; // end program
                        break;

                    default:
                        Console.WriteLine("Invalid option. Choose 1 to 8."); // handles wrong input
                        break;
                }
            }
        }
        static void RunAlgorithm1(ComplexityFunctions cf)
        {
            Console.WriteLine("Task A: Algorithm1"); // show what we are running
            Console.Write("Enter n: "); // Algorithm1 expects n from Console input
            cf.Algorithm1(); // runs the algorithm exactly as written in ComplexityFunctions.cs
            Console.WriteLine("Big O conclusion for Algorithm1: O(n^2)"); // required Task A final statement
        }
        static void RunAlgorithm2(ComplexityFunctions cf)
        {
            Console.WriteLine("Task A: Algorithm2"); // show what we are running
            Console.Write("Enter n: "); // Algorithm2 expects n from Console input
            cf.Algorithm2(); // runs the algorithm exactly as written in ComplexityFunctions.cs
            Console.WriteLine("Big O conclusion for Algorithm2: O(n * sqrt(n))"); // required Task A final statement
        }
        static void DemoSortInts()
        {
            int[] nums = new int[] { 9, 3, 7, 1, 8, 2, 5, 4, 6 }; // sample input to prove sorting works
            Console.WriteLine("Before sort: " + string.Join(", ", nums)); // show array before sorting
            SortUtils.QuickSortGen(nums); // Task B: generic quicksort called with int[]
            Console.WriteLine("After sort:  " + string.Join(", ", nums)); // show array after sorting
        }

        static void DemoSortStrings()
        {
            string[] words = new string[] { "zebra", "apple", "mango", "banana", "pear" }; // sample strings
            Console.WriteLine("Before sort: " + string.Join(", ", words)); // show before sorting
            SortUtils.QuickSortGen(words); // Task B: generic quicksort called with string[]
            Console.WriteLine("After sort:  " + string.Join(", ", words)); // show after sorting
        }
        static void AddAidItem(List<AidItem> items)
        {
            Console.Write("Name: "); // item name prompt
            string name = Console.ReadLine(); // read item name

            int weight;
            while (true)
            {
                Console.Write("Weight (1-50): "); // Task C: weight range requirement
                if (int.TryParse(Console.ReadLine(), out weight)) break; // accept numeric weight
                Console.WriteLine("Weight must be a number."); // explain error
            }

            int priority;
            while (true)
            {
                Console.Write("Priority (1-10): "); // Task C: priority range requirement
                if (int.TryParse(Console.ReadLine(), out priority)) break; // accept numeric priority
                Console.WriteLine("Priority must be a number."); // explain error
            }

            AidItem item = new AidItem(name, weight, priority); // Task C: create AidItem object
            items.Add(item); // store it so greedy can use it later

            Console.WriteLine("Added: " + item); // show the stored item including ratio
        }
        static void ViewAidItems(List<AidItem> items)
        {
            if (items.Count == 0)
            {
                Console.WriteLine("No AidItems have been added yet."); // specific message if empty
                return;
            }

            Console.WriteLine("All AidItems (shows PriorityRatio = priority/weight):"); // explains exactly what user sees
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine(items[i]); // prints name, weight, priority, ratio
            }
        }
        static void RunGreedy(List<AidItem> items)
        {
            if (items.Count == 0)
            {
                Console.WriteLine("Add some AidItems first before running greedy selection."); // specific guidance
                return;
            }

            double limit = 75; // Task C: maximum total weight allowed
            List<AidItem> chosen = GreedyUtils.GetGreedyOptimTasks(items, limit); // Task C: greedy picks best ratio items

            int totalPriority = 0; // sum of chosen priorities
            int totalWeight = 0; // sum of chosen weights

            Console.WriteLine("Chosen loadout (picked in sorted PriorityRatio order):"); // explains selection rule
            for (int i = 0; i < chosen.Count; i++)
            {
                Console.WriteLine(chosen[i]); // show each chosen item
                totalPriority += chosen[i].Priority; // add priority for final score
                totalWeight += chosen[i].Weight; // add weight to show final kg total
            }

            Console.WriteLine("Total weight used: " + totalWeight + "kg out of 75kg"); // show constraint result
            Console.WriteLine("Total priority score: " + totalPriority); // show final greedy score
        }
        // Exercise 3 Summary (what I was required to do and what I did)
        //
        // Task A (Complexity / Big O):
        // - Requirement: work out and state the Big O for Algorithm1 and Algorithm2.
        // - What I did: I kept Algorithm1 and Algorithm2 in ComplexityFunctions.cs and added full working in comments.
        //   In this Program.cs, options 1 and 2 run those algorithms and then print the final Big O conclusions:
        //   Algorithm1 = O(n^2) and Algorithm2 = O(n * sqrt(n)).
        //
        // Task B (Generic QuickSort):
        // - Requirement: implement a generic QuickSort method QuickSortGen<T> using IComparable so it can sort different types.
        // - What I did: I implemented QuickSortGen<T> in SortUtils.cs using CompareTo for comparisons.
        //   In this Program.cs, options 3 and 4 demonstrate it working on int[] and string[] so it proves the sort is generic.
        //
        // Task C (Greedy Aid Item Selection):
        // - Requirement: create an AidItem class with name, weight (1–50), priority (1–10) and PriorityRatio = priority/weight,
        //   then choose items greedily by highest PriorityRatio without exceeding a 75kg limit.
        // - What I did: I built AidItem.cs with PriorityRatio and a CompareTo that sorts by highest ratio first.
        //   GreedyUtils.cs uses QuickSortGen to sort AidItems, then selects whole items while totalWeight + item.Weight <= 75.
        //   In this Program.cs, options 5–7 let the user add items, view items (including ratio), and run the greedy selection,
        //   then it prints the chosen items plus total weight and total priority score.

    }
}
