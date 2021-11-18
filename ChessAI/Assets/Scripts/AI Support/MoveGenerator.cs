using System.Collections.Generic;
using UnityEngine;

namespace Chess.EngineUtility
{
    public class MoveGenerator
    {
        #region Class variables

        private Position position; // Position for which the moves are generated for
        private List<ushort> legalMoves; // List used to store generated legal moves
        private bool inCheck; // true if the king is checked by at least one pieces
        private bool inDoubleCheck; // true if the king is checked by at least two pieces
        private byte genForColorIndex; // Index of the player that the moves are generated for | 0=white, 1=black
        private byte genForColorIndexInverse; // Index of the player that is attacking the player that the moves are generated for | 0=white, 1=black
        private bool includeQuietMoves; // If set to false only capture, check and promotion move will be generated (), otherwise all legal moves are generated
        private bool includeChecks; // If set to false only capture and promotions will be generated
        private byte defKingIndex; // Index of the defending side's king
        private ulong defKingBB; // Bitboard contacting the defending king
        private byte attKingIndex; // Index of the attacking side's kink
        private bool pinsExistInPosition; // True if there is at lest one absolute pin in the position

        private ulong underPawnAttackBB; // Bitboard containing squares attacked by the opponents pawns
        private ulong underKnightAttackBB; // Bitboard containing squares attacked by the opponents knights
        private ulong underBishopAttackBB; // Bitboard containing squares attacked by the opponents bishops
        private ulong underRookAttackBB; // Bitboard containing squares attacked by the opponents rooks
        private ulong underQueenAttackBB; // Bitboard containing squares attacked by the opponents queens
        private ulong underKingAttackBB; // Bitboard containing squares attacked by the opponents king
        private ulong underAttackBB; // Bitboard containing squares attacked by the opponents pieces 
        private ulong checkRayBB; // Bitboard containing rays that check the king
        private ulong checkingPieceBB; // Bitboard containing piece that is currently checking the king
        private ulong pinRayBB; // Bitboard containing rays creating absolute pins

        private ulong promotionRankMask; // Mask for the rank of proration for the genFor player
        private ulong remainingPieces; // Bitboard of remaining pieces

        #endregion

        #region Core

        public List<ushort> GenerateLegalMoves(Position position, byte genForColorIndex, bool includeQuietMoves = true, bool includeChecks = true)
        {
            InitSearch(position, genForColorIndex, includeQuietMoves, includeChecks);
            CalculateBitBoards();
            GenerateLegalMoves();

            #region TESTING
            //var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            //var type = assembly.GetType("UnityEditor.LogEntries");
            //var method = type.GetMethod("Clear");
            //method.Invoke(new object(), null);
            //foreach (ushort move in legalMoves)
            //{
            //    Debug.Log($"From {Move.GetFrom(move)}, To {Move.GetTo(move)}, Flag {Move.GetFlag(move)} ");           
            //}
            //Debug.Log("Pin Ray BB: \n" + BitboardUtility.FormatBitboard(pinRayBB));
            //Debug.Log("under Bishop Attack BB: \n" + BitboardUtility.FormatBitboard(underBishopAttackBB));
            //Debug.Log("under Rook Attack BB: \n" + BitboardUtility.FormatBitboard(underRookAttackBB));
            //Debug.Log("under Queen Attack BB: \n" + BitboardUtility.FormatBitboard(underQueenAttackBB));
            //Debug.Log("under Attack BB: \n" + BitboardUtility.FormatBitboard(underAttackBB));
            //Debug.Log("Check Ray BB: \n" + BitboardUtility.FormatBitboard(checkRayBB));
            //Debug.Log("pins exist " + pinsExistInPosition);
            #endregion

            return legalMoves; // Returns list containing generated moves
        }

        #endregion

        #region Utilities

        // Sets parameters to initial values for the search
        private void InitSearch(Position position, byte genForColorIndex, bool includeQuietMoves, bool includeChecks)
        {
            this.position = position;
            this.legalMoves = new List<ushort>(256);
            this.inCheck = false;
            this.inDoubleCheck = false;
            this.genForColorIndex = genForColorIndex;
            this.genForColorIndexInverse = (byte)(genForColorIndex == 0 ? 1 : 0);
            this.includeQuietMoves = includeQuietMoves;
            this.includeChecks = includeChecks;      
            this.defKingIndex = (byte)BitOps.BitScanForward(position.bitboard.pieces[5 + 7 * this.genForColorIndex]);
            this.defKingBB = this.position.bitboard.pieces[5 + 7 * genForColorIndex];
            this.attKingIndex = (byte)BitOps.BitScanForward(position.bitboard.pieces[5 + 7 * this.genForColorIndexInverse]);
            this.pinsExistInPosition = false;

            this.underPawnAttackBB = 0;
            this.underKnightAttackBB = 0;
            this.underBishopAttackBB = 0;
            this.underRookAttackBB = 0;
            this.underQueenAttackBB = 0;
            this.underKingAttackBB = 0;
            this.underAttackBB = 0;
            this.checkRayBB = 0;
            this.pinRayBB = 0;

            this.promotionRankMask = genForColorIndex == 0 ? Constants.wrapMasks[3] : Constants.wrapMasks[7];
        }

