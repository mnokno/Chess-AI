using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Chess.EngineUtility;
using Chess.Engine;

namespace Chess.UI
{
    public class Board : MonoBehaviour
    {
        // Class variables
        #region Variables

        [HideInInspector]
        public static readonly char[] FENPieceType = new char[6] { 'p', 'n', 'b', 'r', 'q', 'k' }; // Used to convert FEN string to a number
        public string fenString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; //Loads the starting position by default
        public FEN fen; // Reference to the fen instance
        public bool whiteBottom; // Used to decide where to generate white pieces and how to annotate the board
        public bool hvh; // Human versus human
        public bool whiteHumman; // True if human play white
        public bool useClock; // Used to deiced whether or not this board will use clock to time move (introduces loses by time)
        public bool autoPremutateBoard; // Used to deiced whether or not to automatically orientate this board
        [HideInInspector]
        public bool whiteToMove; // True if white is to move

        public BoardTheme.SquareTheme squareTheme; // Holds color theme of the chess board squares
        public BoardTheme.LegalMoveTheme legalMoveTheme; // Holds sprites and colors used to show legal moves
        public BoardTheme.AnnotationTheme annotationTheme; // Holds color and font used to annotate the chess board
        public Transform squareParentContainer; // Target parent transform for all squares 
        public Transform boardAnnotationCanvasTransform; // Canvas for the board annotation
        [HideInInspector]
        public string[] fileAnnotations = new string[8] { "a", "b", "c", "d", "e", "f", "g", "h" }; // Used to convert file index to a file annotation
        [HideInInspector]
        public string[] rankAnnotations = new string[8] { "1", "2", "3", "4", "5", "6", "7", "8" }; // Used to convert rank index to a rank annotation

        public Transform pieceContainer; // Target parent for chess pieces
        public SpriteAtlas pieceAtlas; // Sprite atlas containing all piece sprites
        public float pieceScale; // Used to scale the piece assets style_1-0.0375f style_2-0.0417f

        public Square[] squares = new Square[64]; // Stores reference to all squares
        public Piece[] pieces = new Piece[64]; // Stores reference to all pieces

        public ChessEngineManager engineManager; // Reference to the chess engine used by this board
        public BoardInputManager inputManager; // Reference to the board input manager

        #endregion

        // Responsible for board initialization
        #region Initialization

