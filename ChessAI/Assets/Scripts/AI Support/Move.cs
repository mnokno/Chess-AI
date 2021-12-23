using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public static class Move
    {
        // Class utilities
        #region Utilities

        // Generate a move
        public static ushort GenMove(ushort from, ushort to, ushort flag)
        {
            return (ushort)(((flag & 0xf) << 12) | ((from & 0x3f) << 6) | (to & 0x3f));
        }

        // Returns to square
        public static ushort GetTo(ushort move)
        {
            return (ushort)(move & 0x3f);
        }

        // Returns from square
        public static ushort GetFrom(ushort move)
        {
            return (ushort)((move >> 6) & 0x3f);
        }

        // Returns flag
        public static ushort GetFlag(ushort move)
        {
            return (ushort)((move >> 12) & 0x0f);
        }

        // Converts a PNG move to a ushort move
        public static ushort ConvertPGNToUshort(string PNGMove, Position position)
        {
            // If it was a castling move
            if (PNGMove == "O-O")
            {
                if (position.sideToMove)
                {
                    return GenMove(4, 6, Flag.kingCastle);
                }
                else
                {
                    return GenMove(60, 62, Flag.kingCastle);
                }
            }
            else if (PNGMove == "O-O-O")
            {
                if (position.sideToMove)
                {
                    return GenMove(4, 2, Flag.queenCastle);
                }
                else
                {
                    return GenMove(60, 58, Flag.queenCastle);
                }
            }

            // Local variables
            bool isCapture = PNGMove.Contains("x");
            char pieceToMove = '-';
            char promoteTo = '-';
            int toSquareDataOffset = (PNGMove.Contains("+") ? 1 : 0) + (PNGMove.Contains("#") ? 1 : 0) + (PNGMove.Contains("=") ? 2 : 0);
            int fromSquareDataOffset = (char.IsUpper(PNGMove[0]) ? 1 : 0);
            char fileConstrain = '-';
            char rankConstrain = '-';
            SquareCentric.Squares to;
            SquareCentric.Squares from;
            ushort flag = 999;

            // Gets the piece type to move and piece to promote to if its a promotion move
            if (PNGMove.Contains("=")) // Its a promotion pawn move
            {
                pieceToMove = 'P';
                promoteTo = (PNGMove[PNGMove.Length - 1] == '+' || PNGMove[PNGMove.Length - 1] == '#') ? PNGMove[PNGMove.Length - 2] : PNGMove[PNGMove.Length - 1];
            }
            else if (PNGMove == PNGMove.ToLower()) // Its a non-promotion pawn move
            {
                pieceToMove = 'P';
            }
            else // Its not a pawn move
            {
                pieceToMove = PNGMove[0];
            }

            // Gets the square that the piece is being move to
            to = (SquareCentric.Squares)Enum.Parse(typeof(SquareCentric.Squares), (PNGMove[PNGMove.Length - toSquareDataOffset - 2].ToString() + PNGMove[PNGMove.Length - toSquareDataOffset - 1].ToString()).ToString());

            // Gets the constraints square from which the piece will be moved from
            if (PNGMove.Length - fromSquareDataOffset - toSquareDataOffset - 2 - (isCapture ? 1 : 0) == 1)
            {
                char constrain = PNGMove[PNGMove.Length - toSquareDataOffset - 3 - (isCapture ? 1 : 0)];
                if ("abcdefgh".Contains(constrain.ToString()))
                {
                    fileConstrain = constrain;
                }
                else
                {
                    rankConstrain = constrain;
                }
            }
            else if (PNGMove.Length - fromSquareDataOffset - toSquareDataOffset - 2 - (isCapture ? 1 : 0) == 2)
            {
                fileConstrain = PNGMove[PNGMove.Length - toSquareDataOffset - 4 - (isCapture ? 1 : 0)];
                rankConstrain = PNGMove[PNGMove.Length - toSquareDataOffset - 3 - (isCapture ? 1 : 0)];
            }

            // Returns the index at which the ray intersected one of the targets, returns -1 if there was no intersection between the target and the ray
            int DoesRayHitTarget(short reyDirectionIndex, ushort from, ulong targetsBB, ulong allPiecesBB)
            {
                // For each square to the edge or till the ray is blocked
                for (ushort i = 1; i < Constants.squaresToEdge[reyDirectionIndex][from] + 1; i++)
                {
                    // Calculates the current position along the ray
                    int currentRayPosition = from + (i * Constants.directionOffsets[reyDirectionIndex]);
                    if ((allPiecesBB & Constants.primitiveBitboards[currentRayPosition]) != 0) // Rays intersected with some other piece
                    {
                        if ((targetsBB & Constants.primitiveBitboards[currentRayPosition]) != 0) // Ray intersected with the target
                        {
                            return currentRayPosition;
                        }
                        else // Ray was blocked before it reached the target
                        {
                            return -1;
                        }
                    }
                }
                return -1; // The ray was never block by any pieces (including the target)
            }
            // Converts the piece code form a char to an int that can be used to access the corresponding bitboard from pieces array, returns -1 if the piece char code is invalid
            int GetPieceInt(char pieceChar)
            {
                switch (pieceChar)
                {
                    case ('P'):
                        return 0;
                    case ('N'):
                        return 1;
                    case ('B'):
                        return 2;
                    case ('R'):
                        return 3;
                    case ('Q'):
                        return 4;
                    case ('K'):
                        return 5;
                    default :
                        return -1;
                }
            }

            // Gets the square from which the piece will be moved from
            if (pieceToMove == 'K') // There is only one king for each side
            {
                from = (SquareCentric.Squares)(BitOps.BitScanForward(position.bitboard.pieces[5 + (position.sideToMove ? 0 : 7)]));
            }
            else if (fileConstrain != '-' && rankConstrain != '-') // If the position is specified exactly
            {
                from = (SquareCentric.Squares)Enum.Parse(typeof(SquareCentric.Squares), (fileConstrain + rankConstrain).ToString());
            }
            else // The position is not specified exactly
            {
                // Gets a bitboard containing pieces that satisfy the constraint
                ulong possibleTargetsBB;
                if (fileConstrain != '-')
                {
                    possibleTargetsBB = position.bitboard.pieces[GetPieceInt(pieceToMove) + (position.sideToMove ? 0 : 7)] & Constants.fileMasks[7 - "abcdefgh".IndexOf(fileConstrain)];
                }
                else if (rankConstrain != '-')
                {
                    possibleTargetsBB = position.bitboard.pieces[GetPieceInt(pieceToMove) + (position.sideToMove ? 0 : 7)] & Constants.rankMasks[int.Parse(rankConstrain.ToString()) - 1];
                }
                else
                {
                    possibleTargetsBB = position.bitboard.pieces[GetPieceInt(pieceToMove) + (position.sideToMove ? 0 : 7)];
                }

                // Choses the correct member from all the members that satisfy the constrains
                if (BitOps.PopulationCount(possibleTargetsBB) == 1)
                {
                    from = (SquareCentric.Squares)BitOps.BitScanForward(possibleTargetsBB);
                }
                else
                {
                    while (true)
                    {
                        if (pieceToMove == 'P') // Pawn
                        {
                            if (isCapture)
                            {
                                from = (SquareCentric.Squares)BitOps.BitScanForward(possibleTargetsBB & (position.sideToMove ? PrecomputedMoveData.pawnAttacksBlack[(int)to] : PrecomputedMoveData.pawnAttacksWhite[(int)to]));
                                if (MoveGenerator.CanPieceFromMoveTo((byte)from, (byte)to, position.sideToMove, position))
                                {
                                    break;
                                }
                                else
                                {
                                    possibleTargetsBB &= ~Constants.primitiveBitboards[(byte)from];
                                }
                            }
                            else
                            {
                                int colorOffset = position.sideToMove ? 8 : -8;
                                if ((possibleTargetsBB & Constants.primitiveBitboards[(int)to - colorOffset]) != 0)
                                {
                                    from = (SquareCentric.Squares)BitOps.BitScanForward(possibleTargetsBB & Constants.primitiveBitboards[(int)to - colorOffset]);
                                    if (MoveGenerator.CanPieceFromMoveTo((byte)from, (byte)to, position.sideToMove, position))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        possibleTargetsBB &= ~Constants.primitiveBitboards[(byte)from];
                                    }
                                }
                                else
                                {
                                    from = (SquareCentric.Squares)BitOps.BitScanForward(possibleTargetsBB & Constants.primitiveBitboards[(int)to - (2 * colorOffset)]);
                                    flag = Flag.doublePawnPush;
                                    if (MoveGenerator.CanPieceFromMoveTo((byte)from, (byte)to, position.sideToMove, position))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        possibleTargetsBB &= ~Constants.primitiveBitboards[(byte)from];
                                    }
                                }
                            }
                        }
                        else if (pieceToMove == 'N') // Knight
                        {
                            from = (SquareCentric.Squares)(BitOps.BitScanForward(possibleTargetsBB & PrecomputedMoveData.knightAttacks[(int)to]));
                            if (MoveGenerator.CanPieceFromMoveTo((byte)from, (byte)to, position.sideToMove, position))
                            {
                                break;
                            }
                            else
                            {
                                possibleTargetsBB &= ~Constants.primitiveBitboards[(byte)from];
                            }
                        }
                        else // Bishop, Rook or Queen
                        {
                            short[] rayDirections;
                            if (pieceToMove == 'B')
                            {
                                rayDirections = new short[] { 4, 5, 6, 7 };
                            }
                            else if (pieceToMove == 'R')
                            {
                                rayDirections = new short[] { 0, 1, 2, 3 };
                            }
                            else // Has to be 'Q'
                            {
                                rayDirections = new short[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                            }

                            from = SquareCentric.Squares.a1;
                            ulong allPieces = position.bitboard.pieces[6] | position.bitboard.pieces[13];
                            foreach (short direction in rayDirections)
                            {
                                int result = DoesRayHitTarget(direction, (ushort)to, possibleTargetsBB, allPieces);
                                if (result != -1)
                                {
                                    from = (SquareCentric.Squares)result;
                                    break;
                                }
                            }
                            if (MoveGenerator.CanPieceFromMoveTo((byte)from, (byte)to, position.sideToMove, position))
                            {
                                break;
                            }
                            else
                            {
                                possibleTargetsBB &= ~Constants.primitiveBitboards[(byte)from];
                            }
                        }
                    }
                }
            }
            
            // Decides on the flag
            if (flag != Flag.doublePawnPush)
            {
                if (promoteTo != '-') // Its a promotion move
                {
                    switch (promoteTo)
                    {
                        case ('N'):
                            if (isCapture)
                            {
                                flag = Flag.knightPromotionCapture;
                            }
                            else
                            {
                                flag = Flag.knightPromotion;
                            }
                            break;
                        case ('B'):
                            if (isCapture)
                            {
                                flag = Flag.bishopPromotionCapture;
                            }
                            else
                            {
                                flag = Flag.bishopPromotion;
                            }
                            break;
                        case ('R'):
                            if (isCapture)
                            {
                                flag = Flag.rookPromotionCapture;
                            }
                            else
                            {
                                flag = Flag.rookPromotion;
                            }
                            break;
                        default:
                            if (isCapture)
                            {
                                flag = Flag.queenPromotionCapture;
                            }
                            else
                            {
                                flag = Flag.queenPromotion;
                            }
                            break;
                    }
                }
                else if (isCapture)
                {
                    if (pieceToMove == 'P') // If could be an en-passant capture
                    {
                        if (position.squareCentric.pieces[(int)to] == (byte)SquareCentric.PieceType.Empty)
                        {
                            flag = Flag.epCapture;
                        }
                        else
                        {
                            flag = Flag.capture;
                        }
                    }
                    else
                    {
                        flag = Flag.capture;
                    }
                }
                else
                {
                    flag = Flag.quietMove;
                }
            }

            // Converts and returns the move
            return GenMove((ushort)from, (ushort)to, flag);
        }

        // Converts a ushort move to a PNG move
        public static string ConvertUshortToPNG(ushort move, Position position)
        {
            if (GetFlag(move) == Flag.kingCastle)
            {
                return "O-O";
            }
            else if (GetFlag(move) == Flag.queenCastle)
            {
                return "O-O-O";
            }
            else
            {
                string from = ((SquareCentric.Squares)GetFrom(move)).ToString();
                string to = ((SquareCentric.Squares)GetTo(move)).ToString();
                string pieceToMove = "";
                string promotion = "";
                string capture = position.squareCentric.pieces[GetTo(move)] != (byte)SquareCentric.PieceType.Empty ? "x" : "";
                string check = "";

                position.MakeMove(move);
                int kingIndex = BitOps.BitScanForward(position.bitboard.pieces[5 + (position.sideToMove ? 0 : 7)]); // Gets the location of the defending king
                if (position.SquareAttackedBy(kingIndex, (byte)(position.sideToMove ? 1 : 0)))
                {                                                                                                                                                                                                                                                                                                  
                    check = "+";
                }
                position.UnmakeMove(move);

                switch (position.squareCentric.pieces[GetFrom(move)])
                {
                    case (byte)SquareCentric.PieceType.Pawn:
                        pieceToMove = "";
                        if (GetFlag(move) >= 8) // promotion
                        {
                            int flag = GetFlag(move) >= 12 ? GetFlag(move) - 4 : GetFlag(move);
                            switch (flag)
                            {
                                case (byte)Flag.knightPromotion:
                                    promotion = "N";
                                    break;
                                case (byte)Flag.bishopPromotion:
                                    promotion = "B";
                                    break;
                                case (byte)Flag.rookPromotion:
                                    promotion = "R";
                                    break;
                                case (byte)Flag.queenCastle:
                                    promotion = "Q";
                                    break;
                            }
                        }
                        break;
                    case (byte)SquareCentric.PieceType.Knight:
                        pieceToMove = "N";
                        break;
                    case (byte)SquareCentric.PieceType.Bishop:
                        pieceToMove = "B";
                        break;
                    case (byte)SquareCentric.PieceType.Rook:
                        pieceToMove = "R";
                        break;
                    case (byte)SquareCentric.PieceType.Queen:
                        pieceToMove = "Q";
                        break;
                    case (byte)SquareCentric.PieceType.King:
                        pieceToMove = "K";
                        break;
                }
                
                return pieceToMove + from + capture + to + promotion + check;
            }
        }

        #endregion

        #region Formating functions

        public static string Format(ushort move)
        {
            return $"From: {GetFrom(move)}, To: {GetTo(move)}, Flag: {GetFlag(move)}";
        }

        #endregion
        // Structures and enumerations used by this class
        #region Structures and enumerations

        public readonly struct Flag
        {
            public const ushort quietMove = 0;
            public const ushort doublePawnPush = 1;
            public const ushort kingCastle = 2;
            public const ushort queenCastle = 3;
            public const ushort capture = 4;
            public const ushort epCapture = 5;
            public const ushort knightPromotion = 8;
            public const ushort bishopPromotion = 9;
            public const ushort rookPromotion = 10;
            public const ushort queenPromotion = 11;
            public const ushort knightPromotionCapture = 12;
            public const ushort bishopPromotionCapture = 13;
            public const ushort rookPromotionCapture = 14;
            public const ushort queenPromotionCapture = 15;
        }

        #endregion
    }
}
