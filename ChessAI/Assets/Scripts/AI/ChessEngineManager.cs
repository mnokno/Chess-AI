using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.UI;
using Chess.EngineUtility;
using System.Threading.Tasks;

namespace Chess.Engine
{
    public class ChessEngineManager : MonoBehaviour
    {
        // Class variables
        #region

        // Chess engine manager connections references
        public ChessEngine chessEngine;
        public BoardInputManager inputManager;

        // Info labels
        public TMPro.TextMeshProUGUI evlaText;
        public TMPro.TextMeshProUGUI nodesText;
        public TMPro.TextMeshProUGUI timeText;
        public TMPro.TextMeshProUGUI baseDepthText;
        public TMPro.TextMeshProUGUI maxDepthText;
        public TMPro.TextMeshProUGUI moveText;
        public TMPro.TextMeshProUGUI gameStateText;
        public TMPro.TextMeshProUGUI transpositiontableHitsText;
        public TMPro.TextMeshProUGUI zobristHashText;
        public TMPro.TextMeshProUGUI FENText;

        // Used to stop updating info on the labels
        public bool updateLables = true;

        // Used to play an AI move once its calculated
        public bool calculated = false;
        public ushort moveToPlay = 0;

        #endregion

        // Class Initialization
        #region Initialization

        // Awake is called before Start
        void Awake()
        {
            chessEngine = new ChessEngine();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Starts coroutines
            StartCoroutine("UpdateInfo");
            StartCoroutine("CheckForAIMove");
            // Updates display info
            zobristHashText.text = $"Zobrist Hash: {System.Convert.ToString((long)chessEngine.centralPosition.zobristKey, 2)}";
            // Updates FEN info
            FENText.text = $"FEN: {chessEngine.centralPosition.GetFEN()}";
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
            // Makes the move
            chessEngine.centralPosition.MakeMove(move);
            // Updates Zobrist Hash
            zobristHashText.text = $"Zobrist Hash: {System.Convert.ToString((long)chessEngine.centralPosition.zobristKey, 2)}";
            // Updates FEN info
            FENText.text = $"FEN: {chessEngine.centralPosition.GetFEN()}";
            // Updates game state
            gameStateText.text = $"Game State: {chessEngine.centralPosition.gameState.ToString()}";
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
                Task.Run(() => MakeCalculatedAIMove());
                //MakeCalculatedAIMove();
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
            moveToPlay = chessEngine.CalculateBestMove(5);
            // Sets a flag
            calculated = true;
        }

        #endregion

        // Updates search info
        #region Info Update

        public IEnumerator UpdateInfo()
        {
            while (updateLables)
            {
                // Updates info
                evlaText.text = $"Eval: {chessEngine.eval / 100f}";
                nodesText.text = $"Nodes: {FormatNodeCount(chessEngine.nodes)}";
                timeText.text = $"Time: {chessEngine.stopwatch.ElapsedMilliseconds / 1000f} sec";
                baseDepthText.text = $"Base Depth: {chessEngine.currentBaseDepht - 1}";
                maxDepthText.text = $"Max Depth: {chessEngine.maxDepth}";
                moveText.text = $"Move: {chessEngine.moveString}";
                transpositiontableHitsText.text = $"TT Hits: {chessEngine.transpositiontableHits}";

                // Wait till next info update
                yield return new WaitForSeconds(1f / 60f);
            }
        }

        public string FormatNodeCount(long nodeCount)
        {
            if (nodeCount < 1000)
            {
                return nodeCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (nodeCount < 1000000)
            {
                return nodeCount.ToString("0,.#K", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (nodeCount < 1000000000)
            {
                return nodeCount.ToString("0,,.##M", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                return nodeCount.ToString("0,,,.###B", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        #endregion

        // Checks if an AI move was calculated
        #region Play AI move if ready

        public IEnumerator CheckForAIMove()
        {
            while (updateLables)
            {
                // Updates info
                if (calculated)
                {
                    // Plays the move that the AI thinks is best
                    MakeMove(moveToPlay);
                    inputManager.MakeAIMove(Move.GetFrom(moveToPlay), Move.GetTo(moveToPlay));
                    // Resets the flag
                    calculated = false;
                }

                // Wait till next info update
                yield return new WaitForSeconds(1f / 60f);
            }
        }

        #endregion
    }
}
