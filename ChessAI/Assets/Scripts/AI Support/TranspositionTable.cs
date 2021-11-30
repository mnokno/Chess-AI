using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public class TranspositionTable
    {
        #region Class variables

        #endregion

        #region Constructor

        public TranspositionTable()
        {

        }

        #endregion

        #region Utility

        #endregion

        #region Structures and enums

        public struct HashEntry
        {
            // Struct variables
            public readonly long key;
            public readonly byte depth;
            public readonly NodeType nodeType;
            public readonly int score;
            public readonly ushort move;

            // Struct constructor
            public HashEntry(long key, byte depth, NodeType nodeType, int score, ushort move)
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

