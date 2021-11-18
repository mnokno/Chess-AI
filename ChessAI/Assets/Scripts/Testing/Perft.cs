using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Chess.EngineTests
{
    public class Perft
    {
        #region Class variables

        // Class generation variables
        public Position position = new Position();
        public MoveGenerator moveGenerator = new MoveGenerator();
        public PseudoLegalMoveGenerator pseudoLegalMoveGenerator = new PseudoLegalMoveGenerator();

        // Class counting variables
        int captures;
        int ep;
        int castles;
        int promations;
        int checks;
        int checkmates;

        // Flags
        bool countMoveTypes;

        #endregion

        #region Core functions

        // Public test function
        public void Test(FEN fen, int initialDept, int destinationDept, bool divide, bool countMoveTypes, bool onMainThread)
        {
            // Saves the flags
            this.countMoveTypes = countMoveTypes;

            // Loads the position from a FEN string
            position.LoadFEN(fen);

            // Runs the test
            if (onMainThread)
            {
                StartTest(initialDept, destinationDept, divide); // Main thread option
            }
            else
            {
                Task.Run(() => StartTest(initialDept, destinationDept, divide)); // New thread option
            }
        }

        // Public bulk test
        public void BulkTest(bool onMainThread)
        {
            // Runs the bulk Test
            if (onMainThread)
            {
                StartBulkTest(); // Main thread option
            }
            else
            {
                Task.Run(() => StartBulkTest()); // New thread option
            }
        }

        // Start a bulk test
        private void StartBulkTest()
        {
            // Creates and start the timer
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Reads the .json file to a string
            string json = File.ReadAllText(Application.streamingAssetsPath + "/Perft Testing Position.json");
            // Converts the jsonString to an array of objects
            TestPositionCollection testPositions = JsonUtility.FromJson<TestPositionCollection>(json);

            // Counters
            int passed = 1;
            int currentTestNumber = 0;
            int totalNumberOfTests = testPositions.positions.Length;

            // Runs the perft test for each positions
            foreach (TestPosition testPosition in testPositions.positions)
            {
                position.LoadFEN(new FEN(testPosition.fen));
                long testResult = PerftTest(testPosition.depth, false);
                Debug.Log($"{(testPosition.nodes == testResult ? "PASSED" : "FAILED")} --- FEN: \"{testPosition.fen}\" -- Depth: {testPosition.depth} -- ExpNodes: {testPosition.nodes} -- ResNodes: {testResult} -- TestNumber: {currentTestNumber}/{totalNumberOfTests}");
                passed += testPosition.nodes == testResult ? 1 : 0;
                currentTestNumber++;
            }

            // Logs the bulk result
            Debug.Log($"PASSED: {passed}/{totalNumberOfTests}");

            // Stops the timer and logs the time elapsed
            stopwatch.Stop();
            Debug.Log($"Elapsed time is {stopwatch.ElapsedMilliseconds / 1000f} seconds");
        }

        // Private initialization of the test
        private void StartTest(int initialDept, int destinationDept, bool divide)
        {
            // Creates and start the timer
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Runs perft for each depth
            for (int i = initialDept; i <= destinationDept; i++)
            {
                // Initiates move type counters
                InitMoveTypeCount();

                if (countMoveTypes)
                {
                    Debug.Log($"Dept: {i} -- Nodes: {PerftTest(i, divide)} -- Captures: {captures}, Ep: {ep} -- Castles: {castles} -- Promotions: {promations} -- Checks: {checks} -- Checkmates: {checkmates}");
                }
                else
                {
                    Debug.Log($"Dept: {i} -- Nodes: {PerftTest(i, divide)}");
                }
            }

            // Stops the timer and logs the time elapsed
            stopwatch.Stop();
            Debug.Log($"Elapsed time is {stopwatch.ElapsedMilliseconds / 1000f} seconds");
        }

        // Private recursive pert functions
        private long PerftTest(int dept, bool divide)
        {
            List<ushort> legalMoves = moveGenerator.GenerateLegalMoves(position, (byte)(position.sideToMove ? 0 : 1));
            //List<ushort> legalMoves = pseudoLegalMoveGenerator.GenerateLegalMoves(position);
            int nMoves = legalMoves.Count;
            long nodes = 0;
            long subNodes;

            if (countMoveTypes)
            {
                // Returns 1 since the end of a branch was reached
                if (dept == 0)
                {
                    return 1;
                }
            }
            else
            {
                // If the depth is 1 then the number of moves from the list is returned
                if (dept == 1 && !divide)
                {
                    return nMoves;
                }
                else if(dept == 0)
                {
                    return 1;
                }
            }


            // If the report is not detailed
            if (!divide)
            {
                for (int i = 0; i < nMoves; i++)
                {
                    // Runs PerftTest
                    position.MakeMove(legalMoves[i]);
                    nodes += PerftTest(dept - 1, false);
                    // Checks if move classification is enabled
                    if (countMoveTypes)
                    {
                        if (dept == 1)
                        {
                            ClassifeMove(legalMoves[i]);
                        }
                    }
                    position.UnmakeMove(legalMoves[i]);
                }
            }
            else // The report is detailed
            {
                for (int i = 0; i < nMoves; i++)
                {
                    // Saves the result of a PerftTest
                    position.MakeMove(legalMoves[i]);
                    subNodes = PerftTest(dept - 1, false);
                    // Checks if move classification is enabled
                    if (countMoveTypes)
                    {
                        // Classifies the move that was made
                        if (dept == 0)
                        {
                            ClassifeMove(legalMoves[i]);
                        }
                    }
                    position.UnmakeMove(legalMoves[i]);

                    // Logs the resoles of the sub branch test
                    Debug.Log($"{(Squares)Move.GetFrom(legalMoves[i])}{(Squares)Move.GetTo(legalMoves[i])}: {subNodes}");

                    // Updates the global count
                    nodes += subNodes;
                }
            }

            // Returns the total number of nodes
            return nodes;
        }

        #endregion

        #region Helper functions

        // Initializes move type counting
        private void InitMoveTypeCount()
        {
            // Sets all counters to 0
            captures = 0;
            ep = 0;
            castles = 0;
            promations = 0;
            checks = 0;
            checkmates = 0;
        }

        // Classifies the move and updates counters
        private void ClassifeMove(ushort move)
        {
            // Classifies the move
            if (Move.GetFlag(move) == Move.Flag.capture)
            {
                captures++;
            }
            else if (Move.GetFlag(move) == Move.Flag.epCapture)
            {
                captures++;
                ep++;
            }
            else if (Move.GetFlag(move) == Move.Flag.kingCastle || Move.GetFlag(move) == Move.Flag.queenCastle)
            {
                castles++;
            }
            else if (Move.GetFlag(move) == Move.Flag.knightPromotion || Move.GetFlag(move) == Move.Flag.bishopPromotion || Move.GetFlag(move) == Move.Flag.rookPromotion || Move.GetFlag(move) == Move.Flag.queenPromotion)
            {
                promations++;
            }
            else if (Move.GetFlag(move) == Move.Flag.knightPromotionCapture || Move.GetFlag(move) == Move.Flag.bishopPromotionCapture || Move.GetFlag(move) == Move.Flag.rookPromotionCapture || Move.GetFlag(move) == Move.Flag.queenPromotionCapture)
            {
                promations++;
                captures++;
            }

            // Checks if the move was a check
            if (moveGenerator.SquareAttackedBy(position, (byte)(position.sideToMove ? 0 : 1), (byte)BitOps.BitScanForward(position.bitboard.pieces[(position.sideToMove ? 0 : 1) * 7 + 5])))
            {
                checks++;
                // Checks if the move was a check mate
                if (moveGenerator.GenerateLegalMoves(position, (byte)(position.sideToMove ? 0 : 1)).Count == 0)
                {
                    checkmates++;
                }
            }
        }

        #endregion

        #region Enums and structs

        // Maps square ID to rank-file
        private enum Squares
        {
            a1, b1, c1, d1, e1, f1, g1, h1,
            a2, b2, c2, d2, e2, f2, g2, h2,
            a3, b3, c3, d3, e3, f3, g3, h3,
            a4, b4, c4, d4, e4, f4, g4, h4,
            a5, b5, c5, d5, e5, f5, g5, h5,
            a6, b6, c6, d6, e6, f6, g6, h6,
            a7, b7, c7, d7, e7, f7, g7, h7,
            a8, b8, c8, d8, e8, f8, g8, h8
        };

        // Used to parse .json test data
        [Serializable]
        private struct TestPositionCollection
        {
            public TestPosition[] positions;
        }
        // Used to parse .json test data
        [Serializable]
        private struct TestPosition
        {
            public int depth;
            public long nodes;
            public string fen;
        }

        #endregion
    }
}

