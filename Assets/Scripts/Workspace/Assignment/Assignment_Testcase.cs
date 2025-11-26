using NUnit.Framework;
using UnityEngine;
using AssignmentSystem.Services;
using System.Linq;
using NUnit.Framework.Internal;

namespace Assignment
{
    public class Assignment_Testcase
    {
        private IAssignment assignment;

        [SetUp]
        public void Setup()
        {
            // Reset static state before each test
            AssignmentDebugConsole.Clear();

            // Use StudentSolution as the test subject
            assignment = new StudentSolution();
        }

        [TearDown]
        public void Teardown()
        {
            if (assignment is MonoBehaviour)
            {
                MonoBehaviour.DestroyImmediate(assignment as MonoBehaviour);
            }
        }

        #region Lecture

        [Category("Lecture")]
        [TestCase(5, "120", TestName = "LCT01_RecursiveFactorial_5", Description = "Factorial of 5")]
        [TestCase(0, "1", TestName = "LCT01_RecursiveFactorial_0", Description = "Factorial of 0")]
        [TestCase(1, "1", TestName = "LCT01_RecursiveFactorial_1", Description = "Factorial of 1")]
        [TestCase(3, "6", TestName = "LCT01_RecursiveFactorial_3", Description = "Factorial of 3")]
        [TestCase(4, "24", TestName = "LCT01_RecursiveFactorial_4", Description = "Factorial of 4")]
        [TestCase(6, "720", TestName = "LCT01_RecursiveFactorial_6", Description = "Factorial of 6")]
        public void Test_LCT01_RecursiveFactorial(int n, string expected)
        {
            assignment.LCT01_RecursiveFactorial(n);
            string actual = AssignmentDebugConsole.GetOutput().Trim();
            Assert.AreEqual(expected, actual, $"Expected output is {expected} but actual output is {actual}");
        }

        [Category("Lecture")]
        [TestCase(6, "8", TestName = "LCT02_RecursiveFibonacci_6", Description = "Fibonacci of 6")]
        [TestCase(0, "0", TestName = "LCT02_RecursiveFibonacci_0", Description = "Fibonacci of 0")]
        [TestCase(1, "1", TestName = "LCT02_RecursiveFibonacci_1", Description = "Fibonacci of 1")]
        [TestCase(2, "1", TestName = "LCT02_RecursiveFibonacci_2", Description = "Fibonacci of 2")]
        [TestCase(3, "2", TestName = "LCT02_RecursiveFibonacci_3", Description = "Fibonacci of 3")]
        [TestCase(4, "3", TestName = "LCT02_RecursiveFibonacci_4", Description = "Fibonacci of 4")]
        [TestCase(5, "5", TestName = "LCT02_RecursiveFibonacci_5", Description = "Fibonacci of 5")]
        public void Test_LCT02_RecursiveFibonacci(int n, string expected)
        {
            assignment.LCT02_RecursiveFibonacci(n);
            string actual = AssignmentDebugConsole.GetOutput().Trim();
            Assert.AreEqual(expected, actual, $"Expected output is {expected} but actual output is {actual}");
        }

        [Category("Lecture")]
        [TestCase(5, "15", TestName = "LCT03_RecursiveSumOfOneToN_5", Description = "Sum from 1 to 5")]
        [TestCase(0, "0", TestName = "LCT03_RecursiveSumOfOneToN_0", Description = "Sum from 1 to 0")]
        [TestCase(1, "1", TestName = "LCT03_RecursiveSumOfOneToN_1", Description = "Sum from 1 to 1")]
        [TestCase(3, "6", TestName = "LCT03_RecursiveSumOfOneToN_3", Description = "Sum from 1 to 3")]
        [TestCase(10, "55", TestName = "LCT03_RecursiveSumOfOneToN_10", Description = "Sum from 1 to 10")]
        [TestCase(2, "3", TestName = "LCT03_RecursiveSumOfOneToN_2", Description = "Sum from 1 to 2")]
        public void Test_LCT03_RecursiveSumOfOneToN(int n, string expected)
        {
            assignment.LCT03_RecursiveSumOfOneToN(n);
            string actual = AssignmentDebugConsole.GetOutput().Trim();
            Assert.AreEqual(expected, actual, $"Expected output is {expected} but actual output is {actual}");
        }

