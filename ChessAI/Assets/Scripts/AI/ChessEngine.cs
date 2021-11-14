using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.Engine
{
    public class ChessEngine
    {
        // Class variables
        #region Class variables

        public Position centralPosition;
        public MoveGenerator moveGenerator;

        #endregion

        // Class initialization
        #region Initialization

        public ChessEngine()
        {
            centralPosition = new Position();
            moveGenerator = new MoveGenerator();
        }

        #endregion

        // Class utilities
        #region Utilities

        public List<ushort> GenerateLegalMoves(byte genForColorIndex, bool includeQuietMoves = true, bool includeChecks = true)
        {
            return moveGenerator.GenerateLegalMoves(centralPosition, genForColorIndex, includeQuietMoves : includeQuietMoves, includeChecks : includeChecks);
        }

        #endregion
    }
}

