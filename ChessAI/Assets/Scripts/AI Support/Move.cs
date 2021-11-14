using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public static class Move
    {
        // Class utilities
        #region Utilities

        // Generate a move
        public static ushort GenMove(ushort from, ushort to, ushort flag)
        {
            return (ushort)(((flag & 0xf) << 12) | ((from & 0x3f) << 6) | (to & 0x3f));
        }

        // Returns to square
        public static ushort GetTo(ushort move)
        {
            return (ushort)(move & 0x3f);
        }

        // Returns from square
        public static ushort GetFrom(ushort move)
        {
            return (ushort)((move >> 6) & 0x3f);
        }

        // Returns flag
        public static ushort GetFlag(ushort move)
        {
            return (ushort)((move >> 12) & 0x0f);
        }

        #endregion

        // Structures and enumerations used by this class
        #region Structures and enumerations

        public readonly struct Flag
        {
            public static readonly ushort quietMove = 0;
            public static readonly ushort doublePawnPush = 1;
            public static readonly ushort kingCastle = 2;
            public static readonly ushort queenCastle = 3;
            public static readonly ushort capture = 4;
            public static readonly ushort epCapture = 5;
            public static readonly ushort knightPromotion = 8;
            public static readonly ushort bishopPromotion = 9;
            public static readonly ushort rookPromotion = 10;
            public static readonly ushort queenPromotion = 11;
            public static readonly ushort knightPromotionCapture = 12;
            public static readonly ushort bishopPromotionCapture = 13;
            public static readonly ushort rookPromotionCapture = 14;
            public static readonly ushort queenPromotionCapture = 15;
        }
        #endregion
    }
}
