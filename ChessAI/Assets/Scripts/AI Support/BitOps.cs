using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public static class BitOps
    {
        // Class variables
        #region Class variables

        public static readonly int[] index64 = new int[64] {
            0, 47,  1, 56, 48, 27,  2, 60,
           57, 49, 41, 37, 28, 16,  3, 61,
           54, 58, 35, 52, 50, 42, 21, 44,
           38, 32, 29, 23, 17, 11,  4, 62,
           46, 55, 26, 59, 40, 36, 15, 53,
           34, 51, 20, 43, 31, 22, 10, 45,
           25, 39, 14, 33, 19, 30,  9, 24,
           13, 18,  8, 12,  7,  6,  5, 63
        };

        #endregion

        // Class utilities
        #region Class utilities

        // Returns true if the bitboard is empty
        public static bool IsEmpty(ulong bitboard)
        {
            return bitboard == 0;
        }

        // Returns true if the bitboard is single populated / contains only one 1
        public static bool IsSinglePopulated(ulong bitboard)
        {
            return bitboard != 0 && ((bitboard & bitboard - 1) == 0);
        }

        // Returns the number of 1's in a bitboard  
        public static int PopulationCount(ulong bitboard)
        {
            int count = 0;
            while (bitboard != 0)
            {
                count++;
                bitboard &= bitboard - 1; // reset least significant one bit
            }
            return count;
        }

        // Returns the least significant 1 bit 
        public static int BitScanForward(ulong bitboard)
        {
            const ulong debruijn64 = (ulong)(0x03f79d71b4cb0a89);
            Debug.Assert(bitboard != 0);
            return index64[((bitboard ^ (bitboard - 1)) * debruijn64) >> 58];
        }

        #endregion
    }
}

