using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public static class OpeningBook
    {
        #region Class variables

        // Paths to files
        private static string openingGamesDirectory = Application.streamingAssetsPath + "/Opening/OpeningGames.csv";
        private static string openingBookDirectory = Application.streamingAssetsPath + "/Opening/OpeningBook.csv";
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

        // Calculate the opening book from games
        public static void CalculateBook(int gameLimit)
        {
            string[] games = System.IO.File.ReadAllLines(openingGamesDirectory);
            int gameCount = 0;
            Position playBackPosition;
            string[] moves;
            foreach (string game in games)
            {
                gameCount++;
                if (gameCount > gameLimit)
                {
                    break;
                }
                moves = game.Split(" ");
                playBackPosition = new Position();
                int moveNumber = 0;
                for (int i = 0; i < moves.Length - 1; i++)
                {
                    moveNumber++;
                    if (moveNumber > 20)
                    {
                        break;
                    }
                    if (book.ContainsKey(playBackPosition.zobristKey))
                    {
                        // Gets list of all entries attached to this key
                        List<Entry> entries = book[playBackPosition.zobristKey];
                        ushort currentlyEvaluatedMove = Move.ConvertPGNToUshort(moves[i], playBackPosition);

                        // Cheks if the currently evaluated move allready exists in the move list
                        bool entryAllreadyExists = false;
                        for (int j = 0; j < entries.Count; j++)
                        {
                            if (entries[j].move == currentlyEvaluatedMove)
                            {
                                entries[j] = new Entry(entries[j].key, entries[j].move, (ushort)(entries[j].count + 1));
                                entryAllreadyExists = true;
                                break;
                            }
                        }
                        if (!entryAllreadyExists)
                        {
                            Entry newEntry = new Entry(playBackPosition.zobristKey, Move.ConvertPGNToUshort(moves[i], playBackPosition));
                            entries.Add(newEntry);
                        }
                    }
                    else
                    {
                        // Adds this move to the book and creates a list of moves for this key
                        Entry newEntry = new Entry(playBackPosition.zobristKey, Move.ConvertPGNToUshort(moves[i], playBackPosition));
                        book[playBackPosition.zobristKey] = new List<Entry>() { newEntry };
                    }
                    playBackPosition.MakeMove(Move.ConvertPGNToUshort(moves[i], playBackPosition));
                }
            }
            // Logs a massage
            Debug.Log($"Finished claculatig opening book, total games converted: {gameCount - 1}");
        }

        // Saves the book to a data base
        public static void BookToCSV()
        {
            // Creates writers
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(openingBookDirectory);
            // Clears the destination file
            streamWriter.Write("");
            // Writes csv headers
            streamWriter.WriteLine("Key|Moves|Counts");

            // Foreach position
            foreach (ulong key in book.Keys)
            {
                string moves = "";
                string counts = "";

                foreach (Entry entry in book[key])
                {
                    moves += entry.move + " ";
                    counts += entry.count + " ";
                }
                streamWriter.WriteLine(key + "|" + moves.TrimEnd() + "|" + counts.TrimEnd());
            }

            // Disposes of writers
            streamWriter.Close();
            streamWriter.Dispose();
            Debug.Log("The opening book has been saved to csv file");
        }

        // Loads the opening book
        public static void LoadBookFromCSV(bool onMainThread)
        {
            if (!onMainThread)
            {
                System.Threading.Tasks.Task.Run(() => LoadBookFromCSV());
            }
            else
            {
                LoadBookFromCSV();
            }
        }

        // Loads the opening book from file
        private static void LoadBookFromCSV()
        {
            // Reads the opening book from a text file and splits it into key - moves formate array
            string[] openingBook = System.IO.File.ReadAllLines(openingBookDirectory);
            openingBook[0] = "";

            // Converts the openingBook arry to a book dictionary
            foreach (string line in openingBook)
            {
                if (line != "")
                {
                    string[] parts = line.Split("|");
                    string[] moves = parts[1].Split(" ");
                    string[] counts = parts[2].Split(" ");
                    List<Entry> enties = new List<Entry>();
                    for (int i = 0; i < moves.Length; i++)
                    {
                        enties.Add(new Entry(ulong.Parse(parts[0]), ushort.Parse(moves[i]), ushort.Parse(counts[i])));
                    }
                    book.Add(ulong.Parse(parts[0]), enties);
                }
            }

            // Sets off teh hasLoaded falg
            hasLoaded = true;
            // Logs a massage
            Debug.Log("The opening book has loaded");
        }

        // Resturs a random move from the opening book, and 0 if no move was found
        public static ushort GetMove(Position position)
        {
            // Chesk if the opening book has been loaded
            if (hasLoaded)
            {
                book.TryGetValue(position.zobristKey, out List<Entry> entries);
                if (entries != null)
                {
                    // Gets a move form the opening data base
                    ushort move = entries[Random.Range(0, entries.Count - 1)].move;

                    // Check if the move if valid
                    MoveGenerator moveGenerator = new MoveGenerator();
                    ushort[] moves = moveGenerator.GenerateLegalMoves(position, (byte)(position.sideToMove ? 0 : 1)).ToArray();
                    foreach (ushort legalMove in moves)
                    {
                        if (legalMove == move)
                        {
                            return move;
                        }
                    }
                    // The move was not valid
                    return 0;
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

        #region Formating functions

        public static void FormatLiChessEliteGames(string from, string to)
        {
            // Creates stream readers and writers
            System.IO.StreamReader streamReader = new System.IO.StreamReader(Application.streamingAssetsPath + "/Opening/LichessElite/" + from);
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(Application.streamingAssetsPath + "/Opening/" + to);
            // Clears the destination file
            streamWriter.Write("");

            // Removes annoataions
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                if (line != "" && line[0] != '[')
                {
                    string game = line;
                    while (line[line.Length - 1] != '*' && line[line.Length - 2] != '-' && line[line.Length - 4] != '-' || line[line.Length - 1] == 'O')
                    {
                        line = streamReader.ReadLine();
                        game += " " + line;
                    }

                    string[] parts = game.Split(" ");
                    // Only conciders game thta have more then 30 move player by each side
                    if (parts.Length > 120)
                    {
                        game = "";
                        foreach (string part in parts)
                        {
                            if (part[part.Length - 1] != '.')
                            {
                                game += part + " ";
                            }
                        }
                        streamWriter.WriteLine(game.TrimEnd());
                    }
                }
            }

            // Disposes of writers and readers
            streamReader.Close();
            streamReader.Dispose();
            streamWriter.Close();
            streamWriter.Dispose();
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

            public Entry(ulong key, ushort move, ushort count)
            {
                this.key = key;
                this.move = move;
                this.count = count;
            }
        }

        #endregion
    }
}
