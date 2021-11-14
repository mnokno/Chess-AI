using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chess.EngineUtility
{
    public class SquareCentric
    {
        // Class variables 
        #region Class variables

        public byte[] colors = new byte[64];  /* LIGHT, DARK, or EMPTY */
        public byte[] pieces = new byte[64];  /* PAWN, KNIGHT, BISHOP, ROOK, QUEEN, KING, or EMPTY */

        #endregion

        // Responsible for square initialization
        #region Initialization

        // Class constructor create empty
        public SquareCentric()
        {
            // Sets all fields in arrays to empty
            InitArrays();
        }

        // Class constructor loads pieces and colors array
        public SquareCentric(byte[] colors, byte[] pieces)
        {
            this.colors = colors;
            this.pieces = pieces;
        }

        // Class constructor loads FEN
        public SquareCentric(string FEN)
        {
            // Sets arrays to passed FEN
            InitArrays();
            LoadFEN(FEN);
        }

        // Loads position from FEN string
        private void LoadFEN(string FEN)
        {
            // Splits the FEN into ranks rank 8 ... 1
            string[] ranks = FEN.Split('/');

            // Loads each rank
            int currentIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < ranks[7-i].Length; j++)
                {
                    if (char.IsNumber(ranks[7 - i][j]))
                    {
                        currentIndex += int.Parse(ranks[7 - i][j].ToString());
                    }
                    else
                    {
                        colors[currentIndex] = (byte)(char.IsUpper(ranks[7 - i][j]) ? (int)SquareColor.White : (int)SquareColor.Black);
                        pieces[currentIndex] = (byte)(Array.IndexOf(SquareCentricUtility.FENPieceType, char.ToLower(ranks[7 - i][j])));
                        currentIndex++;
                    }
                }
            }
        }

        // Initializes pieces array
        private void InitArrays()
        {
            for(int i = 0; i < 64; i++)
            {
                colors[i] = (int)SquareColor.Empty;
                pieces[i] = (int)PieceType.Empty;
            }
        }

        #endregion

        // Structures and enumerations used by this class
        #region Structures and enumerations

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

        public enum SquareColor
        {
            White = 0,
            Black = 1,
            Empty = 2
        }

        public enum PieceType
        {
            Pawn = 0,
            Knight = 1,
            Bishop = 2,
            Rook = 3,
            Queen = 4,
            King = 5,
            Empty = 6
        }

        #endregion
    }

}
