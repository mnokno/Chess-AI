using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class GamePlayBack
    {
        // Class variables
        private ushort[] moves;
        private Vector2[] times;
        private Board board;
        private GameDataDisplay dataDisplay;
        private int movePointer;
        private EngineUtility.Position playBackPosition;

        // Class constructor
        public GamePlayBack(string moves, string timeUsage, int initialTime, int timeIncrement, Board board, GameDataDisplay gameDataDisplay)
        {
            movePointer = 0;
            this.board = board;
            dataDisplay = gameDataDisplay;
            playBackPosition = new EngineUtility.Position();
            ConvertHistory(moves, timeUsage, initialTime, timeIncrement);
        }

        // Class initialization support
        private void ConvertHistory(string movesString, string timeUsageString, int initialTime, int timeIncrementG)
        {
            string[] stringMoves = movesString.Split(":");
            List<ushort> movesList = new List<ushort>();
            if (stringMoves.Length != 0)
            {
                for (int i = stringMoves.Length - 1; i >= 0; i--)
                {
                    if (stringMoves[i] != "")
                    {
                        movesList.Add(ushort.Parse(stringMoves[i]));
                    }
                }
            }
            moves = movesList.ToArray();

            int timeIncrement = timeIncrementG * 1000;
            int whiteTime = initialTime * 1000;
            int blackTime = whiteTime;
            bool whitesTurn = true;
            string[] timeUsage = timeUsageString.Split(":");
            List<Vector2> timeUsageList = new List<Vector2>();
            if (timeUsage.Length != 0)
            {
                foreach (string timeUse in timeUsage)
                {
                    if (timeUse != "")
                    {
                        if (whitesTurn)
                        {
                            whiteTime -= (int.Parse(timeUse) - timeIncrement);
                            whitesTurn = false;
                        }
                        else
                        {
                            blackTime -= (int.Parse(timeUse) - timeIncrement);
                            whitesTurn = true;
                        }
                        timeUsageList.Add(new Vector2(whiteTime, blackTime));
                    }
                }
            }
            times = timeUsageList.ToArray();
        }

        // Class utilities

        public void Previous()
        {
            if (movePointer != 0)
            {
                movePointer--;
                playBackPosition.UnmakeMove(moves[movePointer]);
                board.LoadFEN(new EngineUtility.FEN(playBackPosition.GetFEN()).GetPiecePlacment());
                dataDisplay.SetTime(times[movePointer].x, times[movePointer].y);
            }
        }

        public void Next()
        {
            if (movePointer < moves.Length)
            {
                playBackPosition.MakeMove(moves[movePointer]);
                board.MakeMove(moves[movePointer]);
                dataDisplay.SetTime(times[movePointer].x, times[movePointer].y);
                movePointer++;
            }
        }
    }
}

