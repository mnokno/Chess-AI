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
        
        // Chess engine connections references
        public Position centralPosition;
        public MoveGenerator moveGenerator;

        // Transposition tables settings
        public ulong mainTransTableSize = 640000;
        // Transposition tables
        public TranspositionTable mainTranspositionTable;

        // Counters
        public int eval = 0;
        public long nodes = 0;
        public byte maxDepth = 0;
        public string moveString = "";
        public int transpositiontableHits = 0;
        // Timer
        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        #endregion

        // Class initialization
        #region Initialization

        public ChessEngine()
        {
            centralPosition = new Position(); // Creates new central position
            moveGenerator = new MoveGenerator(); // Creates new move generators
            mainTranspositionTable = new TranspositionTable(mainTransTableSize); // Creates new transposition table
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
            // Starts a timer
            stopwatch.Restart();
            stopwatch.Start();

            // Resets counters
            eval = 0;
            nodes = 0;
            maxDepth = 0;
            transpositiontableHits = 0;

            // Generates all legal moves
            List<ushort> moves = GenerateLegalMoves((byte)(centralPosition.sideToMove ? 0 : 1));
            // Creates variables to keep track of the best move, invalidism is used as a control with the worst score possible
            ushort bestMove = 0; // Invalid move (gets returned when a side is in a checkmate)
            int bestScore = Constants.positiveInfinity; // worst possible score

            // tests each generated move
            foreach (ushort move in moves)
            {
                centralPosition.MakeMove(move); // Makes the move
                DynamicEvolution dynamicEvolution = new DynamicEvolution(); // Creates new evaluator
                int score = dynamicEvolution.Evaluate(centralPosition, 4, this); // Evaluates the position/this move

                // Checks if the evaluated move is better then the best found move
                if (score < bestScore)
                {
                    bestMove = move;
                    bestScore = score;
                }
       
                centralPosition.UnmakeMove(move); // Unmakes the tested move
            }

            // Stops the timer
            stopwatch.Stop();
            // Saves the evaluation, and move made
            eval = bestScore;
            moveString = ((SquareCentric.Squares)Move.GetFrom(bestMove)).ToString() + ((SquareCentric.Squares)Move.GetTo(bestMove)).ToString(); 
            // Returns the best move
            return bestMove;
        }

        #endregion
    }
}

