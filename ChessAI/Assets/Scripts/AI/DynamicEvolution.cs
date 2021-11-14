using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.Engine
{
    public class DynamicEvolution
    {
        #region Class variables

        public Position position;
        public static StaticEvaluation staticEvaluation = new StaticEvaluation();
        public static PseudoLegalMoveGenerator moveGenerator = new PseudoLegalMoveGenerator();

        #endregion

        #region Core

        public float Evaluate(Position position, int searchDepth) 
        {
            // Updates position for evaluation
            this.position = position;

            // Returns evaluation
            return AlphaBetaEvaluation(searchDepth, float.MinValue, float.MaxValue);
        }

        #endregion

        #region Support functions

        private float AlphaBetaEvaluation(int depth, float alpha, float beta)
        {
            if (depth == 0)
            {
                return staticEvaluation.Evaluate(position) * (position.sideToMove ? 1 : -1);
            }

            List<ushort> moves = moveGenerator.GenerateLegalMoves(position);
            if (moves.Count == 0)
            {
                if (position.PlayerInCheck())
                {
                    return float.MinValue;
                }
                return 0f;
            }

            foreach(ushort move in moves)
            {
                position.MakeMove(move);
                float eval = -AlphaBetaEvaluation(depth - 1, -beta, -alpha);
                position.UnmakeMove(move);

                if (eval >= beta)
                {
                    return beta;
                }
                alpha = Mathf.Max(alpha, eval);
            }

            return alpha;
        }

        #endregion
    }
}

