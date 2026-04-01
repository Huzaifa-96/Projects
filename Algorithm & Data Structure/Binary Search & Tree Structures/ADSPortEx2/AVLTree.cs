using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx2
{
    //AVL Tree implementation for Assessed Exercise 2

    //Hints : 
    //Use lecture materials from Week 6A
    // and lab sheet 'Lab 6: AVL Trees' to aid with implementation...

    //You may need to adjust your other tree classes to allow your AVL tree
    // access to certain attributes and functions

    class AVLTree<T> : BSTree<T> where T : IComparable
    {

        //Functions for EX.2C
        public new void InsertItem(T item)
        {
            root = InsertAVL(root, item); // always replace root with returned balanced subtree
        }
        private Node<T> InsertAVL(Node<T> tree, T item)
        {
            if (tree == null)
                return new Node<T>(item); // base case: create new node

            int compare = item.CompareTo(tree.Data);
            if (compare < 0)
                tree.Left = InsertAVL(tree.Left, item); // recurse left
            else if (compare > 0)
                tree.Right = InsertAVL(tree.Right, item); // recurse right
            else
                tree.Data = item; // update existing value

            UpdateBalance(tree); // recalculate balance factor
            return Rebalance(tree); // rotate if required and return new subtree root
        }

        public new void RemoveItem(T item)
        {
            root = RemoveAVL(root, item); // always rebalance back to root
        }
        private Node<T> RemoveAVL(Node<T> tree, T item)
        {
            if (tree == null)
                return null; // nothing to remove

            int compare = item.CompareTo(tree.Data);
            if (compare < 0)
            {
                tree.Left = RemoveAVL(tree.Left, item); // recurse left
            }
            else if (compare > 0)
            {
                tree.Right = RemoveAVL(tree.Right, item); // recurse right
            }
            else
            {
                // node found: handle removal cases

                if (tree.Left == null && tree.Right == null)
                    return null; // case 1: leaf node

                if (tree.Left == null)
                    return tree.Right; // case 2: only right child

                if (tree.Right == null)
                    return tree.Left; // case 2: only left child

                // case 3: two children
                T successor = LeastItem(tree.Right); // smallest value in right subtree
                tree.Data = successor; // replace current node data
                tree.Right = RemoveAVL(tree.Right, successor); // remove duplicate
            }

            UpdateBalance(tree); // recalculate balance factor after removal
            return Rebalance(tree); // enforce AVL invariant
        }
        //Free space, use as required

        private void UpdateBalance(Node<T> tree)
        {
            if (tree != null)
                tree.BalanceFactor = height(tree.Left) - height(tree.Right); // AVL definition
        }
        private Node<T> Rebalance(Node<T> tree)
        {
            if (tree.BalanceFactor > 1)
            {
                // left-heavy cases
                if (height(tree.Left.Left) < height(tree.Left.Right))
                    tree.Left = RotateLeft(tree.Left); // LR case
                return RotateRight(tree); // LL case
            }

            if (tree.BalanceFactor < -1)
            {
                // right-heavy cases
                if (height(tree.Right.Right) < height(tree.Right.Left))
                    tree.Right = RotateRight(tree.Right); // RL case
                return RotateLeft(tree); // RR case
            }

            return tree; // already balanced
        }
        private Node<T> RotateLeft(Node<T> tree)
        {
            Node<T> newRoot = tree.Right;
            Node<T> movedSubtree = newRoot.Left;

            newRoot.Left = tree;
            tree.Right = movedSubtree;

            UpdateBalance(tree);
            UpdateBalance(newRoot);

            return newRoot;
        }
        private Node<T> RotateRight(Node<T> tree)
        {
            Node<T> newRoot = tree.Left;
            Node<T> movedSubtree = newRoot.Right;

            newRoot.Right = tree;
            tree.Left = movedSubtree;

            UpdateBalance(tree);
            UpdateBalance(newRoot);

            return newRoot;
        }
        private T LeastItem(Node<T> tree)
        {
            while (tree.Left != null)
                tree = tree.Left; // walk to left-most node
            return tree.Data;
        }




    }// End of class
}
