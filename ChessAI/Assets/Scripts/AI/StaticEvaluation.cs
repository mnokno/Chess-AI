using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.Engine
{
    public class StaticEvaluation
    {
        // Class variables
        #region Class variables

        public Position position; // Position to be evaluated

        // Stores values associated with piece types [pawn, knight, bishop, rook, queen, king]
        public static int[] pieceValues = new int[6] { 100, 300, 300, 500, 900, 50000 };

        #endregion

        // Core, combines all sub-functions 
        #region Core

        // Returns static evaluation of a chess position
        public int Evaluate(Position position)
        {
            // Updates position for evaluation
            this.position = position;

            // Creates evaluation score
            int eval = 0;

            // Calculates evaluation score using sub functions
            eval += MaterialEvaluation();

            // Returns evaluation score
            return eval * (position.sideToMove ? 1 : -1);
        }

        #endregion

        // Sub-functions of the main function
        #region Sub-functions

        // Scores the current position on the material count
        public int MaterialEvaluation()
        {
            // Creates evaluation score
            int eval = 0;

            // Calculates white and black material evaluation score
            for (int i = 0; i < 6; i++)
            {
                eval += BitOps.PopulationCount(position.bitboard.pieces[i]) * pieceValues[i]; // White pieces
                eval -= BitOps.PopulationCount(position.bitboard.pieces[i + 7]) * pieceValues[i]; // Black pieces
            }

            // Returns evaluation score
            return eval;
        }

        #endregion

        #region Enums

        public enum PieceType
        {
            Pawn = 0,
            Knight = 1,
            Bishop = 2,
            Rook = 3,
            Queen = 4,
            King = 5,
        }

        #endregion
    }
}

