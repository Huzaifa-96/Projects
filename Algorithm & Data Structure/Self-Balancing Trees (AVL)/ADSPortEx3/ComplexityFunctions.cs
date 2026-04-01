using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx3
{
    class ComplexityFunctions
    {
        //Big O calculation for Assessed Exercise 3A

        //Hints : 
        //Use lecture materials from Week 6B to aid with calculations.

        //See ExampleQuestion() to see a suggested format for showing your
        // working out and final answer, both of which must be shown with correct
        // answers for full marks.


        public void ExampleQuestion()
        {
            //F.C
            Console.WriteLine("Enter a number 1:");          // 1
            int num1 = Int32.Parse(Console.ReadLine());      // 1

            Console.WriteLine("Enter a number 2:");          // 1
            int num2 = Int32.Parse(Console.ReadLine());      // 1

            for (int i = 1; i <= num2; i++)                  // n + 1
            {
                Console.WriteLine(num1.ToString() + " X "
                    + i.ToString() + " = " + (num1 * i));   // n
            }

            Console.WriteLine("End of program...");         // 1

            Console.ReadLine();                             // 1


            //Working out

            // 7 + 2n - All F.Cs added together
            // 2n     - Constants removed
            // n      - Coefficients removed
            // O(n)   - Final answer
        }

        public void Algorithm1()
        {
            int n = Int32.Parse(Console.ReadLine()); // read n (size of input)
            List<int> numbers = new List<int>(); // create a list to store n numbers
            for (int i = 0; i < n; i++)
            {
                numbers.Add(i * 2); // O(1) add each time, total O(n)
            }
            int total = 0; // holds the running total
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    if (numbers[j] % 3 == 0) // constant-time check each inner loop
                        total += numbers[j]; // constant-time add (worst case happens often)
                }
            }
            Console.WriteLine("Total: " + total);
        }
        // Task A: Big O for Algorithm1
        // Build list: for loop runs n times -> O(n)
        //
        // Nested loops:
        // Outer loop runs n times.
        // Inner loop runs (n - i) times for each outer loop.
        //
        // Total operations:
        // = n + (n-1) + (n-2) + ... + 1
        // = n(n+1)/2
        // = (n^2 + n) / 2
        // Drop constants and lower-order term -> O(n^2)
        //
        // Example:
        // If n = 5:
        // Inner loop runs: 5 + 4 + 3 + 2 + 1 = 15 times
        // Growth increases roughly with n^2 as n gets larger.
        //
        // Final answer for Algorithm1: O(n^2)


        public void Algorithm2()
        {

            int n = Int32.Parse(Console.ReadLine()); // read n (upper limit)
            int primeCount = 0;

            for (int i = 2; i <= n; i++)
            {
                bool isPrime = true;  // assume i is prime until proven not prime
                int j = 2;
                while (j * j <= i) // only check up to sqrt(i)
                {
                    if (i % j == 0)
                    {
                        isPrime = false;
                        break;
                    }
                    j++;
                }
                if (isPrime)
                    primeCount++; // if not divisible by anything, count it as prime

            }
            Console.WriteLine("Number of primes up to " + n + ": " + primeCount);

            // Task A: Big O for Algorithm2
            // Outer loop runs for i = 2..n -> about n iterations
            // Inner while loop checks divisibility up to sqrt(i) in the worst case
            // (this happens when i is a prime number)
            //
            // Worst-case total work:
            // ≈ sqrt(2) + sqrt(3) + ... + sqrt(n)
            // This sum grows on the order of n * sqrt(n)
            //
            // Example:
            // If n = 9:
            // i = 2 -> sqrt(2) ≈ 1
            // i = 3 -> sqrt(3) ≈ 1
            // i = 4 -> sqrt(4) = 2
            // i = 5 -> sqrt(5) ≈ 2
            // i = 6 -> sqrt(6) ≈ 2
            // i = 7 -> sqrt(7) ≈ 2
            // i = 8 -> sqrt(8) ≈ 2
            // i = 9 -> sqrt(9) = 3
            // Total checks ≈ 15
            // As n increases, this total grows roughly like n * sqrt(n).
            //
            // Final answer for Algorithm2: O(n * sqrt(n))
        }

    }
}