        // Calculates attack bitboard in the process the values of inCheck and inDoubleCheck are also calculated, most be called after InitSearch
        private void CalculateBitBoards()
        {
            #region Pawn attacks
            // For each opponents pawn
            remainingPieces = position.bitboard.pieces[7 * genForColorIndexInverse]; // Fetches bitboard containing enemy pawns 
            while (remainingPieces != 0) // For each opponents pawn
            {
                int pos = BitOps.BitScanForward(remainingPieces); // Gets the position of the next pawn from the bitboard
                ulong attacksBB = genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksBlack[pos] : PrecomputedMoveData.pawnAttacksWhite[pos]; // Gets the attack bitboard for this pawn
                if ((attacksBB & defKingBB) != 0) // This pawn is checking the defending sides king
                {
                    if (inCheck)
                    {
                        inDoubleCheck = true;
                    }
                    else
                    {
                        inCheck = true;
                        checkingPieceBB = BitboardUtility.GenerateShift(1, pos); // Adds this pawn to the checking pieces bitboard
                    }         
                }
                underPawnAttackBB |= attacksBB; // Adds attacks of this pawn to the pawn attacks bitboard
                remainingPieces ^= BitboardUtility.GenerateShift(1, pos); // Removes this pawn from the remaining pieces bitboard
            }
            #endregion

            #region Knight attacks
            // For each opponents knight
            remainingPieces = position.bitboard.pieces[1 + 7 * genForColorIndexInverse]; // Fetches bitboard containing enemy knights 
            while (remainingPieces != 0) // For each opponents knight
            {
                int pos = BitOps.BitScanForward(remainingPieces); // Gets the position of the next knight from the bitboard
                ulong attacksBB = PrecomputedMoveData.knightAttacks[pos]; // Gets the attack bitboard for this knight
                if ((attacksBB & defKingBB) != 0) // This night is checking the defending sides king
                {
                    if (inCheck)
                    {
                        inDoubleCheck = true;
                    }
                    else
                    {
                        inCheck = true;
                        checkingPieceBB = BitboardUtility.GenerateShift(1, pos); // Adds this knight to the checking pieces bitboard
                    }
                }
                underKnightAttackBB |= attacksBB; // Adds attacks of this knight to the knight attacks bitboard
                remainingPieces ^= BitboardUtility.GenerateShift(1, pos); // Removes this knight from the remaining pieces bitboard
            }
            #endregion

            #region Bishop attacks
            // For each opponents bishop
            remainingPieces = position.bitboard.pieces[2 + 7 * genForColorIndexInverse]; // Fetches bitboard containing enemy bishops
            while (remainingPieces != 0) // For each opponents bishop
            {
                int pos = BitOps.BitScanForward(remainingPieces); // Gets the position of the next bishop from the bitboard
                for (int i = 4; i < 8; i++) // For each move direction
                {
                    ulong rayBB = 0;
                    bool rayBlocked = false;

                    for (int j = 1; j < Constants.squaresToEdge[i][pos] + 1; j++) // For each square in a given direction until edge is reached
                    {
                        ulong cRayPoint = BitboardUtility.GenerateShift(1, pos + Constants.directionOffsets[i] * j); // The currently investigated point on the ray
                        if ((cRayPoint & position.bitboard.pieces[6 + 7 * genForColorIndex]) != 0) // This squared is occupied by the defending side
                        {
                            if ((cRayPoint & position.bitboard.pieces[5 + 7 * genForColorIndex]) != 0) // This square is occupied by the defending sides king
                            {
                                if (rayBlocked) // Some other piece is pinned
                                {
                                    pinRayBB |= rayBB;
                                    pinsExistInPosition = true;
                                    break; // Brakes since that ray came to stop before reaching the edge
                                }
                                else // King is blocking the ray so the king is in direct check
                                {
                                    if (inCheck)
                                    {
                                        inDoubleCheck = true;
                                    }
                                    else
                                    {
                                        inCheck = true;
                                        checkingPieceBB = BitboardUtility.GenerateShift(1, pos); // Adds this bishop to the checking pieces bitboard
                                    }                                
                                    checkRayBB |= rayBB;
                                    rayBB |= cRayPoint;
                                }
                            }
                            else
                            {
                                if (rayBlocked)
                                {
                                    break; // Brakes since that ray came to stop before reaching the edge
                                }
                                else
                                {
                                    rayBlocked = true;
                                    rayBB |= cRayPoint;
                                    underBishopAttackBB |= rayBB;                                    
                                }
                            }
                        }
                        else if ((cRayPoint & position.bitboard.pieces[6 + 7 * genForColorIndexInverse]) != 0) // This squared is occupied by the attacking side
                        {
                            if (rayBlocked)
                            {
                                break; // Brakes since that ray came to stop before reaching the edge
                            }
                            else
                            {
                                rayBlocked = true;
                                underBishopAttackBB |= rayBB | cRayPoint;
                                break; // Brakes since that ray came to stop before reaching the edge
                            }                  
                        }
                        else
                        {
                            rayBB |= cRayPoint;
                        }
                    }

                    if (!rayBlocked)
                    {
                        underBishopAttackBB |= rayBB;
                    }
                }
                remainingPieces ^= BitboardUtility.GenerateShift(1, pos); // Removes this bishop from the remaining pieces bitboard
            }
            #endregion

            #region Rook attacks
            // For each opponents rook
            remainingPieces = position.bitboard.pieces[3 + 7 * genForColorIndexInverse]; // Fetches bitboard containing enemy rooks
            while (remainingPieces != 0) // For each opponents rooks
            {
                int pos = BitOps.BitScanForward(remainingPieces); // Gets the position of the next rooks from the bitboard
                for (int i = 0; i < 4; i++) // For each move direction
                {
                    ulong rayBB = 0;
                    bool rayBlocked = false;

                    for (int j = 1; j < Constants.squaresToEdge[i][pos] + 1; j++) // For each square in a given direction until edge is reached
                    {
                        ulong cRayPoint = BitboardUtility.GenerateShift(1, pos + Constants.directionOffsets[i] * j); // The currently investigated point on the ray
                        if ((cRayPoint & position.bitboard.pieces[6 + 7 * genForColorIndex]) != 0) // This squared is occupied by the defending side
                        {
                            if ((cRayPoint & position.bitboard.pieces[5 + 7 * genForColorIndex]) != 0) // This square is occupied by the defending sides king
                            {
                                if (rayBlocked) // Some other piece is pinned
                                {
                                    pinRayBB |= rayBB;
                                    pinsExistInPosition = true;
                                    break; // Brakes since that ray came to stop before reaching the edge
                                }
                                else // King is blocking the ray so the king is in direct check
                                {
                                    if (inCheck)
                                    {
                                        inDoubleCheck = true;
                                    }
                                    else
                                    {
                                        inCheck = true;
                                        checkingPieceBB = BitboardUtility.GenerateShift(1, pos); // Adds this rook to the checking pieces bitboard
                                    }
                                    checkRayBB |= rayBB;
                                    rayBB |= cRayPoint;
                                }
                            }
                            else
                            {
                                if (rayBlocked)
                                {
                                    break; // Brakes since that ray came to stop before reaching the edge
                                }
                                else
                                {
                                    rayBlocked = true;
                                    rayBB |= cRayPoint;
                                    underRookAttackBB |= rayBB;
                                }
                            }
                        }
                        else if ((cRayPoint & position.bitboard.pieces[6 + 7 * genForColorIndexInverse]) != 0) // This squared is occupied by the attacking side
                        {
                            if (rayBlocked)
                            {
                                break; // Brakes since that ray came to stop before reaching the edge
                            }
                            else
                            {
                                rayBlocked = true;
                                underRookAttackBB |= rayBB | cRayPoint;
                                break; // Brakes since that ray came to stop before reaching the edge
                            }
                        }
                        else
                        {
                            rayBB |= cRayPoint;
                        }
                    }

                    if (!rayBlocked)
                    {
                        underRookAttackBB |= rayBB;
                    }
                }
                remainingPieces ^= BitboardUtility.GenerateShift(1, pos); // Removes this rooks from the remaining pieces bitboard
            }
            #endregion

            #region Queen Attacks
            // For each opponents queen
            remainingPieces = position.bitboard.pieces[4 + 7 * genForColorIndexInverse]; // Fetches bitboard containing enemy queen
            while (remainingPieces != 0) // For each opponents queen
            {
                int pos = BitOps.BitScanForward(remainingPieces); // Gets the position of the next queen from the bitboard
                for (int i = 0; i < 8; i++) // For each move direction
                {
                    ulong rayBB = 0;
                    bool rayBlocked = false;

                    for (int j = 1; j < Constants.squaresToEdge[i][pos] + 1; j++) // For each square in a given direction until edge is reached
                    {
                        ulong cRayPoint = BitboardUtility.GenerateShift(1, pos + Constants.directionOffsets[i] * j); // The currently investigated point on the ray
                        if ((cRayPoint & position.bitboard.pieces[6 + 7 * genForColorIndex]) != 0) // This squared is occupied by the defending side
                        {
                            if ((cRayPoint & position.bitboard.pieces[5 + 7 * genForColorIndex]) != 0) // This square is occupied by the defending sides king
                            {
                                if (rayBlocked) // Some other piece is pinned
                                {
                                    pinRayBB |= rayBB;
                                    pinsExistInPosition = true;
                                    break; // Brakes since that ray came to stop before reaching the edge
                                }
                                else // King is blocking the ray so the king is in direct check
                                {
                                    if (inCheck)
                                    {
                                        inDoubleCheck = true;
                                    }
                                    else
                                    {
                                        inCheck = true;
                                        checkingPieceBB = BitboardUtility.GenerateShift(1, pos); // Adds this queen to the checking pieces bitboard
                                    }
                                    checkRayBB |= rayBB;
                                    rayBB |= cRayPoint;
                                }
                            }
                            else
                            {
                                if (rayBlocked)
                                {
                                    break; // Brakes since that ray came to stop before reaching the edge
                                }
                                else
                                {
                                    rayBlocked = true;
                                    rayBB |= cRayPoint;
                                    underQueenAttackBB |= rayBB;
                                }
                            }
                        }
                        else if ((cRayPoint & position.bitboard.pieces[6 + 7 * genForColorIndexInverse]) != 0) // This squared is occupied by the attacking side
                        {
                            if (rayBlocked)
                            {
                                break; // Brakes since that ray came to stop before reaching the edge
                            }
                            else
                            {
                                rayBlocked = true;
                                underQueenAttackBB |= rayBB | cRayPoint;
                                break; // Brakes since that ray came to stop before reaching the edge
                            }
                        }
                        else
                        {
                            rayBB |= cRayPoint;
                        }
                    }

                    if (!rayBlocked)
                    {
                        underQueenAttackBB |= rayBB;
                    }
                }
                remainingPieces ^= BitboardUtility.GenerateShift(1, pos); // Removes this queen from the remaining pieces bitboard
            }
            #endregion

            #region King Attacks
            // For opponents king
            underKingAttackBB = PrecomputedMoveData.kingAttacks[attKingIndex]; // Adds attacks of this knight to the knight attacks bitboard
            #endregion

            // Calculates under attack bitboard
            underAttackBB = underPawnAttackBB | underKnightAttackBB | underBishopAttackBB | underRookAttackBB | underQueenAttackBB | underKingAttackBB;
        }

