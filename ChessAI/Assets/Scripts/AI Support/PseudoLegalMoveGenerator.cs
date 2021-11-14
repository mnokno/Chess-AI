using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public class PseudoLegalMoveGenerator 
    {
        // Class variables
        #region Variables

        private List<ushort> moves = new List<ushort>(); // Stores all moves
        private Position position; // Stores reference to a position for which the moves are generated
        private byte generateForWhiteByte; // Determines whenever moves are generated for white or black, 0 = white, 1 = black
        private bool generateForWhite; // Determines whenever moves are generated for white or black
        private bool includeQuietMoves; // Does not generate quite moves if set to false
        private int alliesColorOffset; // Allies color offset
        private int enemiesColorOffset; // Enemies color offset

        private ushort squareForm; // Square which is currently occupied by the piece
        private ushort squareTo; // Square that the piece can move to
        private ulong remainingPieces; // Bitboard of remaining pieces
        private ulong attcks; // Attacks bitboard
        private ulong permitSquares; // Squares that do not contain ally pieces

        #endregion

        // Move generation functions
        #region Move generation

        public List<ushort> GeneratePseudoLegalMoves(Position position, bool includeQuietMoves = true)
        {
            InitMoveGenGenerateMoveseration(position, includeQuietMoves); // Initializes move generation 
            GeneratePawnMoves();
            GenerateKnightMoves();
            GenerateBishopMoves();
            GenerateRookMoves();
            GenerateQueenMoves();
            GenerateKingMoves();
            GenerateCastleMoves();
            return moves;
        }

        public List<ushort> GenerateLegalMoves(Position position, bool includeQuietMoves = true)
        {
            List<ushort> pseudoLegalMoves = GeneratePseudoLegalMoves(position, includeQuietMoves: includeQuietMoves);
            List<ushort> legalMoves = new List<ushort>();

            int c = 0;
            foreach (ushort move in pseudoLegalMoves)
            {
                position.MakeMove(move);
                if (!SquareAttackedBy(BitOps.BitScanForward(position.bitboard.pieces[5 + alliesColorOffset]), generateForWhite ? 1 : 0))
                {
                    legalMoves.Add(move);
                }
                position.UnmakeMove(move);
                c++;
            }

            return legalMoves;
        }

        private void GeneratePawnMoves()
        {

            short colorOffset = (short)(position.sideToMove ? 8 : -8); // For pawn pushes
            byte startRank = (byte)(position.sideToMove ? 1 : 6); // If at starting rank a double pawn push is possible
            byte rankBeforePromotion = (byte)(position.sideToMove ? 6 : 1); // If at rank before promotion, the pawn ill be promoted
            remainingPieces = position.bitboard.pieces[0 + alliesColorOffset]; // Bitboard containing all pawns waiting for their move to be generated

            while (remainingPieces != 0)
            {
                squareForm = (ushort)BitOps.BitScanForward(remainingPieces); // Get the initial square
                byte oneSquareAhead = (byte)(squareForm + colorOffset); // Used multiple times, saved to a variable to save time
                byte twoSquareAhead = (byte)(squareForm + colorOffset * 2); // Used multiple times, saved to a variable to save time
                byte rank = (byte)Mathf.Floor(squareForm / 8); // Gets the initial rank
                attcks = (generateForWhite ? PrecomputedMoveData.pawnAttacksWhite[squareForm] : PrecomputedMoveData.pawnAttacksBlack[squareForm]) & permitSquares & position.bitboard.pieces[6 + enemiesColorOffset]; // Removes attacks that attack ally pieces and empty squares

                // Generates captures
                while (attcks != 0) 
                {
                    squareTo = (ushort)BitOps.BitScanForward(attcks); // Gets the attacked square
                    if (rank == rankBeforePromotion) // Capture with promotion
                    {
                        moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.knightPromotionCapture)); // Adds this move to the generated move list
                        moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.bishopPromotionCapture)); // Adds this move to the generated move list
                        moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.rookPromotionCapture)); // Adds this move to the generated move list
                        moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.queenPromotionCapture)); // Adds this move to the generated move list
                    }
                    else // Normal pawn capture
                    {
                        moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.capture)); // Adds this move to the generated move list
                    }
                    attcks ^= BitboardUtility.GenerateShift(1, squareTo); // Removes this attack from attacks                         
                }

                // Generates en-passant captures
                if (position.enPassantTargetFile != 8)
                {
                    attcks = (generateForWhite ? PrecomputedMoveData.pawnAttacksWhite[squareForm] : PrecomputedMoveData.pawnAttacksBlack[squareForm])
                        & BitboardUtility.GenerateShift(1, Constants.enPassantFileToIndex[generateForWhiteByte][position.enPassantTargetFile]); // Removes attacks that attack ally pieces and all ranks but the en-passant rank

                    if (attcks != 0)
                    {
                        moves.Add(Move.GenMove(squareForm, Constants.enPassantFileToIndex[generateForWhiteByte][position.enPassantTargetFile], Move.Flag.epCapture)); // Adds this move to the generated move list
                    }
                }


                // Generates pawn pushes
                if (position.squareCentric.pieces[oneSquareAhead] == (byte)SquareCentric.PieceType.Empty) 
                {
                    if (rank == rankBeforePromotion) // promotion move
                    {
                        moves.Add(Move.GenMove(squareForm, oneSquareAhead, Move.Flag.knightPromotion)); // Adds this move to the generated move list
                        moves.Add(Move.GenMove(squareForm, oneSquareAhead, Move.Flag.bishopPromotion)); // Adds this move to the generated move list
                        moves.Add(Move.GenMove(squareForm, oneSquareAhead, Move.Flag.rookPromotion)); // Adds this move to the generated move list
                        moves.Add(Move.GenMove(squareForm, oneSquareAhead, Move.Flag.queenPromotion)); // Adds this move to the generated move list
                    }
                    else // Normal pawn push 
                    {
                        moves.Add(Move.GenMove(squareForm, oneSquareAhead, Move.Flag.quietMove)); // Adds this move to the generated move list
                        if (rank == startRank && position.squareCentric.pieces[twoSquareAhead] == (byte)SquareCentric.PieceType.Empty) // Double pawn push
                        {
                            moves.Add(Move.GenMove(squareForm, twoSquareAhead, Move.Flag.doublePawnPush)); // Adds this move to the generated move list
                        }
                    }
                }

                remainingPieces ^= BitboardUtility.GenerateShift(1, squareForm); // Removes this knight from remainingPieces bitboard since the move for this piece where generated
            }
        }

        private void GenerateKnightMoves()
        {
            remainingPieces = position.bitboard.pieces[1 + alliesColorOffset]; // Bitboard containing all knights waiting for their move to be generated
            while (remainingPieces != 0)
            {
                squareForm = (ushort)BitOps.BitScanForward(remainingPieces); // Get the initial index for current knight
                attcks = PrecomputedMoveData.knightAttacks[squareForm] & permitSquares; // Removes attacks that attack ally pieces
                while (attcks != 0)
                {
                    squareTo = (ushort)BitOps.BitScanForward(attcks); // Gets the attacked square
                    moves.Add(Move.GenMove(squareForm, squareTo, position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Empty ? Move.Flag.quietMove : Move.Flag.capture)); // Adds this move to the generated move list
                    attcks ^= BitboardUtility.GenerateShift(1, squareTo); // Removes this attack from attacks              
                }
                remainingPieces ^= BitboardUtility.GenerateShift(1, squareForm); // Removes this knight from remainingPieces bitboard since the move for this piece where generated
            }
        }

        private void GenerateBishopMoves()
        {
            remainingPieces = position.bitboard.pieces[2 + alliesColorOffset]; // Bitboard containing all bishop waiting for their move to be generated
            while (remainingPieces != 0)
            {
                squareForm = (ushort)BitOps.BitScanForward(remainingPieces); // Get the initial index for current bishop
                for (int i = 4; i < 8; i++) // For each direction
                {
                    for (int j = 0; j < Constants.squaresToEdge[i][squareForm]; j++) // For each square till the edge in a given direction
                    {
                        byte squareTo = (byte)(squareForm + Constants.directionOffsets[i] * (j + 1));
                        if (position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Empty) // Empty Square
                        {
                            moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.quietMove)); // Adds this move to the generated move list
                        }
                        else if (position.squareCentric.colors[squareTo] == generateForWhiteByte) // Allie piece blocking they ray
                        {
                            break; // Ends this ray
                        }
                        else // Opponents piece blocking they ray
                        {
                            moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.capture)); // Adds this move to the generated move list
                            break; // Ends this ray
                        }
                    }
                }
                remainingPieces ^= BitboardUtility.GenerateShift(1, squareForm); // Removes this bishop from remainingPieces bitboard since the move for this piece where generated
            }
        }

        private void GenerateRookMoves()
        {
            remainingPieces = position.bitboard.pieces[3 + alliesColorOffset]; // Bitboard containing all rook waiting for their move to be generated
            while (remainingPieces != 0)
            {
                squareForm = (ushort)BitOps.BitScanForward(remainingPieces); // Get the initial index for current rook
                for (int i = 0; i < 4; i++) // For each direction
                {
                    for (int j = 0; j < Constants.squaresToEdge[i][squareForm]; j++) // For each square till the edge in a given direction
                    {
                        byte squareTo = (byte)(squareForm + Constants.directionOffsets[i] * (j + 1));
                        if (position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Empty) // Empty Square
                        {
                            moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.quietMove)); // Adds this move to the generated move list
                        }
                        else if (position.squareCentric.colors[squareTo] == generateForWhiteByte) // Allie piece blocking they ray
                        {
                            break; // Ends this ray
                        }
                        else // Opponents piece blocking they ray
                        {
                            moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.capture)); // Adds this move to the generated move list
                            break; // Ends this ray
                        }
                    }
                }
                remainingPieces ^= BitboardUtility.GenerateShift(1, squareForm); // Removes this rook from remainingPieces bitboard since the move for this piece where generated
            }
        }

        private void GenerateQueenMoves()
        {
            remainingPieces = position.bitboard.pieces[4 + alliesColorOffset]; // Bitboard containing all queen waiting for their move to be generated
            while (remainingPieces != 0)
            {
                squareForm = (ushort)BitOps.BitScanForward(remainingPieces); // Get the initial index for current queen
                for (int i = 0; i < 8; i++) // For each direction
                {
                    for (int j = 0; j < Constants.squaresToEdge[i][squareForm]; j++) // For each square till the edge in a given direction
                    {
                        byte squareTo = (byte)(squareForm + Constants.directionOffsets[i] * (j + 1));
                        if (position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Empty) // Empty Square
                        {
                            moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.quietMove)); // Adds this move to the generated move list
                        }
                        else if (position.squareCentric.colors[squareTo] == generateForWhiteByte) // Allie piece blocking they ray
                        {
                            break; // Ends this ray
                        }
                        else // Opponents piece blocking they ray
                        {
                            moves.Add(Move.GenMove(squareForm, squareTo, Move.Flag.capture)); // Adds this move to the generated move list
                            break; // Ends this ray
                        }
                    }
                }
                remainingPieces ^= BitboardUtility.GenerateShift(1, squareForm); // Removes this queen from remainingPieces bitboard since the move for this piece where generated
            }
        }

        private void GenerateKingMoves()
        {
            squareForm = (ushort)BitOps.BitScanForward(position.bitboard.pieces[5 + alliesColorOffset]); // Get the initial index for current king
            attcks = PrecomputedMoveData.kingAttacks[squareForm] & permitSquares; // Removes attacks that attack ally pieces
            while (attcks != 0)
            {
                squareTo = (ushort)BitOps.BitScanForward(attcks); // Gets the attacked square
                moves.Add(Move.GenMove(squareForm, squareTo, position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Empty ? Move.Flag.quietMove : Move.Flag.capture)); // Adds this move to the generated move list
                attcks ^= BitboardUtility.GenerateShift(1, squareTo); // Removes this attack from attacks              
            }
        }

        private void GenerateCastleMoves()
        {
            byte opponentColor = (byte)(generateForWhite ? 1 : 0);
            if (generateForWhite)
            {
                if ((position.castlingRights & 0b1000) != 0) // White queen side
                {
                    if (!SquareAttackedBy(4, opponentColor) && !SquareAttackedBy(3, opponentColor) && !SquareAttackedBy(2, opponentColor)
                        & position.squareCentric.pieces[3] == (byte)SquareCentric.PieceType.Empty & position.squareCentric.pieces[2] == (byte)SquareCentric.PieceType.Empty & position.squareCentric.pieces[1] == (byte)SquareCentric.PieceType.Empty)
                    {
                        moves.Add(Move.GenMove(4, 2, Move.Flag.queenCastle));
                    }
                }
                if ((position.castlingRights & 0b0100) != 0) // White king side
                {
                    if (!SquareAttackedBy(4, opponentColor) && !SquareAttackedBy(5, opponentColor) && !SquareAttackedBy(6, opponentColor)
                        & position.squareCentric.pieces[5] == (byte)SquareCentric.PieceType.Empty & position.squareCentric.pieces[6] == (byte)SquareCentric.PieceType.Empty)
                    {
                        moves.Add(Move.GenMove(4, 6, Move.Flag.kingCastle));
                    }
                }
            }
            else
            {
                if ((position.castlingRights & 0b0010) != 0) // Black queen side
                {
                    if (!SquareAttackedBy(60, opponentColor) && !SquareAttackedBy(59, opponentColor) && !SquareAttackedBy(58, opponentColor)
                        & position.squareCentric.pieces[59] == (byte)SquareCentric.PieceType.Empty & position.squareCentric.pieces[58] == (byte)SquareCentric.PieceType.Empty & position.squareCentric.pieces[57] == (byte)SquareCentric.PieceType.Empty)
                    {
                        moves.Add(Move.GenMove(60, 58, Move.Flag.queenCastle));
                    }
                }
                if ((position.castlingRights & 0b0001) != 0) // Black king side
                {
                    if (!SquareAttackedBy(60, opponentColor) && !SquareAttackedBy(61, opponentColor) && !SquareAttackedBy(62, opponentColor)
                        & position.squareCentric.pieces[61] == (byte)SquareCentric.PieceType.Empty & position.squareCentric.pieces[62] == (byte)SquareCentric.PieceType.Empty)
                    {
                        moves.Add(Move.GenMove(60, 62, Move.Flag.kingCastle));
                    }
                }
            }
        }

        #endregion

        // Support functions
        #region Support functions

        // Returns true if the square is attacked by the attacker
        private bool SquareAttackedBy(int attackedSquare, int attackerColor)
        {
            byte attackerColorOffset = (byte)(attackerColor == 0 ? 0 : 7);

            // Checks if the square is attacked by a king
            if ((position.bitboard.pieces[5 + attackerColorOffset] & PrecomputedMoveData.kingAttacks[attackedSquare]) != 0)
            {
                return true;
            }

            // Checks if the square is attacked by pawn
            if ((position.bitboard.pieces[0 + attackerColorOffset] & (attackerColor == 0 ? PrecomputedMoveData.pawnAttacksBlack[attackedSquare] : PrecomputedMoveData.pawnAttacksWhite[attackedSquare])) != 0)
            {
                return true;
            }

            // Checks if the square is attacked by knight
            if ((position.bitboard.pieces[1 + attackerColorOffset] & PrecomputedMoveData.knightAttacks[attackedSquare]) != 0)
            {
                return true;
            }

            // Checks if the square is attacked by orthogonal attacked (queens and rooks)
            for (int i = 0; i < 4; i++) // For each orthogonal direction
            {
                for (int j = 0; j < Constants.squaresToEdge[i][attackedSquare]; j++)
                {
                    byte squareTo = (byte)(attackedSquare + Constants.directionOffsets[i] * (j + 1));
                    if (position.squareCentric.pieces[squareTo] != (byte)SquareCentric.PieceType.Empty)
                    {
                        if (position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Rook | position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Queen)
                        {
                            if (position.squareCentric.colors[squareTo] == attackerColor) // Attacking rook or queen blocking they ray
                            {
                                return true; // Ends this ray
                            }
                            else // There is a piece blocking the ray
                            {
                                break; // Ends this ray
                            }
                        }
                        else // There is a piece blocking the ray
                        {
                            break; // Ends this ray
                        }
                    }
                }
            }

            // Checks if the square is attacked by diagonals attacked (queens and bishops)
            for (int i = 4; i < 8; i++) // For each orthogonal direction
            {
                for (int j = 0; j < Constants.squaresToEdge[i][attackedSquare]; j++)
                {
                    byte squareTo = (byte)(attackedSquare + Constants.directionOffsets[i] * (j + 1));
                    if (position.squareCentric.pieces[squareTo] != (byte)SquareCentric.PieceType.Empty)
                    {
                        if (position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Bishop | position.squareCentric.pieces[squareTo] == (byte)SquareCentric.PieceType.Queen)
                        {
                            if (position.squareCentric.colors[squareTo] == attackerColor) // Attacking bishop or queen blocking they ray
                            {
                                return true; // Ends this ray
                            }
                            else // There is a piece blocking the ray
                            {
                                break; // Ends this ray
                            }
                        }
                        else // There is a piece blocking the ray
                        {
                            break; // Ends this ray
                        }
                    }
                }
            }

            return false;
        }

        // Returns true if the square is attacked by the attacker
        public bool SquareAttackedBy(Position position, int attackedSquare, int attackerColor)
        {
            InitMoveGenGenerateMoveseration(position, true);
            return SquareAttackedBy(attackedSquare, attackerColor);
        }

        // Initializes move generation
        private void InitMoveGenGenerateMoveseration(Position position, bool includeQuietMoves)
        {
            this.position = position; // Saves reference to position
            this.generateForWhiteByte = (byte)(position.sideToMove ? 0 : 1); // Decides for which piece to generate moves
            this.generateForWhite = position.sideToMove; // Decides for which piece to generate moves
            this.alliesColorOffset = position.sideToMove ? 0 : 7; // Allies color offset
            this.enemiesColorOffset = !position.sideToMove ? 0 : 7; // Enemies color offset
            this.permitSquares = ~position.bitboard.pieces[6 + alliesColorOffset]; // Squares that do not contain ally pieces
            this.includeQuietMoves = includeQuietMoves; // Saves reference to includeQuietMoves
            moves.Clear(); // Clears the move list from old moves
        }

        #endregion
    }
}

