using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx4
{
    //Graph implementation for Assessed Exercise 4

    //Hints : 
    //Use lecture materials from Week 9A
    // and lab sheet 'Lab 9: Graphs Implementation' to aid with base implementation.

    //For 4B, review material from lecture 9B and lab 10 to aid with the implementation of these metrics

    //For 4C, see lecture 10A and lab 10 to aid with your traversal implementation

    class Graph<T> where T : IComparable
    {
        // Stores all nodes in the graph.
        // Nodes are found by linear search (intentional – no Dictionary).
        private LinkedList<GraphNode<T>> nodes;

        public Graph()
        {
            nodes = new LinkedList<GraphNode<T>>();
        }

        //Functions for EX.4A

        // Adds a new station node to the graph.
        public void AddNode(T id, Station station)
        {
            if (station == null)
            {
                Console.WriteLine("AddNode failed: station data was null.");
                return;
            }

            // Prevent duplicate node IDs.
            if (GetNodeByID(id) != null)
            {
                Console.WriteLine("AddNode failed: station ID already exists.");
                return;
            }

            nodes.AddLast(new GraphNode<T>(id, station));
        }

        // Finds a node by ID using linear search.
        public GraphNode<T> GetNodeByID(T id)
        {
            foreach (GraphNode<T> n in nodes)
            {
                if (id.CompareTo(n.ID) == 0)
                    return n;
            }
            return null;
        }


        // Returns total number of nodes in the graph.
        public int NodeCount()
        {
            return nodes.Count;
        }

        // Returns total number of edges by summing adjacency list sizes.
        public int EdgeCount()
        {
            int total = 0;
            foreach (GraphNode<T> n in nodes)
                total += n.GetAdjList().Count;

            return total;
        }



        // Adds an unweighted directed edge.
        public void AddEdge(T from, T to)
        {
            GraphNode<T> n1 = GetNodeByID(from);
            GraphNode<T> n2 = GetNodeByID(to);

            if (n1 == null || n2 == null)
            {
                Console.WriteLine("AddEdge failed: one or both station IDs do not exist.");
                return;
            }

            n1.AddEdge(n2);
        }
        //Functions for EX.4B

        // Adds a weighted edge where weight represents travel time in minutes.
        public void AddWeightedEdge(T from, T to, int weight)
        {
            GraphNode<T> n1 = GetNodeByID(from);
            GraphNode<T> n2 = GetNodeByID(to);

            if (n1 == null || n2 == null)
            {
                Console.WriteLine("AddWeightedEdge failed: one or both station IDs do not exist.");
                return;
            }

            if (weight < 0)
            {
                Console.WriteLine("AddWeightedEdge failed: travel time cannot be negative.");
                return;
            }

            n1.AddEdgeWithWeight(n2, weight);
        }

        // Calculates average number of outbound edges per node.
        public double AverageOutbound()
        {
            int nodeCount = NodeCount();
            if (nodeCount == 0) return 0;

            return (double)EdgeCount() / nodeCount;
        }

        // Calculates average travel time across all edges.
        public double AverageTravelTime()
        {
            int edgeCount = EdgeCount();
            if (edgeCount == 0) return 0;

            long totalWeight = 0;

            foreach (GraphNode<T> n in nodes)
                foreach (int w in n.GetWeightList())
                    totalWeight += w;

            return (double)totalWeight / edgeCount;
        }

        // Returns all neighbour IDs connected from a given station.
        public List<T> GetAllConnections(T id)
        {
            List<T> result = new List<T>();
            GraphNode<T> n = GetNodeByID(id);
            if (n == null) return result;

            foreach (T neighbour in n.GetAdjList())
                result.Add(neighbour);

            return result;
        }

        //Functions for EX.4C

        // Breadth-First Search traversal.
        public void BFS(T startID, ref List<T> visited)
        {
            visited.Clear();

            GraphNode<T> start = GetNodeByID(startID);
            if (start == null) return;

            Queue<T> q = new Queue<T>(); // BFS uses a queue to explore level by level
            visited.Add(startID);
            q.Enqueue(startID);

            while (q.Count > 0) // continues until there are no more nodes to explore
            {
                T currentID = q.Dequeue();
                GraphNode<T> current = GetNodeByID(currentID);

                foreach (T neighbour in current.GetAdjList()) // checks each neighbour of current node
                {
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        q.Enqueue(neighbour);
                    }
                }
            }
        }

        public void DFS(T startID, ref List<T> visited)
        {
            visited.Clear(); // ensures output list is empty before DFS begins

            GraphNode<T> start = GetNodeByID(startID);
            if (start == null) return;  // DFS cannot run if start node is missing


            dfsVisit(startID, ref visited); // starts the recursive depth first search
        }
        
        private void dfsVisit(T currentID, ref List<T> visited)
        {
            visited.Add(currentID); // records that we have visited this node now
            GraphNode<T> current = GetNodeByID(currentID);

            foreach (T neighbour in current.GetAdjList()) // checks each neighbour ID
            {
                if (!visited.Contains(neighbour)) // prevents infinite recursion in cycles
                    dfsVisit(neighbour, ref visited); // goes deeper into the neighbour before coming back
            }
        }

        public void FastestRoute(T startID, ref List<T> route)
        {
            route.Clear(); // output list will store the route in the exact order travelled

            GraphNode<T> start = GetNodeByID(startID);
            if (start == null) return;

            List<T> visited = new List<T>();
            visited.Add(startID);
            route.Add(startID);

            while (true) // repeats until there is no unvisited neighbour to move to
            {
                bool found = false;
                int bestWeight = int.MaxValue;
                T bestTo = default(T);

                // checks every connected station
                foreach (T v in visited) 
                {
                    GraphNode<T> vNode = GetNodeByID(v);

                    foreach (T neighbour in vNode.GetAdjList())
                    {
                        if (visited.Contains(neighbour)) continue;

                        if (vNode.TryGetWeightTo(neighbour, out int w) && w < bestWeight)
                        {
                            bestWeight = w;
                            bestTo = neighbour;
                            found = true;
                        }
                    }
                }

                if (!found) break; // stops when all neighbours are either visited or no weights exist

                visited.Add(bestTo); // moves to the next station chosen by the greedy rule
                route.Add(bestTo); // records the station in the route output list
            }
        }
        // Helper to show full station details.
        public string DescribeStation(T id)
        {
            GraphNode<T> n = GetNodeByID(id); // Look up the node associated with the given station ID
            if (n == null) return "Station not found.";
            return $"ID {id}: {n.StationData}";
        }

        // Returns a list of all station IDs currently stored in the graph.
        // Used by the menu to allow users to see which stations exist.
        public List<T> GetAllStationIDs()
        {
            List<T> ids = new List<T>(); // Will store every station ID found in the graph
            foreach (GraphNode<T> n in nodes) // Walk through all graph nodes
                ids.Add(n.ID);
            return ids;
        }


        //Free space, use as needed




    }//End of class
}
