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
        #region Class variables

        // Chess engine manager connections references
        public ChessEngine chessEngine;
        public BoardInputManager inputManager;

        // Info Settings
        public bool updateLables = true;
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
        public TMPro.TextMeshProUGUI PGNText;

        // Game setting
        public Common.ChessGameDataManager chessGameDataManager;
        // Used to play an AI move once its calculated
        public bool calculated = false;
        public ushort moveToPlay = 0;
        // Settings
        public bool usedOpeningBook;

        #endregion

        // Class Initialization
        #region Initialization

        // Awake is called before Start
        void Awake()
        {
            // Create a new chess engine
            chessEngine = new ChessEngine();
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine("CheckForAIMove");
            if (updateLables)
            {
                // Starts coroutines
                StartCoroutine("UpdateInfo");
                // Updates display info
                zobristHashText.text = $"Zobrist Hash: {System.Convert.ToString((long)chessEngine.centralPosition.zobristKey, 2)}";
                // Updates FEN info
                FENText.text = $"FEN: {chessEngine.centralPosition.GetFEN()}";
            }

            // Finds data manager
            chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
            // Start coroutine for detecting timeout
            if (GetComponent<Board>().useClock)
            {
                StartCoroutine(nameof(CheckGameState));
            }
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
            // Updates game state
            if(chessEngine.GenerateLegalMoves((byte)(chessEngine.centralPosition.sideToMove ? 0 : 1)).Count == 0)
            {
                if (chessEngine.centralPosition.PlayerInCheck())
                {
                    chessEngine.centralPosition.gameState = Position.GameState.Checkmate;
                }
                else
                {
                    chessEngine.centralPosition.gameState = Position.GameState.Stalemate;
                }
            }
            // Updates detailed display
            if (updateLables)
            {
                zobristHashText.text = $"Zobrist Hash: {System.Convert.ToString((long)chessEngine.centralPosition.zobristKey, 2)}";
                FENText.text = $"FEN: {chessEngine.centralPosition.GetFEN()}";
                gameStateText.text = $"Game State: {chessEngine.centralPosition.gameState}";
                PGNText.text = $"PGN: {chessEngine.centralPosition.GetPGN()}";
            }
            // If the game is over the result is displayed
            if (chessEngine.centralPosition.gameState != Position.GameState.OnGoing)
            {
                // Updates and shows the display
                FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay(chessEngine.centralPosition, true);
            }
        }

        // Plays an AI generated move
        public void MakeAIMove()
        {
            // Gets recommended time
            float currentTime = chessEngine.centralPosition.sideToMove ? chessEngine.centralPosition.clock.GetCurrentTimes().x : chessEngine.centralPosition.clock.GetCurrentTimes().y;
            int moveNumber = chessEngine.centralPosition.historicMoveData.Count / 2;
            float time = TimeManagement.GetRecomendedTime(currentTime / 1000f, chessGameDataManager.chessGameData.timeIncrement, chessGameDataManager.chessGameData.initialTime, moveNumber);
            // Easy 10, Medium 20, Hard 30
            if (chessGameDataManager.chessGameData.AiStrength == "1") // Easy
            {
                MakeAIMove(new ChessEngineManager.MoveGenerationProfile(10, time, true));
            }
            else if (chessGameDataManager.chessGameData.AiStrength == "2") // Medium
            {
                MakeAIMove(new ChessEngineManager.MoveGenerationProfile(20, time, true));
            }
            else // Hard
            {
                MakeAIMove(new ChessEngineManager.MoveGenerationProfile(30, time, true));
            }
        }

        // Plays an AI generated move
        public void MakeAIMove(MoveGenerationProfile moveGenerationProfile, bool random = false)
        {
            if (chessEngine.centralPosition.gameState == Position.GameState.OnGoing)
            {
                if (random)
                {
                    // Makes random AI move
                    MakeRandomAIMove();
                }
                else
                {
                    // Makes calculated AI move
                    if (chessEngine.centralPosition.historicMoveData.Count >= moveGenerationProfile.bookLimit)
                    {
                        Task.Run(() => MakeCalculatedAIMove(moveGenerationProfile));
                    }
                    else
                    {
                        // Fetches a move from the opening books
                        moveToPlay = OpeningBook.GetMove(chessEngine.centralPosition);
                        // Checks if the move is valid
                        if (moveToPlay == 0)
                        {
                            // Calculates the best moves, since the move was not valid
                            Task.Run(() => MakeCalculatedAIMove(moveGenerationProfile));
                        }
                        else
                        {
                            calculated = true;
                        }
                    }
                }
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
        private void MakeCalculatedAIMove(MoveGenerationProfile moveGenerationProfile)
        {
            // Calculates AI move
            moveToPlay = chessEngine.CalculateBestMove(moveGenerationProfile.timeLimit);
            // Returns a move at a lower depth
            ushort GetLowerDepth(int lowerBy)
            {
                if (chessEngine.generatedMoves.Count == 0)
                {
                    return 0;
                }
                int depthChange = lowerBy;
                while (true)
                {
                    if (chessEngine.generatedMoves.Count >= depthChange + 1)
                    {
                        return chessEngine.generatedMoves[chessEngine.generatedMoves.Count - depthChange - 1];
                    }
                    depthChange--;
                }
            }
            if (chessGameDataManager.chessGameData.AiStrength == "1") // Easy --- Max - 4
            {
                moveToPlay = GetLowerDepth(4);
            }
            else if (chessGameDataManager.chessGameData.AiStrength == "2") // Medium --- Max - 2
            {
                moveToPlay = GetLowerDepth(2);
            }
            // Sets a flag
            calculated = true;
        }

        private IEnumerator CheckGameState()
        {
            void EndGame()
            {
                // Stops the clocks
                chessEngine.centralPosition.clock.StopClock();
                // Cancels search
                chessEngine.CancelSearch();
                // Disables the chess board
                inputManager.EndGame();
            }

            while (true)
            {
                // Checks for surrender
                if (chessEngine.centralPosition.gameState == Position.GameState.Surrender)
                {
                    chessGameDataManager.chessGameData.gameResultCode = Position.GameState.Surrender.ToString();
                    if (inputManager.parrentBoard.whiteHumman)
                    {
                        // Updates game result
                        chessGameDataManager.chessGameData.gameResult = "0-1";
                        // Updates and shows the display
                        FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay("Surrender", "Black Won", true);
                    }
                    else
                    {
                        // Updates game result
                        chessGameDataManager.chessGameData.gameResult = "1-0";
                        // Updates and shows the display
                        FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay("Surrender", "White Won", true);
                    }
                    // Ends the game
                    EndGame();
                    break;
                }
                // Checks for timeouts
                Vector2 times = chessEngine.centralPosition.clock.GetCurrentTimes();
                if (times.x <= 0)
                {
                    chessGameDataManager.chessGameData.gameResultCode = Position.GameState.OutOfTime.ToString();
                    // Checks if the material is sufficient
                    if (chessEngine.centralPosition.IsMaterialSufficient(false))
                    {
                        // Updates game result
                        chessGameDataManager.chessGameData.gameResult = "0-1";
                        // Updates and shows the display
                        FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay("On Time", "Black Won", true);
                    }
                    else
                    {
                        // Updates game result
                        chessGameDataManager.chessGameData.gameResult = "1/2-1/2";
                        // Updates and shows the display
                        FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay("On Time", "Draw", true);
                    }
                    // Ends the game
                    EndGame();
                    // Updates game state
                    chessEngine.centralPosition.gameState = Position.GameState.OutOfTime;
                    break;
                }
                else if (times.y <= 0)
                {
                    chessGameDataManager.chessGameData.gameResultCode = Position.GameState.OutOfTime.ToString();
                    // Checks if the material is sufficient
                    if (chessEngine.centralPosition.IsMaterialSufficient(true))
                    {
                        // Updates game result
                        chessGameDataManager.chessGameData.gameResult = "1-0";
                        // Updates and shows the display
                        FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay("On Time", "White Won", true);
                    }
                    else
                    {
                        // Updates game result
                        chessGameDataManager.chessGameData.gameResult = "1/2-1/2";
                        // Updates and shows the display
                        FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay("On Time", "Draw", true);
                    }
                    // Ends the game
                    EndGame();
                    // Updates game state
                    chessEngine.centralPosition.gameState = Position.GameState.OutOfTime;
                    break;
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        public void SurrenderHuman()
        {
            if (inputManager.parrentBoard.whiteHumman)
            {
                // Updates and shows the display
                FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay("Surrender", "Black Won", true);
            }
            else
            {
                // Updates and shows the display
                FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay("Surrender", "White Won", true);
            }
            // Stops the clocks
            chessEngine.centralPosition.clock.StopClock();
            // Cancels search
            chessEngine.CancelSearch();
            // Disables the chess board
            inputManager.EndGame();
            // Updates game state
            chessEngine.centralPosition.gameState = Position.GameState.Surrender;
        }

        public void EvaluatePosition(FEN fen)
        {
            // Stops preview evaluation
            chessEngine.CancelSearch();
            // Create a new chess engine
            chessEngine = new ChessEngine();
            chessEngine.LoadFEN(fen);
            // Start the evaluation
            Task.Run(() => chessEngine.Evaluate());
            // Updates display info
            zobristHashText.text = $"Zobrist Hash: {System.Convert.ToString((long)chessEngine.centralPosition.zobristKey, 2)}";
            // Updates FEN info
            FENText.text = $"FEN: {fen.GetInitialStringFEN()}";
        }

        #endregion

        // Updates search info
        #region Info Update

        public IEnumerator UpdateInfo()
        {
            while (updateLables)
            {
                // Updates info
                if (Constants.negativeInfinity + 1000 > chessEngine.eval)
                {
                    evlaText.text = $"Eval: -M{Mathf.Abs(Constants.negativeInfinity - chessEngine.eval)}";
                }
                else if (Constants.positiveInfinity - 1000 < chessEngine.eval)
                {
                    evlaText.text = $"Eval: M{(Constants.positiveInfinity - chessEngine.eval)}";
                }
                else
                {
                    evlaText.text = $"Eval: {chessEngine.eval / 100f}";
                }
                nodesText.text = $"Nodes: {FormatNodeCount(chessEngine.nodes)}";
                timeText.text = $"Time: {chessEngine.stopwatch.ElapsedMilliseconds / 1000f} sec";
                baseDepthText.text = $"Base Depth: {chessEngine.currentBaseDepht - 1}";
                maxDepthText.text = $"Max Depth: {chessEngine.maxDepth}";
                moveText.text = $"Move: {chessEngine.moveString}";
                transpositiontableHitsText.text = $"TT Hits: {FormatNodeCount(chessEngine.transpositiontableHits)}";

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

        private bool PlayRandom()
        {
            if (chessGameDataManager.chessGameData.AiStrength == "1") // Easy --- 5% for a random move
            {
                return Random.Range(0, 20) == 0;
            }
            else if (chessGameDataManager.chessGameData.AiStrength == "2") // Medium --- 2.5% for a random move
            {
                return Random.Range(0, 40) == 0;
            }
            else // Hard --- 0% for a random move
            {
                return false;
            }
        }

        public IEnumerator CheckForAIMove()
        {
            while (true)
            {
                // Updates info
                if (calculated)
                {
                    // There is a chance the AI will blonder
                    if (PlayRandom())
                    {
                        // Generates all legal moves
                        List<ushort> moves = chessEngine.GenerateLegalMoves((byte)(chessEngine.centralPosition.sideToMove ? 0 : 1));
                        // Choses a random move
                        moveToPlay = moves[Random.Range(0, moves.Count - 1)];
                        MakeMove(moveToPlay);
                        inputManager.MakeAIMove(Move.GetFrom(moveToPlay), Move.GetTo(moveToPlay));
                        // Sets a flag
                        calculated = false;
                        // TESTING START
                        Debug.Log("Random Move");
                        // TEST END
                    }
                    else
                    {
                        // Plays the move that the AI thinks is best
                        MakeMove(moveToPlay);
                        inputManager.MakeAIMove(Move.GetFrom(moveToPlay), Move.GetTo(moveToPlay));
                        // Resets the flag
                        calculated = false;
                    }

                }

                // Wait till next info update
                yield return new WaitForSeconds(1f / 60f);
            }
        }

        #endregion

        // Structures
        #region Structures

        public struct MoveGenerationProfile
        {
            public byte bookLimit;
            public float timeLimit;
            public bool autoCalculate;

            public MoveGenerationProfile(byte bookLimit, float timeLimit, bool autoCalculate)
            {
                this.bookLimit = bookLimit;
                this.timeLimit = timeLimit;
                this.autoCalculate = autoCalculate;
            }
        }

        #endregion
    }
}