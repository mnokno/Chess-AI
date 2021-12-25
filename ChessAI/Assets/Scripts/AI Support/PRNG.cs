using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    // Implementation of xorshift64star Pseudo-Random Number Generator
    public class PRNG
    {
        #region Class variables

        private ulong state;

        #endregion

        #region Class constructor

        public PRNG(ulong seed)
        {
            state = seed;
        }

        #endregion

        #region Class utilities 

        public ulong NextUlong()
        {
            state ^= state >> 12;
            state ^= state << 25;
            state ^= state >> 27;
            return state * 2685821657736338717;
        }

        #endregion
    }
}

