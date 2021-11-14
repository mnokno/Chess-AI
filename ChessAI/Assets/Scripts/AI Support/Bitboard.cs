using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public class Bitboard
    {
        // Class variables
        #region Class variables

        // To get desired bitboard use pieces[(int)Bitboard.PieceType + 7*(int)Bitboard.PlayerColor]
        public UInt64[] pieces; // Combines all primitive bitboards into one array for easy access
        
        #endregion

        // Responsible for square initialization
        #region Initialization

        // Class constructor create empty
        public Bitboard()
        {
            PieceArrayInit(); // Initializes pieces array
        }

        // Class constructor loads pieces array
        public Bitboard(ulong[] pieces)
        {
            this.pieces = pieces;
        }

        // Class constructor loads FEN
        public Bitboard(string FEN)
        {
            PieceArrayInit(); // Initializes pieces array
            LoadFEN(FEN); // Loads FEN
        }

        // Loads position from FEN string 
        private void LoadFEN(string FEN)
        {
            // Splits FEN into ranks rank 8 ... 1
            string[] ranks = FEN.Split('/');

            // Loads each rank
            int currentIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < ranks[7 - i].Length; j++)
                {
                    if (char.IsNumber(ranks[7 - i][j]))
                    {
                        currentIndex += int.Parse(ranks[7 - i][j].ToString());
                    }
                    else
                    {
                        int colorMultiplier = char.IsUpper(ranks[7 - i][j]) ? (int)PlayerColor.White : (int)PlayerColor.Black;
                        pieces[Array.IndexOf(BitboardUtility.FENPieceType, char.ToLower(ranks[7 - i][j])) + 7 * colorMultiplier] ^= BitboardUtility.GenerateShift(1, currentIndex);
                        pieces[(int)PieceType.All + 7 * colorMultiplier] ^= BitboardUtility.GenerateShift(1, currentIndex);
                        currentIndex++;
                    }
                }
            }
        }

        // Initializes pieces array
        private void PieceArrayInit()
        {
            // Populates pieces array with primitive bit boards
            pieces = new UInt64[14] {
                0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0
            };
        }

        #endregion

        // Structures and enumerations used by this class
        #region Structures and enumerations

        // Maps piece type to a number
        public enum PieceType
        {
            Pawn,
            Knight,
            Bishop,
            Rook,
            Queen,
            King,
            All
        }

        // Maps player/piece color to a number
        public enum PlayerColor
        {
            White,
            Black
        }

        // Maps square ID to rank-file
        public enum Squares
        {
            a1, b1, c1, d1, e1, f1, g1, h1,
            a2, b2, c2, d2, e2, f2, g2, h2,
            a3, b3, c3, d3, e3, f3, g3, h3,
            a4, b4, c4, d4, e4, f4, g4, h4,
            a5, b5, c5, d5, e5, f5, g5, h5,
            a6, b6, c6, d6, e6, f6, g6, h6,
            a7, b7, c7, d7, e7, f7, g7, h7,
            a8, b8, c8, d8, e8, f8, g8, h8
        };

        // Maps ray directions
        public enum CompassRose
        {
            noWe = 7, nort = 8, noEa = 9,
            west = -1, east = 1,
            soWe = -9, sout = -8, soEa = -7
        }

        #endregion
    }
}
    
