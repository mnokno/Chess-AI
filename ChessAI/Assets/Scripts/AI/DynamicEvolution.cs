using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.Engine
{
    public class DynamicEvolution
    {
        #region Class variables

        // Checks after this depth will not be examined in quiesce search
        public int iterationCheckCap = 10;
        // Position to be evaluated
        public Position position;
        public static StaticEvaluation staticEvaluation = new StaticEvaluation();
        public static MoveGenerator moveGenerator = new MoveGenerator();
        private ChessEngine chessEngine;
        // Depth for which all the node are search
        public int coreDepthSearch;
        // Used to cancel a search
        private bool cancelSearch = false;

        #endregion

        #region Core

        public int Evaluate(Position position, int searchDepth, ChessEngine chessEngine, int alpha = Constants.negativeInfinity, int beta = Constants.positiveInfinity) 
        {
            // Updates position for evaluation
            this.position = position;
            // Saves reference to chess engine
            this.chessEngine = chessEngine;
            // Save base depth
            coreDepthSearch = searchDepth;
            // Retest cancellation flag
            cancelSearch = false;
            // Returns evaluation
            return AlphaBetaEvaluation(alpha, beta, searchDepth);
        }

        public void CancelSearch()
        {
            cancelSearch = true;
        }

        #endregion

        #region Support functions

        private int AlphaBetaEvaluation(int alpha, int beta, int depth)
        {
            // Checks if the search was canceled
            if (cancelSearch)
            {
                return 0;
            }

            // Checks for draws
            if (position.gameState != Position.GameState.OnGoing)
            {
                return 0;
            }

            // Checks if the root of the basic search was reached
            if (depth == 0)
            {
                //return staticEvaluation.Evaluate(position);
                chessEngine.nodes++;
                return QuiesceAlphaBetaEvaluation(alpha, beta, 0);
            }

            // Generates list of all legal moves
            List<ushort> moves = moveGenerator.GenerateLegalMoves(position, (byte)(position.sideToMove ? 0 : 1));
            MoveOrdering moveOrdering = new MoveOrdering(position);
            moveOrdering.OrderMoves(moves, moveGenerator);

            // Detects checkmates and stalemate
            if (moves.Count == 0)
            {
                if (position.PlayerInCheck())
                {
                    chessEngine.nodes++;
                    position.gameState = Position.GameState.Checkmate;
                    return Constants.negativeInfinity;
                }
                else
                {
                    chessEngine.nodes++;
                    position.gameState = Position.GameState.Stalemate;
                    return 0;
                }
            }

            // Evaluates each generated move
            foreach(ushort move in moves)
            {
                position.MakeMove(move);
                int eval = -AlphaBetaEvaluation(-beta, -alpha, depth - 1);
                position.UnmakeMove(move);

                if (eval >= beta)
                {
                    chessEngine.nodes++;
                    return beta;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }

            // If beta was never returned alpha is returned
            chessEngine.nodes++;
            return alpha;
        }

        private int QuiesceAlphaBetaEvaluation(int alpha, int beta, int iteration)
        {
            // Checks if the search was canceled
            if (cancelSearch)
            {
                return 0;
            }

            // Updates counter for info display
            if ((iteration + 5) > chessEngine.maxDepth)
            {
                chessEngine.maxDepth = (byte)(iteration + 5);
            }
            // Calculates a static score since a player does not have to make a bad capture, instead of a bad capture a static score is used
            int staticScore = staticEvaluation.Evaluate(position); 

            if (staticScore >= beta) // Fail soft
            {
                chessEngine.nodes++;
                return beta;
            }
            if (staticScore > alpha) // Fail hard
            {
                alpha = staticScore;
            }

            // Generates all legal moves
            List<ushort> moves = moveGenerator.GenerateLegalMoves(position, (byte)(position.sideToMove ? 0 : 1), includeQuiet: false, includeChecks: (iteration < iterationCheckCap));
            MoveOrdering moveOrdering = new MoveOrdering(position);
            moveOrdering.OrderMoves(moves, moveGenerator);

            // Checks for check mates
            if (moveGenerator.inCheck && (moves.Count == 0))
            {
                return Constants.negativeInfinity;
            }

            foreach (ushort move in moves)
            {
                position.MakeMove(move);
                int eval = -QuiesceAlphaBetaEvaluation(-beta, -alpha, (iteration + 1));
                position.UnmakeMove(move);

                if (eval >= beta)
                {
                    chessEngine.nodes++;
                    return beta;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }
            chessEngine.nodes++;
            return alpha;
        }

        #endregion
    }
}

