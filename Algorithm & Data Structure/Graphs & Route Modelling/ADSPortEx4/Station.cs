using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx4
{
    class Station : IComparable
    {
        private string name;                 // Human-readable station name (e.g. "Piccadilly")
        private int passengerCapacity;       // Maximum safe passenger capacity for planning

        // Constructor forces station data to be provided at creation time.
        public Station(string name, int passengerCapacity)
        {
            Name = name;
            PassengerCapacity = passengerCapacity;
        }


        public string Name
        {
            get { return name; }
            set
            {
                // If the user enters nothing or only spaces, store a safe default.
                if (string.IsNullOrWhiteSpace(value))
                    name = "Unknown";
                else
                    name = value.Trim(); // Remove leading/trailing spaces
            }
        }
        public int PassengerCapacity
        {
            get { return passengerCapacity; }
            set
            {
                // Negative passenger capacity makes no sense in a planning model.
                if (value < 0)
                    passengerCapacity = 0;
                else
                    passengerCapacity = value;
            }
        }
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Station other = obj as Station;
            if (other == null)
                throw new ArgumentException("Object is not a Station");

            // Stations are compared alphabetically by name.
            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        // ToString provides a clear one-line description used by the menu.
        public override string ToString()
        {
            return $"{Name} (Capacity: {PassengerCapacity})";
        }
    }
}