        // Generates all legal moves in a given position, most be called after CalculateBitBoards
        private void GenerateLegalMoves()
        {
            if (inDoubleCheck) // If the king is in a double check only king moves have potential to be legal
            {
                #region King moves
                // Moves the defended king can make to squares that are not under attack or occupied by allies pieces
                GenMovesForKing(PrecomputedMoveData.kingAttacks[defKingIndex] & ~(underAttackBB | position.bitboard.pieces[6 + 7 * genForColorIndex]));
                #endregion
            }
            else if (pinsExistInPosition)
            {
                if (inCheck)
                {
                    #region Pawn moves
                    remainingPieces = position.bitboard.pieces[0 + 7 * genForColorIndex]; // Bitboard congaing all friendly pawns
                    while (remainingPieces != 0) // For each friendly knight
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This pawn is not pinned
                        {
                            GenMovesForPawnPushesNotPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnPushesWhite[from] : PrecomputedMoveData.pawnPushesBlack[from]) & checkRayBB, from);
                            GenMovesForPawnCaptureNotPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksWhite[from] : PrecomputedMoveData.pawnAttacksBlack[from]) & checkingPieceBB, from);
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Knight moves
                    remainingPieces = position.bitboard.pieces[1 + 7 * genForColorIndex]; // Bitboard containing all friendly knight
                    while (remainingPieces != 0) // For each friendly knight
                    {
                        int from = BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This knight is not pinned
                        {
                            GenMovesForKnightNotPinned(PrecomputedMoveData.knightAttacks[from] & (checkingPieceBB | checkRayBB), (ushort)from);
                        } 
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    ulong permittedMoves = checkingPieceBB | checkRayBB; // Bitboard containing move that will stop to check, used for sliding pieces move generation

                    #region Bishop moves
                    remainingPieces = position.bitboard.pieces[2 + 7 * genForColorIndex]; // Bitboard containing all friendly bishops 
                    while (remainingPieces != 0) // For each bishop
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This bishop is not pinned
                        {
                            if ((PrecomputedMoveData.bishopAttacks[from] & permittedMoves) != 0)
                            {
                                GenMovesForBishopNotPinned(permittedMoves, from);
                            }
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Rook moves
                    remainingPieces = position.bitboard.pieces[3 + 7 * genForColorIndex]; // Bitboard containing all friendly rooks 
                    while (remainingPieces != 0) // For each rook
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This rook is not pinned
                        {
                            if ((PrecomputedMoveData.rookAttacks[from] & permittedMoves) != 0)
                            {
                                GenMovesForRookNotPinned(permittedMoves, from);
                            }
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Queen moves
                    remainingPieces = position.bitboard.pieces[4 + 7 * genForColorIndex]; // Bitboard containing all friendly queens 
                    while (remainingPieces != 0) // For each queen
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This queen is not pinned
                        {
                            if ((PrecomputedMoveData.queenAttacks[from] & permittedMoves) != 0)
                            {
                                GenMovesForQueenNotPinned(permittedMoves, from);
                            }
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region En-peasant
                    if (position.enPassantTargetFile != 8) // If an en-passant capture is possible 
                    {
                        if ((checkingPieceBB & Constants.primitiveBitboards[genForColorIndex == 0 ? 32 + position.enPassantTargetFile : 24 + position.enPassantTargetFile]) != 0)
                        {
                            ulong eligiblePawns = (position.bitboard.pieces[0 + 7 * genForColorIndex] & (genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksBlack[40 + position.enPassantTargetFile] : PrecomputedMoveData.pawnAttacksWhite[16 + position.enPassantTargetFile])) & ~pinRayBB; // Only pawns from this bit board can perform this en-peasant capture
                            if (eligiblePawns != 0) // There is at lest one pawn that can perform this en-passant capture
                            {
                                GenEnPassant(eligiblePawns);
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Pawn moves
                    remainingPieces = position.bitboard.pieces[0 + 7 * genForColorIndex]; // Bitboard congaing all friendly pawns
                    while (remainingPieces != 0) // For each friendly knight
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This pawn is not pinned
                        {
                            GenMovesForPawnPushesNotPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnPushesWhite[from] : PrecomputedMoveData.pawnPushesBlack[from]) & ~(position.bitboard.pieces[6] | position.bitboard.pieces[13]), from);
                            GenMovesForPawnCaptureNotPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksWhite[from] : PrecomputedMoveData.pawnAttacksBlack[from]) & position.bitboard.pieces[6 + 7 * genForColorIndexInverse], from);
                        }
                        else // This pawn is pinned
                        {
                            byte pinDir = GetPinDirection(from);
                            GenMovesForPawnPushesPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnPushesWhite[from] : PrecomputedMoveData.pawnPushesBlack[from]) & ~(position.bitboard.pieces[6] | position.bitboard.pieces[13]), from, pinDir);
                            GenMovesForPawnCapturePinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksWhite[from] : PrecomputedMoveData.pawnAttacksBlack[from]) & position.bitboard.pieces[6 + 7 * genForColorIndexInverse], from, pinDir);
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Knight moves
                    remainingPieces = position.bitboard.pieces[1 + 7 * genForColorIndex]; // Bitboard containing all friendly knight
                    while (remainingPieces != 0) // For each friendly knight
                    {
                        int from = BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This knight is not pinned
                        {
                            GenMovesForKnightNotPinned(PrecomputedMoveData.knightAttacks[from] & ~position.bitboard.pieces[6 + 7 * genForColorIndex], (ushort)from);
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    ulong permittedMoves = ~position.bitboard.pieces[6 + genForColorIndex]; // Bitboard containing move that will stop to check, used for sliding pieces move generation

                    #region Bishop moves
                    remainingPieces = position.bitboard.pieces[2 + 7 * genForColorIndex]; // Bitboard containing all friendly bishops 
                    while (remainingPieces != 0) // For each bishop
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This bishop is not pinned
                        {
                            if ((PrecomputedMoveData.bishopAttacks[from] & permittedMoves) != 0)
                            {
                                GenMovesForBishopNotPinned(permittedMoves, from);
                            }
                        }
                        else // This bishop is pinned
                        {
                            if ((PrecomputedMoveData.bishopAttacks[from] & permittedMoves) != 0)
                            {                                
                                GenMovesForBishopPinned(permittedMoves, from);
                            }
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Rook moves
                    remainingPieces = position.bitboard.pieces[3 + 7 * genForColorIndex]; // Bitboard containing all friendly rooks 
                    while (remainingPieces != 0) // For each rook
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This rook is not pinned
                        {
                            if ((PrecomputedMoveData.rookAttacks[from] & permittedMoves) != 0)
                            {
                                GenMovesForRookNotPinned(permittedMoves, from);
                            }
                        }
                        else // This rook is pinned
                        {
                            if ((PrecomputedMoveData.rookAttacks[from] & permittedMoves) != 0)
                            {
                                GenMovesForRookPinned(permittedMoves, from);
                            }
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Queen moves
                    remainingPieces = position.bitboard.pieces[4 + 7 * genForColorIndex]; // Bitboard containing all friendly queens 
                    while (remainingPieces != 0) // For each queen
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((Constants.primitiveBitboards[from] & pinRayBB) == 0) // This queen is not pinned
                        {
                            if ((PrecomputedMoveData.queenAttacks[from] & permittedMoves) != 0)
                            {
                                GenMovesForQueenNotPinned(permittedMoves, from);
                            }
                        }
                        else // This queen is pinned
                        {
                            if ((PrecomputedMoveData.queenAttacks[from] & permittedMoves) != 0)
                            {
                                GenMovesForQueenPinned(permittedMoves, from);
                            }
                        }
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region En-peasant
                    if (position.enPassantTargetFile != 8) // If an en-passant capture is possible 
                    {
                        ulong eligiblePawnsNotPinned = (position.bitboard.pieces[0 + 7 * genForColorIndex] & (genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksBlack[40 + position.enPassantTargetFile] : PrecomputedMoveData.pawnAttacksWhite[16 + position.enPassantTargetFile])) & ~pinRayBB; // Only pawns from this bit board can perform this en-peasant capture and are not pinned
                        ulong eligiblePawnPinned = (position.bitboard.pieces[0 + 7 * genForColorIndex] & (genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksBlack[40 + position.enPassantTargetFile] : PrecomputedMoveData.pawnAttacksWhite[16 + position.enPassantTargetFile])) & pinRayBB; // Only pawns from this bit board can perform this en-peasant capture and are pinned
                        if (eligiblePawnsNotPinned != 0) // There is at lest one pawn that is not pinned and can perform this en-passant capture
                        {
                            GenEnPassant(eligiblePawnsNotPinned);
                        }
                        if (eligiblePawnPinned != 0) // There is at least one pawn that is pinned and can perform this en-passant capture
                        {
                            GenEnPassant(eligiblePawnPinned);
                        }
                    }
                    #endregion

                    #region Castling
                    GenMovesForCastling();
                    #endregion
                }

                #region King moves
                // Moves the defended king can make to squares that are not under attack or occupied by allies pieces
                GenMovesForKing(PrecomputedMoveData.kingAttacks[defKingIndex] & ~(underAttackBB | position.bitboard.pieces[6 + 7 * genForColorIndex]));
                #endregion
            }
            else
            {
                if (inCheck) // Only king, capturing checking piece or intercepting check ray moves have potential to be legal
                {
                    #region Pawn moves (en-peasant is handled later)
                    remainingPieces = position.bitboard.pieces[0 + 7 * genForColorIndex]; // Bitboard congaing all friendly pawns
                    while (remainingPieces != 0) // For each friendly knight
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        GenMovesForPawnPushesNotPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnPushesWhite[from] : PrecomputedMoveData.pawnPushesBlack[from]) & checkRayBB, from);
                        GenMovesForPawnCaptureNotPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksWhite[from] : PrecomputedMoveData.pawnAttacksBlack[from]) & checkingPieceBB, from);
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Knight move
                    remainingPieces = position.bitboard.pieces[1 + 7 * genForColorIndex]; // Bitboard containing all friendly knight
                    while (remainingPieces != 0) // For each friendly knight
                    {
                        int from = BitOps.BitScanForward(remainingPieces);
                        GenMovesForKnightNotPinned(PrecomputedMoveData.knightAttacks[from] & (checkingPieceBB | checkRayBB), (ushort)from);
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    ulong permittedMoves = checkingPieceBB | checkRayBB; // Bitboard containing move that will stop to check, used for sliding pieces move generation
                    
                    #region Bishop moves
                    remainingPieces = position.bitboard.pieces[2 + 7 * genForColorIndex]; // Bitboard containing all friendly bishops 
                    while (remainingPieces != 0) // For each bishop
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((PrecomputedMoveData.bishopAttacks[from] & permittedMoves) != 0)
                        {
                            GenMovesForBishopNotPinned(permittedMoves, from);
                        }                        
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Rook moves
                    remainingPieces = position.bitboard.pieces[3 + 7 * genForColorIndex]; // Bitboard containing all friendly rooks 
                    while (remainingPieces != 0) // For each rook
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((PrecomputedMoveData.rookAttacks[from] & permittedMoves) != 0)
                        {
                            GenMovesForRookNotPinned(permittedMoves, from);
                        }                  
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Queen moves
                    remainingPieces = position.bitboard.pieces[4 + 7 * genForColorIndex]; // Bitboard containing all friendly queens 
                    while (remainingPieces != 0) // For each queen
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        if ((PrecomputedMoveData.queenAttacks[from] & permittedMoves) != 0)
                        {
                            GenMovesForQueenNotPinned(permittedMoves, from);
                        }              
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region En-peasant
                    if (position.enPassantTargetFile != 8) // If an en-passant capture is possible 
                    {
                        if ((checkingPieceBB & Constants.primitiveBitboards[genForColorIndex == 0 ? 32 + position.enPassantTargetFile : 24 + position.enPassantTargetFile]) != 0)
                        {
                            ulong eligiblePawns = position.bitboard.pieces[0 + 7 * genForColorIndex] & (genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksBlack[40 + position.enPassantTargetFile] : PrecomputedMoveData.pawnAttacksWhite[16 + position.enPassantTargetFile]); // Only pawns from this bit board can perform this en-peasant capture
                            if (eligiblePawns != 0) // There is at lest one pawn that can perform this en-passant capture
                            {
                                GenEnPassant(eligiblePawns);
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Pawn moves (en-peasant is handled later)
                    remainingPieces = position.bitboard.pieces[0 + 7 * genForColorIndex]; // Bitboard congaing all friendly pawns
                    while (remainingPieces != 0) // For each friendly knight
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        GenMovesForPawnPushesNotPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnPushesWhite[from] : PrecomputedMoveData.pawnPushesBlack[from]) & ~(position.bitboard.pieces[6] | position.bitboard.pieces[13]), from);
                        GenMovesForPawnCaptureNotPinned((genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksWhite[from] : PrecomputedMoveData.pawnAttacksBlack[from]) & position.bitboard.pieces[6 + 7 * genForColorIndexInverse], from);
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Knight move
                    remainingPieces = position.bitboard.pieces[1 + 7 * genForColorIndex]; // Bitboard containing all friendly knight
                    while (remainingPieces != 0) // For each friendly knight
                    {
                        int from = BitOps.BitScanForward(remainingPieces);
                        GenMovesForKnightNotPinned(PrecomputedMoveData.knightAttacks[from] & ~position.bitboard.pieces[6 + 7 * genForColorIndex], (ushort)from);
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    ulong permittedMoves = ~position.bitboard.pieces[6 + genForColorIndex]; // Bitboard containing move that will stop to check, used for sliding pieces move generation

                    #region Bishop moves
                    remainingPieces = position.bitboard.pieces[2 + 7 * genForColorIndex]; // Bitboard containing all friendly bishops 
                    while (remainingPieces != 0) // For each bishop
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        GenMovesForBishopNotPinned(permittedMoves, from);
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Rook moves
                    remainingPieces = position.bitboard.pieces[3 + 7 * genForColorIndex]; // Bitboard containing all friendly rooks 
                    while (remainingPieces != 0) // For each rook
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        GenMovesForRookNotPinned(permittedMoves, from);
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region Queen moves
                    remainingPieces = position.bitboard.pieces[4 + 7 * genForColorIndex]; // Bitboard containing all friendly queens 
                    while (remainingPieces != 0) // For each queen
                    {
                        ushort from = (ushort)BitOps.BitScanForward(remainingPieces);
                        GenMovesForQueenNotPinned(permittedMoves, from);
                        remainingPieces ^= Constants.primitiveBitboards[from];
                    }
                    #endregion

                    #region En-peasant
                    if (position.enPassantTargetFile != 8) // If an en-passant capture is possible 
                    {
                        ulong eligiblePawns = position.bitboard.pieces[0 + 7 * genForColorIndex] & (genForColorIndex == 0 ? PrecomputedMoveData.pawnAttacksBlack[40 + position.enPassantTargetFile] : PrecomputedMoveData.pawnAttacksWhite[16 + position.enPassantTargetFile]); // Only pawns from this bit board can perform this en-peasant capture
                        if (eligiblePawns != 0) // There is at lest one pawn that can perform this en-passant capture
                        {
                            GenEnPassant(eligiblePawns);
                        }
                    }
                    #endregion

                    #region Castling
                    GenMovesForCastling();
                    #endregion
                }

                #region King moves
                // Moves the defended king can make to squares that are not under attack or occupied by allies pieces
                GenMovesForKing(PrecomputedMoveData.kingAttacks[defKingIndex] & ~(underAttackBB | position.bitboard.pieces[6 + 7 * genForColorIndex]));
                #endregion
            }
        }

