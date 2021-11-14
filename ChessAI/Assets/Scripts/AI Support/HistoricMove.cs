using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public static class HistoricMove
    {
        /// En-passant target file bits 0..3 --- 4 bits
        /// Castling rights 4..7 --- 4 bits
        /// Captured piece type 8..11 --- 4 bit
        /// Calf-move clock 12..19 --- 8 bits

        /// Class utilizes
        #region Utilizes

        /// Returns a historic move
        public static uint GenHistoricMove(byte enPassantTargetFile, byte castlingRights, byte capturedPiece, byte halfmoveClock)
        {
            uint historicMove = 0u; // Creates historic move initially 0

            historicMove |= (uint)(enPassantTargetFile & 0xf); // Adds en-passant target file  
            historicMove |= (uint)((castlingRights & 0xf) << 4); // Adds castling rights
            historicMove |= (uint)((capturedPiece & 0xf) << 8); // Adds captured piece
            historicMove |= (uint)((halfmoveClock & 0xff) << 12); // Adds half-move clock

            return historicMove; // Returns generated historic move
        }

        /// Returns en-passant target file form historic move unit
        public static byte GetEnPassantTargetFile(uint historicMove)
        {
            return (byte)(historicMove & 0xf);
        }

        /// Returns castling rights form historic move unit
        public static byte GetCastlingRights(uint historicMove)
        {
            return (byte)((historicMove >> 4) & 0xf);
        }

        /// Returns captured piece form historic move unit
        public static byte GetCapturedPiece(uint historicMove)
        {
            return (byte)((historicMove >> 8) & 0xf);
        }

        /// Returns half-move clock form historic move unit
        public static byte GetHalfmoveClock(uint historicMove)
        {
            return (byte)((historicMove >> 12) & 0xff);
        }

        #endregion
    }
}

