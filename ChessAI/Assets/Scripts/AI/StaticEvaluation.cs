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
        public float[] pieceValues = new float[6] { 1f, 3f, 3f, 5f, 9f, 99999f };

        #endregion

        // Core, combines all sub-functions 
        #region Core

        // Returns static evaluation of a chess position
        public float Evaluate(Position position)
        {
            // Updates position for evaluation
            this.position = position;

            // Creates evaluation score
            float eval = 0f;

            // Calculates evaluation score using sub functions
            eval += MaterialEvaluation();

            // Returns evaluation score
            return eval;
        }

        #endregion

        // Sub-functions of the main function
        #region Sub-functions

        // Scores the current position on the material count
        public float MaterialEvaluation()
        {
            // Creates evaluation score
            float eval = 0f;

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
    }
}

