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
            int gameIndex = 1; // Debug
            foreach (string game in games)
            {
                Debug.Log($"Game Index: {gameIndex} ------------------------------------------------------------------------------------------------------------------");
                string[] moves = game.Replace(" 1/2-1/2", "").Replace(" 0-1", "").Replace(" 1-0", "").Replace("\r", "").Split(" ");
                Position playBackPosition = new Position();
                foreach (string move in moves)
                {
                    playBackPosition.MakeMove(Move.ConvertPGNToUshort(move.Replace(" ", ""), playBackPosition));
                }
                gameIndex ++; // Debug
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
