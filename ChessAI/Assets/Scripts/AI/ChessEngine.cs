using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;
using System.Threading.Tasks;
using System;

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
        // Creates new evaluator instance
        public DynamicEvolution dynamicEvolution = new DynamicEvolution(); 

        // Counters
        public int eval = 0;
        public long nodes = 0;
        public byte maxDepth = 0;
        public string moveString = "";
        public int transpositiontableHits = 0;
        // Timer
        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Used for progressive depth search
        public ushort bestMove = ushort.MaxValue; // Set initially to an invalid move
        public ushort previousBestMove = ushort.MaxValue; // Set initially to an invalid move
        public byte currentBaseDepht = 0;
        private bool cancelSearch = false;

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

        // Calculate best move in the given time limit 
        public ushort CalculateBestMove(float timeLimit)
        {
            // Resets flag, and parameters
            cancelSearch = false;
            currentBaseDepht = 1;
            // Schedules task cancellation
            CancelSearch(timeLimit);
            // Starts a timer
            stopwatch.Restart();
            stopwatch.Start();
            // Resets counters
            eval = 0;
            nodes = 0;
            maxDepth = 0;
            transpositiontableHits = 0;

            while (!cancelSearch)
            {
                // Calculates best move at current depth
                ushort move = CalculateBestMove(currentBaseDepht);
                if (!cancelSearch)
                {
                    // Saves the search results
                    previousBestMove = bestMove;
                    bestMove = move;
                    // Updates Depth
                    currentBaseDepht++;
                }
            }

            // Stops the timer
            stopwatch.Stop();
            // Returns the best move found
            return bestMove;
        }

        // Stops the search imminently
        public void CancelSearch()
        {
            dynamicEvolution.CancelSearch();
            cancelSearch = true;
        }

        // Stops the search
        public async void CancelSearch(float delay)
        {
            // Waits the time delay
            await Task.Delay(TimeSpan.FromSeconds(delay));
            // Cancels the search
            dynamicEvolution.CancelSearch();
            cancelSearch = true;
        }

        // Calculates and returns the best move, depth need to be at least 1
        private ushort CalculateBestMove(byte depth)
        {
            // Generates all legal moves
            List<ushort> moves = GenerateLegalMoves((byte)(centralPosition.sideToMove ? 0 : 1));
            // Creates variables to keep track of the best move, invalidism is used as a control with the worst score possible
            ushort bestMove = moves.Count != 0 ? moves[0] : (ushort)0; // Invalid move (gets returned when a side is in a checkmate)
            int bestScore = Constants.positiveInfinity; // worst possible score      

            // tests each generated move
            foreach (ushort move in moves)
            {
                // Check if the search was canceled
                if (cancelSearch)
                {
                    return 0;
                }

                centralPosition.MakeMove(move); // Makes the move
                int score = dynamicEvolution.Evaluate(centralPosition, depth - 1, this, beta: bestScore); // Evaluates the position/this move

                // Checks if the evaluated move is better then the best found move
                if (score < bestScore)
                {
                    bestMove = move;
                    bestScore = score;
                }
                
                centralPosition.UnmakeMove(move); // Unmakes the tested move
            }

            // Saves the evaluation, and move made
            eval = bestScore;
            moveString = ((SquareCentric.Squares)Move.GetFrom(bestMove)).ToString() + ((SquareCentric.Squares)Move.GetTo(bestMove)).ToString(); 
            // Returns the best move
            return bestMove;
        }

        #endregion
    }
}

