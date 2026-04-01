using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx2
{
    //Binary Search Tree implementation for Assessed Exercise 2

    //Hints : 
    //Use lecture materials from Week 5
    // and lab sheet 'Lab 5: BinTree and BSTree' to aid with implementation.

    class BSTree<T> : BinTree<T> where T : IComparable
    {

        public BSTree()
        {
            root = null; // start with an empty BST
        }

        //Functions for EX.2A
        public void InsertItem(T item)
        {
            insertItem(item, ref root); // use helper to insert starting from root
        }
        private void insertItem(T item, ref Node<T> tree)
        {
            if (tree == null) // if this spot is empty
            {
                tree = new Node<T>(item); // create a new node here
            }
            else
            {
                int compare = item.CompareTo(tree.Data); // compare new item with current node data
                if (compare < 0) insertItem(item, ref tree.Left); // if smaller, go to left subtree
                else if (compare > 0) insertItem(item, ref tree.Right); // if bigger, go to right subtree
                else tree.Data = item; // if equal, replace the data (update existing)
            }
        }

        public int Height()
        {
            return height(root); // compute height starting from root
        }
        protected int height(Node<T> tree)
        {
            if (tree == null) return 0; // height of an empty tree is 0
            int left = height(tree.Left); // get height of left subtree
            int right = height(tree.Right); // get height of right subtree
            return 1 + Math.Max(left, right); // height is 1 plus the taller subtree
        }

        public T EarlieseGame()
        {
            if (root == null) return default(T); // if tree is empty, return default
            VideoGame earliest = null; // will store the earliest game found
            findEarliest(root, ref earliest); // search through the tree
            if (earliest == null) return default(T); // if none found, return default
            return (T)(object)earliest; // cast VideoGame back to T
        }
        private void findEarliest(Node<T> tree, ref VideoGame earliest)
        {
            if (tree == null) return; 
            findEarliest(tree.Left, ref earliest); // search left subtree first

            VideoGame vg = tree.Data as VideoGame; // try to treat data as VideoGame
            if (vg != null)
            {
                if (earliest == null || vg.Releaseyear < earliest.Releaseyear) // if first or earlier year
                    earliest = vg; // remember this as the earliest so far
            }

            findEarliest(tree.Right, ref earliest); // then search right subtree
        }

        //Functions for EX.2B

        public int Count()
        {
            return count(root); // count nodes starting from root
        }
        protected int count(Node<T> tree)
        {
            if (tree == null) return 0; // no node here means count 0
            // count this node (1) plus all nodes in left and right subtrees
            return 1 + count(tree.Left) + count(tree.Right);
        }

        public void Update(T item)
        {
            update(item, root); // look for matching item and update it
        }
        private void update(T item, Node<T> tree)
        {
            if (tree == null) return; // reached empty spot, nothing to update
            int compare = item.CompareTo(tree.Data); // compare item with this node
            if (compare < 0) update(item, tree.Left); // go left if smaller
            else if (compare > 0) update(item, tree.Right); // go right if bigger
            else tree.Data = item; // titles match here, so update the node
        }
        // List all games with a given year
        public string ListByYear(int year)
        {
            string buffer = ""; // text that will hold all matches
            listByYear(root, year, ref buffer); // search entire tree
            if (string.IsNullOrEmpty(buffer)) buffer = "No games found for that year."; // if none, show message
            return buffer; // return the result text
        }

        private void listByYear(Node<T> tree, int year, ref string buffer)
        {
            if (tree == null) return; // nothing to check here
            listByYear(tree.Left, year, ref buffer); // search left side first

            VideoGame vg = tree.Data as VideoGame; // try to treat data as VideoGame
            if (vg != null && vg.Releaseyear == year) // check if year matches user input
            {
                buffer += vg.ToString() + Environment.NewLine; // add this game to the result text
            }

            listByYear(tree.Right, year, ref buffer); // then search right side
        }


        //Free space, use as necessary to address task requirements... 
        // I used this space to add the ListByYear method because the task in Exercise 2B
        // asks me to list all games that were released in a year the user gives. The code
        // above does an in-order search through the whole tree and checks each node to see
        // if the game's release year matches the year the user typed. If it matches, the
        // game is added to the text buffer. When the search is finished, the method returns
        // all the matching games as one string. If no games match that year, the method
        // returns a message telling the user that no games were found. This method is here
        // because it is a BSTree task, not an AVLTree task.




    }// End of class
}
