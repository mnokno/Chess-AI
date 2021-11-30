using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.Engine
{
    public class DynamicEvolution
    {
        #region Class variables

        public int iterationCheckCap = 10;
        public Position position;
        public static StaticEvaluation staticEvaluation = new StaticEvaluation();
        public static MoveGenerator moveGenerator = new MoveGenerator();
        private ChessEngine chessEngine;
        public int coreDepthSearch;

        #endregion

        #region Core

        public int Evaluate(Position position, int searchDepth, ChessEngine chessEngine) 
        {
            // Updates position for evaluation
            this.position = position;
            // Saves reference to chess engine
            this.chessEngine = chessEngine;
            // Save base depth
            coreDepthSearch = searchDepth;
            // Returns evaluation
            return AlphaBetaEvaluation(Constants.negativeInfinity, Constants.positiveInfinity, searchDepth);
        }

        #endregion

        #region Support functions

        private int AlphaBetaEvaluation(int alpha, int beta, int depth)
        {
            // Used to store eval 
            int score;
            // Used to determine node type
            TranspositionTable.NodeType nodeType = TranspositionTable.NodeType.alphaCutoff;

            // Checks if the evaluation for this position can be fetched from the transposition table
            int transpositionScore = chessEngine.mainTranspositionTable.LookupEvaluation((byte)depth, alpha, beta);
            if (transpositionScore != TranspositionTable.LookupFailed)
            {
                chessEngine.nodes++;
                chessEngine.transpositiontableHits++;
                return transpositionScore;
            }

            // Checks if the root of the basic search was reached
            if (depth == 0)
            {
                //return staticEvaluation.Evaluate(position);
                chessEngine.nodes++;
                score = QuiesceAlphaBetaEvaluation(alpha, beta, 0);
                // Saves the result to transposition table
                chessEngine.mainTranspositionTable.AddEntry((byte)depth, TranspositionTable.NodeType.exact, score, 0);
                return score;
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
                    // Saves the result to transposition table
                    chessEngine.mainTranspositionTable.AddEntry((byte)depth, TranspositionTable.NodeType.exact, Constants.negativeInfinity, 0);
                    return Constants.negativeInfinity;
                }
                else
                {
                    chessEngine.nodes++;
                    // Saves the result to transposition table
                    chessEngine.mainTranspositionTable.AddEntry((byte)depth, TranspositionTable.NodeType.exact, 0, 0);
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
                    // Saves the result to transposition table
                    chessEngine.mainTranspositionTable.AddEntry((byte)depth, TranspositionTable.NodeType.betaCutoff, beta, move);
                    chessEngine.nodes++;
                    return beta;
                }
                if (eval > alpha)
                {
                    nodeType = TranspositionTable.NodeType.exact;
                    alpha = eval;
                }
            }

            // Saves the result to transposition table
            chessEngine.mainTranspositionTable.AddEntry((byte)depth, nodeType, alpha, 0);
            // If beta was never returned alpha is returned
            chessEngine.nodes++;
            return alpha;
        }

        private int QuiesceAlphaBetaEvaluation(int alpha, int beta, int iteration)
        {
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

