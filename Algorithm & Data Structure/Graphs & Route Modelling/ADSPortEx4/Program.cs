using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx4
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Graph<int> graph = new Graph<int>();          // Creates the metro graph using int IDs (simple for user input + lookup)
            bool running = true;                          // Controls the main menu loop (true = keep showing menu)

            while (running)                               // Repeats until the user chooses Exit
            {
                Console.WriteLine();                      // Prints a blank line to keep menu output readable between actions
                Console.WriteLine("Assessed Exercise 4 Menu"); // Title so user knows they are in Exercise 4 program
                Console.WriteLine("1. Add Station Node");        // Task A: create a new station node with Station data
                Console.WriteLine("2. Add Edge (unweighted)");   // Task A: create an unweighted connection
                Console.WriteLine("3. Show NodeCount and EdgeCount"); // Task A: graph size metrics (nodes + edges)
                Console.WriteLine("4. Add Weighted Edge travel time"); // Task B: weighted edge = travel time minutes
                Console.WriteLine("5. AverageOutbound");         // Task B: average outgoing edges per node
                Console.WriteLine("6. AverageTravelTime");       // Task B: average travel time across all edges
                Console.WriteLine("7. GetAllConnections");       // Task B: list all neighbours from a station
                Console.WriteLine("8. BFS traversal");           // Task C: BFS visit order from a start node
                Console.WriteLine("9. DFS traversal)");           // Task C: DFS visit order from a start node
                Console.WriteLine("10. FastestRoute (greedy lowest travel time, no revisits)"); // Task C: greedy traversal-style route
                Console.WriteLine("11. View all stations");               // Extra helper: print all station details currently stored
                Console.WriteLine("12. Exit");                            // Ends the program
                Console.Write("Choose option: ");                         // Prompts user to type a menu number

                string option = Console.ReadLine();       
                Console.WriteLine();                      

                switch (option)                           // Chooses which menu action to perform
                {
                    case "1": AddStation(graph); break;   // Calls Task A station creation function
                    case "2": AddEdge(graph); break;      // Calls Task A unweighted edge function
                    case "3": ShowCounts(graph); break;   // Prints NodeCount and EdgeCount for Task A
                    case "4": AddWeightedEdge(graph); break; // Calls Task B weighted edge function
                    case "5": Console.WriteLine("AverageOutbound = " + graph.AverageOutbound()); break; // Prints Task B metric
                    case "6": Console.WriteLine("AverageTravelTime = " + graph.AverageTravelTime()); break; // Prints Task B metric
                    case "7": ShowConnections(graph); break; // Calls Task B adjacency lookup function
                    case "8": RunBFS(graph); break;       // Calls Task C BFS traversal function
                    case "9": RunDFS(graph); break;       // Calls Task C DFS traversal function
                    case "10": RunFastestRoute(graph); break; // Calls Task C greedy route function
                    case "11": ViewStations(graph); break; // Prints all stored stations so user can verify graph contents
                    case "12": running = false; break;    // Sets running false so the while loop ends and program exits
                    default: Console.WriteLine("Invalid option. Choose 1 to 12."); break; // Explains valid range if user types wrong value
                }
            }
        }
        // Task A: Adds a station node to the graph with real Station details (name + passenger capacity).
        static void AddStation(Graph<int> graph)
        {
            Console.Write("Station ID (int): ");                          
            if (!int.TryParse(Console.ReadLine(), out int id)) // Prevents crashes if user types letters
            {
                Console.WriteLine("AddStation failed: ID must be a valid integer."); 
                return; // Stop this function because we cannot continue without an ID
            }

            Console.Write("Station name: ");                             
            string name = Console.ReadLine();                              

            Console.Write("Passenger capacity (int): ");                   
            if (!int.TryParse(Console.ReadLine(), out int cap)) // Ensures the capacity is numeric
            {
                Console.WriteLine("AddStation failed: capacity must be a valid integer."); 
                return;                                                   
            }

            graph.AddNode(id, new Station(name, cap)); // Task A: store a node with the Station data attached
            Console.WriteLine("AddStation complete.");                     
        }
        // Task A: Adds an unweighted directed edge from one station to another.
        static void AddEdge(Graph<int> graph)
        {
            Console.Write("From station ID: ");                            
            if (!int.TryParse(Console.ReadLine(), out int from)) // Ensures it is a valid integer
            {
                Console.WriteLine("AddEdge failed: source ID must be an integer."); 
                return; // Stop because without a valid source we cannot create an edge
            }

            Console.Write("To station ID: ");                             
            if (!int.TryParse(Console.ReadLine(), out int to)) // Ensures it is a valid integer
            {
                Console.WriteLine("AddEdge failed: destination ID must be an integer."); // Clear error message
                return; // Stop because without a valid destination we cannot create an edge
            }

            graph.AddEdge(from, to); // Task A: records the adjacency connection in the graph
            Console.WriteLine("AddEdge complete.");                       
        }
        // Task A: Prints how many nodes and edges currently exist in the graph.
        static void ShowCounts(Graph<int> graph)
        {
            Console.WriteLine("NodeCount = " + graph.NodeCount());         
            Console.WriteLine("EdgeCount = " + graph.EdgeCount());         
        }
        // Task B: Adds a weighted directed edge where weight means travel time in minutes.
        static void AddWeightedEdge(Graph<int> graph)
        {
            Console.Write("From station ID: ");                            
            if (!int.TryParse(Console.ReadLine(), out int from)) // Validate source ID input
            {
                Console.WriteLine("AddWeightedEdge failed: source ID must be an integer.");
                return; // Stop because a valid from ID is required
            }

            Console.Write("To station ID: ");                              
            if (!int.TryParse(Console.ReadLine(), out int to)) // Validate destination ID input
            {
                Console.WriteLine("AddWeightedEdge failed: destination ID must be an integer."); 
                return;                                                   
            }

            Console.Write("Travel time in minutes: ");                    
            if (!int.TryParse(Console.ReadLine(), out int weight)) // Validate weight input
            {
                Console.WriteLine("AddWeightedEdge failed: travel time must be an integer."); 
                return;                                                  
            }

            graph.AddWeightedEdge(from, to, weight); // Task B: stores weight aligned with neighbour in GraphNode
            Console.WriteLine("AddWeightedEdge complete.");               
        }

        // Task B: Prints all outgoing connections from a given station.
        static void ShowConnections(Graph<int> graph)
        {
            Console.Write("Station ID: ");                                 
            if (!int.TryParse(Console.ReadLine(), out int id)) // Validate station ID input
            {
                Console.WriteLine("GetAllConnections failed: ID must be an integer.");
                return;                                                  
            }

            List<int> connections = graph.GetAllConnections(id);           
            if (connections.Count == 0) // Empty list means either station missing or no outgoing edges
            {
                Console.WriteLine("No outgoing connections found, or station ID does not exist."); 
                return;                                                   
            }

            Console.WriteLine("Outgoing connections from " + id + " = " + string.Join(", ", connections)); 
        }
        // Task C: Runs BFS traversal and prints the visit order.
        static void RunBFS(Graph<int> graph)
        {
            Console.Write("Start station ID: ");                           
            if (!int.TryParse(Console.ReadLine(), out int start)) // Validate start ID input
            {
                Console.WriteLine("BFS failed: start ID must be an integer."); 
                return; // Stop because BFS needs a valid start ID
            }

            List<int> visited = new List<int>();                           
            graph.BFS(start, ref visited); // Task C: BFS uses a queue internally and fills visited list

            if (visited.Count == 0)  // If empty, BFS could not run (usually start ID missing)
            {
                Console.WriteLine("BFS produced no output. Check the station ID exists."); 
                return; // Stop because there is no traversal to display
            }

            Console.WriteLine("BFS visit order: " + string.Join(" -> ", visited)); 
        }

        // Task C: Runs DFS traversal and prints the visit order.
        static void RunDFS(Graph<int> graph)
        {
            Console.Write("Start station ID: ");             
            if (!int.TryParse(Console.ReadLine(), out int start)) // Validate start ID input
            {
                Console.WriteLine("DFS failed: start ID must be an integer."); 
                return;                                                   
            }

            List<int> visited = new List<int>();                          
            graph.DFS(start, ref visited);                                 

            if (visited.Count == 0) // If empty, DFS could not run (usually start ID missing)
            {
                Console.WriteLine("DFS produced no output. Check the station ID exists."); 
                return; // Stop because there is no traversal to display
            }

            Console.WriteLine("DFS visit order: " + string.Join(" -> ", visited)); 
        }
        // Task C: Runs FastestRoute and prints the route + station details.
        static void RunFastestRoute(Graph<int> graph)
        {
            Console.Write("Start station ID: ");                           
            if (!int.TryParse(Console.ReadLine(), out int start)) // Validate start ID input
            {
                Console.WriteLine("FastestRoute failed: start ID must be an integer."); 
                return; // Stop because route cannot start without a valid ID
            }

            List<int> route = new List<int>();                             
            graph.FastestRoute(start, ref route); // Task C: grows route by repeatedly choosing lowest travel time to an unvisited station

            if (route.Count == 0)  // Empty means route did not run (usually start ID missing)
            {
                Console.WriteLine("FastestRoute produced no output. Check the station ID exists."); // Explains likely cause
                return; // Stop because nothing can be printed
            }

            Console.WriteLine("FastestRoute order: " + string.Join(" -> ", route)); 
            Console.WriteLine();                                           
            Console.WriteLine("Station details on the route:");            

            for (int i = 0; i < route.Count; i++) // Prints full station details for each station ID in route
            {
                Console.WriteLine(graph.DescribeStation(route[i]));        
            }
        }

        static void ViewStations(Graph<int> graph)
        {
            List<int> ids = graph.GetAllStationIDs(); // Gets a list of all node IDs stored in the graph
            if (ids.Count == 0)  // If there are no nodes, there are no stations to display
            {
                Console.WriteLine("No stations have been added yet.");     
                return;                                                   
            }

            Console.WriteLine("Stations in the network:");                 

            for (int i = 0; i < ids.Count; i++)  // Loop through every station ID
            {
                Console.WriteLine(graph.DescribeStation(ids[i]));         
            }
        }

        // Exercise 4 Requirements Summary (specific)
        //
        // Task A:
        // I created a list based graph where nodes are stored in a LinkedList and found using a linear scan (no Dictionary).
        // I added AddNode to store station IDs, AddEdge to store outbound adjacency links, and NumNodes and NumEdges
        // so the program can print NodeCount and EdgeCount from the stored adjacency lists.
        //
        // Task B:
        // I added weighted edges where the weight is travel time in minutes. Each node stores neighbour IDs in adjList
        // and stores matching weights in weightList in the same positions. I implemented AverageOutbound using
        // totalEdges divided by totalNodes, and AverageTravelTime using totalWeight divided by totalEdges.
        // I also implemented GetAllConnections to return the adjacency list for a chosen station ID.
        //
        // Task C:
        // I implemented BFS using a Queue and DFS using recursion, both returning the visit order in a List.
        // I implemented FastestRoute as a greedy route: from the current station it checks all unvisited neighbours,
        // chooses the neighbour with the smallest travel time weight, moves there, and repeats until no unvisited
        // neighbour exists, then returns the full route taken.

    }
}
