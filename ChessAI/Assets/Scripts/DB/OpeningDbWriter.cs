using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;

namespace Chess.DB
{
    public class OpeningDbWriter : OpeningDbReader
    {
        #region Class utilities

        public void AddNewMove(ulong key, ushort move)
        {
            // Throws an exception if the data base has not been opend
            if (!this.isOpen)
            {
                throw new Exception("The opening data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectivly.");
            }

            // Cheks if the at least one move for this key exists
            Record record = TryGetRecord(key);
            if (record.isValid) // The current record is updated
            {
                // Chekcs if the move allready exists
                if (record.moves.Contains(move.ToString())) // Count needs to be updated
                {
                    // Delets the old record
                    dbcmd.CommandText = $"DELETE FROM OpeningBook WHERE Key={key};";
                    dbcmd.ExecuteNonQuery();
                    // Creates a new updated version
                    //WriteNewRecord(key, move.ToString(), 1.ToString());
                }
                else
                {
                    // Delets the old record
                    dbcmd.CommandText = $"DELETE FROM OpeningBook WHERE Key={key};";
                    dbcmd.ExecuteNonQuery();
                    // Creates a new updated version
                    WriteNewRecord(key, record.moves + " " + move.ToString(), record.moves + " 1");
                }
            }
            else // New record is created
            {
                WriteNewRecord(key, move.ToString(), 1.ToString());
            }
        }

        public void WriteNewRecord(ulong key, string moves, string counts)
        {
            // Throws an exception if the data base has not been opend
            if (!this.isOpen)
            {
                throw new Exception("The opening data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectivly.");
            }

            // Create new text command
            dbcmd.CommandText = $"INSERT INTO 'OpeningBook'" +
                                    "('Key', 'Moves', 'Counts')" +
                                    $"VALUES('{key}', '{moves}', '{counts}');";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }

        public void ClearDB()
        {
            // Throws an exception if the data base has not been opend
            if (!this.isOpen)
            {
                throw new Exception("The opening data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectivly.");
            }

            // DDelets all record from the data base
            dbcmd.CommandText = $"DELETE FROM OpeningBook;";
            dbcmd.ExecuteNonQuery();
        }

        #endregion
    }
}
