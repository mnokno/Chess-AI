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
        // Reference to the board, when a new move is added this board reference is used to get the zobrist hash for the current position
        public readonly Position position;
        // This constant is returned if evaluation lookup failed (the position was not found in the table)
        public const int LookupFailed = -2000000000;

        #endregion

        #region Constructor

        public TranspositionTable(ulong size, Position position)
        {
            // Saves parameters
            this.size = size;
            this.position = position;
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
        public void AddEntry(byte depth, NodeType nodeType, int score, ushort move)
        {
            // Calculates the index
            ulong index = GetIndex();
            // Only adds this entry if its evaluated to a greater depth then the currently stored location
            if (depth > entries[index].depth)
            {
                entries[index] = new HashEntry(position.zobristKey, depth, nodeType, score, move);
            }
        }

        // Gets entry from the hash table
        public HashEntry GetEntry()
        {
            return entries[GetIndex()];
        }

        public int LookupEvaluation(byte depth, int alpha, int beta)
        {
            // Fetches the value from the array
            HashEntry entry = entries[GetIndex()];

            // Checks if the correct entry was found
            if (entry.key == position.zobristKey)
            {
                // Check if the evaluation up to date of the stored position (the evaluation cant be used if it was evaluated at a lower depth)
                if (entry.depth >= depth)
                {
                    if (entry.nodeType == NodeType.exact)
                    {
                        return entry.score;
                    }
                    else if (entry.nodeType == NodeType.alphaCutoff && entry.score <= alpha)
                    {
                        return entry.score;
                    }
                    else if (entry.score >= beta) // The node type has to be NodeType.betaCutoff
                    {
                        return entry.score;
                    }
                }
            }

            // The lookup was failed
            return LookupFailed;
        }

        // Returns index corresponding to current position on the board
        public ulong GetIndex()
        {
            return position.zobristKey % size;
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
            public readonly ushort move;

            // Struct constructor
            public HashEntry(ulong key, byte depth, NodeType nodeType, int score, ushort move)
            {
                this.key = key;
                this.depth = depth;
                this.nodeType = nodeType;
                this.score = score;
                this.move = move;
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

