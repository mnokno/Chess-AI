using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Engine;
using Chess.EngineUtility;

namespace Chess.UI
{
    public class BoardInputManager : MonoBehaviour
    {
        // Class variables
        #region Class variables

        public Board parrentBoard; // Reference to parent chess board

        private float globalSquareLength; // Length of a sing square in the global scale 
        private MousePieceMovementState movementState; // Current piece movement type

        private Piece selectedPiece; // Stores reference to the currently selected piece 
        private Square highlightedSquare; // Stores reference to the current highlighted square
        private Square[] lastMoveSquares = new Square[2]; // Stores reference to the initial and destination square from the last move
        private List<Square> showingLegalMove = new List<Square>(); // Used to hide legal moves
        private bool primaryDrop; // If a piece is clicked twice its deselected

        public Position localPosition; // Stores position represented by this board
        public MoveGenerator moveGenerator; // Used to show and validates moves

        #endregion

        // Responsible for board initialization
        #region Initialization

        // Awake is called before Start
        void Awake()
        {
            InitVariables(); // Calculates values of class variables
        }

        // Calculate private variables
        private void InitVariables()
        {
            globalSquareLength = parrentBoard.transform.localScale.x / 8f;
            movementState = MousePieceMovementState.None;
            highlightedSquare = null;
            selectedPiece = null;
            localPosition = new Position();
            moveGenerator = new MoveGenerator();
        }

        #endregion

        // Runtime functions (update)
        #region Runtime

        // Update is called once per frame
        void Update()
        {
            // Exits full screen move
            if (Input.GetKeyDown(KeyCode.F)) 
            {
                Screen.fullScreen = !Screen.fullScreen;
            }

            // Snaps the piece to the mouse location if in dragging mode
            if (movementState == MousePieceMovementState.Dragging)
            {
                Drag();
            }

            if (Input.GetMouseButtonDown(0))
            {
                
                if (movementState == MousePieceMovementState.None)
                {
                    // Clicked the square if no piece is currently selected
                    Click();
                }
                else if (movementState == MousePieceMovementState.Selected)
                {              
                    if (GetIndexUnderMouse() == (selectedPiece.rank * 8 + selectedPiece.file)) // Clicked on the current location
                    {
                        movementState = MousePieceMovementState.Dragging;
                    }
                    else if (IsLegal(selectedPiece.rank * 8 + selectedPiece.file, GetIndexUnderMouse()))
                    {
                        // Tries to play the move
                        TryToMakeMove(selectedPiece.rank * 8 + selectedPiece.file, GetIndexUnderMouse());
                    }
                    else if (GetIndexUnderMouse() >= 0 && GetIndexUnderMouse() < 64)
                    {
                        if (parrentBoard.pieces[GetIndexUnderMouse()] != null)
                        {
                            Deselect();
                            Click();
                        }
                    }
                 
                }
            }
            else if (movementState == MousePieceMovementState.Dragging && Input.GetMouseButtonUp(0)) // If in dragging move and the mouse button was lifted the piece is dropped
            {
                Drop();
            }
        }

        #endregion

        // Movement types: drag, drop and click
        #region Movement types

        private void Drag()
        {
            // Gets the local mouse position
            float targetX = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - parrentBoard.transform.position).x;
            float targetY = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - parrentBoard.transform.position).y;

            // Does not allow to drag the pieces outside the chess board  < coordinates.x |  |  < coordinates.y | coordinates.y < (-4 * globalSquareLength))
            targetX = targetX > (4 * globalSquareLength) ? (4 * globalSquareLength) : targetX;
            targetX = targetX < (-4 * globalSquareLength) ? (-4 * globalSquareLength) : targetX;
            targetY = targetY > (4 * globalSquareLength) ? (4 * globalSquareLength) : targetY;
            targetY = targetY < (-4 * globalSquareLength) ? (-4 * globalSquareLength) : targetY;

