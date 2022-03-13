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

            // Sorts the list after the scores have been assigned, multiple sort algorithms have been implemented to achieve maximum performance
            if (moves.Count < 6)
            {
                BoobleSort(moves);
            }
            else if (moves.Count < 200)
            {
                QuickSort(ref moves);
            }
            else
            {
                MergeSort(ref moves);
            }
        }

        // Sorts the list using bobble sort
        private void BoobleSort(List<ushort> moves)
        {
            // Swaps two elements
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

        // Sorts the list using quick sort
        private void QuickSort(ref List<ushort> moves)
        {
            void QuickSort(ref List<ushort> toSort, int beg, int end)
            {
                if (beg < end)
                {
                    // Orders the partition and return the index of the pivot point after ordering 
                    int pivotIndex = Partition(ref toSort, beg, end);

                    // Sorts the partition to the left and right of the pivot index
                    QuickSort(ref toSort, beg, pivotIndex - 1);
                    QuickSort(ref toSort, pivotIndex + 1, end);
                }
            }

            int Partition(ref List<ushort> toSort, int beg, int end)
            {
                // Gets the pivot element (most right element)
                int pivot = scores[end];

                // Orders the partition
                int pivotIndex = beg;
                for (int j = beg; j < end; j++)
                {
                    if (scores[j] > pivot)
                    {
                        (toSort[pivotIndex], toSort[j]) = (toSort[j], toSort[pivotIndex]);
                        (scores[pivotIndex], scores[j]) = (scores[j], scores[pivotIndex]);
                        pivotIndex++;
                    }
                }
                (toSort[pivotIndex], toSort[end]) = (toSort[end], toSort[pivotIndex]);
                (scores[pivotIndex], scores[end]) = (scores[end], scores[pivotIndex]);

                // Return the pivot index
                return pivotIndex;
            }

            QuickSort(ref moves, 0, moves.Count - 1);
        }

        // Sorts the list using merge sort
        private void MergeSort(ref List<ushort> moves)
        {
            void Merge(ref List<ushort> toSort, int beg, int mid, int end)
            {
                // Extracts the arrays
                int leftSize = mid - beg + 1;
                int righSize = end - mid;

                ushort[] leftArray = new ushort[leftSize];
                int[] leftArrayScores = new int[leftSize];
                ushort[] rightArray = new ushort[righSize];
                int[] rightArrayScores = new int[righSize];

                for (int i = 0; i < leftSize; ++i)
                {
                    leftArray[i] = toSort[beg + i];
                    leftArrayScores[i] = scores[beg + i];
                }
                for (int i = 0; i < righSize; ++i)
                {
                    rightArray[i] = toSort[mid + 1 + i];
                    leftArrayScores[i] = scores[beg + i];
                }


                // Index counters
                int leftIndex = 0;
                int rightIndex = 0;
                int mainIndex = beg;

                // Sorts the result data into the array
                while (leftIndex < leftSize && rightIndex < righSize)
                {
                    if (scores[leftIndex] > scores[rightIndex])
                    {
                        toSort[mainIndex] = leftArray[leftIndex];
                        scores[mainIndex] = leftArrayScores[leftIndex];
                        leftIndex++;
                    }
                    else
                    {
                        toSort[mainIndex] = rightArray[rightIndex];
                        scores[mainIndex] = rightArrayScores[rightIndex];
                        rightIndex++;
                    }
                    mainIndex++;
                }

                // Appends remaining elements
                while (leftIndex < leftSize)
                {
                    toSort[mainIndex] = leftArray[leftIndex];
                    scores[mainIndex] = leftArrayScores[leftIndex];
                    leftIndex++;
                    mainIndex++;
                }
                while (rightIndex < righSize)
                {
                    toSort[mainIndex] = rightArray[rightIndex];
                    scores[mainIndex] = rightArrayScores[rightIndex];
                    rightIndex++;
                    mainIndex++;
                }
            }

            void MergeSort(ref List<ushort> toSort, int beg, int end)
            {
                if (beg >= end)
                {
                    return;
                }

                int mid = beg + (end - beg) / 2;
                MergeSort(ref toSort, beg, mid);
                MergeSort(ref toSort, mid + 1, end);
                Merge(ref toSort, beg, mid, end);
            }


            MergeSort(ref moves, 0, moves.Count - 1);
        }

        #endregion
    }
}