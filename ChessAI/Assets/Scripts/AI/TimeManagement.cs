using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Engine
{
    public static class TimeManagement
    {
        /// <summary>
        /// Return recommended time the should be spend on calculating the move in a given time control
        /// </summary>
        /// <returns></returns>
        public static float GetRecomendedTime(float currentTime, float timeIncrement, float initialTime, int moveNumber)
        {
            if (currentTime / initialTime < 0.1) // Less then 10% of time remains, enters anti-flag mode
            {
                float time = currentTime * 0.05f;
                return time > 0.3f ? time : 0.3f;
            }
            if (moveNumber < 20) // Can use up to 62.5% of game time - opening book will save about 25% --- (highest rolling total 62.5%)
            {
                return initialTime * 0.025f + timeIncrement;
            }
            else if (moveNumber < 25) // Will use about 10% of total game time --- (highest rolling total 72.5%)
            {
                return initialTime * 0.02f + timeIncrement;
            }
            else if (moveNumber < 30) // Will use about 7.5% of total game time --- (highest rolling total 80%)
            {
                return initialTime * 0.015f + timeIncrement;
            }
            else if (moveNumber < 40) // Will use about 10 % of total game time --- (highest rolling total 80%)
            {
                return initialTime * 0.01f + timeIncrement;
            }
            else // End game mode
            {
                return initialTime * 0.05f + timeIncrement;
            }
        }
    }
}