        [Category("Lecture")]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, "15", TestName = "LCT04_RecursiveSumOfNumbers_Basic", Description = "Sum of array")]
        [TestCase(new int[] { }, "0", TestName = "LCT04_RecursiveSumOfNumbers_Empty", Description = "Sum of empty array")]
        [TestCase(new int[] { 10 }, "10", TestName = "LCT04_RecursiveSumOfNumbers_Single", Description = "Sum of single element")]
        [TestCase(new int[] { 1, 2, 3 }, "6", TestName = "LCT04_RecursiveSumOfNumbers_Small", Description = "Sum of small array")]
        [TestCase(new int[] { 0, 0, 0 }, "0", TestName = "LCT04_RecursiveSumOfNumbers_Zeros", Description = "Sum of zeros")]
        [TestCase(new int[] { -1, 1 }, "0", TestName = "LCT04_RecursiveSumOfNumbers_Negative", Description = "Sum with negative")]
        public void Test_LCT04_RecursiveSumOfNumbers(int[] numbers, string expected)
        {
            assignment.LCT04_RecursiveSumOfNumbers(numbers);
            string actual = AssignmentDebugConsole.GetOutput().Trim();
            Assert.AreEqual(expected, actual, $"Expected output is {expected} but actual output is {actual}");
        }

        #endregion

        #region Assignment

        [Category("Assignment")]
        [TestCase(2, 3, "8", TestName = "ASN01_RecursivePower_2_3", Description = "2^3")]
        [TestCase(5, 0, "1", TestName = "ASN01_RecursivePower_5_0", Description = "5^0")]
        [TestCase(3, 2, "9", TestName = "ASN01_RecursivePower_3_2", Description = "3^2")]
        [TestCase(4, 2, "16", TestName = "ASN01_RecursivePower_4_2", Description = "4^2")]
        [TestCase(1, 5, "1", TestName = "ASN01_RecursivePower_1_5", Description = "1^5")]
        [TestCase(10, 1, "10", TestName = "ASN01_RecursivePower_10_1", Description = "10^1")]
        public void Test_ASN01_RecursivePower(int baseNum, int exponent, string expected)
        {
            assignment.ASN01_RecursivePower(baseNum, exponent);
            string actual = AssignmentDebugConsole.GetOutput().Trim();
            Assert.AreEqual(expected, actual, $"Expected output is {expected} but actual output is {actual}");
        }

        [Category("Assignment")]
        [TestCase("radar", "is a palindrome", TestName = "ASN02_IsPalindrome_Radar", Description = "Radar is palindrome")]
        [TestCase("hello", "is not a palindrome", TestName = "ASN02_IsPalindrome_Hello", Description = "Hello is not palindrome")]
        [TestCase("a", "is a palindrome", TestName = "ASN02_IsPalindrome_Single", Description = "Single char is palindrome")]
        [TestCase("", "is a palindrome", TestName = "ASN02_IsPalindrome_Empty", Description = "Empty string is palindrome")]
        [TestCase("aba", "is a palindrome", TestName = "ASN02_IsPalindrome_Aba", Description = "Aba is palindrome")]
        [TestCase("abcba", "is a palindrome", TestName = "ASN02_IsPalindrome_Abcba", Description = "Abcba is palindrome")]
        [TestCase("ab", "is not a palindrome", TestName = "ASN02_IsPalindrome_Ab", Description = "Ab is not palindrome")]
        [TestCase("aa", "is a palindrome", TestName = "ASN02_IsPalindrome_Aa", Description = "Aa is palindrome")]
        public void Test_ASN02_IsPalindrome(string str, string expected)
        {
            assignment.ASN02_IsPalindrome(str);
            string actual = AssignmentDebugConsole.GetOutput().Trim();
            Assert.AreEqual(expected, actual, $"Expected output is {expected} but actual output is {actual}");
        }

        [Category("Assignment")]
        [TestCase(48, 18, "6", TestName = "ASN03_RecursiveGCD_48_18", Description = "GCD of 48 and 18")]
        [TestCase(100, 75, "25", TestName = "ASN03_RecursiveGCD_100_75", Description = "GCD of 100 and 75")]
        [TestCase(7, 3, "1", TestName = "ASN03_RecursiveGCD_7_3", Description = "GCD of 7 and 3")]
        [TestCase(54, 24, "6", TestName = "ASN03_RecursiveGCD_54_24", Description = "GCD of 54 and 24")]
        [TestCase(17, 13, "1", TestName = "ASN03_RecursiveGCD_17_13", Description = "GCD of 17 and 13")]
        [TestCase(25, 15, "5", TestName = "ASN03_RecursiveGCD_25_15", Description = "GCD of 25 and 15")]
        public void Test_ASN03_RecursiveGCD(int a, int b, string expected)
        {
            assignment.ASN03_RecursiveGCD(a, b);
            string actual = AssignmentDebugConsole.GetOutput().Trim();
            Assert.AreEqual(expected, actual, $"Expected output is {expected} but actual output is {actual}");
        }

        [Category("Assignment")]
        [TestCase(new int[] { 1, 3, 5, 7, 9 }, 5, "2", TestName = "ASN04_RecursiveBinarySearch_Found", Description = "Target found")]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, 6, "-1", TestName = "ASN04_RecursiveBinarySearch_NotFound", Description = "Target not found")]
        [TestCase(new int[] { 10 }, 10, "0", TestName = "ASN04_RecursiveBinarySearch_Single", Description = "Single element found")]
        [TestCase(new int[] { 1, 2, 3 }, 0, "-1", TestName = "ASN04_RecursiveBinarySearch_NotFoundLow", Description = "Target less than all")]
        [TestCase(new int[] { 2, 4, 6, 8, 10 }, 4, "1", TestName = "ASN04_RecursiveBinarySearch_Even", Description = "Target in even position")]
        [TestCase(new int[] { 1, 3, 5 }, 1, "0", TestName = "ASN04_RecursiveBinarySearch_First", Description = "Target is first")]
        [TestCase(new int[] { 1, 2, 3, 4 }, 5, "-1", TestName = "ASN04_RecursiveBinarySearch_NotFoundHigh", Description = "Target greater than all")]
        [TestCase(new int[] { 5 }, 6, "-1", TestName = "ASN04_RecursiveBinarySearch_SingleNotFound", Description = "Single element not found")]
        public void Test_ASN04_RecursiveBinarySearch(int[] arr, int target, string expected)
        {
            assignment.ASN04_RecursiveBinarySearch(arr, target);
            string actual = AssignmentDebugConsole.GetOutput().Trim();
            Assert.AreEqual(expected, actual, $"Expected output is {expected} but actual output is {actual}");
        }

        #endregion


        #region Extra

        [Category("Extra")]
        [TestCase]
        public void EX01_FindWords_Pattern01()
        {
            string[,] board = new string[,]
            {
                { "o", "a", "a", "n" },
                { "e", "t", "a", "e" },
                { "i", "h", "k", "r" },
                { "i", "f", "l", "v" }
            };

            string[] words = new string[] { "oath", "pea", "eat", "rain" };
            assignment.EX01_FindWords(board, words);
            string[] expected = new string[] { "oath", "eat" };

            string output = AssignmentDebugConsole.GetOutput().Trim();
            string[] actual = output.Split("\n").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            Assert.AreEqual(expected.Length, actual.Length);
            foreach (var word in expected)
            {
                Debug.Log($"Checking for expected word: {word}");
                if (!actual.Contains(word))
                {
                    Assert.Fail($"Expected found word should be \n{string.Join('\n', expected)}\nBut actual output is \n{string.Join('\n', actual)}");
                }
            }
        }

        [Category("Extra")]
        [TestCase]
        public void EX01_FindWords_Pattern02()
        {
            string[,] board = new string[,]
            {
                { "a", "b" },
                { "c", "d" }
            };

            string[] words = new string[] { "abcb" };
            assignment.EX01_FindWords(board, words);
            string[] expected = new string[] { };

            string output = AssignmentDebugConsole.GetOutput().Trim();
            string[] actual = output.Split("\n").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            Assert.AreEqual(expected.Length, actual.Length);
            foreach (var word in expected)
            {
                Debug.Log($"Checking for expected word: {word}");
                if (!actual.Contains(word))
                {
                    Assert.Fail($"Expected found word should be \n{string.Join('\n', expected)}\nBut actual output is \n{string.Join('\n', actual)}");
                }
            }
        }

        [Category("Extra")]
        [TestCase]
        public void EX01_FindWords_Pattern03()
        {
            string[,] board = new string[,]
            {
                { "a", "b", "c" },
                { "a", "e", "d" },
                { "a", "f", "g" }
            };

            string[] words = new string[] { "abcdefg" };
            assignment.EX01_FindWords(board, words);
            string[] expected = new string[] { "abcdefg" };

            string output = AssignmentDebugConsole.GetOutput().Trim();
            string[] actual = output.Split("\n").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            Assert.AreEqual(expected.Length, actual.Length);
            foreach (var word in expected)
            {
                Debug.Log($"Checking for expected word: {word}");
                if (!actual.Contains(word))
                {
                    Assert.Fail($"Expected found word should be \n{string.Join('\n', expected)}\nBut actual output is \n{string.Join('\n', actual)}");
                }
            }
        }

        [Category("Extra")]
        [TestCase]
        public void EX01_FindWords_Pattern04()
        {
            string[,] board = new string[,]
            {
                { "a", "b", "c", "e" },
                { "s", "f", "c", "s" },
                { "a", "d", "e", "e" }
            };

            string[] words = new string[] { "see", "abcced", "abcb" };
            assignment.EX01_FindWords(board, words);
            string[] expected = new string[] { "see", "abcced" };

            string output = AssignmentDebugConsole.GetOutput().Trim();
            string[] actual = output.Split("\n").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            Assert.AreEqual(expected.Length, actual.Length);
            foreach (var word in expected)
            {
                Debug.Log($"Checking for expected word: {word}");
                if (!actual.Contains(word))
                {
                    Assert.Fail($"Expected found word should be \n{string.Join('\n', expected)}\nBut actual output is \n{string.Join('\n', actual)}");
                }
            }
        }

        [Category("Extra")]
        [TestCase]
        public void EX01_FindWords_Pattern05()
        {
            string[,] board = new string[,]
            {
                { "t", "h", "i", "s", "i", "s", "a" },
                { "s", "i", "m", "p", "l", "e", "w" },
                { "i", "m", "p", "l", "e", "x", "o" },
                { "l", "i", "f", "e", "s", "t", "r" },
                { "l", "d", "a", "n", "u", "y", "d" },
                { "y", "r", "f", "m", "l", "p", "m" },
                { "e", "a", "s", "y", "r", "e", "a" },
                { "w", "o", "r", "l", "d", "s", "l" },
                { "b", "i", "g", "t", "e", "s", "t" },
                { "c", "o", "m", "p", "l", "e", "x" }
            };

            string[] words = new string[] {
                "this", "simple", "life", "world", "test", "complex",
                "impossible", "verylongwordthatdoesnotexist", "xyz",
                "word", "big", "easy", "hard", "path", "grid",
                "thi", "sim", "wor", "tes", "com", "pat", "row", "col"
            };
            assignment.EX01_FindWords(board, words);
            string[] expected = new string[] {
                "this",
                "simple",
                "life",
                "world",
                "test",
                "complex",
                "word",
                "big",
                "easy",
                "thi",
                "sim",
                "wor",
                "tes",
                "com",
                "row",
            };

            string output = AssignmentDebugConsole.GetOutput().Trim();
            string[] actual = output.Split("\n").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            Assert.AreEqual(expected.Length, actual.Length);
            foreach (var word in expected)
            {
                Debug.Log($"Checking for expected word: {word}");
                if (!actual.Contains(word))
                {
                    Assert.Fail($"Expected found word should be \n{string.Join('\n', expected)}\nBut actual output is \n{string.Join('\n', actual)}");
                }
            }
        }


        #endregion

    }
}