        #region Move generation for not pinned pieces

        private void GenMovesForPawnPushesNotPinned(ulong remainingMove, ushort from)
        {
            while (remainingMove != 0)
            {
                ushort to = (ushort)BitOps.BitScanForward(remainingMove);
                ulong toBB = Constants.primitiveBitboards[to];
                if (Mathf.Abs(to - from) == 16) // Its a double pawn push
                {
                    if (position.squareCentric.pieces[from + 8 - 16 * genForColorIndex] == (byte)SquareCentric.PieceType.Empty)
                    {
                        legalMoves.Add(Move.GenMove(from, to, Move.Flag.doublePawnPush));
                    }
                }
                else
                {
                    if ((promotionRankMask & toBB) == 0)
                    {
                        legalMoves.Add(Move.GenMove(from, to, Move.Flag.knightPromotion));
                        legalMoves.Add(Move.GenMove(from, to, Move.Flag.bishopPromotion));
                        legalMoves.Add(Move.GenMove(from, to, Move.Flag.rookPromotion));
                        legalMoves.Add(Move.GenMove(from, to, Move.Flag.queenPromotion));
                    }
                    else
                    {
                        legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                    }
                }
                remainingMove ^= toBB;
            }
        }

        private void GenMovesForPawnCaptureNotPinned(ulong remainingMove, ushort from)
        {
            while (remainingMove != 0)
            {
                ushort to = (ushort)BitOps.BitScanForward(remainingMove);
                ulong toBB = BitboardUtility.GenerateShift(1, to);
                if ((promotionRankMask & toBB) == 0)
                {
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.knightPromotionCapture));
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.bishopPromotionCapture));
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.rookPromotionCapture));
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.queenPromotionCapture));
                }
                else
                {
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                }
                remainingMove ^= toBB;
            }
        }

        private void GenMovesForKnightNotPinned(ulong remainingMove, ushort from)
        {
            while (remainingMove != 0)
            {
                ushort to = (ushort)BitOps.BitScanForward(remainingMove);
                if ((Constants.primitiveBitboards[to] & position.bitboard.pieces[6 + 7 * genForColorIndexInverse]) == 0) // This knight move is not capering an opponent piece
                {
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                }
                else // This knight move is capturing an opponents piece
                {
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                }
                remainingMove ^= Constants.primitiveBitboards[to]; // Removes this move form remaining moves bitboard
            }
        }

        private void GenMovesForBishopNotPinned(ulong permittedMoves, ushort from)
        {
            for (int i = 4; i < 8; i++)
            {
                for (int j = 1; j < Constants.squaresToEdge[i][from] + 1; j++)
                {
                    ushort to = (ushort)(from + j * Constants.directionOffsets[i]);
                    if (position.squareCentric.colors[to] == (byte)SquareCentric.SquareColor.Empty)
                    {
                        if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                        {
                            legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                        }
                    }
                    else if (position.squareCentric.colors[to] == genForColorIndexInverse)
                    {
                        if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                        {
                            legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                        }
                        break; // Ends this ray since it is blacked by opponents piece
                    }
                    else
                    {
                        break; // Ends this ray since it is blacked by a friendly piece
                    }
                }
            }
        }

        private void GenMovesForRookNotPinned(ulong permittedMoves, ushort from)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < Constants.squaresToEdge[i][from] + 1; j++)
                {
                    ushort to = (ushort)(from + j * Constants.directionOffsets[i]);
                    if (position.squareCentric.colors[to] == (byte)SquareCentric.SquareColor.Empty)
                    {
                        if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                        {
                            legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                        }
                    }
                    else if (position.squareCentric.colors[to] == genForColorIndexInverse)
                    {
                        if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                        {
                            legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                        }
                        break; // Ends this ray since it is blacked by opponents piece
                    }
                    else
                    {
                        break; // Ends this ray since it is blacked by a friendly piece
                    }
                }
            }
        }

        private void GenMovesForQueenNotPinned(ulong permittedMoves, ushort from)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 1; j < Constants.squaresToEdge[i][from] + 1; j++)
                {
                    ushort to = (ushort)(from + j * Constants.directionOffsets[i]);
                    if (position.squareCentric.colors[to] == (byte)SquareCentric.SquareColor.Empty)
                    {
                        if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                        {
                            legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                        }
                    }
                    else if (position.squareCentric.colors[to] == genForColorIndexInverse)
                    {
                        if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                        {
                            legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                        }
                        break; // Ends this ray since it is blacked by opponents piece
                    }
                    else
                    {
                        break; // Ends this ray since it is blacked by a friendly piece
                    }
                }
            }
        }

        #endregion

        #region Move generation for pinned pieces

        private void GenMovesForPawnPushesPinned(ulong remainingMove, ushort from, byte pinDir)
        {
            if (pinDir == 8)
            {
                while (remainingMove != 0)
                {
                    ushort to = (ushort)BitOps.BitScanForward(remainingMove);
                    ulong toBB = BitboardUtility.GenerateShift(1, to);
                    if (Mathf.Abs(to - from) == 16) // Its a double pawn push
                    {
                        if (position.squareCentric.pieces[from + 8 - 16 * genForColorIndex] == (byte)SquareCentric.PieceType.Empty)
                        {
                            legalMoves.Add(Move.GenMove(from, to, Move.Flag.doublePawnPush));
                        }
                    }
                    else // Its impossible to push a pawn to promotion if its pinned, so there is no need to check for it
                    {
                        legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                    }
                    remainingMove ^= toBB;
                }
            }
        }

        private void GenMovesForPawnCapturePinned(ulong remainingMove, ushort from, byte pinDir)
        {

            if (pinDir == 7)
            {
                remainingMove &= Constants.primitiveBitboards[from + 7 - (14 * genForColorIndex)];
            }
            else if (pinDir == 9)
            {
                remainingMove &= Constants.primitiveBitboards[from + 9 - (18 * genForColorIndex)];
            }
            else
            {
                remainingMove = 0;
            }

            while (remainingMove != 0)
            {
                ushort to = (ushort)BitOps.BitScanForward(remainingMove);
                ulong toBB = BitboardUtility.GenerateShift(1, to);
                if ((promotionRankMask & toBB) == 0)
                {
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.knightPromotionCapture));
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.bishopPromotionCapture));
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.rookPromotionCapture));
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.queenPromotionCapture));
                }
                else
                {
                    legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                }
                remainingMove ^= toBB;
            }
        }

        private void GenMovesForBishopPinned(ulong permittedMoves, ushort from)
        {
            byte pinDir = GetPinDirection(from); // Gets the absolute value of a pin direction
            for (int i = 4; i < 8; i++)
            {
                if (Mathf.Abs(Constants.directionOffsets[i]) == pinDir) // This direction will slide this bishop along this pin direction
                {
                    for (int j = 1; j < Constants.squaresToEdge[i][from] + 1; j++)
                    {
                        ushort to = (ushort)(from + j * Constants.directionOffsets[i]);
                        if (position.squareCentric.colors[to] == (byte)SquareCentric.SquareColor.Empty)
                        {
                            if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                            {
                                legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                            }
                        }
                        else if (position.squareCentric.colors[to] == genForColorIndexInverse)
                        {
                            if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                            {
                                legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                            }
                            break; // Ends this ray since it is blacked by opponents piece
                        }
                        else
                        {
                            break; // Ends this ray since it is blacked by a friendly piece
                        }
                    }
                }
            }
        }

        private void GenMovesForRookPinned(ulong permittedMoves, ushort from)
        {
            byte pinDir = GetPinDirection(from); // Gets the absolute value of a pin direction
            for (int i = 0; i < 4; i++)
            {
                if (Mathf.Abs(Constants.directionOffsets[i]) == pinDir) // This direction will slide this rook along this pin direction
                {
                    for (int j = 1; j < Constants.squaresToEdge[i][from] + 1; j++)
                    {
                        ushort to = (ushort)(from + j * Constants.directionOffsets[i]);
                        if (position.squareCentric.colors[to] == (byte)SquareCentric.SquareColor.Empty)
                        {
                            if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                            {
                                legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                            }
                        }
                        else if (position.squareCentric.colors[to] == genForColorIndexInverse)
                        {
                            if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                            {
                                legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                            }
                            break; // Ends this ray since it is blacked by opponents piece
                        }
                        else
                        {
                            break; // Ends this ray since it is blacked by a friendly piece
                        }
                    }
                }
            }
        }

        private void GenMovesForQueenPinned(ulong permittedMoves, ushort from)
        {
            byte pinDir = GetPinDirection(from); // Gets the absolute value of a pin direction
            for (int i = 0; i < 8; i++)
            {
                if (Mathf.Abs(Constants.directionOffsets[i]) == pinDir) // This direction will slide this queen along this pin direction
                {
                    for (int j = 1; j < Constants.squaresToEdge[i][from] + 1; j++)
                    {
                        ushort to = (ushort)(from + j * Constants.directionOffsets[i]);
                        if (position.squareCentric.colors[to] == (byte)SquareCentric.SquareColor.Empty)
                        {
                            if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                            {
                                legalMoves.Add(Move.GenMove(from, to, Move.Flag.quietMove));
                            }
                        }
                        else if (position.squareCentric.colors[to] == genForColorIndexInverse)
                        {
                            if ((permittedMoves & Constants.primitiveBitboards[to]) != 0)
                            {
                                legalMoves.Add(Move.GenMove(from, to, Move.Flag.capture));
                            }
                            break; // Ends this ray since it is blacked by opponents piece
                        }
                        else
                        {
                            break; // Ends this ray since it is blacked by a friendly piece
                        }
                    }
                }
            }
        }

        #endregion

        #region En-Passant move generation

        private void GenEnPassant(ulong eligiblePawns)
        {
            // Generates the en-passant move if its legal
            while (eligiblePawns != 0)
            {
                ushort from = (ushort)BitOps.BitScanForward(eligiblePawns);
                GenSingleEnPassant(from);
                eligiblePawns ^= Constants.primitiveBitboards[from];
            }
        }

        private void GenSingleEnPassant(ushort from)
        {
            // Checks if the en passant move is legal
            ushort to = (ushort)(genForColorIndex == 0 ? 40 + position.enPassantTargetFile : 16 + position.enPassantTargetFile); // The destination of the pawn performing the ep-capture
            ulong afterEnPassantBlockers = (position.bitboard.pieces[6] | position.bitboard.pieces[13]) ^ (Constants.primitiveBitboards[from] | Constants.primitiveBitboards[genForColorIndex == 0 ? 32 + position.enPassantTargetFile : 24 + position.enPassantTargetFile] | Constants.primitiveBitboards[to]); // State of the blockers after en-passant capture
            for (int i = 0; i < 4; i++) // Check for orthogonal exposer checks
            {
                for (int j = 1; j < Constants.squaresToEdge[i][defKingIndex] + 1; j++) // For each square to edge
                {
                    ulong rayPoint = Constants.primitiveBitboards[defKingIndex + j * Constants.directionOffsets[i]]; // Currently checked point on the ray
                    if ((afterEnPassantBlockers & rayPoint) != 0) // The check ray has been blacked
                    {
                        if (((position.bitboard.pieces[3 + 7 * genForColorIndexInverse] | position.bitboard.pieces[4 + 7 * genForColorIndexInverse]) & rayPoint) == 0)
                        {
                            break; // The ray is blacked by a non-checking piece
                        }
                        else
                        {
                            return; // The ray is blacked by a checking piece
                        }
                    }
                }
            }
            for (int i = 4; i < 8; i++) // Check for diagonal exposer checks
            {
                for (int j = 1; j < Constants.squaresToEdge[i][defKingIndex] + 1; j++) // For each square to edge
                {
                    ulong rayPoint = Constants.primitiveBitboards[defKingIndex + j * Constants.directionOffsets[i]]; // Currently checked point on the ray
                    if ((afterEnPassantBlockers & rayPoint) != 0) // The check ray has been blacked
                    {
                        if (((position.bitboard.pieces[2 + 7 * genForColorIndexInverse] | position.bitboard.pieces[4 + 7 * genForColorIndexInverse]) & rayPoint) == 0)
                        {
                            break; // The ray is blacked by a non-checking piece
                        }
                        else
                        {
                            return; // The ray is blacked by a checking piece
                        }
                    }
                }
            }

            // Adds the move
            legalMoves.Add(Move.GenMove(from, to, Move.Flag.epCapture));
        }

        #endregion


        #region Move generation for independent pieces (king, castling)

        // King cant be pinned so only one implementation of this function is required
        private void GenMovesForKing(ulong remainingMove)
        {
            while (remainingMove != 0)
            {
                ushort to = (ushort)(BitOps.BitScanForward(remainingMove));
                if ((Constants.primitiveBitboards[to] & position.bitboard.pieces[6 + 7 * genForColorIndexInverse]) == 0) // This king move is not capering an opponent piece
                {
                    legalMoves.Add(Move.GenMove(defKingIndex, to, Move.Flag.quietMove));
                }
                else // This king move is capturing an opponents piece
                {
                    legalMoves.Add(Move.GenMove(defKingIndex, to, Move.Flag.capture));
                }
                remainingMove ^= Constants.primitiveBitboards[to]; // Removes this move form remaining moves bitboard
            }
        }

        // Pins don't effect castling (int this implementation)
        private void GenMovesForCastling()
        {
            if (position.castlingRights != 0)
            {
                if (genForColorIndex == 0)
                {
                    if ((position.castlingRights & 0b1000) != 0) // White queen side
                    {
                        if (((Constants.castleNotAttackedMask[0] & underAttackBB) == 0) && ((Constants.castleEmptyMasks[0] & (position.bitboard.pieces[6] | position.bitboard.pieces[13])) == 0))
                        {
                            legalMoves.Add(Move.GenMove(4, 2, Move.Flag.queenCastle));
                        }
                    }
                    if ((position.castlingRights & 0b0100) != 0) // White king side
                    {
                        if (((Constants.castleNotAttackedMask[1] & underAttackBB) == 0) && ((Constants.castleEmptyMasks[1] & (position.bitboard.pieces[6] | position.bitboard.pieces[13])) == 0))
                        {
                            legalMoves.Add(Move.GenMove(4, 6, Move.Flag.kingCastle));
                        }
                    }
                }
                else
                {
                    if ((position.castlingRights & 0b0010) != 0) // Black queen side
                    {
                        if (((Constants.castleNotAttackedMask[2] & underAttackBB) == 0) && ((Constants.castleEmptyMasks[2] & (position.bitboard.pieces[6] | position.bitboard.pieces[13])) == 0))
                        {
                            legalMoves.Add(Move.GenMove(60, 58, Move.Flag.queenCastle));
                        }
                    }
                    if ((position.castlingRights & 0b0001) != 0) // Black king side
                    {
                        if (((Constants.castleNotAttackedMask[3] & underAttackBB) == 0) && ((Constants.castleEmptyMasks[3] & (position.bitboard.pieces[6] | position.bitboard.pieces[13])) == 0))
                        {
                            legalMoves.Add(Move.GenMove(60, 62, Move.Flag.kingCastle));
                        }
                    }
                }
            }
        }

        #endregion

        #region Gin direction

        // Returns the absolute value of a pin directions relative to the friendly king
        private byte GetPinDirection(ushort pinnedPieceIndex)
        {
            if (Constants.indexToRankFileTable[pinnedPieceIndex][0] == Constants.indexToRankFileTable[defKingIndex][0]) // Same file, vertical pin
            {
                return 8; // Returns the pin direction offset
            }
            else if (Constants.indexToRankFileTable[pinnedPieceIndex][1] == Constants.indexToRankFileTable[defKingIndex][1]) // Same rank, horizontal pin
            {
                return 1; // Returns the pin direction offset
            }
            else if ((Constants.indexToRankFileTable[pinnedPieceIndex][0] - Constants.indexToRankFileTable[defKingIndex][0]) == 
                      Constants.indexToRankFileTable[pinnedPieceIndex][1] - Constants.indexToRankFileTable[defKingIndex][1]) // Diagonal pin with positive gradient
            {
                return 9; // Returns the pin direction offset
            }
            else // Diagonal pin with negative gradient
            {
                return 7; // Returns the pin direction offset
            }
        }

        // Returns the absolute value of a pin directions relative to pinnedToIndex
        private byte GetPinDirection(ushort pinnedPieceIndex, ushort pinnedToIndex)
        {
            if (Constants.indexToRankFileTable[pinnedPieceIndex][0] == Constants.indexToRankFileTable[pinnedToIndex][0]) // Same file, vertical pin
            {
                return 8; // Returns the pin direction offset
            }
            else if (Constants.indexToRankFileTable[pinnedPieceIndex][1] == Constants.indexToRankFileTable[pinnedToIndex][1]) // Same rank, horizontal pin
            {
                return 1; // Returns the pin direction offset
            }
            else if ((Constants.indexToRankFileTable[pinnedPieceIndex][0] - Constants.indexToRankFileTable[pinnedToIndex][0]) ==
                      Constants.indexToRankFileTable[pinnedPieceIndex][1] - Constants.indexToRankFileTable[pinnedToIndex][1]) // Diagonal pin with positive gradient
            {
                return 9; // Returns the pin direction offset
            }
            else // Diagonal pin with negative gradient
            {
                return 7; // Returns the pin direction offset
            }
        }

        #endregion

        #endregion

        #region Square under attack

        // Returns true if the square if under attack
        public bool SquareAttackedBy(Position position, byte genForColorIndex, byte square)
        {
            InitSearch(position, genForColorIndex, true, true);
            CalculateBitBoards();
            return (underAttackBB & Constants.primitiveBitboards[square]) != 0;
        }

        #endregion
    }
}

