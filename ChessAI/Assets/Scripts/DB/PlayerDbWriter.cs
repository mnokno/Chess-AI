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
        #region Players table management

        public void WriteToPlayers(PlayerRecord playerRecord)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
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
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Deletes a player with the given username
            dbcmd.CommandText = $"DELETE FROM Players WHERE Username='{username}';";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }

        public void UpdatePlayers(string username, string newUsername, string newDifficulty)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Deletes a player with the given username
            dbcmd.CommandText = $"UPDATE Players SET Username='{newUsername}', DefaultDifficulty='{newDifficulty}' WHERE Username='{username}';";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }

        #endregion

        #region Saved game table management

        public void WriteToSavedGames(SavedGameRecord savedGameRecord)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create new text command
            dbcmd.CommandText = $"INSERT INTO 'SavedGames'" +
                                    "('Player_ID', 'Moves', 'TimeUsage', 'AIStrength', 'IsHumanWhite', 'StartDate', 'GameTitle', 'UnmakesLimit', 'UnmakesMade', 'TimeControll')" +
                                    $"VALUES('{savedGameRecord.playerID}', " +
                                    $"'{savedGameRecord.moves}', " +
                                    $"'{savedGameRecord.timeUsage}', " +
                                    $"'{savedGameRecord.AIStrength}', " +
                                    $"'{savedGameRecord.isHumanWhite}', " +
                                    $"'{savedGameRecord.startDate}', " +
                                    $"'{savedGameRecord.gameTitle}', " +
                                    $"'{savedGameRecord.unmakesLimit}', " +
                                    $"'{savedGameRecord.unmakesMade}', " +
                                    $"'{savedGameRecord.timeControll}');";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }

        public void UpdateSavedGames(SavedGameRecord savedGameRecord, string gameName, int userID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Create new text command
            dbcmd.CommandText = $"UPDATE SavedGames SET" +
                                    "('Player_ID', 'Moves', 'TimeUsage', 'AIStrength', 'IsHumanWhite', 'StartDate', 'GameTitle', 'UnmakesLimit', 'UnmakesMade', 'TimeControll')" +
                                    $"VALUES('{savedGameRecord.playerID}', " +
                                    $"'{savedGameRecord.moves}', " +
                                    $"'{savedGameRecord.timeUsage}', " +
                                    $"'{savedGameRecord.AIStrength}', " +
                                    $"'{savedGameRecord.isHumanWhite}', " +
                                    $"'{savedGameRecord.startDate}', " +
                                    $"'{savedGameRecord.gameTitle}', " +
                                    $"'{savedGameRecord.unmakesLimit}', " +
                                    $"'{savedGameRecord.unmakesMade}', " +
                                    $"'{savedGameRecord.timeControll}')" +
                                    $"WHERE Player_ID='{userID}' AND GameTitle='{gameName}';";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }

        public void DeleteFromSavedGames(int gameID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Deletes a game with the given gameID
            dbcmd.CommandText = $"DELETE FROM SavedGame WHERE Game_ID='{gameID}';";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }

        public void DeleteFromSavedGames(string gameName, int userID)
        {
            // Throws an exception if the data base has not been opened
            if (!this.isOpen)
            {
                throw new Exception("The players data base has to be opened before you can perform a read operation!\nYou can open and close connection to the data base by calling OpenDB and CloseDb respectively.");
            }

            // Deletes a game with the given gameID
            dbcmd.CommandText = $"DELETE FROM SavedGame WHERE Player_ID='{userID}' AND GameTitle='{gameName}';";
            // Executes the command
            dbcmd.ExecuteNonQuery();
        }

        #endregion
    }
}