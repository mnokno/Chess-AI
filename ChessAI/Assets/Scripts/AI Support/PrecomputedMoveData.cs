using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chess.EngineUtility
{
    public static class PrecomputedMoveData
    {
        // Class static variables
        #region Class variables

        private static readonly int[] directionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 }; // First 4 are orthogonal, last 4 are diagonals (N, S, W, E, NW, SE, NE, SW)
        private static readonly int[] knightAttackOffsets = { 15, 17, -17, -15, 10, -6, 6, -10 }; // All squares that a night can attack

        public static readonly byte[][] pawnMovesWhite = new byte[64][]; // White pawn move table NOTE: for captures only 
        public static readonly ulong[] pawnAttacksWhite = new ulong[64]; // White pawn attacks table NOTE: for captures only
        public static readonly ulong[] pawnPushesWhite = new ulong[64]; // White pawn pushes table NOTE: for pushes only
        public static readonly byte[][] pawnMovesBlack = new byte[64][]; // Black pawn move table NOTE: for captures only
        public static readonly ulong[] pawnAttacksBlack = new ulong[64]; // Black pawn attacks table NOTE: for captures only
        public static readonly ulong[] pawnPushesBlack = new ulong[64]; // Black pawn pushes table NOTE: for pushes only
        public static readonly byte[][] knightMoves = new byte[64][]; // Knight move table
        public static readonly ulong[] knightAttacks = new ulong[64]; // Knight attacks table
        public static readonly byte[][] bishopMoves = new byte[64][]; // Bishop move table 
        public static readonly ulong[] bishopAttacks = new ulong[64]; // Bishop attacks table
        public static readonly byte[][] rookMoves = new byte[64][]; // Rook move table 
        public static readonly ulong[] rookAttacks = new ulong[64]; // Rook attacks table
        public static readonly byte[][] queenMoves = new byte[64][]; // Queen move table
        public static readonly ulong[] queenAttacks = new ulong[64]; // Queen attacks table
        public static readonly byte[][] kingMoves = new byte[64][]; // King move table
        public static readonly ulong[] kingAttacks = new ulong[64]; // King attacks table

        #endregion

        // Class initialization
        #region Class initialization

        static PrecomputedMoveData()
        {
            CalculateWhitePawnTables();
            CalculateBlackPawnTables();
            CalculateKnightTables();
            CalculateBishopTables();
            CalculateRookTables();
            CalculateQueenTables();
            CalculateKingTables();
        }

        #endregion

        // Responsible for calculating static tables calculations 
        #region Static tables calculations

        // Calculates white pawn attack and move tables
        private static void CalculateWhitePawnTables()
        {
            for (int i = 0; i < 64; i++)
            {
                List<byte> whitePawnCurrentMoves = new List<byte>();
                ulong whitePawnCurrentAttacksBitboard = 0;

                foreach(int pawnMoveOffset in new int[] { 7, 9 } )
                {
                    int whitePawnMoveSquare = i + pawnMoveOffset;
                    if (whitePawnMoveSquare >= 0 && whitePawnMoveSquare < 64) // Ensures that the knight has not pawn the chess board
                    {
                        int fileDest = whitePawnMoveSquare % 8;
                        int rankDest = (whitePawnMoveSquare - fileDest) / 8;
                        int fileInit = i % 8;
                        int rankInit = (i - fileInit) / 8;
                        if (Mathf.Abs(fileInit - fileDest) == 1 & Mathf.Abs(rankInit - rankDest) == 1) // Ensures that the pawn has not raped around the edge of the chess board
                        {
                            whitePawnCurrentMoves.Add((byte)whitePawnMoveSquare);
                            whitePawnCurrentAttacksBitboard |= 1ul << whitePawnMoveSquare;
                        }
                    }
                }

                pawnMovesWhite[i] = whitePawnCurrentMoves.ToArray();
                pawnAttacksWhite[i] = whitePawnCurrentAttacksBitboard;

                if (i >= 8 & i <= 15) // Double pawn push available
                {
                    pawnPushesWhite[i] = BitboardUtility.GenerateShift(1, i + 8) | BitboardUtility.GenerateShift(1, i + 16);
                }
                else if (i < 56) // Not on the last rank
                {
                    pawnPushesWhite[i] = BitboardUtility.GenerateShift(1, i + 8);
                }
            }
        }

        // Calculates black pawn attack and move tables
        private static void CalculateBlackPawnTables()
        {
            for (int i = 0; i < 64; i++)
            {
                List<byte> blackPawnCurrentMoves = new List<byte>();
                ulong blackPawnCurrentAttacksBitboard = 0;

                foreach (int pawnMoveOffset in new int[] { -7, -9 })
                {
                    int blackPawnMoveSquare = i + pawnMoveOffset;
                    if (blackPawnMoveSquare >= 0 && blackPawnMoveSquare < 64) // Ensures that the knight has not pawn the chess board
                    {
                        int fileDest = blackPawnMoveSquare % 8;
                        int rankDest = (blackPawnMoveSquare - fileDest) / 8;
                        int fileInit = i % 8;
                        int rankInit = (i - fileInit) / 8;
                        if (Mathf.Abs(fileInit - fileDest) == 1 & Mathf.Abs(rankInit - rankDest) == 1) // Ensures that the pawn has not raped around the edge of the chess board
                        {
                            blackPawnCurrentMoves.Add((byte)blackPawnMoveSquare);
                            blackPawnCurrentAttacksBitboard |= 1ul << blackPawnMoveSquare;
                        }
                    }
                }

                pawnMovesBlack[i] = blackPawnCurrentMoves.ToArray();
                pawnAttacksBlack[i] = blackPawnCurrentAttacksBitboard;

                if (i >= 48 & i <= 55) // Double pawn push available
                {
                    pawnPushesBlack[i] = BitboardUtility.GenerateShift(1, i - 8) | BitboardUtility.GenerateShift(1, i - 16);
                }
                else if (i > 7) // Not on the last rank
                {
                    pawnPushesBlack[i] = BitboardUtility.GenerateShift(1, i - 8);
                }
            }
        }

        // Calculates knight attack and move tables
        private static void CalculateKnightTables()
        {
            for (int i = 0; i < 64; i++)
            {
                List<byte> knightCurrentMoves = new List<byte>();
                ulong knightCurrentAttacksBitboard = 0;

                foreach (int knightMoveOffset in knightAttackOffsets)
                {
                    int knightMoveSquare = i + knightMoveOffset;
                    if (knightMoveSquare >= 0 && knightMoveSquare < 64) // Ensures that the knight has not jumps off the chess board
                    {
                        int fileDest = knightMoveSquare % 8;
                        int rankDest = (knightMoveSquare - fileDest) / 8;
                        int fileInit = i % 8;
                        int rankInit = (i - fileInit) / 8;
                        if ((Mathf.Abs(fileDest - fileInit) + Mathf.Abs(rankDest - rankInit)) == 3) // Ensures that the knight has not raped around the edge of the chess board
                        {
                            knightCurrentMoves.Add((byte) knightMoveSquare);
                            knightCurrentAttacksBitboard |= (1ul << knightMoveSquare);
                        }
                    }
                }

                knightMoves[i] = knightCurrentMoves.ToArray();
                knightAttacks[i] = knightCurrentAttacksBitboard;
            }
        }

        // Calculate bishop attack and move tables
        private static void CalculateBishopTables()
        {
            for (int i = 0; i < 64; i++)
            {
                List<byte> bishopCurrentMoves = new List<byte>();
                ulong bishopCurrentAttacksBitboard = 0;

                foreach(int offset in new int[] { 7, -7, 9, -9 })
                {
                    int steps = 1;
                    while (true)
                    {
                        int bishopMoveSquare = i + offset * steps;
                        if (bishopMoveSquare >= 0 && bishopMoveSquare < 64) // Ensures that the bishop has not exit the chess board
                        {
                            int fileDest = bishopMoveSquare % 8;
                            int rankDest = (bishopMoveSquare - fileDest) / 8;
                            int fileInit = i % 8;
                            int rankInit = (i - fileInit) / 8;
                            if (Mathf.Abs(fileDest - fileInit) == Mathf.Abs(rankDest - rankInit)) // Ensures that the bishop has not raped around the edge of the chess board
                            {
                                bishopCurrentMoves.Add((byte)bishopMoveSquare);
                                bishopCurrentAttacksBitboard |= (1ul << bishopMoveSquare);
                                steps++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                bishopMoves[i] = bishopCurrentMoves.ToArray();
                bishopAttacks[i] = bishopCurrentAttacksBitboard;
            }
        }

        // Calculate rook attack and move tables
        private static void CalculateRookTables()
        {
            for (int i = 0; i < 64; i++)
            {
                List<byte> rookCurrentMoves = new List<byte>();
                ulong rookCurrentAttacksBitboard = 0;

                foreach (int offset in new int[] { 8, -8, -1, 1 })
                {
                    int steps = 1;
                    while (true)
                    {
                        int rookMoveSquare = i + offset * steps;
                        if (rookMoveSquare >= 0 && rookMoveSquare < 64) // Ensures that the rook has not exit the chess board
                        {
                            int fileDest = rookMoveSquare % 8;
                            int rankDest = (rookMoveSquare - fileDest) / 8;
                            int fileInit = i % 8;
                            int rankInit = (i - fileInit) / 8;
                            if (Mathf.Abs(fileDest - fileInit) == 0 | Mathf.Abs(rankDest - rankInit) == 0) // Ensures that the rook has not raped around the edge of the chess board
                            {
                                rookCurrentMoves.Add((byte)rookMoveSquare);
                                rookCurrentAttacksBitboard |= (1ul << rookMoveSquare);
                                steps++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                rookMoves[i] = rookCurrentMoves.ToArray();
                rookAttacks[i] = rookCurrentAttacksBitboard;
            }
        }

        // Calculates queen attack and move tables --- NOTE: This function requires BISHOP TABLES and ROOK TABLES to be calculated first
        private static void CalculateQueenTables()
        {
            for(int i = 0; i < 64; i++)
            {
                // Joins the bishop and rook tables
                byte[] queenCurrentMoves = bishopMoves[i];
                Array.Resize(ref queenCurrentMoves, bishopMoves[i].Length + rookMoves[i].Length);
                Array.Copy(rookMoves[i], 0, queenCurrentMoves, bishopMoves[i].Length, rookMoves[i].Length);

                queenMoves[i] = queenCurrentMoves;
                queenAttacks[i] = bishopAttacks[i] | rookAttacks[i];
            }
        }

        // Calculates king attack and move tables
        private static void CalculateKingTables()
        {
            for (int i = 0; i < 64; i++)
            {
                List<byte> kingCurrentMoves = new List<byte>();
                ulong kingCurrentAttacksBitboard = 0;

                foreach (int kingMoveOffset in directionOffsets)
                {
                    int kingMoveSquare = i + kingMoveOffset;
                    if (kingMoveSquare >= 0 && kingMoveSquare < 64) // Ensures that the king has not exit the chess board
                    {
                        int fileDest = kingMoveSquare % 8;
                        int rankDest = (kingMoveSquare - fileDest) / 8;
                        int fileInit = i % 8;
                        int rankInit = (i - fileInit) / 8;
                        if (Mathf.Max(Mathf.Abs(fileDest - fileInit), Mathf.Abs(rankDest - rankInit)) == 1) // Ensures that the king has not raped around the edge of the chess board
                        {
                            kingCurrentMoves.Add((byte)kingMoveSquare);
                            kingCurrentAttacksBitboard |= (1ul << kingMoveSquare);
                        }
                    }
                }

                kingMoves[i] = kingCurrentMoves.ToArray();
                kingAttacks[i] = kingCurrentAttacksBitboard;
            }
        }

        #endregion
    }
}

