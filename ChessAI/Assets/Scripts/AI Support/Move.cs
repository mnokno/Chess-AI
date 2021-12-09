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

        // Converts a PNG move to a ushort move
        public static ushort ConvertPGNToUshort(string PNGMove, Position position)
        {
            return 0;
        }

        // Converts a ushort move to a PNG move
        public static string ConvertUshortToPNG(ushort move, Position position)
        {
            if (GetFlag(move) == Flag.kingCastle)
            {
                return "O-O";
            }
            else if (GetFlag(move) == Flag.queenCastle)
            {
                return "O-O-O";
            }
            else
            {
                string from = ((SquareCentric.Squares)GetFrom(move)).ToString();
                string to = ((SquareCentric.Squares)GetTo(move)).ToString();
                string pieceToMove = "";
                string promotion = "";
                string capture = position.squareCentric.pieces[GetTo(move)] != (byte)SquareCentric.PieceType.Empty ? "x" : "";
                string check = "";

                position.MakeMove(move);
                int kingIndex = BitOps.BitScanForward(position.bitboard.pieces[5 + (position.sideToMove ? 0 : 7)]); // Gets the location of the defending king
                if (position.SquareAttackedBy(kingIndex, (byte)(position.sideToMove ? 1 : 0)))
                {                                                                                                                                                                                                                                                                                                  
                    check = "+";
                }
                position.UnmakeMove(move);

                switch (position.squareCentric.pieces[GetFrom(move)])
                {
                    case (byte)SquareCentric.PieceType.Pawn:
                        pieceToMove = "";
                        if (GetFlag(move) >= 8) // promotion
                        {
                            int flag = GetFlag(move) >= 12 ? GetFlag(move) - 4 : GetFlag(move);
                            switch (flag)
                            {
                                case (byte)Flag.knightPromotion:
                                    promotion = "N";
                                    break;
                                case (byte)Flag.bishopPromotion:
                                    promotion = "B";
                                    break;
                                case (byte)Flag.rookPromotion:
                                    promotion = "R";
                                    break;
                                case (byte)Flag.queenCastle:
                                    promotion = "Q";
                                    break;
                            }
                        }
                        break;
                    case (byte)SquareCentric.PieceType.Knight:
                        pieceToMove = "N";
                        break;
                    case (byte)SquareCentric.PieceType.Bishop:
                        pieceToMove = "B";
                        break;
                    case (byte)SquareCentric.PieceType.Rook:
                        pieceToMove = "R";
                        break;
                    case (byte)SquareCentric.PieceType.Queen:
                        pieceToMove = "Q";
                        break;
                    case (byte)SquareCentric.PieceType.King:
                        pieceToMove = "K";
                        break;
                }
                
                return pieceToMove + from + capture + to + promotion + check;
            }
        }


        #endregion

        // Structures and enumerations used by this class
        #region Structures and enumerations

        public readonly struct Flag
        {
            public const ushort quietMove = 0;
            public const ushort doublePawnPush = 1;
            public const ushort kingCastle = 2;
            public const ushort queenCastle = 3;
            public const ushort capture = 4;
            public const ushort epCapture = 5;
            public const ushort knightPromotion = 8;
            public const ushort bishopPromotion = 9;
            public const ushort rookPromotion = 10;
            public const ushort queenPromotion = 11;
            public const ushort knightPromotionCapture = 12;
            public const ushort bishopPromotionCapture = 13;
            public const ushort rookPromotionCapture = 14;
            public const ushort queenPromotionCapture = 15;
        }

        #endregion
    }
}
