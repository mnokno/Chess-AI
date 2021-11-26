using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.Engine
{
    public class MoveOrdering
    {
        #region Class variables

        private int[] scores;
        private Position position;

        #endregion

        #region Class constructor

        public MoveOrdering(Position position)
        {
            // Saves reference to the passed variables
            this.position = position;
        }

        #endregion

        #region Utility

        // Returns an ordered list of moves  
        public void OrderMoves(List<ushort> moves, MoveGenerator moveGenerator)
        {
            // Creates a list of scored moves
            scores = new int[moves.Count];

            // Scores each move and adds it to the score moves array
            for (int i = 0; i < moves.Count; i++)
            {
                // Initiates local variables
                int score = 0;
                ushort form = Move.GetFrom(moves[i]);
                byte pieceToMove = position.squareCentric.pieces[form];
                byte pieceToTake = position.squareCentric.pieces[Move.GetTo(moves[i])];
                int flag = Move.GetFlag(moves[i]);

                // Adjusts the score base on the capture efficiency (if its a capture move)
                if (pieceToTake != (byte)SquareCentric.PieceType.Empty)
                {
                    if (pieceToMove == (byte)SquareCentric.PieceType.King)
                    {
                        // If the king can capture this piece that means that this piece was unprotected 
                        score += 3600 + StaticEvaluation.pieceValues[pieceToTake] * 4;
                    }
                    else
                    {
                        // We add additional 1000 points since we want even bad capture like QxB to be check before quiet moves
                        score += 900 + StaticEvaluation.pieceValues[pieceToTake] - StaticEvaluation.pieceValues[pieceToMove];
                    }
                }

                // Checks if the piece to move is a pawn
                if (pieceToMove == (byte)SquareCentric.PieceType.Pawn)
                {
                    // Adds additional points if its a promotion move
                    if (flag == Move.Flag.knightPromotion || flag == Move.Flag.knightPromotionCapture)
                    {
                        score += StaticEvaluation.pieceValues[(int)StaticEvaluation.PieceType.Knight];
                    }
                    else if (flag == Move.Flag.bishopPromotion || flag == Move.Flag.bishopPromotionCapture)
                    {
                        score += StaticEvaluation.pieceValues[(int)StaticEvaluation.PieceType.Bishop];
                    }
                    else if (flag == Move.Flag.rookPromotion || flag == Move.Flag.rookPromotionCapture)
                    {
                        score += StaticEvaluation.pieceValues[(int)StaticEvaluation.PieceType.Rook];
                    }
                    else if (flag == Move.Flag.queenPromotion || flag == Move.Flag.queenPromotionCapture)
                    {
                        score += StaticEvaluation.pieceValues[(int)StaticEvaluation.PieceType.Queen];
                    }
                }
                else
                {
                    // Subtracts points if a move is entering a square controlled by the opponents pawns, (does not apply to pawn moves)
                    if ((Constants.primitiveBitboards[form] & moveGenerator.underPawnAttackBB) != 0)
                    {
                        score -= 1000;
                    }
                }

                // Adds the move to score move array
                scores[i] = score;
            }

            // Sorts the list after the scores have been assigned
            SortMoves(moves);
        }

        // Sorts the list using quick sort
        private void SortMoves(List<ushort> moves)
        {
            // Swaps tow elements
            void Swap(int indexA, int indexB)
            {
                (scores[indexA], scores[indexB]) = (scores[indexB], scores[indexA]);
                (moves[indexA], moves[indexB]) = (moves[indexB], moves[indexA]);
            }

            // Bobble sort for now
            for (int i = 0; i < moves.Count - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    int swapIndex = j - 1;
                    if (scores[swapIndex] < scores[j])
                    {
                        Swap(swapIndex, j);
                    }
                }
            }
        }

        #endregion
    }
}