using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.Engine
{
    public class ChessEngine
    {
        // Class variables
        #region Class variables

        public Position centralPosition;
        public MoveGenerator moveGenerator;

        #endregion

        // Class initialization
        #region Initialization

        public ChessEngine()
        {
            centralPosition = new Position(); // Creates new central position
            moveGenerator = new MoveGenerator(); // Creates new move generators
        }

        #endregion

        // Class utilities
        #region Utilities

        // Returns a list of legal moves in a given position
        public List<ushort> GenerateLegalMoves(byte genForColorIndex, bool includeQuiet = true, bool includeChecks = true)
        {
            return moveGenerator.GenerateLegalMoves(centralPosition, genForColorIndex, includeQuiet : includeQuiet, includeChecks : includeChecks);
        }

        // Load fen to the central position
        public void LoadFEN(FEN fen)
        {
            centralPosition.LoadFEN(fen);
        }

        // Calculates and returns the best move
        public ushort CalculateBestMove()
        {
            // Generates all legal moves
            List<ushort> moves = GenerateLegalMoves((byte)(centralPosition.sideToMove ? 0 : 1));
            // Creates variables to keep track of the best move, invalidism is used as a control with the worst score possible
            ushort bestMove = 0; // Invalid move (if never gets returned)
            int bestScore = int.MaxValue; // worst possible score

            // tests each generated move
            foreach (ushort move in moves)
            {
                centralPosition.MakeMove(move); // Makes the move
                DynamicEvolution dynamicEvolution = new DynamicEvolution(); // Creates new evaluator
                int score = dynamicEvolution.Evaluate(centralPosition, 3); // Evaluates the position/this move

                // Checks if the evaluated move is better then the best found move
                if (score < bestScore)
                {
                    bestMove = move;
                    bestScore = score;
                }
       
                centralPosition.UnmakeMove(move); // Unmakes the tested move
            }
            return bestMove; // Returns the best move
        }

        #endregion
    }
}

