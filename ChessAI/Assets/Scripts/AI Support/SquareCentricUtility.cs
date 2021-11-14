using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chess.EngineUtility
{
    public static class SquareCentricUtility
    {
        // Class variables
        #region Class variables

        public static readonly char[] FENPieceType = new char[6] { 'p', 'n', 'b', 'r', 'q', 'k' }; // Used to convert FEN string to a number

        #endregion

        // Utilities
        #region Utilities

        // Returns formate square centric
        public static string FormateSquareCentric(SquareCentric squareCentric)
        {
            string formateSquareCentricpPieces = "";
            string formateSquareCentricpColors = "";
            string tmp = "";

            // Formats pieces array 
            for (int i = 0; i < 8; i++)
            {
                tmp = "";
                for (int j = 0; j < 8; j++)
                {
                    tmp += " " + ((SquareCentric.PieceType)squareCentric.pieces[i * 8 + j]).ToString();
                }
                formateSquareCentricpPieces = tmp + "\n" + formateSquareCentricpPieces;
            }
            formateSquareCentricpPieces = "Pieces: " + "\n" + formateSquareCentricpPieces;

            // Formats color array
            for (int i = 0; i < 8; i++)
            {
                tmp = "";
                for (int j = 0; j < 8; j++)
                {
                    tmp += " " + ((SquareCentric.SquareColor)squareCentric.colors[i * 8 + j]).ToString();
                }
                formateSquareCentricpColors = tmp + "\n" + formateSquareCentricpColors;
            }
            formateSquareCentricpColors = "Color: " + "\n" + formateSquareCentricpColors;

            return formateSquareCentricpPieces + "\n\n" + formateSquareCentricpColors;
        }

        #endregion

        // Square centric combined move functions
        #region Square centric combined move functions

        public static void MakeMove(ref SquareCentric squareCentric, ushort flag, ushort from, ushort to, bool sideToMove, byte pieceToMove = 10, byte promoteTo = 10)
        {
            SquareCentric.SquareColor color = sideToMove ? SquareCentric.SquareColor.White : SquareCentric.SquareColor.Black; // Get color of the player making move
            if (flag == 0 | flag == 1 | flag == 4) // Quite move | double pawn push | capture
            {
                MakeStandardMove(ref squareCentric, color, (SquareCentric.PieceType)pieceToMove, from, to);
            }
            else if (flag == 2 | flag == 3) // Castling move
            {
                if (flag == 2) // King side castle
                {
                    MakeCastleMove(ref squareCentric, color, Position.CastlingDirection.KingSide);
                }
                else // Queen side castle
                {
                    MakeCastleMove(ref squareCentric, color, Position.CastlingDirection.QueenSide);
                }
            }
            else if (flag == 5) // En-passant capture
            {
                MakeEnpassantMove(ref squareCentric, color, from, to);
            }
            else if (flag >= 8 & flag <= 15) // Promotion move
            {
                MakePromotionMove(ref squareCentric, color, from, to, (SquareCentric.PieceType)promoteTo);
            }
        }

        public static void UnmakeMove(ref SquareCentric squareCentric, ushort flag, ushort from, ushort to, bool sideToMove, byte pieceToMove = 10, byte pieceToTake = 10)
        {
            SquareCentric.SquareColor color = sideToMove ? SquareCentric.SquareColor.White : SquareCentric.SquareColor.Black; // Get color of the player making move
            if (flag == 0 | flag == 1) // Quiet move | double pawn push
            {
                UnmakeStandardMove(ref squareCentric, color, (SquareCentric.PieceType)pieceToMove, from, to);
            }
            else if (flag == 2 | flag == 3) // Castling move
            {        
                if (flag == 2) // King side
                {
                    UnmakeCastleMove(ref squareCentric, color, Position.CastlingDirection.KingSide);
                }
                else // Queen side
                {
                    UnmakeCastleMove(ref squareCentric, color, Position.CastlingDirection.QueenSide);
                }
            }
            else if (flag == 4) // Capture move
            {
                UnmakeStandardMove(ref squareCentric, color, (SquareCentric.PieceType)pieceToMove, from, to, (SquareCentric.PieceType)pieceToTake);
            }
            else if (flag == 5) // En-passant
            {
                UnmakeEnpassantMove(ref squareCentric, color, from, to);
            }
            else if (flag >= 8 & flag <= 15) // Promotion move
            {
                if (flag < 12) // Promotion without capture
                {
                    UnmakePromotionMove(ref squareCentric, color, from, to);
                }
                else // Promotion with capture
                {
                    UnmakePromotionMove(ref squareCentric, color, from, to, (SquareCentric.PieceType)pieceToTake);
                }
            }
        }

        #endregion

        // Square centric make move functions
        #region Square centric make move functions

        /// Makes a standard move
        public static void MakeStandardMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, SquareCentric.PieceType piece, int currentIndex, int destinationIndex)
        {
            squareCentric.colors[currentIndex] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the currentIndex
            squareCentric.pieces[currentIndex] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the currentIndex
            squareCentric.colors[destinationIndex] = (byte)color; // Add color to the destinationIndex
            squareCentric.pieces[destinationIndex] = (byte)piece; // Add piece to the destinationIndex
        }
        
        /// Makes a castle move
        public static void MakeCastleMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, Position.CastlingDirection castlingDirection)
        {
            // Calculates offsets
            ushort colorOffset = (ushort)(color == SquareCentric.SquareColor.White ? 0 : 56);
            ushort kingEndOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 6 : 2);
            ushort rookStartOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 7 : 0);
            ushort rookEndOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 5 : 3);

            squareCentric.colors[4 + colorOffset] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the currentIndex | for king
            squareCentric.pieces[4 + colorOffset] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the currentIndex | for king
            squareCentric.colors[rookStartOffest + colorOffset] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the currentIndex | for rook
            squareCentric.pieces[rookStartOffest + colorOffset] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the currentIndex | for rook
            squareCentric.colors[kingEndOffest + colorOffset] = (byte)color; // Add color to the destinationIndex | for king
            squareCentric.pieces[kingEndOffest + colorOffset] = (byte)SquareCentric.PieceType.King; ; // Add piece to the destinationIndex | for king
            squareCentric.colors[rookEndOffest + colorOffset] = (byte)color; // Add color to the destinationIndex | for rook
            squareCentric.pieces[rookEndOffest + colorOffset] = (byte)SquareCentric.PieceType.Rook; // Add piece to the destinationIndex | for rook
        }

        /// Makes a promotion move
        public static void MakePromotionMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, int currentIndex, int destinationIndex, SquareCentric.PieceType promoteTo)
        {
            squareCentric.colors[currentIndex] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the currentIndex
            squareCentric.pieces[currentIndex] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the currentIndex
            squareCentric.colors[destinationIndex] = (byte)color; // Add color to the destinationIndex
            squareCentric.pieces[destinationIndex] = (byte)promoteTo; // Add piece to the destinationIndex
        }

        /// Makes a en-passant move
        public static void MakeEnpassantMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, int currentIndex, int destinationIndex)
        {
            ushort capturedIndex = (ushort)(destinationIndex + (color == SquareCentric.SquareColor.White ? -8 : 8)); // Calculates offset
            squareCentric.colors[currentIndex] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the currentIndex
            squareCentric.pieces[currentIndex] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the currentIndex
            squareCentric.colors[capturedIndex] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the capturedIndex
            squareCentric.pieces[capturedIndex] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the capturedIndex
            squareCentric.colors[destinationIndex] = (byte)color; // Add color to the destinationIndex
            squareCentric.pieces[destinationIndex] = (byte)SquareCentric.PieceType.Pawn; // Add piece to the destinationIndex
        }

        #endregion

        // Square centric unmake move functions
        #region Square centric unmake functions

        /// Unmakes a standard non-capture move
        public static void UnmakeStandardMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, SquareCentric.PieceType piece, int currentIndex, int destinationIndex)
        {
            squareCentric.colors[destinationIndex] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the destinationIndex
            squareCentric.pieces[destinationIndex] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the destinationIndex
            squareCentric.colors[currentIndex] = (byte)color; // Add color to the currentIndex
            squareCentric.pieces[currentIndex] = (byte)piece; // Add piece to the currentIndex
        }

        /// Unmakes a standard capture move
        public static void UnmakeStandardMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, SquareCentric.PieceType piece, int currentIndex, int destinationIndex, SquareCentric.PieceType pieceToTake)
        {
            squareCentric.colors[destinationIndex] = (byte)(color == SquareCentric.SquareColor.White ? SquareCentric.SquareColor.Black : SquareCentric.SquareColor.White); // Removes color from the destinationIndex
            squareCentric.pieces[destinationIndex] = (byte)pieceToTake; // Removes piece from the destinationIndex
            squareCentric.colors[currentIndex] = (byte)color; // Add color to the currentIndex
            squareCentric.pieces[currentIndex] = (byte)piece; // Add piece to the currentIndex
        }

        /// Unmakes a castling move
        public static void UnmakeCastleMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, Position.CastlingDirection castlingDirection)
        {
            // Calculates offsets
            ushort colorOffset = (ushort)(color == SquareCentric.SquareColor.White ? 0 : 56);
            ushort kingEndOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 6 : 2);
            ushort rookStartOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 7 : 0);
            ushort rookEndOffest = (ushort)(castlingDirection == Position.CastlingDirection.KingSide ? 5 : 3);

            squareCentric.colors[kingEndOffest + colorOffset] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the destinationIndex | for king
            squareCentric.pieces[kingEndOffest + colorOffset] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the destinationIndex | for king
            squareCentric.colors[rookEndOffest + colorOffset] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the destinationIndex | for rook
            squareCentric.pieces[rookEndOffest + colorOffset] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the destinationIndex | for rook
            squareCentric.colors[4 + colorOffset] = (byte)color; // Add color to the currentIndex | for king
            squareCentric.pieces[4 + colorOffset] = (byte)SquareCentric.PieceType.King; ; // Add piece to the currentIndex | for king
            squareCentric.colors[rookStartOffest + colorOffset] = (byte)color; // Add color to the currentIndex | for rook
            squareCentric.pieces[rookStartOffest + colorOffset] = (byte)SquareCentric.PieceType.Rook; // Add piece to the currentIndex | for rook
        }

        /// Unmakes a non-capture proration move
        public static void UnmakePromotionMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, int currentIndex, int destinationIndex)
        {
            squareCentric.colors[destinationIndex] = (byte)SquareCentric.SquareColor.Empty; // Removes color from the destinationIndex
            squareCentric.pieces[destinationIndex] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the destinationIndex
            squareCentric.colors[currentIndex] = (byte)color; // Add color to the currentIndex
            squareCentric.pieces[currentIndex] = (byte)SquareCentric.PieceType.Pawn; // Add piece to the currentIndex
        }

        /// Unmakes a capture proration move
        public static void UnmakePromotionMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, int currentIndex, int destinationIndex, SquareCentric.PieceType pieceToTake)
        {
            squareCentric.colors[destinationIndex] = (byte)(color == SquareCentric.SquareColor.White ? SquareCentric.SquareColor.Black : SquareCentric.SquareColor.White); // Removes color from the destinationIndex
            squareCentric.pieces[destinationIndex] = (byte)pieceToTake; // Removes piece from the destinationIndex
            squareCentric.colors[currentIndex] = (byte)color; // Add color to the currentIndex
            squareCentric.pieces[currentIndex] = (byte)SquareCentric.PieceType.Pawn; // Add piece to the currentIndex
        }

        /// Unmakes a en-passant move
        public static void UnmakeEnpassantMove(ref SquareCentric squareCentric, SquareCentric.SquareColor color, int currentIndex, int destinationIndex)
        {
            ushort capturedIndex = (ushort)(destinationIndex + (color == SquareCentric.SquareColor.White ? -8 : 8)); // Calculates offset
            squareCentric.colors[currentIndex] = (byte)color; // Add color to the currentIndex
            squareCentric.pieces[currentIndex] = (byte)SquareCentric.PieceType.Pawn; // Add piece to the currentIndex
            squareCentric.colors[capturedIndex] = (byte)(color == SquareCentric.SquareColor.White ? SquareCentric.SquareColor.Black : SquareCentric.SquareColor.White); // Add color to the capturedIndex
            squareCentric.pieces[capturedIndex] = (byte)SquareCentric.PieceType.Pawn; // Add piece to the capturedIndex
            squareCentric.colors[destinationIndex] = (byte)SquareCentric.SquareColor.Empty; // Remove color from the destinationIndex
            squareCentric.pieces[destinationIndex] = (byte)SquareCentric.PieceType.Empty; // Removes piece from the destinationIndex
        }

        #endregion
    }
}
