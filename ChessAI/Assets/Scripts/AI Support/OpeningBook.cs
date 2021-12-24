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
        private static Dictionary<ulong, List<Entry>> book = new Dictionary<ulong, List<Entry>>();
        // Array containing all the games from the opening book

        #endregion

        #region Class Constructor

        static OpeningBook()
        {
            string[] games = System.IO.File.ReadAllText(openingBookDirectory).Split('\n');
            foreach (string game in games)
            {
                string[] moves = game.Replace(" 1/2-1/2", "").Replace(" 0-1", "").Replace(" 1-0", "").Replace("\r", "").Split(" ");
                Position playBackPosition = new Position();
                foreach (string move in moves)
                {
                    playBackPosition.MakeMove(Move.ConvertPGNToUshort(move.Replace(" ", ""), playBackPosition));
                    if (book.ContainsKey(playBackPosition.zobristKey))
                    {
                        // Gets list of all entries attached to this key
                        List<Entry> entries = book[playBackPosition.zobristKey];
                        ushort currentlyEvaluatedMove = Move.ConvertPGNToUshort(move, playBackPosition);

                        // Cheks if the currently evaluated move allready exists in the move list
                        bool entryAllreadyExists = false;
                        for (int i = 0; i < entries.Count; i++)
                        {
                            if (entries[i].move == currentlyEvaluatedMove)
                            {
                                entries[i].IncreaseCount();
                                entryAllreadyExists = true;
                                break;
                            }
                        }
                        if (!entryAllreadyExists)
                        {
                            Entry newEntry = new Entry(playBackPosition.zobristKey, Move.ConvertPGNToUshort(move, playBackPosition));
                            entries.Add(newEntry);
                        }
                    }
                    else
                    {
                        // Adds this move to the book and creates a list of moves for this key
                        Entry newEntry = new Entry(playBackPosition.zobristKey, Move.ConvertPGNToUshort(move, playBackPosition));
                        book[playBackPosition.zobristKey] = new List<Entry> { newEntry };
                    }
                }
            }
        }

        public static void RunMe()
        {
            return;
        }

        #endregion

        #region Class Utility

        public static string GetStringBook()
        {
            // Converts the book from a dictiorary to a formated string
            string formatedBook = "";
            
            // Foreach position
            foreach(ulong key in book.Keys)
            {
                string bookLine = key.ToString() + " :";
                foreach(Entry entry in book[key])
                {
                    bookLine += " " + entry.move + "-" + entry.count;
                }
                formatedBook += bookLine + "\n";
            }

            // Returns the formated book
            return formatedBook;
        }
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

            public void IncreaseCount()
            {
                count++;
            }
        }

        #endregion
    }
}
