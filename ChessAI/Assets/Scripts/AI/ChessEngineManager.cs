using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.UI;
using Chess.EngineUtility;

namespace Chess.Engine
{
    public class ChessEngineManager : MonoBehaviour
    {
        // Class variables
        #region

        public ChessEngine chessEngine;
        public BoardInputManager inputManager;

        #endregion

        // Class Initialization
        #region Initialization

        // Awake is called before Start
        void Awake()
        {
            chessEngine = new ChessEngine();
        }

        #endregion

        // Class utilities
        #region Utilities

        // Returns a list of all legal moves
        public List<ushort> GetLegalMoves()
        {
            return chessEngine.moveGenerator.GenerateLegalMoves(chessEngine.centralPosition, (byte)(chessEngine.centralPosition.sideToMove ? 0 : 1));
        }

        // Makes a move (it could be a human or AI chosen move)
        public void MakeMove(ushort move)
        {
            chessEngine.centralPosition.MakeMove(move);

            #region TESTING
            DynamicEvolution dynamicEvolution = new DynamicEvolution();
            FindObjectOfType<TMPro.TextMeshProUGUI>().text = $"Eval: {dynamicEvolution.Evaluate(chessEngine.centralPosition, 2)}";
            #endregion
        }

        // Makes an AI generated move
        public void MakeAIMove(bool random = false)
        {
            if (random)
            {
                // Makes random AI move
                MakeRandomAIMove();
            }
            else
            {
                // Makes calculated AI move
                MakeCalculatedAIMove();
            }
        }

        // Control for the chess AI
        private void MakeRandomAIMove()
        {
            // Generates all legal moves
            List<ushort> moves = chessEngine.GenerateLegalMoves((byte)(chessEngine.centralPosition.sideToMove ? 0 : 1));
            // Choses a random move
            ushort move = moves[Random.Range(0, moves.Count - 1)];
            // Plays the move
            MakeMove(move);
            inputManager.MakeAIMove(Move.GetFrom(move), Move.GetTo(move));
        }

        // Makes a calculated move
        private void MakeCalculatedAIMove()
        {
            // Calculates the best move
            ushort bestMove = chessEngine.CalculateBestMove();

            // Plays the move that the AI thinks is best
            MakeMove(bestMove);
            inputManager.MakeAIMove(Move.GetFrom(bestMove), Move.GetTo(bestMove));
        }

        #endregion
    }
}