        void Awake()
        {
            Common.ChessGameDataManager chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
            // Updates clock settings
            if (useClock)
            {
                if (chessGameDataManager != null)
                {
                    if (chessGameDataManager.chessGameData.newGame)
                    {
                        // Initiates clock
                        engineManager.chessEngine.centralPosition.InitClock(chessGameDataManager.chessGameData.initialTime, chessGameDataManager.chessGameData.timeIncrement);
                    }
                    else
                    {
                        // Initiates clock
                        engineManager.chessEngine.centralPosition.InitClock(chessGameDataManager.chessGameData.initialTime, chessGameDataManager.chessGameData.timeIncrement);
                        engineManager.chessEngine.centralPosition.clock.whitesTurn = chessGameDataManager.chessGameData.isHumanWhite;
                    }
                }
                else
                {
                    engineManager.chessEngine.centralPosition.InitClock(1, 180);
                }
            }

            // Updates board setting
            if (chessGameDataManager != null && autoPremutateBoard)
            {
                if (chessGameDataManager.chessGameData.isHumanWhite)
                {
                    whiteBottom = true;
                    whiteHumman = true;
                }
                else
                {
                    whiteBottom = false;
                    whiteHumman = false;
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            // Disables/hides the board preview area
            GetComponentInParent<SpriteRenderer>().enabled = false;
            // Generates the chess board
            GenerateBoard();
            // Check what to do
            Common.ChessGameDataManager chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
            if (chessGameDataManager != null && !chessGameDataManager.chessGameData.newGame && chessGameDataManager.chessGameData.loadGame)
            {
                LoadPosition(chessGameDataManager);
            }
            else
            {
                // Converts FEN string to FEN instance
                fen = new FEN(fenString);
                // Sets the player to move
                whiteToMove = fen.GetSideToMove();
                // Loads the chess pieces
                LoadFEN(fen.GetPiecePlacment(), false);
                // Loads position for the input manager
                inputManager.LoadFEN(fen);
                // Loads position for the engine manager
                engineManager.chessEngine.LoadFEN(fen);
            }

            // Checks if the game begins with an AI move
            if (!hvh && whiteHumman != whiteToMove)
            {
                // If so an AI move is played
                engineManager.MakeAIMove();
            }
        }

        // Loads saved position
        private void LoadPosition(Common.ChessGameDataManager chessGameDataManager)
        {
            // Finds gameUI
            GameUI gameUI = FindObjectOfType<GameUI>();
            string[] timeUsage = chessGameDataManager.chessGameData.timeUsage.Split(":");
            // Calculate the position FEN
            string[] moves = chessGameDataManager.chessGameData.moves.Split(":");
            if (moves.Length != 0)
            {
                for (int i = moves.Length - 1; i >= 0; i--)
                {
                    if (moves[i] != "")
                    {
                        ushort move = ushort.Parse(moves[i]);
                        if (gameUI != null)
                        {
                            int turnNumer = (int)Math.Ceiling(engineManager.chessEngine.centralPosition.historicMoveData.Count / 2d);
                            gameUI.LogMove(turnNumer, Move.ConvertUshortToPNG(move, engineManager.chessEngine.centralPosition), int.Parse(timeUsage[timeUsage.Length - i - 1]));
                        }

                        engineManager.MakeMove(move);
                    }
                }
            }

            // Calculates time
            float timeIncrement = chessGameDataManager.chessGameData.timeIncrement * 1000;
            bool whitesTurn = true;           
            if (moves.Length != 0)
            {
                foreach (string timeUse in timeUsage)
                {
                    if (timeUse != "")
                    {
                        engineManager.chessEngine.centralPosition.timeTakenPerMove.Add(int.Parse(timeUse));
                        engineManager.chessEngine.centralPosition.clock.AddTime(whitesTurn, -int.Parse(timeUse) + timeIncrement);
                        whitesTurn = !whitesTurn;
                    }
                }
            }

            // Converts FEN string to FEN instance
            fen = new FEN(engineManager.chessEngine.centralPosition.GetFEN());
            // Sets the player to move
            whiteToMove = fen.GetSideToMove();
            // Loads the chess pieces
            LoadFEN(fen.GetPiecePlacment(), true);
            // Loads position for the input manager
            inputManager.LoadFEN(fen);
        }

        // Generates board interface
        public void GenerateBoard()
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Square square = new Square(this, file, rank, rank * 8 + file);
                    squares[rank * 8 + file] = square;
                }
            }
        }

