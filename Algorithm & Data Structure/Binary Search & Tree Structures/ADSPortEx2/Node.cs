using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx2
{
    //Node (tree) implementation for Assessed Exercise 2

    //Hints : 
    //Use lecture materials from Week 4B
    // and lab sheet 'Lab 5: BinTree and BSTree' to aid with implementation.

    class Node<T> where T : IComparable
    {
        private T data; // this holds the value stored in the node
        public Node<T> Left, Right; // links to the left and right child nodes
        private int balanceFactor = 0; // used by AVL tree to track balance


        public Node(T item)
        {
            data = item; // store the value passed in
            Left = null; // start with no left child
            Right = null; // start with no right child
        }

        public T Data
        {
            get { return data; } // return the stored value
            set { data = value; } // update the stored value
        }
        public int BalanceFactor
        {
            get { return balanceFactor; } // return current balance factor
            set { balanceFactor = value; } // update balance factor (used for AVL rotations)
        }

    } // End of class
}
