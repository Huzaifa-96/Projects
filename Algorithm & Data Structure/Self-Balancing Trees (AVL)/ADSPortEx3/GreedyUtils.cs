using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx3
{
    class GreedyUtils
    {
        //Greedy algorithm implementation for Assessed Exercise 3

        //Hints : 
        //Use lecture materials from Week 8 to aid with implementation.

        public static List<AidItem> GetGreedyOptimTasks(List<AidItem> items, double limit)
        {
            List<AidItem> chosen = new List<AidItem>(); // stores the final selected items
            if (items == null) return chosen; // no list given, return empty result
            if (items.Count == 0) return chosen; // no items to pick, return empty result

            AidItem[] copy = items.ToArray(); // copy to array so we can sort without changing original list
            SortUtils.QuickSortGen(copy); // Task B + Task C link: uses generic quicksort + AidItem.CompareTo ordering

            double totalWeight = 0; // track current total weight in kg

            for (int i = 0; i < copy.Length; i++) // scan items in sorted order (best ratio first)
            {
                AidItem current = copy[i]; // current item in ranked list

                // Task C: weight constraint must not exceed limit (75kg)
                if (totalWeight + current.Weight <= limit) // check if adding this item stays within limit
                {
                    chosen.Add(current); // accept this item into the loadout
                    totalWeight += current.Weight; // increase total weight after adding item
                }
                // If it doesn’t fit, we skip it and try the next best ratio item.
            }
            return chosen; // return the chosen items so Program.cs can display totals
        }

    }
}
