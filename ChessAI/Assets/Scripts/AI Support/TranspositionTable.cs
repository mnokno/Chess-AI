using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public class TranspositionTable
    {
        #region Class variables

        // Array storing all the has entries
        public HashEntry[] entries;
        // Size of the hash table
        public readonly ulong size;
        // This constant is returned if evaluation lookup failed (the position was not found in the table)
        public const int LookupFailed = -2000000000;

        #endregion

        #region Constructor

        public TranspositionTable(ulong size)
        {
            // Saves parameters
            this.size = size;
            // Initializes an empty table
            entries = new HashEntry[size];
            Clear();
        }

        #endregion

        #region Utility

        // Clears the table
        public void Clear()
        {
            // Replaces each entry with a new clear entry
            for (ulong i = 0; i < size; i++)
            {
                entries[i] = new HashEntry();
            }
        }

        // Adds entry to the hash table
        public void AddEntry(byte depth, NodeType nodeType, int score, ulong zobristKey, Bitboard bitboard) // FOR TESTING , Bitboard bitboard
        {
            // Calculates the index
            ulong index = zobristKey % size;
            if (entries[index].key == zobristKey)
            {
                if (!BitboardUtility.CompareBitboard(bitboard, entries[index].bitboard))
                {
                    Debug.Log($"Key collision TYPE 1");
                }
            }
            entries[index] = new HashEntry(zobristKey, depth, nodeType, score, bitboard); // FOR TESTING , bitboard
        }

        public int LookupEvaluation(byte depth, int alpha, int beta, ulong zobristKey)
        {
            // Fetches the value from the array
            HashEntry entry = entries[zobristKey % size];

            // Checks if the correct entry was found
            if (entry.key == zobristKey)
            {
                // Check if the evaluation up to date of the stored position (the evaluation cant be used if it was evaluated at a lower depth)
                if (entry.depth >= depth)
                {
                    if (entry.nodeType == NodeType.exact)
                    {
                        return entry.score;
                    }
                    //else if (entry.nodeType == NodeType.alphaCutoff && entry.score <= alpha)
                    //{
                    //    return alpha;
                    //}
                    //else if (entry.score >= beta) // The node type has to be NodeType.betaCutoff
                    //{
                     //   return beta;
                    //}
                }
            }

            // The lookup was failed
            return LookupFailed;
        }

        #endregion

        #region Structures and enums

        public struct HashEntry
        {
            // Struct variables
            public readonly ulong key;
            public readonly byte depth;
            public readonly NodeType nodeType;
            public readonly int score;
            public readonly Bitboard bitboard; // FOR TESTING

            public HashEntry(ulong key, byte depth, NodeType nodeType, int score, Bitboard bitboard) // FOR TESTING (, Bitboard bitboard)
            {
                this.key = key;
                this.depth = depth;
                this.nodeType = nodeType;
                this.score = score;
                this.bitboard = bitboard; // FRO TESTING
            }
        } 

        public enum NodeType
        {
            exact = 0, // PV-node aka exact
            alphaCutoff = 1, // All-node aka upper bound
            betaCutoff = 2 // Cut-node aka lower bound
        }

        #endregion
    }
}

