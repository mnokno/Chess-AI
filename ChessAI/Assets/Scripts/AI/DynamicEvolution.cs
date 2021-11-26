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

        #endregion

        #region Core

        public int Evaluate(Position position, int searchDepth) 
        {
            // Updates position for evaluation
            this.position = position;

            // Returns evaluation
            return AlphaBetaEvaluation(-65536, 65536, searchDepth);
        }

        #endregion

        #region Support functions

        private int AlphaBetaEvaluation(int alpha, int beta, int depth)
        {
            // Checks if the root of the basic search was reached
            if (depth == 0)
            {
                //return staticEvaluation.Evaluate(position);
                return QuiesceAlphaBetaEvaluation(alpha, beta, 0);
            }

            // Generates list of all legal moves
            List<ushort> moves = moveGenerator.GenerateLegalMoves(position, (byte)(position.sideToMove ? 0 : 1));

            // Detects checkmates and stalemate
            if (moves.Count == 0)
            {
                if (position.PlayerInCheck())
                {
                    return int.MinValue;
                }
                else
                {
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
                    return beta;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }

            // If beta was never returned alpha is returned
            return alpha;
        }

        private int QuiesceAlphaBetaEvaluation(int alpha, int beta, int iteration)
        {
            int stand_pat = staticEvaluation.Evaluate(position);
            if (stand_pat >= beta)
            {
                return beta;
            }
            if (stand_pat > alpha)
            {
                alpha = stand_pat;
            }

            List<ushort> moves = moveGenerator.GenerateLegalMoves(position, (byte)(position.sideToMove ? 0 : 1), includeQuiet: false, includeChecks: (iteration < iterationCheckCap));
            foreach(ushort move in moves)
            {
                position.MakeMove(move);
                int eval = -QuiesceAlphaBetaEvaluation(-beta, -alpha, (iteration + 1));
                position.UnmakeMove(move);

                if (eval >= beta)
                {
                    return beta;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }
            return alpha;
        }

        #endregion
    }
}

