using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.EngineTests
{
    public static class MakeUnmakeMove
    {

        public static void Test()
        {
            Position position = new Position();

            string initBitboard = BitboardUtility.FormatBitboard(position.bitboard);
            string initSquareCentric = SquareCentricUtility.FormateSquareCentric(position.squareCentric);
            ulong initZobristKey = position.zobristKey;
            bool initSideToMove = position.sideToMove; 
            byte initCastlingRights = position.castlingRights; 
            byte initEnPassantTargetFile = position.enPassantTargetFile; 
            byte initHalfmoveClock = position.halfmoveClock;

            List<ushort> moves = new List<ushort>()
            {
                Move.GenMove(10, 26, Move.Flag.doublePawnPush), // c4
                Move.GenMove(52, 36, Move.Flag.doublePawnPush), // e5
                Move.GenMove(1, 18, Move.Flag.quietMove), // Nc3
                Move.GenMove(36, 28, Move.Flag.quietMove), // e4
                Move.GenMove(13, 29, Move.Flag.doublePawnPush), // f4
                Move.GenMove(28, 21, Move.Flag.epCapture), // exf4

                Move.GenMove(6, 21, Move.Flag.capture), // Nxf3
                Move.GenMove(61, 25, Move.Flag.quietMove), // Bb4
                Move.GenMove(12, 20, Move.Flag.quietMove), // e3
                Move.GenMove(62, 45, Move.Flag.quietMove), // Nf6
                Move.GenMove(5, 19, Move.Flag.quietMove), // Bd3

                Move.GenMove(57, 42, Move.Flag.quietMove), // Nc6
                Move.GenMove(0, 1, Move.Flag.quietMove), // Rb1
                Move.GenMove(56, 57, Move.Flag.quietMove), // Rb8
                Move.GenMove(0, 0, Move.Flag.kingCastle), // O-O
                Move.GenMove(0, 0, Move.Flag.kingCastle), // O-O

                Move.GenMove(8, 16, Move.Flag.quietMove), // a3
                Move.GenMove(48, 32, Move.Flag.doublePawnPush), // a5
                Move.GenMove(16, 25, Move.Flag.capture), // axb4

                Move.GenMove(32, 24, Move.Flag.quietMove), // a4
                Move.GenMove(25, 33, Move.Flag.quietMove), // b5
                Move.GenMove(24, 16, Move.Flag.quietMove), // a3
                Move.GenMove(33, 41, Move.Flag.quietMove), // b6

                Move.GenMove(16, 8, Move.Flag.quietMove), // a2
                Move.GenMove(41, 50, Move.Flag.capture), // b6xc7
                Move.GenMove(8, 0, Move.Flag.rookPromotion), // a1=R
                Move.GenMove(50, 59, Move.Flag.queenPromotionCapture), // c7xd8=Q
            };

            for (int i = 0; i < moves.Count; i++)
            {
                for (int j = 0; j < i+1; j++)
                {
                    position.MakeMove(moves[j]);
                }

                if (false) // Optimally prints the positions log
                {
                    #pragma warning disable CS0162 // Unreachable code detected
                    Debug.Log(
                    #pragma warning disable CS0162 // Unreachable code detected
                        $"TEST_LOG_B TEST_ID:{i}, \n" +
                        $"Zobrist key : {position.zobristKey}, \n" +
                        $"Side to move : {position.sideToMove}, \n" +
                        $"Castling rights : {position.castlingRights}, \n" +
                        $"En-passant target file : {position.enPassantTargetFile}, \n" +
                        $"Half-move clock : {position.halfmoveClock}, \n\n" +
                        SquareCentricUtility.FormateSquareCentric(position.squareCentric) + "\n" +
                        BitboardUtility.FormatBitboard(position.bitboard)
                    );
                }

                for (int j = 0; j < i+1; j++)
                {
                    position.UnmakeMove(moves[i - j]);
                }

                if (false) // Optimally prints the positions log
                {
                    #pragma warning disable CS0162 // Unreachable code detected
                    Debug.Log(
                    #pragma warning restore CS0162 // Unreachable code detected
                        $"TEST_LOG_A TEST_ID:{i}, \n" +
                        $"Zobrist key : {position.zobristKey}, \n" +
                        $"Side to move : {position.sideToMove}, \n" +
                        $"Castling rights : {position.castlingRights}, \n" +
                        $"En-passant target file : {position.enPassantTargetFile}, \n" +
                        $"Half-move clock : {position.halfmoveClock}, \n\n" +
                        SquareCentricUtility.FormateSquareCentric(position.squareCentric) + "\n" +
                        BitboardUtility.FormatBitboard(position.bitboard)
                    );
                }

                string resBitboard = BitboardUtility.FormatBitboard(position.bitboard);
                string resSquareCentric = SquareCentricUtility.FormateSquareCentric(position.squareCentric);
                ulong resZobristKey = position.zobristKey;
                bool resSideToMove = position.sideToMove;
                byte resCastlingRights = position.castlingRights;
                byte resEnPassantTargetFile = position.enPassantTargetFile;
                byte resHalfmoveClock = position.halfmoveClock;

                if (true) // Optimally prints the test results
                {
                    #pragma warning disable CS0162 // Unreachable code detected
                    Debug.Log(
                    #pragma warning restore CS0162 // Unreachable code detected
                    $"TEST_RESULT TEST_ID:{i}, \n" +
                    $"Bitboard test {(initBitboard == resBitboard ? "PASSED" : "FAILED")}, \n" +
                    $"Square centric test {(initSquareCentric == resSquareCentric ? "PASSED" : "FAILED")}, \n" +
                    $"Zobrist key test {(initZobristKey == resZobristKey ? "PASSED" : "FAILED")}, \n" +
                    $"Side to move test {(initSideToMove == resSideToMove ? "PASSED" : "FAILED")}, \n" +
                    $"Castling rights test {(initCastlingRights == resCastlingRights ? "PASSED" : "FAILED")}, \n" +
                    $"En-passant target file test {(initEnPassantTargetFile == resEnPassantTargetFile ? "PASSED" : "FAILED")}, \n" +
                    $"Half-move clock test {(initHalfmoveClock == resHalfmoveClock ? "PASSED" : "FAILED")}, "
                    );
                }
            }
        }
    }
}

