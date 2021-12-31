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
        #region Class utilities

        // Returns a player record
        public PlayerRecord TryGetRecord(string name)
        {
            // Throws an exception if the data base has not been opend
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectivly.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT Player_ID, Username, DefaultDifficulty FROM Players WHERE Username='{name}' LIMIT 1;";
            // Executes teh command
            IDataReader reader = dbcmd.ExecuteReader();

            // Cheks if the record was found, if not returns null
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

        #endregion
    }
}