            // Snaps the piece to he mouse position
            selectedPiece.piece.transform.localPosition = new Vector2(targetX / parrentBoard.transform.localScale.x, targetY / parrentBoard.transform.localScale.y);
        }

        private void Drop()
        {
            // Changes to movement mode from dragging to clicking since the piece was dropped
            movementState = MousePieceMovementState.Selected;

            // If a piece is clicked twice its deselected
            if (GetIndexUnderMouse() == (selectedPiece.rank * 8 + selectedPiece.file))
            {
                if (!primaryDrop)
                {
                    Deselect();
                    primaryDrop = false;
                    return;
                }
            }
            primaryDrop = false;

            // Tries to play the move
            TryToMakeMove(selectedPiece.rank * 8 + selectedPiece.file, GetIndexUnderMouse(), animate:false); 
        }

        private void Click()
        {
            if (movementState == MousePieceMovementState.None)
            {
                // Dehighlight currently selected square 
                if (highlightedSquare != null)
                {
                    highlightedSquare.Highlighted(false); 
                }

                // Gets index under mouse
                int squareIndex = GetIndexUnderMouse(); 

                // Checks if the index is inside the bounds of the chess board
                if (squareIndex != -1)
                {
                    // Checks if the clicked square contains a piece
                    if (parrentBoard.pieces[squareIndex] != null)
                    {
                        // Clicked the square if no piece is currently selected
                        Select(squareIndex);
                    }
                }
            }
            else if (movementState == MousePieceMovementState.Selected)
            {
                // Tries to play the move
                TryToMakeMove(selectedPiece.rank * 8 + selectedPiece.file, GetIndexUnderMouse());
            }
        }

        #endregion

        // Utilities
        #region Utilities

        // Loads a FEN string
        public void LoadFEN(FEN fen)
        {
            localPosition.LoadFEN(fen);
        }

        // Shows legal moves
        private void ShowLegalMoves(int from)
        {
            if ((parrentBoard.whiteHumman ? selectedPiece.pieceType.color == Piece.Color.White : selectedPiece.pieceType.color == Piece.Color.Black) || parrentBoard.hvh)
            {
                List<ushort> legalMoves = moveGenerator.GenerateLegalMoves(localPosition, (byte)(localPosition.sideToMove ? 0 : 1)); // Gets a list of legal moves
                List<ushort> validLegalMoves = new List<ushort>(); // Used to store legal moves for a given square
                // Selects all legal moves, form passed square
                foreach (ushort legalMove in legalMoves)
                {
                    if (Move.GetFrom(legalMove) == from)
                    {
                        validLegalMoves.Add(legalMove);
                    }
                }

                // Displays all legal move from validLegalMoves
                foreach (ushort move in validLegalMoves)
                {
                    if (parrentBoard.pieces[Move.GetTo(move)] != null)
                    {
                        parrentBoard.squares[Move.GetTo(move)].LegalCaptureSpriteVisible(true);
                        showingLegalMove.Add(parrentBoard.squares[Move.GetTo(move)]);
                    }
                    else
                    {
                        parrentBoard.squares[Move.GetTo(move)].LegalNonCaptureSpriteVisible(true);
                        showingLegalMove.Add(parrentBoard.squares[Move.GetTo(move)]);
                    }
                }
            }
        }

        // Hides legal moves
        private void HideLegalMoves()
        {
            // Hides all legal moves
            foreach(Square square in showingLegalMove)
            {
                square.LegalCaptureSpriteVisible(false);
                square.LegalNonCaptureSpriteVisible(false);
            }
        }

        // Selects piece at a given index
        private void Select(int squareIndex)
        {
            movementState = MousePieceMovementState.Dragging; // Enters dragging moment
            selectedPiece = parrentBoard.pieces[squareIndex]; // Selected the clicked piece
            highlightedSquare = parrentBoard.squares[squareIndex]; // Saves clicked square as the highlighted square
            highlightedSquare.Highlighted(true); // Highlights clicked square
            selectedPiece.piece.GetComponent<SpriteRenderer>().sortingOrder = 21; // Makes the selected piece appear over other pieces
            primaryDrop = true; // Used to determined whenever to selected or deselect this piece later
            ShowLegalMoves(squareIndex); // Shows all legal moves for a selected piece
        }

        // Selected currently selected piece
        private void Deselect()
        {
            movementState = MousePieceMovementState.None; // Sets movement mode to none
            selectedPiece.ResetPosition(); // Resets the position
            selectedPiece.piece.GetComponent<SpriteRenderer>().sortingOrder = 20; // Changes back the ordering layer
            highlightedSquare.Highlighted(false); // dehighlight's highlighted square
            selectedPiece = null; // Sets to null since to piece is selected
            highlightedSquare = null; // Sets to null since to square is highlighted
            HideLegalMoves(); // Hides all legal moves
            if (lastMoveSquares[1] != null)
            {
                lastMoveSquares[1].Highlighted(true); // Highlights the last to square in case it was dehighlighted
            }
        }

        // Only make the move if the player owns that piece, its their turn and its a legal move
        private void TryToMakeMove(int from, int to, bool animate = true)
        {
            if (from >= 0 & from < 64 & from >= 0 & from < 64) 
            {
                // Checks if the player has permission to move selected piece
                if ((parrentBoard.whiteHumman ? selectedPiece.pieceType.color == Piece.Color.White : selectedPiece.pieceType.color == Piece.Color.Black) || parrentBoard.hvh)
                {
                    // Checks if the move is legal
                    if (IsLegal(from, to))
                    {
                        // Makes the move
                        MakeHumanMove(from, to, animate:animate);
                    }
                    else // Attempt to make a illegal move
                    {
                        selectedPiece.ResetPosition(); // Resets pieces position
                    }
                }
                else // Player does not have permission to move that piece
                {
                    selectedPiece.ResetPosition(); // Resets pieces position
                }
            }
        }

        // Makes the move and updates central position
        public void MakeHumanMove(int from, int to, bool animate = true)
        {
            parrentBoard.engineManager.MakeMove(ConvertToMove(from, to));
            MakeAIMove(from, to, animate:animate);
            if (!parrentBoard.hvh)
            {
                parrentBoard.engineManager.MakeAIMove(new ChessEngineManager.MoveGenerationProfile(10, 5));
            }
        }

        // Makes the move and assumes the central position has already been updated
        public void MakeAIMove(int from, int to, bool animate = true)
        {
            // Updates local position
            localPosition.MakeMove(ConvertToMove(from, to));
            // Plays the move sound
            PlayMoveSound(ConvertToMove(from, to));
            // Makes the move on the visual chess board
            parrentBoard.MakeMove(ConvertToMove(from, to), animate:animate);
            // Deselects the piece
            if (selectedPiece != null)
            {
                Deselect();
            }

            // Deselects last move
            if (lastMoveSquares[0] != null)
            {
                lastMoveSquares[0].Highlighted(false);
            }
            if (lastMoveSquares[1] != null)
            {
                lastMoveSquares[1].Highlighted(false);
            }

            // Highlights this move
            lastMoveSquares[0] = parrentBoard.squares[from];
            lastMoveSquares[1] = parrentBoard.squares[to];
            lastMoveSquares[0].Highlighted(true);
            lastMoveSquares[1].Highlighted(true);

            // TESTING 
            // ShowQuiescenceMoves();
            // END TESTING
        }

        // Plays a sound for the made move, call after the local position has been updated
        public void PlayMoveSound(ushort move)
        {
            int flag = Move.GetFlag(move);
            AudioManager audioManager = FindObjectOfType<AudioManager>();

            if (KingInCheck(localPosition.sideToMove))
            {
                audioManager.Play("move-check");
            }
            else if (flag == 0 | flag == 1) // quiet move or a double pawn push
            {
                if (parrentBoard.hvh)
                {
                    audioManager.Play("move-self");
                }
                else
                {
                    if (parrentBoard.whiteHumman & parrentBoard.whiteToMove)
                    {
                        audioManager.Play("move-self");
                    }
                    else
                    {
                        audioManager.Play("move-opponent");
                    }
                }
            }
            else if (flag == 2 | flag == 3) // king or queen side castle
            {
                audioManager.Play("castle");
            }
            else if (flag == 4 | flag == 5) // capture or en-passant
            {
                audioManager.Play("capture");
            }
            else if (flag >= 8 && flag <= 15)
            {
                audioManager.Play("promotion");
            }
        }

        #endregion

        // Support functions
        #region Support functions

        // Returns true if a given move is legal
        private bool IsLegal(int from, int to)
        {
            // Checks if the from and to index are inside the chess board
            if (from >= 0 && from < 64 && to >= 0 && to < 64)
            {
                List<ushort> legalMoves = moveGenerator.GenerateLegalMoves(localPosition, (byte)(localPosition.sideToMove ? 0 : 1)); // Gets a list of legal moves
                // Checks if the list of all legal moves contains a move from to
                foreach (ushort legalMove in legalMoves)
                {
                    if (legalMove == ConvertToMove(from, to))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        // Returns square index current under mouse : return -1 if the mouse course is outside the bounds of parent chess board
        private int GetIndexUnderMouse()
        {
            return GetIndexFromCoordinates(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        // Returns square index from coordinates : return -1 if the mouse course is outside the bounds of parent chess board
        private int GetIndexFromCoordinates(Vector2 coordinates)
        {
            // Converts to global coordinate to local
            coordinates -= (Vector2)parrentBoard.transform.position;

            // Checks if the pass coordinate is inside the bounds of the parent chess board
            if ((4*globalSquareLength) < coordinates.x | coordinates.x  < (-4*globalSquareLength) | (4 * globalSquareLength) < coordinates.y | coordinates.y < (-4 * globalSquareLength))
            {
                return -1; // Returns -1 since the coordinates are outside the bounds of the chess board
            }
            else
            {              
                int file = 4 + Mathf.FloorToInt(coordinates.x / globalSquareLength); // Calculates file                
                int rank = 4 + Mathf.FloorToInt(coordinates.y / globalSquareLength); // Calculate rank              
                int squareIndex = parrentBoard.whiteBottom ? rank * 8 + file : 63 - (rank * 8 + file); // Calculates square index                
                return squareIndex; // return square index
            }
        }

        // Returns a move in a (form, to, flag) form for a given (from to) move form
        private ushort ConvertToMove(int from, int to)
        {
            // Converts the square index to file and rank
            int fromFile = from % 8;
            int fromRank = (from - fromFile) / 8;
            int toFile = to % 8;
            int toRank = (to - toFile) / 8;

            // Gets the flag
            int flag;
            Piece.Type pieceToMove = parrentBoard.pieces[from].pieceType.type;
            bool isCapture = parrentBoard.pieces[to] != null;

            switch (pieceToMove)
            {
                case Piece.Type.Pawn: // En-passant double pawn push or a normal move
                    if (Mathf.Abs(fromRank - toRank) == 2) // Double pawn push
                    {
                        flag = Move.Flag.doublePawnPush;
                    }
                    else if (toRank == 0 | toRank == 7) // Promotion move
                    {
                        if (isCapture)
                        {
                            flag = Move.Flag.queenPromotionCapture;
                        }
                        else
                        {
                            flag = Move.Flag.queenPromotion;
                        }
                    }
                    else if (fromFile != toFile && !isCapture) // En-passant
                    {
                        flag = Move.Flag.epCapture;
                    }
                    else // Normal move
                    {
                        if (isCapture)
                        {
                            flag = Move.Flag.capture;
                        }
                        else
                        {
                            flag = Move.Flag.quietMove;
                        }
                    }
                    break;

                case Piece.Type.King: // Castle or a normal move                   
                    if (fromFile -  toFile > 1 && fromRank == toRank) // Queen side castling
                    {
                        flag = Move.Flag.queenCastle;
                        to = localPosition.sideToMove ? 2 : 58;
                    }
                    else if (fromFile - toFile < -1 && fromRank == toRank) // King side castling
                    {
                        flag = Move.Flag.kingCastle;
                        to = localPosition.sideToMove ? 6 : 62;
                    }
                    else // Normal move
                    {
                        if (isCapture)
                        {
                            flag = Move.Flag.capture;
                        }
                        else
                        {
                            flag = Move.Flag.quietMove;
                        }
                    }
                    break;

                default: // Normal move
                    if (isCapture)
                    {
                        flag = Move.Flag.capture;
                    }
                    else
                    {
                        flag = Move.Flag.quietMove;
                    }
                    break;
            }

            return Move.GenMove((ushort)from, (ushort)to, (ushort)flag);
        }

        // Returns true is the king is under attack
        private bool KingInCheck(bool whiteKing)
        {
            int kingIndex = BitOps.BitScanForward(localPosition.bitboard.pieces[5 + (whiteKing ? 0 : 7)]); // Gets the location of the defending king
            return moveGenerator.SquareAttackedBy(localPosition, (byte)(whiteKing ? 0 : 1), (byte)kingIndex); // Checks if the defending king is under attack, aka in check
        }

        #endregion

        // Structures and enumerations used by this class   
        #region Structures and enumerations

        // Used to represent type of piece moment currently in use
        public enum MousePieceMovementState
        {
            None,
            Selected,
            Dragging
        }

        #endregion

        // Testing
        #region Testing

        public void ShowQuiescenceMoves()
        {
            ushort[] nonQuietMoves = moveGenerator.GenerateLegalMoves(localPosition, (byte)(localPosition.sideToMove ? 0 : 1), includeQuiet:false).ToArray();
            ushort[] nonQuietNonCheckMoves = moveGenerator.GenerateLegalMoves(localPosition, (byte)(localPosition.sideToMove ? 0 : 1), includeQuiet: false, includeChecks: false).ToArray();
            bool[] visited = new bool[64];
            
            foreach(Square square in parrentBoard.squares)
            {
                square.ResetColor();
            }
            
            foreach(ushort move in nonQuietMoves)
            {
                parrentBoard.squares[Move.GetTo(move)].quad.GetComponent<MeshRenderer>().material.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                visited[Move.GetTo(move)] = true;
            }
            foreach (ushort move in nonQuietNonCheckMoves)
            {
                if (visited[Move.GetTo(move)] == true)
                {
                    parrentBoard.squares[Move.GetTo(move)].quad.GetComponent<MeshRenderer>().material.color = Color.gray;
                }
                else
                {
                    parrentBoard.squares[Move.GetTo(move)].quad.GetComponent<MeshRenderer>().material.color = Color.blue;
                }
                
            }
        }

        #endregion
    }
}
