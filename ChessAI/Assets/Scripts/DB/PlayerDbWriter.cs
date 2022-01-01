using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Data;
using Mono.Data.Sqlite;
using System;

namespace Chess.DB
{
    public class PlayerDbWriter : PlayerDb
    {
        public void WriteToPlayers(PlayerRecord playerRecord)
        {
            // Throws an exception if the data base has not been opend
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectivly.");
            }

            // Create new text command
            dbcmd.CommandText = $"INSERT INTO 'Players'" +
                                    "('Username', 'DefaultDifficulty')" +
                                    $"VALUES('{playerRecord.username}', '{playerRecord.defaultDifficulty}');";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }

        public void DeleteFromPlayers(string username)
        {
            // Throws an exception if the data base has not been opend
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectivly.");
            }

            // Delets a player with the given username
            dbcmd.CommandText = $"DELETE FROM OpeningBook WHERE Username='{username}';";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }
    }
}