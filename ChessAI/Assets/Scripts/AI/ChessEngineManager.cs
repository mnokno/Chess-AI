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

        // Start is called before the first frame update  
        void Start()
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
            FindObjectOfType<TMPro.TextMeshProUGUI>().text = $"Eval: {dynamicEvolution.Evaluate(chessEngine.centralPosition, 1)}";

            #endregion
        }

        public void MakeAIMove()
        {
            Invoke("MakeAIMoveR", 1f);
        }

        public void MakeAIMoveR()
        {
            // Generates all legal moves
            List<ushort> moves = chessEngine.GenerateLegalMoves((byte)(chessEngine.centralPosition.sideToMove ? 0 : 1));
            // Choses a random move
            ushort move = moves[Random.Range(0, moves.Count - 1)];

            MakeMove(move);
            inputManager.MakeAIMove(Move.GetFrom(move), Move.GetTo(move));
        }

        #endregion
    }
}
