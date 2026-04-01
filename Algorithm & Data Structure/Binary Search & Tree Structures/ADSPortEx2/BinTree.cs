using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx2
{
    //Binary Tree implementation for Assessed Exercise 2

    //Hints : 
    //Use lecture materials from Week 4B
    // and lab sheet 'Lab 5: BinTree and BSTree' to aid with implementation...

    class BinTree<T> where T : IComparable
    {
        protected Node<T> root; // top node of the tree (starting point)
        public BinTree()
        {
            root = null; // make an empty tree with no nodes
        }

        public BinTree(Node<T> node)
        {
            root = node; // make a tree where the given node is the root
        }


        //Functions for EX.2A
        public void InOrder(ref string buffer)
        {
            InOrder(root, ref buffer); // start the in-order visit at the root
        }
        private void InOrder(Node<T> tree, ref string buffer)
        {
            if (tree != null) // only work if this node is not empty
            {
                InOrder(tree.Left, ref buffer); // first visit all nodes in left subtree
                buffer += tree.Data.ToString() + Environment.NewLine; // then add this node's data to the text
                InOrder(tree.Right, ref buffer); // finally visit all nodes in right subtree
            }
        }

        public void PreOrder(ref string buffer)
        {
            PreOrder(root, ref buffer); // start pre-order visit at the root
        }
        private void PreOrder(Node<T> tree, ref string buffer)
        {
            if (tree != null)
            {
                buffer += tree.Data.ToString() + Environment.NewLine; // first add this node's data
                PreOrder(tree.Left, ref buffer); // then visit left subtree
                PreOrder(tree.Right, ref buffer); // then visit right subtree
            }
        }

        public void PostOrder(ref string buffer)
        {
            PostOrder(root, ref buffer); // start post-order visit at the root
        }

        private void PostOrder(Node<T> tree, ref string buffer)
        {
            if (tree != null)
            {
                PostOrder(tree.Left, ref buffer); // first visit left subtree
                PostOrder(tree.Right, ref buffer); // then visit right subtree
                buffer += tree.Data.ToString() + Environment.NewLine; // finally add this node's data
            }
        }


        //Free space, use as necessary to address task requirements... 
        // No extra methods were added here. The BinTree class only needs the three
        // traversal methods for this exercise. All other required operations, such
        // as inserting, updating, counting games, finding earliest year and
        // balancing the tree, are implemented in the BSTree and AVLTree classes.
        // So this class stays simple and acts as the base tree structure.





    } // End of class
}
