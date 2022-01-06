using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Chess.EngineUtility
{
    public class Clock
    {
        #region Class variables

        public int initialTime { get; private set; }
        private int timeIncrement;
        public float reamainignTimeWhite { get; private set; }
        public float reamainignTimeBlack { get; private set; }
        private Position position;

        private float currentTimeTotal = 0;
        private bool whitesTurn;
        public Stopwatch stopwatch { get; private set; }

        #endregion

        #region Class constructor

        // For staring new game
        public Clock(int initialTime, int timeIncrement, Position position)
        {
            whitesTurn = true;
            this.initialTime = initialTime * 60000;
            this.timeIncrement = timeIncrement;
            reamainignTimeWhite = initialTime * 60000f;
            reamainignTimeBlack = initialTime * 60000f;
            this.position = position;
            stopwatch = new Stopwatch();
        }

        // For loading game
        public Clock(int initialTime, int timeIncrement, float reamainignTimeWhite, float reamainignTimeBlack, bool whitesTurn, Position position)
        {
            this.initialTime = initialTime * 60000;
            this.timeIncrement = timeIncrement;
            this.reamainignTimeWhite = reamainignTimeWhite;
            this.reamainignTimeBlack = reamainignTimeBlack;
            this.whitesTurn = whitesTurn;
            this.position = position;
            stopwatch = new Stopwatch();
        }

        #endregion

        #region Class utilities

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
            UnityEngine.Debug.Log($"White: {reamainignTimeWhite / 1000f} Black: {reamainignTimeBlack / 1000f}");
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

        /// <summary>
        /// Return both time in a single vector, x = whites time, y = blacks time
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCurrentTimes()
        {
            if (whitesTurn)
            {
                return new Vector2(reamainignTimeWhite - stopwatch.ElapsedMilliseconds, reamainignTimeBlack);
            }
            else
            {
                return new Vector2(reamainignTimeWhite, reamainignTimeBlack - stopwatch.ElapsedMilliseconds);
            }
        }
        #endregion
    }
}

