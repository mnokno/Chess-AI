using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    /// <summary>
    /// {FEN} ::=  {Piece Placement} ' ' {Side to move} ' ' {Castling ability} ' ' {En passant target square} ' ' {Halfmove clock} ' ' {Fullmove counter}
    /// </summary>
    public class FEN
    {
        #region Class variables

        private string piecePlacment; // <rank8>'/'<rank7>'/'<rank6>'/'<rank5>'/'<rank4>'/'<rank3>'/'<rank2>'/'<rank1>
        private bool sideToMove; // true = white to move, false = black to move
        private byte castlingRights; // bit 0 white queen side, bit 1 white king side, bit 2 black queen side, bit 3 black king side
        private byte enPassantTargetFile; // 0..7 represent a target file, 8 means no target square
        private byte halfmoveClock; // If >= 100 then its draw due to fifty-move rule, resets to 0 after pawn pushes and captures
        private byte fullmoveCounter; // Number of full moves

        #endregion

        #region Class constructor

        // Constructor
        public FEN(string FEN)
        {
            // Splits the FEN into 6 parts
            string[] parts = FEN.Split(' ');

            // Extracts pieces placements
            piecePlacment = parts[0];
            // Extracts side to move
            if (parts.Length >= 2)
            {
                sideToMove = parts[1] == "w" ? true : false;
            }
            else
            {
                sideToMove = true;
            }
            // Extracts castling rights
            if (parts.Length >= 3)
            {
                castlingRights = (byte)((parts[2].Contains("K") ? 0b0100 : 0) | (parts[2].Contains("Q") ? 0b1000 : 0) | (parts[2].Contains("k") ? 0b0001 : 0) | (parts[2].Contains("q") ? 0b0010 : 0));
            }
            else
            {
                castlingRights = 0b1111;
            }
            // Extracts en passant target file
            if (parts.Length >= 4)
            {
                if (parts[3] != "-")
                {
                    enPassantTargetFile = (byte)((int)Enum.Parse(typeof(Squares), parts[3]) % 8);
                }
                else
                {
                    enPassantTargetFile = 8;
                }
            }
            else
            {
                enPassantTargetFile = 8;
            }
            // Extracts half-move clock
            if (parts.Length >= 5)
            {
                if (parts[4] != "-" && parts[4] != "")
                {
                    halfmoveClock = byte.Parse(parts[4]);
                }
                else
                {
                    halfmoveClock = 0;
                }
            }
            else
            {
                halfmoveClock = 0;
            }
            // Extracts full-move counter
            if (parts.Length >= 6)
            {
                if (parts[5] != "-" && parts[5] != "")
                {
                    fullmoveCounter = byte.Parse(parts[5]);
                }
                else
                {
                    fullmoveCounter = 0;
                }
            }
            else
            {
                fullmoveCounter = 0;
            }
        }

        #endregion

        #region Getters

        public string GetPiecePlacment() => piecePlacment;
        public bool GetSideToMove() => sideToMove;
        public byte GetCastlingRights() => castlingRights;
        public byte GetEnPassantTargetFile() => enPassantTargetFile;
        public byte GetHalfmoveClock() => halfmoveClock;
        public byte GetFullmoveCounter() => fullmoveCounter;

        #endregion

        #region Enums

        // Maps square ID to rank-file
        private enum Squares
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

        #endregion
    }
}

