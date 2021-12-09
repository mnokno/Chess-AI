using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public class PieceSquareTables
    {
        public static int CalculateScore(float midGameWeight, float endGameWeight, Position position) 
        {
            // Creates local variables
            float midGameValue = 0;
            float endGameValue = 0;
            int pos;
            short[] midGameTable;
            short[] endGameTable;

            // Calculates total for each game phase
            for (int i = 0; i < 6; i++)
            {
                ulong whitePieces = position.bitboard.pieces[i];
                ulong blackPieces = position.bitboard.pieces[i + 7];
                midGameTable = GetMidGameTable(i);
                endGameTable = GetEndGameTable(i);

                while (whitePieces != 0)
                {
                    pos = BitOps.BitScanForward(whitePieces);
                    int file = pos % 8;
                    int rank = (pos - file) / 8;
                    int tableIndex = (7 - rank) * 8 + file;
                    midGameValue += midGameTable[tableIndex];
                    endGameValue += endGameTable[tableIndex];
                    whitePieces ^= Constants.primitiveBitboards[pos];
                }
                while (blackPieces != 0)
                {
                    pos = BitOps.BitScanForward(blackPieces);
                    midGameValue -= midGameTable[pos];
                    endGameValue -= endGameTable[pos];
                    blackPieces ^= Constants.primitiveBitboards[pos];
                }
            }

            return (int)((midGameValue * midGameWeight) + (endGameValue * endGameWeight));
        }

        public static short[] GetMidGameTable(int tableNumber)
        {
            switch (tableNumber)
            {
                case 0:
                    return midGamePawnTable;
                case 1:
                    return midGameKnightTable;
                case 2:
                    return midGameBishopTable;
                case 3:
                    return midGameRookTable;
                case 4:
                    return midGameQueenTable;
                default:
                    return midGameKingTable;

            }
        }

        public static short[] GetEndGameTable(int tableNumber)
        {
            switch (tableNumber)
            {
                case 0:
                    return endGamePawnTable;
                case 1:
                    return endGameKnightTable;
                case 2:
                    return endGameBishopTable;
                case 3:
                    return endGameRookTable;
                case 4:
                    return endGameQueenTable;
                default:
                    return endGameKingTable;
            }
        }

        public static short[] midGamePawnTable = new short[64]
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        public static short[] endGamePawnTable = new short[64]
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            20, 20, 20, 30, 30, 20, 20, 10,
            10, 15, 15, 25, 25, 15, 15, 10,
            0,  0,  0,  0,  0,  0,  0,  0,
            -5,-5,-5,-5,-5,- 5, -5, -5,
            -5,-10,-10,-20,-20,-10,-10,-5,
            0, 0,  0,  0,  0,  0,  0,  0
        };

        public static short[] midGameKnightTable = new short[64]
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };

        public static short[] endGameKnightTable = new short[64]
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };

        public static short[] midGameBishopTable = new short[64]
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };

        public static short[] endGameBishopTable = new short[64]
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0, 0,  0,  0,  0,  0,-10,
            -10,  0, 10, 10, 10,  10,  0,-10,
            -10,  0, 10, 10, 10,  10,  0,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10,  0, 10, 10, 10, 10, 0,-10,
            -10,  0, 0,  0,  0,  0,  0,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };

        public static short[] midGameRookTable = new short[64]
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0
        };

        public static short[] endGameRookTable = new short[64]
        {
            -10,-5,-5,-5,-5,-5,-5,-10,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 5, 5, 5, 5, 0, -5,
            -5, 0, 5, 5, 5, 5, 0, -5,
            -5, 0, 5, 5, 5, 5, 0, -5,
            -5, 0, 5, 5, 5, 5, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -10,-5,-5,-5,-5,-5,-5,-10
        };

        public static short[] midGameQueenTable = new short[64]
        {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  5,  5,  5,  5,  0, -5,
            0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };

        public static short[] endGameQueenTable = new short[64]
        {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  5,  5,  5,  5,  0, -5,
            -5,  0,  5,  5,  5,  5,  0, -5,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };

        public static short[] midGameKingTable = new short[64]
        {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            20, 20,  0,  0,  0,  0, 20, 20,
            20, 30, 10,  0,  0, 10, 30, 20
        };

        public static short[] endGameKingTable = new short[64]
        {
            -100,-60,-60,-60,-60,-60,-60,-100,
            -60,-40,-20,-20,-20,-20,-40,-60,
            -60,-20, 20, 30, 30, 20,-10,-60,
            -60,-20, 30, 30, 30, 30,-10,-60,
            -60,-20, 30, 30, 30, 30,-10,-60,
            -60,-20, 20, 30, 30, 20,-10,-60,
            -60,-40,-20,-20,-20,-20,-40,-60,
            -100,-60,-60,-60,-60,-60,-60,-100
        };
    }
}

