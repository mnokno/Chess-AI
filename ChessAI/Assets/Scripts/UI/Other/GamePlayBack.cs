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
        private BoardInputManager inputManager;
        private int initialTime;
        private bool evalAfterChange;
        public int moveLeft { get; private set; }

        // Class constructor
        public GamePlayBack(string moves, string timeUsage, int initialTime, int timeIncrement, string difficulty, Board board, GameDataDisplay gameDataDisplay, BoardInputManager boardInputManager, bool evalAfterChange)
        {
            movePointer = 0;
            this.board = board;
            dataDisplay = gameDataDisplay;
            dataDisplay.SetAiName(difficulty);
            dataDisplay.SetTime(initialTime * 1000, initialTime * 1000);
            inputManager = boardInputManager;
            playBackPosition = new EngineUtility.Position();
            this.initialTime = initialTime;
            ConvertHistory(moves, timeUsage, initialTime, timeIncrement);
            moveLeft = moves.Length;
            this.evalAfterChange = evalAfterChange;
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
                board.LoadFEN(new EngineUtility.FEN(playBackPosition.GetFEN()).GetPiecePlacment(), false);
                inputManager.localPosition.UnmakeMove(moves[movePointer]);
                OnNewPosition();
                inputManager.PlayMoveSound(moves[movePointer]);
                dataDisplay.SetTime(times[movePointer].x, times[movePointer].y);

                if (movePointer == 0)
                {
                    dataDisplay.SetTime(initialTime * 1000, initialTime * 1000);
                }

                moveLeft++;
            }
        }

        public void Next()
        {
            if (movePointer < moves.Length)
            {
                playBackPosition.MakeMove(moves[movePointer]);
                board.MakeMove(moves[movePointer]);
                inputManager.localPosition.MakeMove(moves[movePointer]);
                OnNewPosition();
                inputManager.PlayMoveSound(moves[movePointer]);
                dataDisplay.SetTime(times[movePointer].x, times[movePointer].y);
                movePointer++;
                moveLeft--;
            }
        }

        public void First()
        {
            while (movePointer != 0)
            {
                movePointer--;
                playBackPosition.UnmakeMove(moves[movePointer]);
                board.LoadFEN(new EngineUtility.FEN(playBackPosition.GetFEN()).GetPiecePlacment(), false);
                inputManager.localPosition.UnmakeMove(moves[movePointer]);
                inputManager.PlayMoveSound(moves[movePointer]);
                dataDisplay.SetTime(times[movePointer].x, times[movePointer].y);
            }
            OnNewPosition();
            dataDisplay.SetTime(initialTime * 1000, initialTime * 1000);
            moveLeft = moves.Length;
        }

        public void Last()
        {
            while (movePointer < moves.Length)
            {
                playBackPosition.MakeMove(moves[movePointer]);
                board.MakeMove(moves[movePointer]);
                inputManager.localPosition.MakeMove(moves[movePointer]);
                inputManager.PlayMoveSound(moves[movePointer]);
                dataDisplay.SetTime(times[movePointer].x, times[movePointer].y);
                movePointer++;
            }
            OnNewPosition();
            moveLeft = 0;
        }

        private void OnNewPosition()
        {
            if (evalAfterChange)
            {
                Debug.Log(board.inputManager.localPosition.halfmoveClock);
                //Debug.Log(board.inputManager.localPosition.halfmoveClock);
                board.engineManager.EvaluatePosition(new EngineUtility.FEN(board.inputManager.localPosition.GetFEN()));
            }
        }
    }
}