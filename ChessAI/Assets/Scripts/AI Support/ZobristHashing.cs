using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Chess.EngineUtility
{
    public static class ZobristHashing
    {
        /// Class variables
        #region Class variables

        public const int seed = 2361912; // Seed for the random number generator
        public const string randomNumberFileName = "RandomNumbers.txt"; // Assets/StreamingAssets/RandomNumbers.txt
        public static string randomNumberFilePath = Path.Combine(Application.streamingAssetsPath, randomNumberFileName); // Path to the random numbers file
        public static System.Random pseudoRandomNumberGenerator = new System.Random(seed); // Used to generate random numbers

        public static readonly ulong[,,] pieces = new ulong[6, 2, 64]; // [piece type, color, square]
        public static readonly ulong sideToMove; // Side to move
        public static readonly ulong[] castlingRights = new ulong[16]; // Castling rights random numbers
        public static readonly ulong[] enPassantTargetFile = new ulong[9]; // En-passant target square


        #endregion

        // Class utilities
        #region Class utilities

        // Generates pseudo random numbers and writes them to a .txt file
        private static void WriteRandomNumbers()
        {
            pseudoRandomNumberGenerator = new System.Random(seed); // Ensures the correct seed is used
            string randomNumbers = ""; // Stores the random numbers, later written to a .txt file
            int numbersToGenerate = (6 * 2 * 64 + 1 + castlingRights.Length + 9); // Number of random numbers that should be generated 
            while (numbersToGenerate > 0) // Generates all numbers
            {
                randomNumbers += RandomUnsigned64BitNumber();
                numbersToGenerate--;
                randomNumbers += numbersToGenerate > 0 ? "," : "";
            }
            StreamWriter streamWriter = new StreamWriter(randomNumberFilePath); // Creates a stream writer
            streamWriter.Write(randomNumbers); // Writes all the generated numbers to .txt file
            streamWriter.Close(); // Closes the stream writer
        }

        // Returns a queue of pseudo random numbers form the .txt file
        private static Queue<ulong> ReadRandomNumbers()
        {
            if (!File.Exists(randomNumberFilePath)) // If the random numbers do not exits then file is created and the numbers are generated
            {
                WriteRandomNumbers();
            }

            StreamReader streamReader = new StreamReader(randomNumberFilePath); // Creates a stream reader
            string randomNumbers = streamReader.ReadToEnd(); // Read the random number file
            streamReader.Close(); // Closes the stream reader

            Queue<ulong> randomNumbersQueue = new Queue<ulong>(); // Creates a queue
            string[] randomNumbersSplit = randomNumbers.Split(','); // Splits the random numbers string into an array
            for (int i = 0; i < randomNumbersSplit.Length; i++)
            {
                randomNumbersQueue.Enqueue(ulong.Parse(randomNumbersSplit[i])); // Converts the string to a 64 bit unsigned number
            }

            return randomNumbersQueue; // Returns the queue
        }

        // Returns a pseudo random ulong
        private static ulong RandomUnsigned64BitNumber()
        {
            byte[] buffer = new byte[8];
            pseudoRandomNumberGenerator.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        // Constructor 
        static ZobristHashing()
        {
            Queue<ulong> randomNumbers = ReadRandomNumbers(); // Gets a queue of random numbers

            // Gets pseudo random numbers for pieces
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                for (int pieceType = 0; pieceType < 6; pieceType++)
                {
                    pieces[pieceType, (int)Position.PlayerColor.White, squareIndex] = randomNumbers.Dequeue();
                    pieces[pieceType, (int)Position.PlayerColor.Black, squareIndex] = randomNumbers.Dequeue();
                }
            }

            // Gets pseudo random numbers for castling rights
            for (int i = 0; i < 16; i++)
            {
                castlingRights[i] = randomNumbers.Dequeue();
            }

            // Gets pseudo random numbers for en-passant target square
            for (int i = 0; i < enPassantTargetFile.Length; i++)
            {
                enPassantTargetFile[i] = randomNumbers.Dequeue();
            }

            sideToMove = randomNumbers.Dequeue(); // Get random number for the side to move
        }

        // Generates a zobrist key for a given position
        public static ulong GetZobristKey(Position position)
        {
            ulong key = 0ul; // Initializes they key as 0

            for (int i = 0; i < 64; i++) // Hashes each piece
            {
                if (position.squareCentric.colors[i] != (byte)SquareCentric.SquareColor.Empty) // Checks if the square is empty before hashing
                {
                    key ^= pieces[position.squareCentric.pieces[i], position.squareCentric.colors[i], i]; // xor's the key by the piece hash
                }
            }

            if (!position.sideToMove) // If black to move
            {
                key ^= sideToMove; // xor's by the side to move hash
            }

            key ^= castlingRights[position.castlingRights]; // xor's by castling rights hash
            key ^= position.enPassantTargetFile < 8 ? enPassantTargetFile[position.enPassantTargetFile] : enPassantTargetFile[8]; // xor's by en-passant target file hash  

            return key; // Returns the key
        }

        #endregion

        // Incremental zobrist key update functions
        #region Incremental update

        // Standard move
        public static void Update(ref ulong key, bool whiteMoved, byte from, byte to, byte pieceToMove)
        {
            byte playerColor = (byte)(whiteMoved ? 0 : 1);
            key ^= pieces[pieceToMove, playerColor, from]; // Removes the piece from the old square
            key ^= pieces[pieceToMove, playerColor, to]; // Adds the piece to its new square
        }

        // Capture move
        public static void Update(ref ulong key, bool whiteMoved, byte from, byte to, byte pieceToMove, byte capturePiece, bool enPassant)
        {
            //Debug.Log($"Piece To Take {capturePiece}, Current Index {from}, Destination Index {to}"); !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            byte playerColor = (byte)(whiteMoved ? 0 : 1);
            if (!enPassant)
            {
                key ^= pieces[capturePiece, (whiteMoved ? 1 : 0), to]; // Removes the captured piece
                key ^= pieces[pieceToMove, playerColor, from]; // Removes the piece from the old square 
                key ^= pieces[pieceToMove, playerColor, to]; // Adds the piece to its new square
            }
            else
            {
                key ^= pieces[capturePiece, (whiteMoved ? 1 : 0), to + (whiteMoved ? -8 : 8)]; // Removes the capture piece
                key ^= pieces[pieceToMove, playerColor, from]; // Removes the piece from the old square 
                key ^= pieces[pieceToMove, playerColor, to]; // Adds the piece to its new square
            }
        }

        // Promotion move
        public static void Update(ref ulong key, bool whiteMoved, byte from, byte to, byte pieceToMove, byte promoteTo, bool capture, byte capturePiece)
        {
            byte playerColor = (byte)(whiteMoved ? 0 : 1);
            if (!capture)
            {
                key ^= pieces[pieceToMove, playerColor, from]; // Removes the piece from the old square 
                key ^= pieces[promoteTo, playerColor, to]; // Adds the piece to its new square
            }
            else
            {
                key ^= pieces[capturePiece, (whiteMoved ? 1 : 0), to]; // Removes the captured piece
                key ^= pieces[pieceToMove, playerColor, from]; // Removes the piece from the old square 
                key ^= pieces[promoteTo, playerColor, to]; // Adds the piece to its new square
            }
        }

        // Castling move
        public static void Update(ref ulong key, bool whiteMoved, Position.CastlingDirection castlingDirection)
        {
            byte playerColor = (byte)(whiteMoved ? 0 : 1);
            ushort colorOffset = (ushort)(whiteMoved ? 0 : 56);
            ushort kingEndIndex = (ushort)((castlingDirection == Position.CastlingDirection.KingSide ? 6 : 2) + colorOffset);
            ushort rookStartIndex = (ushort)((castlingDirection == Position.CastlingDirection.KingSide ? 7 : 0) + colorOffset);
            ushort rookEndIndex = (ushort)((castlingDirection == Position.CastlingDirection.KingSide ? 5 : 3) + colorOffset);

            key ^= pieces[5, playerColor, 4 + colorOffset]; // Removes king from its old square
            key ^= pieces[5, playerColor, kingEndIndex]; // Adds king to its new square
            key ^= pieces[3, playerColor, rookStartIndex]; // Removes took from its old square
            key ^= pieces[3, playerColor, rookEndIndex]; // Adds rook to its new square 
        }

        // Side to move
        public static void UpdateSideToMove(ref ulong key)
        {
            key ^= sideToMove; // Updates side to move
        }

        // Castling rights
        public static void UpdateCastlingRights(ref ulong key, byte oldCastlingRights, byte newCastlingRights)
        {
            key ^= castlingRights[oldCastlingRights]; // Removes old castling rights
            key ^= castlingRights[newCastlingRights]; // Adds new castling rights
        }

        // En-passant target square
        public static void UpdateEnPassantTargetFile(ref ulong key, byte oldFile, byte newFile)
        {
            key ^= enPassantTargetFile[oldFile]; // Removes old file
            key ^= enPassantTargetFile[newFile]; // Adds new file
        }

        #endregion
    }
}

