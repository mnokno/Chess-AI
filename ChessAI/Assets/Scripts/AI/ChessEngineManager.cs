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

        public List<ushort> GetLegalMoves()
        {
            return chessEngine.moveGenerator.GenerateLegalMoves(chessEngine.centralPosition, (byte)(chessEngine.centralPosition.sideToMove ? 0 : 1));
        }

        public void MakeMove(ushort move)
        {
            chessEngine.centralPosition.MakeMove(move);

            #region TESTING
            DynamicEvolution dynamicEvolution = new DynamicEvolution();
            FindObjectOfType<TMPro.TextMeshProUGUI>().text = $"Eval: {dynamicEvolution.Evaluate(chessEngine.centralPosition, 2)}";
            #endregion
        }

        public void MakeAIMove()
        {
            MakeCalculatedAIMove();
            //Invoke("MakeRandomAIMove", 1f);
        }

        public void MakeRandomAIMove()
        {
            // Generates all legal moves
            List<ushort> moves = chessEngine.GenerateLegalMoves((byte)(chessEngine.centralPosition.sideToMove ? 0 : 1));
            // Choses a random move
            ushort move = moves[Random.Range(0, moves.Count - 1)];

            MakeMove(move);
            inputManager.MakeAIMove(Move.GetFrom(move), Move.GetTo(move));
        }

        public void MakeCalculatedAIMove()
        {
            // Generates all legal moves
            List<ushort> moves = chessEngine.GenerateLegalMoves((byte)(chessEngine.centralPosition.sideToMove ? 0 : 1));
            // Creates variables to keep track of the best move
            ushort bestMove = moves[0];
            int bestScore = int.MaxValue;

            foreach(ushort move in moves)
            {
                chessEngine.centralPosition.MakeMove(move);
                DynamicEvolution dynamicEvolution = new DynamicEvolution();
                int score = dynamicEvolution.Evaluate(chessEngine.centralPosition, 3);
                if (score < bestScore)
                {
                    bestMove = move;
                    bestScore = score;
                }
                chessEngine.centralPosition.UnmakeMove(move);
            }

            // Plays the move that the AI thinks is best
            MakeMove(bestMove);
            inputManager.MakeAIMove(Move.GetFrom(bestMove), Move.GetTo(bestMove));
        }

        #endregion
    }
}
