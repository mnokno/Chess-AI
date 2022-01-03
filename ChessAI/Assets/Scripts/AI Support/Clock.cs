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
        private float reamainignTimeWhite;
        private float reamainignTimeBlack;

        private bool whitesTurn;

        #endregion

        #region Class constructor

        public Clock(int initialTime, int timeIncrement)
        {
            this.initialTime = initialTime;
            this.timeIncrement = timeIncrement;
            reamainignTimeWhite = initialTime;
            reamainignTimeBlack = initialTime;
        }

        public Clock(int initialTime, int timeIncrement, float reamainignTimeWhite, float reamainignTimeBlack, bool whitesTurn)
        {
            this.initialTime = initialTime;
            this.timeIncrement = timeIncrement;
            this.reamainignTimeWhite = reamainignTimeWhite;
            this.reamainignTimeBlack = reamainignTimeBlack;
            this.whitesTurn = whitesTurn;
        }

        #endregion

        #region Class utilites

        public void StartClock()
        {

        }

        public void NextPlayersTurn()
        {

        }

        #endregion
    }
}

