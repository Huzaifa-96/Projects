using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx4
{
    //Graph node implementation for Assessed Exercise 2

    //Hints : 
    //Use lecture materials from Week 9A and lab sheet 'Lab 9: Graphs Implementation' to aid with base implementation...
    //Then see materials for week 10 when moving onto 4B


    class GraphNode<T> where T : IComparable

    {
        private T id;                         // Unique identifier for this station node
        private Station stationData;          // Real station data (name + capacity)
        private LinkedList<T> adjList;  // Adjacency list stores neighbour IDs (outgoing edges).
        private LinkedList<int> weightList; // Weight list stores travel time for each edge.

        public GraphNode(T id, Station stationData)
        {
            this.id = id;
            this.stationData = stationData;

            // Lists start empty; edges are added later.
            adjList = new LinkedList<T>();
            weightList = new LinkedList<int>();
        }

        //Functions for EX.4A

        public T ID
        {
            get { return id; }
        }

        public Station StationData
        {
            get { return stationData; }
            set { stationData = value; }
        }

        // Returns the adjacency list so Graph can inspect neighbours.
        public LinkedList<T> GetAdjList()
        {
            return adjList;
        }

        // Returns the aligned weight list for metrics like AverageTravelTime.
        public LinkedList<int> GetWeightList()
        {
            return weightList;
        }

        // Checks whether an edge already exists to a given neighbour.
        // This prevents duplicate edges being stored.
        private bool HasEdgeTo(T neighbourID)
        {
            foreach (T n in adjList)
            {
                if (n.CompareTo(neighbourID) == 0)
                    return true;
            }
            return false;
        }

        public void AddEdge(GraphNode<T> to)
        {
            if (to == null) return;
            if (HasEdgeTo(to.ID)) return; // Prevent duplicates

            adjList.AddLast(to.ID);
            weightList.AddLast(0); // 0 means no travel time defined
        }


        // Task B: Add a weighted edge (travel time in minutes).
        public void AddEdgeWithWeight(GraphNode<T> to, int weight)
        {
            if (to == null) return;
            if (weight < 0) return; // Travel time cannot be negative

            // If the edge already exists, update its weight instead of duplicating it.
            LinkedListNode<T> a = adjList.First;
            LinkedListNode<int> w = weightList.First;

            while (a != null && w != null)
            {
                if (a.Value.CompareTo(to.ID) == 0)
                {
                    w.Value = weight;
                    return;
                }
                a = a.Next;
                w = w.Next;
            }

            // Otherwise, add a brand-new weighted edge.
            adjList.AddLast(to.ID);
            weightList.AddLast(weight);
        }

        // Retrieves the weight for a specific outgoing edge.
        // Used by FastestRoute to choose the lowest travel time.
        public bool TryGetWeightTo(T neighbourID, out int weight)
        {
            weight = 0;

            LinkedListNode<T> a = adjList.First;
            LinkedListNode<int> w = weightList.First;

            // Walk both lists together so indices always match.
            while (a != null && w != null)
            {
                if (a.Value.CompareTo(neighbourID) == 0)
                {
                    weight = w.Value;
                    return true;
                }
                a = a.Next;
                w = w.Next;
            }

            return false; // No edge found
        }



    }// End of class
}
