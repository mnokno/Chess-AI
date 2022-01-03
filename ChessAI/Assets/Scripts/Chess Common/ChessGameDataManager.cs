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

        public struct ChessGameData
        {
            public bool loadGame;
            public bool newGame;
            public int gameID;
            public string AiStrength;
            public string moves;
            public bool isHumanWhite;
            public string timeUsage;
            public float timeLeft;
            public int initialTime;
            public int timeIncrement;
            public int unmakesLimit;
            public int unmakesMade;
            public bool whereUnamkesEnabled;
            public string startDate;
            public string endDate;
            public string gameTitle;
        }
    }
}

