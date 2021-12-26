using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public static class OpeningBook
    {
        #region Class variables

        // Paths to files
        private static string openingGamesDirectory = Application.streamingAssetsPath + "/OpeningGames.txt";
        private static string openingBookDirectory = Application.streamingAssetsPath + "/OpeningBook.txt";
        // Opening book as dictionary
        private static Dictionary<ulong, List<Entry>> book = new Dictionary<ulong, List<Entry>>();
        // Flag to show wheter or not the book has loaded
        public static bool hasLoaded { get; private set; }

        #endregion

        #region Class constructor

        static OpeningBook()
        {
            hasLoaded = true;
        }

        #endregion

        #region Class Utility

        // Saves the book to a text file

        public static void CalculateBook()
        {
            string[] games = System.IO.File.ReadAllText(openingGamesDirectory).Split('\n');
            foreach (string game in games)
            {
                string[] moves = game.Replace(" 1/2-1/2", "").Replace(" 0-1", "").Replace(" 1-0", "").Replace("\r", "").Split(" ");
                Position playBackPosition = new Position();
                int moveNumber = 0;
                foreach (string move in moves)
                {
                    moveNumber++;
                    if (moveNumber >= 30)
                    {
                        break;
                    }
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
                                if (entries[i].FEN != playBackPosition.GetFEN())
                                {
                                    Debug.Log("Colision");
                                    Debug.Log(entries[i].FEN + " --- " + playBackPosition.GetFEN());
                                }
                                entries[i] = new Entry(entries[i].key, entries[i].move, (ushort)(entries[i].count + 1), playBackPosition.GetFEN());
                                entryAllreadyExists = true;
                                break;
                            }
                        }
                        if (!entryAllreadyExists)
                        {
                            Entry newEntry = new Entry(playBackPosition.zobristKey, Move.ConvertPGNToUshort(move, playBackPosition), playBackPosition.GetFEN());
                            entries.Add(newEntry);
                        }
                    }
                    else
                    {
                        // Adds this move to the book and creates a list of moves for this key
                        Entry newEntry = new Entry(playBackPosition.zobristKey, Move.ConvertPGNToUshort(move, playBackPosition), playBackPosition.GetFEN());
                        book[playBackPosition.zobristKey] = new List<Entry>() { newEntry };
                    }
                    playBackPosition.MakeMove(Move.ConvertPGNToUshort(move, playBackPosition));
                }
            }
            Debug.Log("Finished");
        }

        public static void BookToFile()
        {
            System.IO.File.WriteAllTextAsync(openingBookDirectory, GetStringBook());
        }

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
            return formatedBook.TrimEnd();
        }

        public static void LoadBookFromFile(bool onMainThread)
        {
            if (!onMainThread)
            {
                System.Threading.Tasks.Task.Run(() => LoadBookFromFile());
            }
            else
            {
                LoadBookFromFile();
            }
        }

        private static void LoadBookFromFile()
        {
            // Reads the opening book from a text file and splits it into key - moves formate array
            string[] openingBook = System.IO.File.ReadAllText(openingBookDirectory).Split('\n');

            // Converts the openingBook arry to a book dictionary
            foreach (string line in openingBook)
            {
                string[] parts = line.Split(" ");
                List<Entry> enties = new List<Entry>();
                for (int i = 2; i < parts.Length; i++)
                {
                    enties.Add(new Entry(ulong.Parse(parts[0]), ushort.Parse(parts[i].Split("-")[0]), ushort.Parse(parts[i].Split("-")[1]), ""));
                }
                book.Add(ulong.Parse(parts[0]), enties);
            }

            // Sets off teh hasLoaded falg
            hasLoaded = true;
        }

        // Resturs a random move from the opening book, and 0 if no move was found
        public static ushort GetMove(ulong positionKey)
        {
            // Chesk if the opening book has been loaded
            if (hasLoaded)
            {
                book.TryGetValue(positionKey, out List<Entry> entries);
                if (entries != null)
                {
                    return entries[Random.Range(0, entries.Count - 1)].move;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region Structures

        public struct Entry
        {
            public ulong key;
            public ushort move;
            public ushort count;
            public string FEN;

            public Entry(ulong key, ushort move, string FEN)
            {
                this.key = key;
                this.move = move;
                this.count = 1;
                this.FEN = FEN;
            }

            public Entry(ulong key, ushort move, ushort count, string FEN)
            {
                this.key = key;
                this.move = move;
                this.count = count;
                this.FEN = FEN;
            }
        }

        #endregion
    }
}
