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

        // Stores values associated with piece types [pawn, knight, bishop, rook, queen, king] and the game phases
        public static ushort[] pieceValues = new ushort[6] { 100, 320, 330, 500, 900, 0 };
        public static ushort[] midGamePieceValues = new ushort[6] { 82, 337, 365, 477, 1025, 0 };
        public static ushort[] endGamePieceValues = new ushort[6] { 94, 281, 297, 512, 936, 0 };
        // Game phases weights
        public float midGameWeight;
        public float endGameWeight;

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
            // Calculates game phase
            CalculateGamePhase();

            // Calculates evaluation score using sub functions
            eval += MaterialEvaluation();
            eval += MaterialEvaluationUsingPieceTables();

            // Returns evaluation score
            return eval * (position.sideToMove ? 1 : -1);
        }

        #endregion

        // Sub-functions of the main function
        #region Sub-functions

        // Scores the current position base on the material count
        public int MaterialEvaluation()
        {
            // Creates evaluation score
            int eval = 0;

            // Calculates white and black material evaluation score
            for (int i = 0; i < 6; i++)
            {
                int whiteCount = BitOps.PopulationCount(position.bitboard.pieces[i]);
                int blackCount = BitOps.PopulationCount(position.bitboard.pieces[i + 7]);
                eval += (int)(whiteCount * midGamePieceValues[i] * midGameWeight); // Mid game white eval
                eval += (int)(whiteCount * endGamePieceValues[i] * endGameWeight); // End game white eval
                eval -= (int)(blackCount * midGamePieceValues[i] * midGameWeight); // Mid game black eval
                eval -= (int)(blackCount * endGamePieceValues[i] * endGameWeight); // End game black eval
            }

            // Returns evaluation score
            return eval;
        }

        // Scores the current position base on the material count and piece placement
        public int MaterialEvaluationUsingPieceTables()
        {
            // Calculates and returns evaluation score
            return PieceSquareTables.CalculateScore(midGameWeight, endGameWeight, position);
        }
        
        // Calculates game phase
        public void CalculateGamePhase()
        {
            // create a variable to store the total
            int total = 0;

            // Calculates the total
            for (int i = 0; i < 6; i++)
            {
                total += BitOps.PopulationCount(position.bitboard.pieces[i] | position.bitboard.pieces[i + 7]) * pieceValues[i];
            }

            // Calculates mid and end game weights
            endGameWeight = (6600 - total) / 6600;
            midGameWeight = 1f - endGameWeight;
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

