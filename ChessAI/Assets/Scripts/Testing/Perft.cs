using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.EngineTests
{
    public class Perft
    {
        public static readonly string[] testPosition = new string[14];
        public Position position = new Position();
        public MoveGenerator moveGenerator = new MoveGenerator();
        //public PseudoLegalMoveGenerator pseudoLegalMoveGenerator = new PseudoLegalMoveGenerator();

        ulong captures = 0;
        ulong ep = 0;
        ulong castle = 0;

        public void Test(int dept, int testNumber)
        {
            position.LoadFEN("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R");
            Task.Run(() => Test(dept));
            //Test(dept);
        }

        private void Test(int dept)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            for (int i = 1; i <= dept; i++)
            {
                captures = 0;
                ep = 0;
                castle = 0;
                Debug.Log($"Dept: {i}, Nodes: {PerftTest(i)}");
                Debug.Log($"Captures: {captures}, ep: {ep}, castle: {castle}");
            }
            stopwatch.Stop();
            Debug.Log($"Elapsed time is {stopwatch.ElapsedMilliseconds} milliseconds");
        }

        private ulong PerftTest(int dept)
        {
            List<ushort> legalMoves = moveGenerator.GenerateLegalMoves(position, (byte)(position.sideToMove ? 0 : 1));
            //List<ushort> legalMoves = pseudoLegalMoveGenerator.GenerateLegalMoves(position);
            int nMoves = legalMoves.Count;
            ulong nodes = 0;

            if (dept == 1)
            {
                foreach(ushort move in legalMoves)
                {
                    if (Move.GetFlag(move) == 4)
                    {
                        captures++;
                    }
                    else if (Move.GetFlag(move) == 5)
                    {
                        ep++;
                    }
                    else if (Move.GetFlag(move) == 2 || Move.GetFlag(move) == 3)
                    {
                        castle++;
                    }
                }
                return (ulong)nMoves;
            }

            for (int i = 0; i < nMoves; i++)
            {
                position.MakeMove(legalMoves[i]);
                nodes += PerftTest(dept - 1);
                position.UnmakeMove(legalMoves[i]);
            }

            return nodes;
        }
    }
}

