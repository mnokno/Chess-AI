using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chess.EngineUtility
{
    public static class BitboardUtility
    {
        // Class variables
        #region Class variables

        public static readonly char[] FENPieceType = new char[6] { 'p', 'n', 'b', 'r', 'q', 'k' }; // Used to convert FEN string to a number

        #endregion

        // Utilities
        #region Utilities

        // Returns a long shift
        public static ulong GenerateShift(ulong x, int s)
        {
            return (s > 0) ? (x << s) : (x >> -s);
        }

        // Prints a bit board to the console (for testing and debugging)
        public static string FormatBitboard(ulong bitboard)
        {
            string bitboardUnformated = Convert.ToString((long)bitboard, 2);
            for (int i = bitboardUnformated.Length; i < 64; i++)
            {
                bitboardUnformated = "0" + bitboardUnformated;
            }

            string bitboardFormated = "";
            char[] tmp = new char[8];
            for (int i = 0; i < 8; i++)
            {
                Array.Copy(bitboardUnformated.ToCharArray(), 8*i, tmp, 0, 8);
                Array.Reverse(tmp);
                bitboardFormated += String.Join("", tmp);
                bitboardFormated += "\n";
            }

            return bitboardFormated;
        }

        // Prints all bitboard form a bitboard object to the console (for testing and debugging)
        public static string FormatBitboard(Bitboard bitboard)
        {
            string log = "";
            for (int i = 0; i < 14; i++)
            {
                string subLog = "";
                if (i < 7)
                {
                    subLog = "White " + ((Bitboard.PieceType)i).ToString() + "\n";
                }
                else
                {
                    subLog = "Black " + ((Bitboard.PieceType)(i-7)).ToString() + "\n";
                }
                subLog += FormatBitboard(bitboard.pieces[i]);
                log += subLog + "\n";
            }

            return log;
        }

        #endregion

        // Bitboard combined move functions
        #region Bitboard combined move functions
        
        /// Makes a move on a bitboard
        public static void MakeMove(ref Bitboard bitboard, ushort flag, ushort from, ushort to, bool sideToMove, byte pieceToMove=10, byte pieceToTake=10, byte promoteTo=10)
        {
            Bitboard.PlayerColor color = sideToMove ? Bitboard.PlayerColor.White : Bitboard.PlayerColor.Black; // Get color of the player making move
            if (flag == 0 | flag == 1) // Standard non-capture move
            {
                // Standard move or double pawn push
                StandardMove(ref bitboard, color, (Bitboard.PieceType)pieceToMove, from, to);
            }
            else if (flag == 2 | flag == 3) // Castling move
            {
                if (flag == 2) // King side
                {
                    CastleMove(ref bitboard, color, Position.CastlingDirection.KingSide);
                }
                else // Queen side
                {
                    CastleMove(ref bitboard, color, Position.CastlingDirection.QueenSide);
                }
            }
            else if (flag == 4) // Capture move
            {
                StandardMove(ref bitboard, color, (Bitboard.PieceType)pieceToMove, from, to, (Bitboard.PieceType)pieceToTake);
            }
            else if (flag == 5) // En-passant
            {
                EnpassantMove(ref bitboard, color, from, to);
            }
            else if (flag >= 8 & flag <= 15) // Promotion move
            {
                if (flag < 12) // Promotion without capture
                {
                    PromotionMove(ref bitboard, color, from, to, (Bitboard.PieceType)promoteTo);
                }
                else // Promotion with capture
                {
                    PromotionMove(ref bitboard, color, from, to, (Bitboard.PieceType)promoteTo, (Bitboard.PieceType)pieceToTake);
                }
            }
        }

        /// Unmakes a move on a bitboard
        public static void UnmakeMove(ref Bitboard bitboard, ushort flag, ushort from, ushort to, bool sideToMove, byte pieceToMove = 10, byte pieceToTake = 10, byte promoteTo = 10)
        {
            MakeMove(ref bitboard, flag, from, to, sideToMove, pieceToMove: pieceToMove, pieceToTake: pieceToTake, promoteTo: promoteTo);
        }

        #endregion

        // Bitboard move functions, on a bit board a move function is self inverse
        #region Bitboard move functions

        /// Non-capture move
        public static void StandardMove(ref Bitboard bitboard, Bitboard.PlayerColor color, Bitboard.PieceType piece, int currentIndex, int destinationIndex)
        {
            // Calculates bitboards
            UInt64 moveBitboard = BitboardUtility.GenerateShift(1, currentIndex) + BitboardUtility.GenerateShift(1, destinationIndex);

            // Applies bitboards
            bitboard.pieces[(int)piece + 7 * (int)color] ^= moveBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)color] ^= moveBitboard;
        }

        /// Capture move
        public static void StandardMove(ref Bitboard bitboard, Bitboard.PlayerColor color, Bitboard.PieceType piece, int currentIndex, int destinationIndex, Bitboard.PieceType pieceToTake)
        {
            // Calculates bitboards
            UInt64 moveBitboard = BitboardUtility.GenerateShift(1, currentIndex) + BitboardUtility.GenerateShift(1, destinationIndex);
            UInt64 takeBitboard = BitboardUtility.GenerateShift(1, destinationIndex);

            // Applies bitboards
            bitboard.pieces[(int)piece + 7 * (int)color] ^= moveBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)color] ^= moveBitboard;
            bitboard.pieces[(int)pieceToTake + 7 * (int)Invert(color)] ^= takeBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)Invert(color)] ^= takeBitboard;
        }

        /// Castle move
        public static void CastleMove(ref Bitboard bitboard, Bitboard.PlayerColor color, Position.CastlingDirection castlingDirection)
        {
            // Calculates bitboards
            ushort colorOffset = (ushort)(color == Bitboard.PlayerColor.White ? 0 : 56);
            ushort kingEndOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 6 : 2);
            ushort rookStartOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 7 : 0);
            ushort rookEndOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 5 : 3);

            UInt64 moveKingBitboard = BitboardUtility.GenerateShift(1, 4 + colorOffset) + BitboardUtility.GenerateShift(1, kingEndOffest + colorOffset); ;
            UInt64 moveRookBitboard = BitboardUtility.GenerateShift(1, rookStartOffest + colorOffset) + BitboardUtility.GenerateShift(1, rookEndOffest + colorOffset);

            // Applies bitboards
            bitboard.pieces[(int)Bitboard.PieceType.King + 7 * (int)color] ^= moveKingBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.Rook + 7 * (int)color] ^= moveRookBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)color] ^= moveKingBitboard ^ moveRookBitboard;
        }

        /// Non-capture promotion move
        public static void PromotionMove(ref Bitboard bitboard, Bitboard.PlayerColor color, int currentIndex, int destinationIndex, Bitboard.PieceType promoteTo)
        {
            // Calculates bitboards
            UInt64 moveFromBitboard = BitboardUtility.GenerateShift(1, currentIndex);
            UInt64 moveToBitboard = BitboardUtility.GenerateShift(1, destinationIndex);

            // Applies bitboards
            bitboard.pieces[(int)Bitboard.PieceType.Pawn + 7 * (int)color] ^= moveFromBitboard;
            bitboard.pieces[(int)promoteTo + 7 * (int)color] ^= moveToBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)color] ^= moveFromBitboard ^ moveToBitboard;
        }

        /// Capture proration
        public static void PromotionMove(ref Bitboard bitboard, Bitboard.PlayerColor color, int currentIndex, int destinationIndex, Bitboard.PieceType promoteTo, Bitboard.PieceType pieceToTake)
        {
            // Calculates bitboards
            UInt64 moveFromBitboard = BitboardUtility.GenerateShift(1, currentIndex);
            UInt64 moveToBitboard = BitboardUtility.GenerateShift(1, destinationIndex);

            // Applies bitboards
            bitboard.pieces[(int)Bitboard.PieceType.Pawn + 7 * (int)color] ^= moveFromBitboard;
            bitboard.pieces[(int)promoteTo + 7 * (int)color] ^= moveToBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)color] ^= moveFromBitboard ^ moveToBitboard;
            bitboard.pieces[(int)pieceToTake + 7 * (int)Invert(color)] ^= moveToBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)Invert(color)] ^= moveToBitboard;
        }

        /// En-passant move
        public static void EnpassantMove(ref Bitboard bitboard, Bitboard.PlayerColor color, int currentIndex, int destinationIndex)
        {
            // Calculates bitboard
            int capturedIndex = destinationIndex + (color == Bitboard.PlayerColor.White ? -8 : 8);
            UInt64 moveFromBitboard = BitboardUtility.GenerateShift(1, currentIndex);
            UInt64 moveToBitboard = BitboardUtility.GenerateShift(1, destinationIndex);
            UInt64 capturedBitboard = BitboardUtility.GenerateShift(1, capturedIndex);

            // Applies bitboards
            bitboard.pieces[(int)Bitboard.PieceType.Pawn + 7 * (int)color] ^= moveFromBitboard ^ moveToBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)color] ^= moveFromBitboard ^ moveToBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.Pawn + 7 * (int)Invert(color)] ^= capturedBitboard;
            bitboard.pieces[(int)Bitboard.PieceType.All + 7 * (int)Invert(color)] ^= capturedBitboard;
        }

        #endregion

        // Helper functions 
        #region Helper functions
        public static Bitboard.PlayerColor Invert(Bitboard.PlayerColor color)
        {
            return color == Bitboard.PlayerColor.White ? Bitboard.PlayerColor.Black : Bitboard.PlayerColor.White;
        }

        #endregion
    }
}
