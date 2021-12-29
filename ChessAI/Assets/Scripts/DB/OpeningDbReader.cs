using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;

namespace Chess.DB
{
    public class OpeningDbReader : OpeningDb
    {
        #region Class utilities

        // Returns move data for a given key, returns null if not recored corresponding to that key has found
        public Record TryGetRecord(ulong key)
        {
            // Throws an exception if the data base has not been opend
            if (!this.isOpen)
            {
                throw new Exception("The opening data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectivly.");
            }

            // Create a command to check if the record exists in the data base
            dbcmd.CommandText = $"SELECT Moves, Counts FROM OpeningBook WHERE Key='{key}' LIMIT 1;";
            // Executes teh command
            IDataReader reader = dbcmd.ExecuteReader();

            // Cheks if the record was found, if not returns null
            if (reader.IsDBNull(0))
            {
                // Disposes of the reader
                reader.Close();
                reader.Dispose();
                // Returns null since the record was not found
                return new Record("", "", false);
            }

            // Gets the record
            reader.Read();
            string moves = reader.GetString(0);
            string counts = reader.GetString(1);
            // Disposes of the reader
            reader.Close();
            reader.Dispose();

            // Returns the record
            return new Record(moves, counts, true);
        }

        #endregion
    }
}