        // Destroys the chess board
        public void DestroyBoard()
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    DestroyImmediate(squares[rank * 8 + file].quad);
                    DestroyImmediate(squares[rank * 8 + file].legalCaptureSprite);
                    DestroyImmediate(squares[rank * 8 + file].legalNonCaptureSprite);
                    foreach(GameObject go in squares[rank * 8 + file].annotations)
                    {
                        DestroyImmediate(go);
                    }
                }
            }
        }

        // Removes all existing pieces and loads new FEN
        public void LoadFEN(string FEN, bool reGenBoard)
        {
            // Stops potential animations
            StopAllCoroutines();

            // Regenerates the chess board
            if (reGenBoard) 
            {
                DestroyBoard();
                GenerateBoard();
            }

            // Splits the FEN into ranks rank 8 ... 1
            string[] ranks = FEN.Split('/');

            // Deletes all old pieces
            for(int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i] != null)
                {
                    pieces[i].Destroy(animate: false);
                    pieces[i] = null;
                }
            }

            // Loads each rank
            int currentIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < ranks[7 - i].Length; j++)
                {
                    if (char.IsNumber(ranks[7 - i][j]))
                    {
                        currentIndex += int.Parse(ranks[7 - i][j].ToString());
                    }
                    else
                    {
                        pieces[currentIndex] = new Piece(
                            this,
                            new Piece.PieceType() {
                                color = char.IsUpper(ranks[7 - i][j]) ? Piece.Color.White : Piece.Color.Black,
                                type = (Piece.Type)Array.IndexOf(FENPieceType, char.ToLower(ranks[7 - i][j]))
                                },
                            currentIndex % 8,
                            (currentIndex - (currentIndex % 8)) / 8
                            );
                        currentIndex++;
                    }
                }
            }
        }

        // Updates position of the pieces
        public void MakeMove(ushort move, bool animate=true)
        {
            // Converts the square index to file and rank
            int from = Move.GetFrom(move);
            int to = Move.GetTo(move);
            int toFile = to % 8;
            int toRank = (to - toFile) / 8;
            int flag = Move.GetFlag(move);

            if (flag == 0 | flag == 1) // quite move and double pawn push
            {
                // Moves the piece
                pieces[from].Move(toFile, toRank, animate:animate);
                pieces[to] = pieces[from];
                pieces[from] = null;
            }
            else if (flag == 2 | flag == 3) // King or queen side castling
            {
                // Calculates offsets
                ushort colorOffsetIndex = (ushort)(whiteToMove ? 0 : 56);
                ushort colorOffsetFileRank = (ushort)(whiteToMove ? 0 : 7);
                ushort kingEndOffest = (ushort)(flag == 2 ? 6 : 2);
                ushort rookStartOffest = (ushort)(flag == 2 ? 7 : 0);
                ushort rookEndOffest = (ushort)(flag == 2 ? 5 : 3);

                // Move the king
                pieces[4 + colorOffsetIndex].Move(kingEndOffest, colorOffsetFileRank, animate: animate);
                pieces[kingEndOffest + colorOffsetIndex] = pieces[4 + colorOffsetIndex];
                pieces[4 + colorOffsetIndex] = null;

                // Move the rook
                pieces[rookStartOffest + colorOffsetIndex].Move(rookEndOffest, colorOffsetFileRank, animate: animate);
                pieces[rookEndOffest + colorOffsetIndex] = pieces[rookStartOffest + colorOffsetIndex];
                pieces[rookStartOffest + colorOffsetIndex] = null;
            }
            else if (flag == 4) // Capture move
            {
                // Destroys the captured piece
                pieces[to].Destroy(animate: animate);

                // Moves the piece
                pieces[from].Move(toFile, toRank, animate: animate);
                pieces[to] = pieces[from];
                pieces[from] = null;
            }
            else if (flag == 5) // En-passant
            {
                // Calculates the index of the capture pawn
                int capturedIndex = to + (whiteToMove ? -8 : 8);

                // Destroys the captured pawn
                pieces[capturedIndex].Destroy(animate: animate);
                pieces[capturedIndex] = null;

                // Moves the pawn
                pieces[from].Move(toFile, toRank, animate: animate);
                pieces[to] = pieces[from];
                pieces[from] = null;
            }
            else if (flag >= 8 && flag <= 15) // Promotion move
            {
                if (flag >= 12) // Promotion with capture
                {
                    // Destroys the captured piece
                    pieces[to].Destroy(animate: animate);

                    // Moves and promotes the pawn
                    pieces[from].Move(toFile, toRank, animate: animate);
                    pieces[from].Promote(new Piece.PieceType() { type = (Piece.Type)(flag - 11), color = pieces[from].pieceType.color }, animate: animate);
                    pieces[to] = pieces[from];
                    pieces[from] = null;
                }
                else // Promotion with out capture
                {
                    // Moves and promotes the pawn
                    pieces[from].Move(toFile, toRank, animate: animate);
                    pieces[from].Promote(new Piece.PieceType() { type = (Piece.Type)(flag - 7), color = pieces[from].pieceType.color}, animate: animate);
                    pieces[to] = pieces[from];                
                    pieces[from] = null;
                }
            }

            // Passes the turns
            whiteToMove = !whiteToMove;
        }

        // Takes back two move (one turn)
        public void TakeBack()
        {
            inputManager.Deselect();
            engineManager.chessEngine.centralPosition.TakeBack();
            string FEN = engineManager.chessEngine.centralPosition.GetFEN();
            LoadFEN((new FEN(FEN)).GetPiecePlacment(), false);
            inputManager.LoadFEN(new FEN(FEN));
        }

        #endregion

        // Support functions for piece and square class
        #region Support functions

        // Exposes destroy function
        public void DestoryGO(GameObject toDestroy)
        {
            Destroy(toDestroy);
        }

        #endregion
    }
}
