using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;

namespace Chess.DB
{
    public class OpeningDb
    {
        #region Class variables

        protected static readonly string conn = "URI=file:" + Application.streamingAssetsPath + "/Opening/OpeningDB.db"; // Path to the opening database
        protected IDbConnection dbconn; // Connection to the data base
        protected IDbCommand dbcmd; // Command interface for the data base
        protected bool isOpen; // true if teh connection to the data base is open
        protected static bool isOpenGlobal = false; // true if the data base is open by some instance of openingDB

        #endregion

        #region Class constructor

        public OpeningDb()
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

        #region Structure

        public struct Record
        {
            public bool isValid;
            public string moves;
            public string counts;

            public Record(string moves, string counts, bool isValid)
            {
                this.moves = moves;
                this.counts = counts;
                this.isValid = isValid;
            }
        }

        #endregion
    }
}