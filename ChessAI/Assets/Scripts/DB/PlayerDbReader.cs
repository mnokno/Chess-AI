using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Data;
using Mono.Data.Sqlite;
using System;

namespace Chess.DB
{
    public class PlayerDbReader : PlayerDb
    {
        #region Players table management

        // Returns a player record
        public PlayerRecord TryGetPlayersRecord(string name)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM Players WHERE Username='{name}' LIMIT 1;";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Checks if the record was found, if not returns null
            if (reader.IsDBNull(0))
            {
                // Disposes of the reader
                reader.Close();
                reader.Dispose();
                // Returns null since the record was not found
                return new PlayerRecord(0, "", "", false);
            }

            // Gets the record
            reader.Read();
            int Player_ID = reader.GetInt32(0);
            string Username = reader.GetString(1);
            string DefaultDifficulty = reader.GetString(2);

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the record
            return new PlayerRecord(Player_ID, Username, DefaultDifficulty, true);
        }

        // Reads players
        public PlayerRecord TryGetPlayersRecord(int playerID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM Players WHERE Player_ID='{playerID}' LIMIT 1;";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Checks if the record was found, if not returns null
            if (reader.IsDBNull(0))
            {
                // Disposes of the reader
                reader.Close();
                reader.Dispose();
                // Returns null since the record was not found
                return new PlayerRecord(0, "", "", false);
            }

            // Gets the record
            reader.Read();
            int Player_ID = reader.GetInt32(0);
            string Username = reader.GetString(1);
            string DefaultDifficulty = reader.GetString(2);

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the record
            return new PlayerRecord(Player_ID, Username, DefaultDifficulty, true);
        }

