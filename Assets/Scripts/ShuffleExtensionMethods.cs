using System.Collections.Generic;
using UnityEngine;

public static class ShuffleExtensionMethods
{
    /// <summary>
    /// Generic list shuffler
    /// </summary>
    /// <typeparam name="T">Type of items contained in list</typeparam>
    /// <param name="list">The list to shuffle</param>
    public static void Shuffle<T>(this List<T> list)
    {
        // Shuffle by swapping each item position with another in the list's range
        for (int i = 0; i < list.Count; i++)
        {
            int swapIdx = Random.Range(0, list.Count);
            T tempItem = list[i];
            list[i] = list[swapIdx];
            list[swapIdx] = tempItem;
        }
    }

    /// <summary>
    /// Generic stack shuffler
    /// </summary>
    /// <typeparam name="T">Type of items contained in stack</typeparam>
    /// <param name="stack">The stack to shuffle</param>
    public static void Shuffle<T>(this Stack<T> stack)
    {
        List<T> stackList = new List<T>(stack);
        stackList.Shuffle();

        stack.Clear();
        foreach(T item in stackList)
        {
            stack.Push(item);
        }
    }
}