using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Fingering
{
    // Fingering Cost Tables
    // From Bosch et al. (http://web.gps.caltech.edu/~tsai/files/HartBoschTsai_2000.pdf)

    private int[,] lowerWhiteUpperWhite = 
    {
        {1,1,1,1,1,1,1,2,2,2,2,3},
        {1,1,1,1,1,1,1,1,1,2,2,3},
        {2,2,1,1,1,1,1,1,1,1,1,2},
        {3,3,2,2,1,1,1,1,1,1,1,1},
        {2,2,3,3,4,4,4,4,4,4,4,4},
        {1,1,1,1,2,2,3,3,3,3,3,3},
        {2,2,1,1,1,1,2,3,3,3,3,3},
        {3,3,2,2,1,1,1,1,1,2,2,2},
        {2,2,3,3,4,4,4,4,4,4,4,4},
        {1,1,2,2,3,3,3,3,3,3,3,3},
        {3,3,1,1,1,1,3,3,3,3,3,3},
        {2,2,4,4,4,4,4,4,4,4,4,4},
        {1,1,1,1,3,3,3,3,3,3,3,3},
        {4,4,4,4,4,4,4,4,4,4,4,4}
    };
    private int[,] lowerWhiteUpperBlack =
    {
        {1,1,1,1,1,1,1,1,2,2,3,0},
        {1,1,1,1,1,1,1,1,1,1,2,0},
        {2,2,2,1,1,1,1,1,1,1,1,0},
        {3,3,3,2,2,2,1,1,1,1,1,0},
        {4,4,4,4,4,4,4,4,4,4,4,0},
        {1,1,1,2,2,3,3,3,3,3,3,0},
        {2,1,1,1,1,2,2,2,3,3,3,0},
        {3,2,2,2,2,1,1,1,2,2,3,0},
        {4,4,4,4,4,4,4,4,4,4,4,0},
        {1,1,1,3,3,3,3,3,3,3,3,0},
        {3,2,2,2,2,2,3,3,3,3,3,0},
        {4,4,4,4,4,4,4,4,4,4,4,0},
        {2,2,2,2,3,3,3,3,3,3,3,0},
        {4,4,4,4,4,4,4,4,4,4,4,0}
    };
    private int[,] lowerBlackUpperWhite =
    {
        {3,2,2,1,1,2,2,2,3,3,3,0},
        {3,2,2,1,1,1,2,2,2,2,3,0},
        {3,3,3,1,1,1,1,1,2,2,2,0},
        {3,3,3,2,2,2,1,1,1,1,1,0},
        {2,3,3,4,4,4,4,4,4,4,4,0},
        {1,1,1,2,2,3,3,3,3,3,3,0},
        {2,1,1,1,1,2,3,3,3,3,3,0},
        {3,2,2,1,1,1,1,1,1,1,2,0},
        {2,3,3,4,4,4,4,4,4,4,4,0},
        {1,1,1,3,3,3,3,3,3,3,3,0},
        {2,1,1,1,1,1,2,2,3,3,3,0},
        {3,4,4,4,4,4,4,4,4,4,4,0},
        {1,1,1,2,2,3,3,3,3,3,3,0},
        {3,4,4,4,4,4,4,4,4,4,4,0}
    };
    private int[,] lowerBlackUpperBlack =
    {
        {0,2,2,2,2,0,3,3,3,3,0,3},
        {0,2,2,2,2,0,2,2,2,2,0,2},
        {0,3,2,2,2,0,2,1,1,1,0,2},
        {0,3,3,3,3,0,2,1,1,1,0,1},
        {0,2,3,3,4,0,4,4,4,4,0,4},
        {0,1,1,1,2,0,3,3,3,3,0,3},
        {0,2,1,1,1,0,2,3,3,3,0,3},
        {0,3,2,2,1,0,1,1,1,2,0,2},
        {0,3,4,4,4,0,4,4,4,4,0,4},
        {0,1,1,1,2,0,3,3,3,3,0,3},
        {0,3,1,1,2,0,3,3,3,3,0,3},
        {0,4,4,4,4,0,4,4,4,4,0,4},
        {0,2,2,2,3,0,3,3,3,3,0,3},
        {0,4,4,4,4,0,4,4,4,4,0,4}
    };

    private FingerPair[] fingers =
    {
        new FingerPair() { lower = 1, upper = 2 },
        new FingerPair() { lower = 1, upper = 3 },
        new FingerPair() { lower = 1, upper = 4 },
        new FingerPair() { lower = 1, upper = 5 },
        new FingerPair() { lower = 2, upper = 1 },
        new FingerPair() { lower = 2, upper = 3 },
        new FingerPair() { lower = 2, upper = 4 },
        new FingerPair() { lower = 2, upper = 5 },
        new FingerPair() { lower = 3, upper = 1 },
        new FingerPair() { lower = 3, upper = 4 },
        new FingerPair() { lower = 3, upper = 5 },
        new FingerPair() { lower = 4, upper = 1 },
        new FingerPair() { lower = 4, upper = 5 },
        new FingerPair() { lower = 5, upper = 1 },
    };

    // ================================

    private PlayPiano piano;

    public Fingering(PlayPiano piano)
    {
        this.piano = piano;
    }

    /// <summary>
    /// Uses a fingering algorithm to determine the optimal finger to follow the currently used one. Input and output finger is from (1-5).
    /// </summary>
    /// <param name="currentFinger"></param>
    /// <param name="lowerNote"></param>
    /// <param name="upperNote"></param>
    /// <param name="isDecreasing"></param>
    /// <returns></returns>
    public int GetOptimalFinger(int currentFinger, int lowerNote, int upperNote, bool isDecreasing)
    {
        int interval = (upperNote - lowerNote) - 1;
        bool lowerBlack = piano.IsBlackKey(lowerNote);
        bool upperBlack = piano.IsBlackKey(upperNote);

        // Determine, which cost table to choose from
        int[,] costTable;
        if (!lowerBlack && !upperBlack)
            costTable = lowerWhiteUpperWhite;
        else if (!lowerBlack && upperBlack)
            costTable = lowerWhiteUpperBlack;
        else if (lowerBlack && !upperBlack)
            costTable = lowerBlackUpperWhite;
        else
            costTable = lowerBlackUpperBlack;

        // Get cost table column of interval
        int[] col = costTable.GetColumn(interval);

        // Search for index with minimal cost
        int minCost = 9999;
        int minIndex = -1;
        for (int i = 0; i < col.Length; i++)
        {
            // Only consider cost, if current finger matches
            if ((!isDecreasing && fingers[i].lower == currentFinger) || (isDecreasing && fingers[i].upper == currentFinger))
            {
                if (col[i] < minCost)
                {
                    minCost = col[i];
                    minIndex = i;
                }
            }
        }

        int optimalFinger = !isDecreasing ? fingers[minIndex].upper : fingers[minIndex].lower;
        return optimalFinger;
    }


    private struct FingerPair
    {
        public int lower;
        public int upper;
    }
    
}

public static class ArrayExt
{
    public static T[] GetRow<T>(this T[,] array, int row)
    {
        if (!typeof(T).IsPrimitive)
            throw new InvalidOperationException("Not supported for managed types.");

        if (array == null)
            throw new ArgumentNullException("array");

        int cols = array.GetUpperBound(1) + 1;
        T[] result = new T[cols];

        int size = 0;

        if (typeof(T) == typeof(bool))
            size = 1;
        else if (typeof(T) == typeof(char))
            size = 2;
        else if (typeof(T) == typeof(int))
            size = 4;

        Buffer.BlockCopy(array, row * cols * size, result, 0, cols * size);

        return result;
    }

    public static T[] GetColumn<T>(this T[,] matrix, int columnNumber)
    {
        return Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, columnNumber])
                .ToArray();
    }
}

