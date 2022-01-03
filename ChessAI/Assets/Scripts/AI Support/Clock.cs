using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Chess.EngineUtility
{
    public class Clock
    {
        #region Class variables

        private int initialTime;
        private int timeIncrement;
        public float reamainignTimeWhite { get; private set; }
        public float reamainignTimeBlack { get; private set; }
        private Position position;

        private float currentTimeTotal = 0;
        private bool whitesTurn;
        public Stopwatch stopwatch { get; private set; }

        #endregion

        #region Class constructor

        public Clock(int initialTime, int timeIncrement, Position position)
        {
            this.initialTime = initialTime;
            this.timeIncrement = timeIncrement;
            reamainignTimeWhite = initialTime;
            reamainignTimeBlack = initialTime;
            this.position = position;
        }

        public Clock(int initialTime, int timeIncrement, float reamainignTimeWhite, float reamainignTimeBlack, bool whitesTurn, Position position)
        {
            this.initialTime = initialTime;
            this.timeIncrement = timeIncrement;
            this.reamainignTimeWhite = reamainignTimeWhite;
            this.reamainignTimeBlack = reamainignTimeBlack;
            this.whitesTurn = whitesTurn;
            this.position = position;
        }

        #endregion

        #region Class utilites

        public void StartClock()
        {
            stopwatch.Start();
        }

        public void NextPlayersTurn()
        {
            stopwatch.Stop();
            float timeTaken = currentTimeTotal + stopwatch.ElapsedMilliseconds; // Time taken to make a move
            position.timeTakenPerMove.Add(timeTaken); // Logs the time taken to make this move
            if (whitesTurn)
            {
                reamainignTimeWhite -= timeTaken; // Updates remaining time
                reamainignTimeWhite += timeIncrement; // Adds time increment
            }
            else
            {
                reamainignTimeBlack -= timeTaken; // Updates remaining time
                reamainignTimeBlack += timeIncrement; // Adds time increment
            }
            currentTimeTotal = 0;
            whitesTurn = !whitesTurn;
            stopwatch.Reset();
            stopwatch.Start();
        }

        public void Resume()
        {
            stopwatch.Start();
        }

        public void Pause()
        {
            stopwatch.Stop();
            currentTimeTotal += stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
        }

        #endregion
    }
}

