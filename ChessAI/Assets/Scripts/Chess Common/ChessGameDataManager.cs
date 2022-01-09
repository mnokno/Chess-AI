using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Common
{
    public class ChessGameDataManager : MonoBehaviour
    {
        public static ChessGameDataManager instance;
        public ChessGameData chessGameData;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
        }

        [System.Serializable]
        public struct ChessGameData
        {
            // To tell other classes what should be done whit the chess data
            public bool loadGame;
            public bool newGame;
            public bool saved;
            // Reference to the game DI (only when the game is read from database)
            public int gameID;
            // Data for loading game
            public string AiStrength;
            public string moves;
            public bool isHumanWhite;
            public string timeUsage;
            public float timeLeft;
            public int initialTime;
            public int timeIncrement;
            public int unmakesLimit;
            public int unmakesMade;
            // For game review
            public string startDate;
            public string endDate;
            public string gameTitle;
            public string gameResult;
            public string gameResultCode;
        }
    }
}

