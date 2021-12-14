using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public static class OpeningBook
    {
        #region Class variables

        // Directory of the opening book
        private static string openingBookDirectory = Application.streamingAssetsPath + "/OpeningBook.txt";
        private static Dictionary<ulong, Entry> book = new Dictionary<ulong, Entry>();
        // Array containing all the games from the opening book

        #endregion

        #region Class Constructor

        static OpeningBook()
        {
            string[] games = System.IO.File.ReadAllText(openingBookDirectory).Split('\n');
            foreach (string game in games)
            {
                Debug.Log(game);
            }
        }

        public static void RunMe()
        {
            return;
        }

        #endregion

        #region Class Utility

        #endregion

        #region Structures

        public struct Entry
        {
            public ulong key;
            public ushort move;
            public ushort count;

            public Entry(ulong key, ushort move)
            {
                this.key = key;
                this.move = move;
                this.count = 1;
            }
        }

        #endregion
    }
}
