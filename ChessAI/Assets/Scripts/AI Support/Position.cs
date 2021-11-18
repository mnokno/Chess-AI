using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chess.EngineUtility
{
    public class Position
    {
        /// Class variables
        #region Class variables

        public Bitboard bitboard; // Bitboard board representation for the current position
        public SquareCentric squareCentric; // Square centric board representation for the current position

        public bool sideToMove; // true : white --- false : black
        public byte castlingRights; // bit 0 : white queen side --- bit 1 : white king side --- bit 2 : black queen side --- bit 3 : black king side
        public byte enPassantTargetFile; // 0..7 represent a target file, 8 means no target square
        public byte halfmoveClock; // If >= 100 then its draw due to fifty-move rule, resets to 0 after pawn pushes and captures

        public ulong zobristKey; // Zobrist key for this position
        public Stack<ulong> boardStateHistory = new Stack<ulong>(); // Stack filled with zobrist keys used to determine three fold rule
        public Stack<uint> historicMoveData = new Stack<uint>(); // Stores information used to unmake a move that can't be restored from a child position

        #endregion

        /// Responsible for square initialization
        #region Initialization

        /// Class constructor
        public Position()
        {
            this.bitboard = new Bitboard("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
            this.squareCentric = new SquareCentric("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
            this.sideToMove = true;
            this.castlingRights = 0b1111;
            this.enPassantTargetFile = 8;
            this.halfmoveClock = 0;
            this.zobristKey = ZobristHashing.GetZobristKey(this);
            boardStateHistory.Push(zobristKey);
        }

        #endregion

        /// General use functions
        #region General

        public void LoadFEN(FEN fen)
        {
            // Loads information from the FEN string
            sideToMove = fen.GetSideToMove();
            castlingRights = fen.GetCastlingRights();
            enPassantTargetFile = fen.GetEnPassantTargetFile();
            halfmoveClock = fen.GetFullmoveCounter();
            // Loads the position on the bit board
            bitboard = new Bitboard(fen.GetPiecePlacment());
            // Loads the potion on the square centric
            squareCentric = new SquareCentric(fen.GetPiecePlacment());
            // Updates zobristKey
            this.zobristKey = ZobristHashing.GetZobristKey(this);
            boardStateHistory.Clear();
            boardStateHistory.Push(zobristKey);
        }

        // Returns true of the player to move is in check
        public bool PlayerInCheck()
        {
            if (sideToMove)
            {
                return SquareAttackedBy(BitOps.BitScanForward(bitboard.pieces[5]), 1);
            }
            else
            {
                return SquareAttackedBy(BitOps.BitScanForward(bitboard.pieces[11]), 0);
            }
        }

        public bool SquareAttackedBy(int attackedSquare, int attackerColor)
        {
            PseudoLegalMoveGenerator pseudoLegalMoveGenerator = new PseudoLegalMoveGenerator();
            return pseudoLegalMoveGenerator.SquareAttackedBy(this, attackedSquare, attackerColor);
        }
        #endregion

        /// Used to make and unmake a move
        #region Move

        /// Makes a move
        public void MakeMove(ushort move)
        {
            // Converts move into parts
            byte from = (byte)Move.GetFrom(move);
            byte to = (byte)Move.GetTo(move);
            byte flag = (byte)Move.GetFlag(move);

            byte pieceToMove = squareCentric.pieces[from]; // Get piece to move
            byte promoteTo = GetPromoteTo(flag); // Get piece to promote to, set to 10 if its not a promotion flag/move
            byte pieceToTake = (byte)(squareCentric.colors[to] == (byte)SquareCentric.SquareColor.Empty ? 10 : squareCentric.pieces[to]); // Get captured piece, set to 10 if no piece is captured

            historicMoveData.Push(HistoricMove.GenHistoricMove(enPassantTargetFile, castlingRights, pieceToTake, halfmoveClock)); // Creates a historic record of the potion in the current sate, used later to unmake this move
            BitboardUtility.MakeMove(ref bitboard, flag, from, to, sideToMove, pieceToMove: pieceToMove, pieceToTake: pieceToTake, promoteTo: promoteTo); // Makes the move on a bitboard
            SquareCentricUtility.MakeMove(ref squareCentric, flag, from, to, sideToMove, pieceToMove: pieceToMove, promoteTo: promoteTo); // Makes the move on a square centric

            if (flag == 0 | flag == 1) // Standard non-capture move
            {
                if (flag == 0) // Standard move
                {
                    ZobristHashing.UpdateEnPassantTargetFile(ref zobristKey, enPassantTargetFile, 8); // Updates en passant target file in zobrist key
                    enPassantTargetFile = 8; // Updates en-passant target file
                    halfmoveClock++; // Increases half move clock
                    ZobristHashing.Update(ref zobristKey, sideToMove, from, to, pieceToMove); // Updates position in the zobrist key

                    if (pieceToMove == (byte)SquareCentric.PieceType.King)
                    {
                        byte oldRight = castlingRights; // Saves old castling rights
                        castlingRights &= (byte)(sideToMove ? 0b0011 : 0b1100); // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRight, castlingRights); // Updates castling rights in zobrist key
                    }
                    else if (pieceToMove == (byte)SquareCentric.PieceType.Rook)
                    {
                        if (sideToMove) // Player could be moving their rook so castling rights have to be updated
                        {
                            if (from == 0) // White queen side
                            {
                                byte oldRights = castlingRights; // Saves old castling rights
                                castlingRights &= 0b0111; // Updates castling rights
                                ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                            }
                            else if (from == 7) // White king side
                            {
                                byte oldRights = castlingRights; // Saves old castling rights
                                castlingRights &= 0b1011; // Updates castling rights
                                ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                            }
                        }
                        else
                        {
                            if (from == 56) // Black queen side
                            {
                                byte oldRights = castlingRights; // Saves old castling rights
                                castlingRights &= 0b1101; // Updates castling rights
                                ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                            }
                            else if (from == 63) // Black king side
                            {
                                byte oldRights = castlingRights; // Saves old castling rights
                                castlingRights &= 0b1110; // Updates castling rights
                                ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                            }
                        }
                    }
                    else if (pieceToMove == (byte)SquareCentric.PieceType.Pawn)
                    {
                        halfmoveClock = 0; // Resets half-move clock after pawn push
                    }
                }
                else // Double pawn push
                {         
                    byte oldEnPassant = enPassantTargetFile; // Saves old en passant target file
                    enPassantTargetFile = (byte)(to % 8); // Updates en-passant target file
                    ZobristHashing.UpdateEnPassantTargetFile(ref zobristKey, oldEnPassant, enPassantTargetFile); // Updates en passant target file in zobrist key
                    halfmoveClock = 0; // Resets half-move clock after pawn push  
                }

            }
            else if (flag == 2 | flag == 3) // Castling move
            {
                if (flag == 2) // King side
                {
                    ZobristHashing.Update(ref zobristKey, sideToMove, CastlingDirection.KingSide); // Updates position in zobrist key
                }
                else // Queen side
                {
                    ZobristHashing.Update(ref zobristKey, sideToMove, CastlingDirection.QueenSide); // Updates position in zobrist key
                }
                if (sideToMove)
                {
                    ZobristHashing.UpdateCastlingRights(ref zobristKey, castlingRights, 0b0000); // Updates castling rights in the zobrist key
                    castlingRights &= 0b0011; // Updates castling rights
                }
                else
                {
                    ZobristHashing.UpdateCastlingRights(ref zobristKey, castlingRights, 0b0000); // Updates castling rights in the zobrist key
                    castlingRights &= 0b1100; // Updates castling rights
                }
                halfmoveClock++; // Increases calf move clock
                enPassantTargetFile = 8; // Updates en-passant target file
            }
            else if (flag == 4) // Capture move
            {
                ZobristHashing.UpdateEnPassantTargetFile(ref zobristKey, enPassantTargetFile, 8); // Updates en passant target file in zobrist key
                enPassantTargetFile = 8; // Updates en-passant target file
                halfmoveClock = 0; // Resets half move clock
                ZobristHashing.Update(ref zobristKey, sideToMove, from, to, pieceToMove, pieceToTake, false); // Updates position in the zobrist key

                if (!sideToMove) // Could be capturing a rook, so a castling rights has to be updated, or the rook itself could be capering
                {
                    if (to == 0) // White queen side
                    {
                        byte oldRights = castlingRights; // Saves old castling rights
                        castlingRights &= 0b0111; // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                    }
                    else if (to == 7) // White king side
                    {
                        byte oldRights = castlingRights; // Saves old castling rights
                        castlingRights &= 0b1011; // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                    }
                    if (from == 56) // Black queen side
                    {
                        byte oldRights = castlingRights; // Saves old castling rights
                        castlingRights &= 0b1101; // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                    }
                    else if (from == 63) // Black king side
                    {
                        byte oldRights = castlingRights; // Saves old castling rights
                        castlingRights &= 0b1110; // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                    }
                }
                else
                {
                    if (to == 56) // Black queen side
                    {
                        byte oldRights = castlingRights; // Saves old castling rights
                        castlingRights &= 0b1101; // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                    }
                    else if (to == 63) // Black king side
                    {
                        byte oldRights = castlingRights; // Saves old castling rights
                        castlingRights &= 0b1110; // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                    }
                    if (from == 0) // White queen side
                    {
                        byte oldRights = castlingRights; // Saves old castling rights
                        castlingRights &= 0b0111; // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                    }
                    else if (from == 7) // White king side
                    {
                        byte oldRights = castlingRights; // Saves old castling rights
                        castlingRights &= 0b1011; // Updates castling rights
                        ZobristHashing.UpdateCastlingRights(ref zobristKey, oldRights, castlingRights); // Updates castling rights in zobrist key
                    }
                }
            }
            else if (flag == 5) // En-passant
            {
                ZobristHashing.UpdateEnPassantTargetFile(ref zobristKey, enPassantTargetFile, 8); // Updates en passant target file in zobrist key
                enPassantTargetFile = 8; // Updates en-passant target file
                halfmoveClock = 0; // Resets half move clock
                ZobristHashing.Update(ref zobristKey, sideToMove, from, to, pieceToMove, (byte)SquareCentric.PieceType.Pawn, true); // Updates position in the zobrist key
            }
            else if (flag >= 8 & flag <= 15) // Promotion move
            {
                ZobristHashing.UpdateEnPassantTargetFile(ref zobristKey, enPassantTargetFile, 8); // Updates en passant target file in zobrist key
                enPassantTargetFile = 8; // Updates en-passant target file
                halfmoveClock = 0; // Resets half move clock

                if (flag < 12) // Promotion without capture
                {
                    ZobristHashing.Update(ref zobristKey, sideToMove, from, to, pieceToMove, (byte)(flag-7), false, 10); // Updates position in the zobrist key
                }
                else // Promotion with capture
                {
                    ZobristHashing.Update(ref zobristKey, sideToMove, from, to, pieceToMove, (byte)(flag - 11), true, pieceToTake); // Updates position in the zobrist key
                }
            }

            boardStateHistory.Push(zobristKey); // boardStateHistory
            sideToMove = !sideToMove; // Updates side to move
        }

        /// Unmakes a move
        public void UnmakeMove(ushort move)
        {
            // Reads historic data
            uint moveHistoricData = historicMoveData.Pop();

            // Converts move into parts
            ushort from = Move.GetFrom(move);
            ushort to = Move.GetTo(move);
            ushort flag = Move.GetFlag(move);

            byte movedPiece = squareCentric.pieces[to]; // Get piece to move
            byte promotedTo = GetPromoteTo(flag); // Get piece to promote to, set to 10 if its not a promotion flag/move
            byte capturedPiece = HistoricMove.GetCapturedPiece(moveHistoricData); // Get captured piece, set to 10 if no pieces are captured
            sideToMove = !sideToMove; // Updates side to move

            boardStateHistory.Pop(); // Removes this make from the move history
            BitboardUtility.UnmakeMove(ref bitboard, flag, from, to, sideToMove, pieceToMove: movedPiece, pieceToTake: capturedPiece, promoteTo: promotedTo); // Unmakes the move on a bitboard
            SquareCentricUtility.UnmakeMove(ref squareCentric, flag, from, to, sideToMove, pieceToMove: movedPiece, pieceToTake: capturedPiece); // Unmakes the move on a square centric
            
            castlingRights = HistoricMove.GetCastlingRights(moveHistoricData); // Updates castlingRights
            enPassantTargetFile = HistoricMove.GetEnPassantTargetFile(moveHistoricData); // Updates enPassantTargetFile
            halfmoveClock = HistoricMove.GetHalfmoveClock(moveHistoricData); // Updates halfmoveClock
            zobristKey = boardStateHistory.Peek(); // Updates zobristKey
        }

        #endregion

        /// Helper functions
        #region Helper functions

        /// Returns piece type to Promote to specified by the flag, returns 10 if its not a proration move
        public static byte GetPromoteTo(ushort flag)
        {
            if (flag < 8)
            {
                return 10;
            }
            if (flag >= 8 & flag <= 11)
            {
                return (byte)(flag - 7);
            }
            else // if (flag >= 12 & flag <= 15)
            {
                return (byte)(flag - 11);
            }
        }

        #endregion

        /// Structures and enumerations used by this class
        #region Structures and enumerations

        /// Maps castling direction to a number
        public enum CastlingDirection
        {
            QueenSide,
            KingSide
        }

        /// Maps player/piece color to a number
        public enum PlayerColor
        {
            White,
            Black
        }

        #endregion
    }
}
