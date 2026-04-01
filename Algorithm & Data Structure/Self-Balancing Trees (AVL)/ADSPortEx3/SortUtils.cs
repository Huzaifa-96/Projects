using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx3
{
    //Generic QuickSort implementation for Assessed Exercise 3

    //Hints : 
    //Use lecture materials from Week 7 to aid with implementation.

    //Use the provided QuickSort function as a starting point for
    // adjusting the functionality to make it generic. Make use of IComparable
    // as you have done during other structure implementations

    //Your implemented function can be utilised as part EX.3C

    class SortUtils
    {
        static void swap(ref int x, ref int y)
        {
            int temp = x;
            x = y;
            y = temp;
        }

        static public void QuickSort(int[] items, int left, int right)
        {
            int i, j;
            i = left; j = right;
            int pivot = items[left];

            while (i <= j)
            {
                for (; (items[i] < pivot) && (i < right); i++) ;
                for (; (pivot < items[j]) && (j > left); j--) ;

                if (i <= j)
                    swap(ref items[i++], ref items[j--]);
            }

            if (left < j) QuickSort(items, left, j);
            if (i < right) QuickSort(items, i, right);
        }


        static public void QuickSortGen<T>(T[] a) where T : IComparable
        {
            if (a == null) return; // no array given, nothing to sort
            if (a.Length <= 1) return; // 0 or 1 item is already sorted
            QuickSortGen(a, 0, a.Length - 1); // sort the full array range
        }
        // This helper does the real recursive partitioning.
        // left and right define the current part of the array being sorted.
        static private void QuickSortGen<T>(T[] a, int left, int right) where T : IComparable
        {
            int i = left; // i will move right looking for items that belong on the other side
            int j = right; // j will move left looking for items that belong on the other side
            T pivot = a[left]; // pivot chosen from the left index (same approach as lecture examples)

            while (i <= j) // keep partitioning until i and j cross
            {
                while (a[i].CompareTo(pivot) < 0 && i < right) i++; // move i until a[i] >= pivot
                while (pivot.CompareTo(a[j]) < 0 && j > left) j--; // move j until a[j] <= pivot

                if (i <= j) // if pointers have not crossed
                {
                    T temp = a[i]; // store a[i] for swapping
                    a[i] = a[j]; // put a[j] into a[i]
                    a[j] = temp; // put old a[i] into a[j]
                    i++; // move i forward after swap
                    j--; // move j backward after swap
                }
            }

            if (left < j) QuickSortGen(a, left, j); // sort the left partition (items <= pivot region)
            if (i < right) QuickSortGen(a, i, right); // sort the right partition (items >= pivot region)
        }


    }
}
