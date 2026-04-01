using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS_Assess1
{
    //Queue implementation for Assessed Exercise 1

    //Please use lecture and lab materials from Week 2A to aid with implementation.

    //Do not delete the current function definitions, just replace the exceptions with the required functionality.

    // Don't forget to properly commit and document your work. 
    // Commit fairly frequently with proper descriptions, i.e. commit after implementing the enqueue function...

    internal class EmergencyQueue
    {
        private int maxsize = 10;
        private EmergencyCall[] store;
        private int head = 0;
        private int tail = 0;
        private int numitems = 0;
        // Functions for EX.1A
        public EmergencyQueue()
        {
            store = new EmergencyCall[maxsize];
        }

        public EmergencyQueue(int size)
        {
            if (size <= 0) maxsize = 10; // default size if invalid
            else maxsize = size;
            store = new EmergencyCall[maxsize]; // allocate storage
        }

        public void Enqueue(EmergencyCall value)
        {
            if (IsFull()) throw new InvalidOperationException("Queue is full"); // cannot add more
            store[tail] = value;
            tail = (tail + 1) % maxsize; // move tail in circle
            numitems++;
        }

        public EmergencyCall Dequeue()
        {
            if (IsEmpty()) throw new InvalidOperationException("Queue is empty"); // nothing to remove
            EmergencyCall item = store[head];
            store[head] = null; // clear slot
            head = (head + 1) % maxsize; // move head in circle
            numitems--;
            return item;
        }

        public EmergencyCall Peek()
        {
            if (IsEmpty()) return null; // no items
            return store[head];
        }

        public int Count()
        {
            return numitems;
        }

        public bool IsEmpty()
        {
            return numitems==0;
        }

        public bool IsFull()
        {
            return numitems==maxsize;
        }

        public override string ToString()
        {
            if (IsEmpty()) return "Queue is empty.";
            StringBuilder sb = new StringBuilder(); // build output text
            sb.AppendLine("Current emergency calls:");

            int index = head;
            for (int i = 0; i < numitems; i++)
            {
                sb.AppendLine($"[{i + 1}] {store[index]}"); // show one call
                index = (index + 1) % maxsize;
            }
            return sb.ToString();
        }

        // Functions for EX.1B

        public EmergencyCall PeekHighestSeverity()
        {
            if (IsEmpty()) return null; // no calls
            EmergencyCall highest = null;

            int index = head;
            for (int i = 0; i < numitems; i++)
            {
                EmergencyCall current = store[index];
                if (highest == null || current.SeverityLevel > highest.SeverityLevel)
                    highest = current; // keep bigger level
                index = (index + 1) % maxsize;
            }
            return highest;
        }

        public void DequeueFirstKCalls(int k)
        {
            if (k <= 0) return; // nothing to do
            int limit = Math.Min(k, numitems); // cannot remove more than we have

            for (int i = 0; i < limit; i++)
            {
                Dequeue(); // reuse remove method
            }
        }

        // Functions for EX.1C
        // Added extra queue operations to support the task, including a method to find the
        // highest-severity call without removing it and another method to remove the first K calls.
        // These functions build on the existing queue structure and reuse earlier methods
        // to keep the logic consistent.

        // Free space, use as necessary to address task requirements.
        // Used this section to expand the queue features in a simple way while keeping the
        // circular queue behaviour the same. This includes looping through items safely and
        // making sure empty or invalid inputs are handled properly.








    } // End of class

}
