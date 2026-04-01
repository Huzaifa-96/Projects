using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx3
{
    //AidItem class implementation for Assessed Exercise 3

    //For use as part of EX.3C

    //Use slides from Week 8A regarding the knapsack algorithm example to aid with implementation

    class AidItem: IComparable
    {
        private string name; // the item name shown to user
        private int weight; // weight in kg (used for the 75kg limit)
        private int priority; // priority score (higher means more important)

        public AidItem(string name, int weight, int priority)
        {
            Name = name; // store name with simple checks
            Weight = weight; // enforce valid range
            Priority = priority; // enforce valid range
        }

        public string Name
        {
            get { return name; } // return item name
            set
            {
                if (string.IsNullOrWhiteSpace(value)) name = "Unknown"; // avoid empty names
                else name = value.Trim(); // remove extra spaces
            }
        }

        public int Weight
        {
            get { return weight; } // return weight
            set
            {
                // Task C gives weight range 1–50, so we clamp it.
                if (value < 1) weight = 1; // minimum weight allowed
                else if (value > 50) weight = 50; // maximum weight allowed
                else weight = value; // accept valid weight
            }
        }

        public int Priority
        {
            get { return priority; } // return priority
            set
            {
                // Task C gives priority range 1–10, so we clamp it.
                if (value < 1) priority = 1; // minimum priority allowed
                else if (value > 10) priority = 10; // maximum priority allowed
                else priority = value; // accept valid priority
            }
        }

        public double PriorityRatio
        {
            get { return (double)Priority / Weight; }
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return -1; // treat null as “lowest”
            AidItem other = obj as AidItem; // try to cast object into AidItem
            if (other == null) throw new ArgumentException("Not an AidItem"); // stop if wrong type

            // Task C: sort by highest PriorityRatio FIRST so greedy picks best ratio items first.
            int ratioCompare = other.PriorityRatio.CompareTo(this.PriorityRatio); // descending ratio order
            if (ratioCompare != 0) return ratioCompare; // if ratios differ, decide by ratio

            // If ratio ties, use priority so higher priority goes first.
            int priorityCompare = other.Priority.CompareTo(this.Priority); // descending priority order
            if (priorityCompare != 0) return priorityCompare; // decide by priority if needed

            // If still tied, pick lighter one first so it fits easier into 75kg.
            int weightCompare = this.Weight.CompareTo(other.Weight); // ascending weight order
            if (weightCompare != 0) return weightCompare; // decide by weight if needed

            // Final tie-break so sorting is consistent (stable ordering by name).
            return string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase); // alphabetical ignoring case
        }
        public override string ToString()
        {
            // This output is used in Program.cs to show the chosen loadout clearly.
            return $"{Name} (W:{Weight}kg, P:{Priority}, R:{PriorityRatio:0.00})";
        }
    }
}
