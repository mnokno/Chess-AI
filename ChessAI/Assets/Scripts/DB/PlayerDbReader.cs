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



        #endregion

        #region

        // Returns true if a given name is taken fro a given user
        public bool IsGameNameTaken(string gameName, int userID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            bool IsGameNameTaken(string tableName)
            {
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

            return IsGameNameTaken("SavedGames") || IsGameNameTaken("GameRecords");
        }

        #endregion
    }
}