        // Returns all player records
        public PlayerRecord[] ReadAllPlayers()
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM Players;";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Reads all player records
            List<PlayerRecord> playerRecords = new List<PlayerRecord>();
            while (reader.Read())
            {
                playerRecords.Add(new PlayerRecord(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), true));
            }

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the player records
            return playerRecords.ToArray();
        }

        #endregion

        #region Saved game table management

        public bool IsSavedGameNameTaken(string gameName, int userID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            return IsGameNameTaken(gameName, userID, "SavedGames");
        }

        public SavedGameRecord[] ReadSavedGames(int playerID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM SavedGames WHERE Player_ID={playerID};";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Reads all player records
            List<SavedGameRecord> playerRecords = new List<SavedGameRecord>();
            while (reader.Read())
            {
                playerRecords.Add(new SavedGameRecord()
                {
                    gameID = reader.GetInt32(0),
                    playerID = reader.GetInt32(1),
                    moves = reader.GetString(2),
                    timeUsage = reader.GetString(3),
                    AIStrength = reader.GetString(4),
                    isHumanWhite = reader.GetString(5),
                    startDate = reader.GetString(6),
                    gameTitle = reader.GetString(7),
                    unmakesLimit = reader.GetInt32(8),
                    unmakesMade = reader.GetInt32(9),
                    timeControll = reader.GetString(10)
                });
            }

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the saved games records
            return playerRecords.ToArray();
        }

        public SavedGameRecord ReadSavedGame(int gameID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM SavedGames WHERE Game_ID={gameID} LIMIT 1;";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Reads all player records
            reader.Read();
            SavedGameRecord playerRecords = new SavedGameRecord()
            {
                gameID = reader.GetInt32(0),
                playerID = reader.GetInt32(1),
                moves = reader.GetString(2),
                timeUsage = reader.GetString(3),
                AIStrength = reader.GetString(4),
                isHumanWhite = reader.GetString(5),
                startDate = reader.GetString(6),
                gameTitle = reader.GetString(7),
                unmakesLimit = reader.GetInt32(8),
                unmakesMade = reader.GetInt32(9),
                timeControll = reader.GetString(10)
            };

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the saved game record
            return playerRecords;
        }

        #endregion

        #region Game records table management

        public bool IsGameRecordNameTaken(string gameName, int userID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            return IsGameNameTaken(gameName, userID, "GameRecords");
        }

        public GameRecord[] ReadGameRecords(int playerID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM GameRecords WHERE Player_ID={playerID};";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Reads all player records
            List<GameRecord> gameRecords = new List<GameRecord>();
            while (reader.Read())
            {
                gameRecords.Add(new GameRecord()
                {
                    gameID = reader.GetInt32(0),
                    playerID = reader.GetInt32(1),
                    moves = reader.GetString(2),
                    timeUsage = reader.GetString(3),
                    AIStrength = reader.GetString(4),
                    isHumanWhite = reader.GetString(5),
                    startDate = reader.GetString(6),
                    endDate = reader.GetString(7),
                    gameTitle = reader.GetString(8),
                    unmakesLimit = reader.GetInt32(9),
                    unmakesMade = reader.GetInt32(10),
                    timeControll = reader.GetString(11),
                    gameResult = reader.GetString(12)
                });
            }

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the games records
            return gameRecords.ToArray();
        }

        public GameRecord ReadGameRecord(int gameID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM GameRecords WHERE Game_ID={gameID} LIMIT 1;";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Reads all player records
            reader.Read();
            GameRecord gameRecord = new GameRecord()
            {
                gameID = reader.GetInt32(0),
                playerID = reader.GetInt32(1),
                moves = reader.GetString(2),
                timeUsage = reader.GetString(3),
                AIStrength = reader.GetString(4),
                isHumanWhite = reader.GetString(5),
                startDate = reader.GetString(6),
                endDate = reader.GetString(7),
                gameTitle = reader.GetString(8),
                unmakesLimit = reader.GetInt32(9),
                unmakesMade = reader.GetInt32(10),
                timeControll = reader.GetString(11),
                gameResult = reader.GetString(12)
            };

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the game record
            return gameRecord;
        }

        public GameRecord ReadRandomGameRecord()
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM GameRecords ORDER BY RANDOM() LIMIT 1;";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Checks if the record was found, if not returns null
            if (reader.IsDBNull(0))
            {
                // Disposes of the reader
                reader.Close();
                reader.Dispose();
                // Returns null since the record was not found
                return new GameRecord() { isValid = false};
            }

            // Reads all player records
            reader.Read();
            GameRecord gameRecord = new GameRecord()
            {
                gameID = reader.GetInt32(0),
                playerID = reader.GetInt32(1),
                moves = reader.GetString(2),
                timeUsage = reader.GetString(3),
                AIStrength = reader.GetString(4),
                isHumanWhite = reader.GetString(5),
                startDate = reader.GetString(6),
                endDate = reader.GetString(7),
                gameTitle = reader.GetString(8),
                unmakesLimit = reader.GetInt32(9),
                unmakesMade = reader.GetInt32(10),
                timeControll = reader.GetString(11),
                gameResult = reader.GetString(12),
                isValid = true
            };

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the game record
            return gameRecord;
        }

        public GameRecord ReadRandomGameRecord(int userID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT * FROM GameRecords WHERE Player_ID={userID} ORDER BY RANDOM() LIMIT 1;";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            // Checks if the record was found, if not returns null
            if (reader.IsDBNull(0))
            {
                // Disposes of the reader
                reader.Close();
                reader.Dispose();
                // Returns null since the record was not found
                return new GameRecord() { isValid = false };
            }

            // Reads all player records
            reader.Read();
            GameRecord gameRecord = new GameRecord()
            {
                gameID = reader.GetInt32(0),
                playerID = reader.GetInt32(1),
                moves = reader.GetString(2),
                timeUsage = reader.GetString(3),
                AIStrength = reader.GetString(4),
                isHumanWhite = reader.GetString(5),
                startDate = reader.GetString(6),
                endDate = reader.GetString(7),
                gameTitle = reader.GetString(8),
                unmakesLimit = reader.GetInt32(9),
                unmakesMade = reader.GetInt32(10),
                timeControll = reader.GetString(11),
                gameResult = reader.GetString(12),
                isValid = true
            };

            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the game record
            return gameRecord;
        }

        #endregion

        #region Other

        public bool IsGameNameTaken(string gameName, int userID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            return IsGameNameTaken(gameName, userID, "SavedGames") || IsGameNameTaken(gameName, userID, "GameRecords");
        }

        private bool IsGameNameTaken(string gameName, int userID, string tableName)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT Game_ID FROM {tableName} WHERE Player_ID='{userID}' AND GameTitle='{gameName}' LIMIT 1;";
            // Executes the command
            IDataReader reader = dbcmd.ExecuteReader();

            if (reader.IsDBNull(0))
            {
                // Disposes of the reader
                reader.Close();
                reader.Dispose();
                // The game name is not taken
                return false;
            }
            else
            {
                // Disposes of the reader
                reader.Close();
                reader.Dispose();
                // The game name is not taken
                return true;
            }
        }

        #endregion
    }
}