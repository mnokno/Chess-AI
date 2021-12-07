using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public static class Constants
    {
        // Class variables
        #region Class variables

        public const int positiveInfinity = 50000000; // Used as infinity but not int.MaxValue to avoid overflows to the signed bit
        public const int negativeInfinity = -50000000; // Used as -infinity but not int.MaxValue overflows to the signed bit

        public static readonly int[] directionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 }; // First 4 are orthogonal, last 4 are diagonals (N, S, W, E, NW, SE, NE, SW)
        public static readonly byte[][] squaresToEdge = new byte[8][]; // Stores number of squares to the edge, use [[N, S, W, E, NW, SE, NE, SW].IndexOf(N)][FromSquareIndex] to retrieve the value

        public static readonly ulong[] rankMasks = new ulong[8] { 0xff, 0xff00, 0xff0000, 0xff000000, 0xff00000000, 0xff0000000000, 0xff000000000000, 0xff00000000000000 }; // Masks for the ranks
        public static readonly ulong[] fileMasks = new ulong[8] { 0x8080808080808080, 0x4040404040404040, 0x2020202020202020, 0x1010101010101010, 0x808080808080808, 0x404040404040404, 0x202020202020202, 0x101010101010101 }; // Masks for the files
        public static readonly ulong[] enEpssantMasks = new ulong[2] { 0xff0000000000, 0xff0000 }; // Contains mask for white and black en-passant captures
        public static readonly byte[][] enPassantFileToIndex = new byte[2][]; // Used to quickly convert en-passant target file to index [color][file]
        public static readonly ulong[] castleEmptyMasks = new ulong[4] { 0xe, 0x60, 0x0e00000000000000, 0x6000000000000000 }; // 0 = whit queen side, 1 = white king side, 2 = black queen side, 3 = black king side
        public static readonly ulong[] castleNotAttackedMask = new ulong[4] { 0x1c, 0x70, 0x1c00000000000000, 0x07000000000000000 }; // 0 = whit queen side, 1 = white king side, 2 = black queen side, 3 = black king side


        // 6 == noWe -> +7     7 == nort -> +8     0 == noEa -> +9
        // 0x7F7F7F7F7F7F7F00  0xFFFFFFFFFFFFFF00  0xFEFEFEFEFEFEFE00
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // . . . . . . . .     . . . . . . . .     . . . . . . . .

        // 5 == west -> -1                         1 == east -> +1
        // 0x7F7F7F7F7F7F7F7F                      0xFEFEFEFEFEFEFEFE
        // 1 1 1 1 1 1 1 .                         . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .                         . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .                         . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .                         . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .                         . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .                         . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .                         . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .                         . 1 1 1 1 1 1 1

        // 4 == soWe -> -9     3 == sout -> -8     2 == soEa -> -7
        // 0x007F7F7F7F7F7F7F  0x00FFFFFFFFFFFFFF  0x00FEFEFEFEFEFEFE
        // . . . . . . . .     . . . . . . . .     . . . . . . . .
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1
        // 1 1 1 1 1 1 1 .     1 1 1 1 1 1 1 1     . 1 1 1 1 1 1 1

        public static readonly ulong[] wrapMasks = new ulong[8] { 0xFEFEFEFEFEFEFE00, 0xFEFEFEFEFEFEFEFE, 0x00FEFEFEFEFEFEFE, 0x00FFFFFFFFFFFFFF, 0x007F7F7F7F7F7F7F, 0x7F7F7F7F7F7F7F7F, 0x7F7F7F7F7F7F7F00, 0xFFFFFFFFFFFFFF00}; // wrap masks [noEa, east, soEa, sout, soWe, west, noWe, nort] 
        public static readonly byte[][] indexToRankFileTable = new byte[64][]; // Maps square index's to a rank and a file access using indexToRankFileTable[index][0=file, 1=rank]
        public static readonly ulong[] primitiveBitboards = new ulong[64]; // Contains all bitboard containing only one 1

        #endregion

        // Class static constructor
        #region Class static constructor

        static Constants()
        {
            CalculateEnPassantFileToIndexTable();
            CalculateSquaresToEdge();
            CalculatesIndexToRankFileTable();
            CalculatePrimitiveBitboards();
        }

        #endregion

        //Class initialization functions
        #region Class initialization functions

        // Calculate enPassantFileToIndex table
        private static void CalculateEnPassantFileToIndexTable()
        {
            for (int i = 0; i < 2; i++) // For each color
            {
                enPassantFileToIndex[i] = new byte[8];
                for (int j = 0; j < 8; j++) // For each rank
                {
                    enPassantFileToIndex[i][j] = (byte)(j + 8 * (5 - (3 * i))); // Calculates the index
                }
            }
        }

        // Calculates square to the edge
        private static void CalculateSquaresToEdge()
        {
            for (int i = 0; i < 8; i++)
            {
                squaresToEdge[i] = new byte[64];
                for (int j = 0; j < 64; j++)
                {
                    byte toEdge = 0;
                    int initIndex = j;
                    int desIndex = j + directionOffsets[i] * (toEdge + 1);

                    int fileDest = desIndex % 8;
                    int rankDest = (desIndex - fileDest) / 8;
                    int fileInit = initIndex % 8;
                    int rankInit = (initIndex - fileInit) / 8;
                    
                    while (desIndex >= 0 && desIndex < 64 && Mathf.Max(Mathf.Abs(fileDest - fileInit), Mathf.Abs(rankDest - rankInit)) == 1)
                    {
                        toEdge++;
                        initIndex = desIndex;
                        desIndex = j + directionOffsets[i] * (toEdge + 1);               

                        fileDest = desIndex % 8;
                        rankDest = (desIndex - fileDest) / 8;
                        fileInit = initIndex % 8;
                        rankInit = (initIndex - fileInit) / 8;
                    }

                    squaresToEdge[i][j] = toEdge;
                }
            }
        }


        // Calculates indexToRankFileTable
        private static void CalculatesIndexToRankFileTable()
        {
            for (int i = 0; i < 64; i++)
            {
                byte[] fileRank = new byte[2];
                fileRank[0] = (byte)(i % 8);
                fileRank[1] = (byte)((i - fileRank[0]) / 8);
                indexToRankFileTable[i] = fileRank;
            }
        }

        // Calculates primitiveBitboards
        private static void CalculatePrimitiveBitboards()
        {
            for (int i = 0; i < 64; i++)
            {
                primitiveBitboards[i] = 1ul << i;
            }
        }

        #endregion
    }
}

