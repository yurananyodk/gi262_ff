using UnityEngine;

namespace Assignment
{
    public interface IAssignment
    {

        #region Lecture

        void LCT01_RecursiveFactorial(int n);
        void LCT02_RecursiveFibonacci(int n);
        void LCT03_RecursiveSumOfOneToN(int n);
        void LCT04_RecursiveSumOfNumbers(int[] numbers);

        #endregion

        #region Assignment

        void ASN01_RecursivePower(int baseNum, int exponent);
        void ASN02_IsPalindrome(string str);
        void ASN03_RecursiveGCD(int a, int b);
        void ASN04_RecursiveBinarySearch(int[] arr, int target);

        #endregion

        #region Extra

        void EX01_FindWords(string[,] board, string[] words);

        #endregion

    }
}