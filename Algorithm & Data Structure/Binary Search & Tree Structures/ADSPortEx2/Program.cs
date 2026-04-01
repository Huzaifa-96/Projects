using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx2
{
    internal class Program
    {
        static void Main(string[] args)
        {


            AVLTree<VideoGame> tree = new AVLTree<VideoGame>(); // create a new AVL tree for VideoGame
            bool running = true; // flag to control main loop

            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("===== Retro Game Tree System ====="); // show menu title
                Console.WriteLine("1. Add new game");
                Console.WriteLine("2. Display all games (choose order)");
                Console.WriteLine("3. Show earliest game by year");
                Console.WriteLine("4. Show tree height");
                Console.WriteLine("5. Show game count");
                Console.WriteLine("6. Update a game by title");
                Console.WriteLine("7. List games by year");
                Console.WriteLine("8. Remove a game by title");
                Console.WriteLine("9. Exit");
                Console.Write("Choose option: ");

                string option = Console.ReadLine(); // read user's choice
                Console.WriteLine();
                switch (option)
                {
                    case "1": AddGame(tree); break; // call function to add game
                    case "2": ShowGames(tree); break; // show tree contents
                    case "3": ShowEarliest(tree); break; // show earliest by release year
                    case "4": ShowHeight(tree); break; // show height of tree
                    case "5": ShowCount(tree); break; // show how many games stored
                    case "6": UpdateGame(tree); break; // update existing game
                    case "7": ListByYear(tree); break; // list games for a given year
                    case "8": RemoveGame(tree); break; // remove game by title
                    case "9": running = false; break; // end loop and exit
                    default: Console.WriteLine("Invalid option"); break; // handle wrong input
                }
            }

            Console.WriteLine("Goodbye."); // exit message
            Console.ReadLine(); // wait before closing

        }
        static void AddGame(AVLTree<VideoGame> tree)
        {
            Console.Write("Game title: ");
            string title = Console.ReadLine(); // read title from user

            Console.Write("Developer: ");
            string dev = Console.ReadLine(); // read developer

            int year;
            while (true)
            {
                Console.Write("Release year: ");
                if (int.TryParse(Console.ReadLine(), out year)) break; // keep asking until a number is given
                Console.WriteLine("Please enter a number.");
            }

            VideoGame game = new VideoGame(title, dev, year); // create new game object
            tree.InsertItem(game); // insert game into AVL tree
            Console.WriteLine("Game added.");
        }
        static void ShowGames(AVLTree<VideoGame> tree)
        {
            Console.WriteLine("1. InOrder");
            Console.WriteLine("2. PreOrder");
            Console.WriteLine("3. PostOrder");
            Console.Write("Choose order: ");

            string choice = Console.ReadLine(); // read user choice for traversal
            string buffer = ""; // will hold the printed games

            switch (choice)
            {
                case "1":
                    tree.InOrder(ref buffer); // visit left, root, right
                    break;
                case "2":
                    tree.PreOrder(ref buffer); // visit root, left, right
                    break;
                case "3":
                    tree.PostOrder(ref buffer); // visit left, right, root
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    return; // stop if wrong option
            }

            Console.WriteLine();
            Console.WriteLine("Games in tree:");
            Console.WriteLine(string.IsNullOrEmpty(buffer) ? "No games in tree." : buffer); // show result or message
        }
        static void ShowEarliest(AVLTree<VideoGame> tree)
        {
            VideoGame earliest = tree.EarlieseGame() as VideoGame; // call function to find earliest game
            if (earliest == null) Console.WriteLine("No games in tree.");
            else Console.WriteLine("Earliest game:\n" + earliest); // print earliest game
        }

        static void ShowHeight(AVLTree<VideoGame> tree)
        {
            int h = tree.Height(); // get height of tree
            Console.WriteLine("Tree height: " + h); // show height to user
        }

        static void ShowCount(AVLTree<VideoGame> tree)
        {
            int c = tree.Count(); // get number of games stored
            Console.WriteLine("Number of games in tree: " + c); // print count
        }
        static void UpdateGame(AVLTree<VideoGame> tree)
        {
            Console.Write("Enter title of game to update: ");
            string title = Console.ReadLine(); // read which game to update

            Console.Write("New developer: ");
            string dev = Console.ReadLine(); // read new developer

            int year;
            while (true)
            {
                Console.Write("New release year: ");
                if (int.TryParse(Console.ReadLine(), out year)) break; // validate numeric year
                Console.WriteLine("Please enter a number.");
            }

            VideoGame updated = new VideoGame(title, dev, year); // make a new object with updated info
            tree.Update(updated); // try to update matching title in tree
            Console.WriteLine("Update done (if title was found).");
        }
        static void ListByYear(AVLTree<VideoGame> tree)
        {
            int year;
            while (true)
            {
                Console.Write("Year to search for: ");
                if (int.TryParse(Console.ReadLine(), out year)) break; // ensure user types a number
                Console.WriteLine("Please enter a number.");
            }

            string result = tree.ListByYear(year); // get list of games with that year
            Console.WriteLine(result); // show the list or message
        }
        static void RemoveGame(AVLTree<VideoGame> tree)
        {
            Console.Write("Enter title of game to remove: ");
            string title = Console.ReadLine(); // read the title user wants to remove
            VideoGame dummy = new VideoGame(title, "x", 0); // create temp game where only title matters
            tree.RemoveItem(dummy); // remove node matching this title
            Console.WriteLine("Remove done (if title was found).");
        }
    }
}
