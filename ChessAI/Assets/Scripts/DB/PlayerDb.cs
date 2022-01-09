using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Data;
using Mono.Data.Sqlite;
using System;

namespace Chess.DB
{
    public class PlayerDb
    {
        #region Class variables

        protected static readonly string conn = "URI=file:" + Application.streamingAssetsPath + "/PlayerPlayedGames.db"; // Path to the opening database
        protected IDbConnection dbconn; // Connection to the data base
        protected IDbCommand dbcmd; // Command interface for the data base
        protected bool isOpen; // true if teh connection to the data base is open
        protected static bool isOpenGlobal = false; // true if the data base is open by some instance of openingDB

        #endregion

        #region Class constructor

        public PlayerDb()
        {
            isOpen = false; // The connection is initially closed
        }

        #endregion

        #region Class utilities

        // Opens connection to the data base
        public void OpenDB()
        {
            // Check if the data base can be close
            if (isOpenGlobal)
            {
                throw new Exception("You can't open the connection to the opening data base since its used by another process/diffrent instance of OpeningDbReader or OpeningDbWriter!");
            }
            else if (isOpen)
            {
                return;
            }

            dbconn = new SqliteConnection(conn); // Create connection to the data base
            dbconn.Open(); // Opens connection
            dbcmd = dbconn.CreateCommand(); // Create command interface for the data base
            isOpen = true; // Marks the connsction as open
            isOpenGlobal = true; // Marks the connsction as open
        }

        // Closes connection to the data base
        public void CloseDB()
        {
            // Check if the data base can be close
            if (!isOpen && isOpenGlobal)
            {
                throw new Exception("You can't close the connection to the opening data base since its used by another process/diffrent instance of OpeningDbReader or OpeningDbWrite!");
            }
            else if (!isOpen)
            {
                return;
            }

            dbconn.Close(); // Loses connection
            dbconn.Dispose(); // Disposes of the conenction
            dbcmd.Dispose(); // Disposes of the command interface
            isOpen = false; // Marks the connection as closed
            isOpenGlobal = false; // Marks the connection as closed
        }

        #endregion

        #region Structures

        /*
         * 
         * CREATE TABLE "Players" (
	     *   "Player_ID"	INTEGER NOT NULL UNIQUE,
	     *   "Username"	TEXT NOT NULL,
	     *   "DefaultDifficulty"	TEXT NOT NULL,
	     *   PRIMARY KEY("Player_ID" AUTOINCREMENT)
         *);
         */
        public struct PlayerRecord
        {
            
            public int playerID;
            public string username;
            public string defaultDifficulty;
            public bool isValid;

            public PlayerRecord(int playerID, string username, string defaultDifficulty, bool isValid)
            {
                this.playerID = playerID;
                this.username = username;
                this.defaultDifficulty = defaultDifficulty;
                this.isValid = isValid;
            }
        }


        /*
         *CREATE TABLE "SavedGames" (
	     *    "Game_ID"	INTEGER NOT NULL UNIQUE,
	     *    "Player_ID"	INTEGER NOT NULL,
	     *    "Moves"	TEXT NOT NULL,
	     *    "TimeUsage"	TEXT NOT NULL,
	     *    "AIStrength"	TEXT NOT NULL,
	     *    "IsHumanWhite"	TEXT NOT NULL,
	     *    "StartDate"	TEXT NOT NULL,
	     *    "GameTitle"	TEXT NOT NULL,
	     *    "UnmakesLimit"	INTEGER NOT NULL,
	     *    "UnmakesMade"	INTEGER NOT NULL,
	     *    "TimeControll"	TEXT NOT NULL,
	     *    FOREIGN KEY("Player_ID") REFERENCES "Players"("Player_ID"),
	     *    PRIMARY KEY("Game_ID" AUTOINCREMENT)
         *);
         */
        public struct SavedGameRecord
        {
            public int gameID;
            public int playerID;
            public string moves;
            public string timeUsage;
            public string AIStrength;
            public string isHumanWhite;
            public string startDate;
            public string gameTitle;
            public int unmakesLimit;
            public int unmakesMade;
            public string timeControll;
            public bool isValid;
        }

        /*
         *CREATE TABLE "GameRecords" (
	     *    "Game_ID"	INTEGER NOT NULL UNIQUE,
	     *    "Player_ID"	INTEGER NOT NULL,
	     *    "Moves"	TEXT NOT NULL,
	     *    "TimeUsage"	TEXT NOT NULL,
	     *    "AIStrength"	TEXT NOT NULL,
	     *    "IsHumanWhite"	TEXT NOT NULL,
	     *    "StartDate"	TEXT NOT NULL,
         *    "EndDate"	TEXT NOT NULL,
	     *    "GameTitle"	TEXT NOT NULL,
	     *    "UnmakesLimit"	INTEGER NOT NULL,
	     *    "UnmakesMade"	INTEGER NOT NULL,
	     *    "TimeControll"	TEXT NOT NULL,
		 *	  "GameResult" TEXT NOT NULL,
	     *    FOREIGN KEY("Player_ID") REFERENCES "Players"("Player_ID"),
	     *    PRIMARY KEY("Game_ID" AUTOINCREMENT)
         *);
         */
        public struct GameRecord
        {
            public int gameID;
            public int playerID;
            public string moves;
            public string timeUsage;
            public string AIStrength;
            public string isHumanWhite;
            public string startDate;
            public string endDate;
            public string gameTitle;
            public int unmakesLimit;
            public int unmakesMade;
            public string timeControll;
            public string gameResult;
            public bool isValid;
        }

        #endregion
    }
}