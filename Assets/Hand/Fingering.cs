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

    private List<FingerPair> fingers = new List<FingerPair>()
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

    public int[] GetOptimalFingersDP(int[] notes)
    {
        int[,] f = new int[notes.Length - 1, 5];
        List<int>[,] minimizerTable = new List<int>[notes.Length - 1, 5];

        // ======================================================
        // STEP 1: CALCULATE COST TABLES f[i](s)
        for (int i = notes.Length - 1; i >= 1; i--)
        {
            // Consider the two current notes that form the next interval
            int n1 = notes[i];
            int n2 = notes[i - 1];

            // Determine upper and lower note of current interval
            int lowerNote = n2;
            int upperNote = n1;
            if (n1 < n2)
            {
                lowerNote = n1;
                upperNote = n2;
            }

            int interval = (upperNote - lowerNote) - 1;
            bool lowerBlack = piano.IsBlackKey(lowerNote);
            bool upperBlack = piano.IsBlackKey(upperNote);
            bool isDecreasing = n1 < n2;

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

            // Interval = -1 -> same note -> use finger from previous minimizer table
            if (interval == -1)
            {
                for (int y = 0; y < 5; y++)
                    minimizerTable[i - 1, y] = new List<int>() { minimizerTable[i, y][0] };
                continue;
            }

            // Intervals > 12 should be considered an octave too
            interval = Math.Min(interval, 11);

            // Get cost table column of interval
            int[] costColumn = costTable.GetColumn(interval);

            // Calculate f[i](s)
            for (int s = 0; s < 5; s++)
            {
                int minCost = 9999;
                int[] sCosts = new int[5];

                for (int x = 0; x < 5; x++)
                {
                    // Get f(s) from the previous intervals (unless first)
                    int previousCost = 0;
                    if (i < notes.Length - 1)
                        previousCost = f[i, x]; // x is the previous s

                    // Get cost table entry for s and x (don't forget the +1)
                    int low = isDecreasing ? x + 1 : s + 1;
                    int upp = isDecreasing ? s + 1 : x + 1;
                    int idx = 0;
                    //int idx = fingers.Select((pair, pairIdx) => new { pair, pairIdx }).First(a => a.pair.lower == low && a.pair.upper == upp).pairIdx;
                    //int idx = fingers.IndexOf(fingers.First(pair => pair.lower == low && pair.upper == upp));
                    for (int y = 0; y < fingers.Count; y++)
                    {
                        if (fingers[y].lower == low && fingers[y].upper == upp)
                        {
                            idx = y;
                            break;
                        }
                    }
                    int cost = costColumn[idx];

                    cost += previousCost;

                    // Impossible fingering -> extra high cost (for convenience to keep the zeroes)
                    if (cost == 0)
                        cost = 99;

                    sCosts[x] = cost;
                    if (cost <= minCost)
                    {
                        minCost = cost;
                    }
                }

                f[i - 1, s] = minCost;
                for (int x = 0; x < 5; x++)
                {
                    if (sCosts[x] == minCost)
                    {
                        if (minimizerTable[i - 1, s] == null)
                            minimizerTable[i - 1, s] = new List<int>();
                        minimizerTable[i - 1, s].Add(x);
                    }
                }

            }
            
        }

        // ======================================================
        // STEP 2: REVERSE COST TABLES TO FIND OPTIMAL FINGERING
        int[] optimalFingers = new int[notes.Length];

        // Calculate the very first s by looking at the minimal cost
        int firstMin = 9999;
        int firstFinger = 0;
        int firstS = 0;
        for (int s = 0; s < 5; s++)
        {
            int cost = f[0, s];
            if (cost < firstMin)
            {
                firstMin = cost;
                firstFinger = s;
                firstS = minimizerTable[0, s][0]; // Just use first finger for now (if multiple possibilities exist) 
            }
        }

        optimalFingers[0] = firstFinger + 1;
        optimalFingers[1] = firstS + 1;

        // Proceed until the end with the given s values
        int nextS = firstS;
        for (int i = 2; i < notes.Length - 1; i++)
        {
            Debug.Log(nextS);
            int finger = minimizerTable[i - 1, nextS][0]; // Just use first finger for now (if multiple possibilities exist)
            optimalFingers[i] = finger + 1;
            nextS = finger;
        }

        return optimalFingers;
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

