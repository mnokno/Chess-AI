using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chess.DB;

namespace Chess.UI
{
    public class ReviewGameUI : MonoBehaviour
    {
        public RectTransform content;
        public GameObject contentItemPrefab;
        private GameObject currentlySelectedItem;
        private Common.ChessGameDataManager chessGameDataManager;
        public GameDataDisplay dataGameDisplay;
        public Board previewBoard;
        public Button review;
        private PlayerDb.PlayerRecord currentPlayerRecord;

        public TMPro.TextMeshProUGUI gameName;
        public TMPro.TextMeshProUGUI aiStrenght;
        public TMPro.TextMeshProUGUI gameResult;
        public TMPro.TextMeshProUGUI timeControll;
        public TMPro.TextMeshProUGUI movesPlayed;
        public TMPro.TextMeshProUGUI creationDate;

        // Start is called before the first frame update
        void Start()
        {
            // Gets the current players profile
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            currentPlayerRecord = reader.TryGetPlayersRecord(PlayerPrefs.GetString("username"));
            reader.CloseDB();
            // Gets chess game data manager
            chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
            // Loads game that can be loaded
            PopulateScrollView();
        }

        // Loads all saved game
        private void PopulateScrollView()
        {
            // Gets all saved game for the current player
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.GameRecord[] gameRecords = reader.ReadGameRecords(currentPlayerRecord.playerID);
            reader.CloseDB();

            // Generate all games
            foreach (PlayerDb.GameRecord gameRecord in gameRecords)
            {
                GameObject contentItem = Instantiate(contentItemPrefab, content);
                FinishedGameItem finishedGameItem = contentItem.GetComponent<FinishedGameItem>();
                finishedGameItem.SetGameName(gameRecord.gameTitle);
                string[] timeControll = gameRecord.timeControll.Split("+");
                finishedGameItem.SetTimeControll(int.Parse(timeControll[0]) / 60 + "|" + timeControll[1]);
                finishedGameItem.SetEndDate(gameRecord.startDate.Split(" ")[0]);
                finishedGameItem.SetAiName(FormateAiName(gameRecord.AIStrength));
                finishedGameItem.SetGameResult(gameRecord.gameResult.Split(":")[0]);
                finishedGameItem.gameID = gameRecord.gameID;
                contentItem.GetComponent<Button>().onClick.AddListener(() => ScrollViewItemBtn(contentItem));
            }
        }

        // On clicked list box item event
        private void ScrollViewItemBtn(GameObject button)
        {
            if (currentlySelectedItem != null)
            {
                currentlySelectedItem.GetComponent<Animator>().SetTrigger("GreenOff");
                if (currentlySelectedItem == button)
                {
                    review.interactable = false;
                    currentlySelectedItem = null;
                    previewBoard.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", false);
                    dataGameDisplay.SetAiName("AI");
                    dataGameDisplay.SetTime(600000, 600000);
                    gameName.text = "Game Name:";
                    aiStrenght.text = "AI Strength:";
                    creationDate.text = "Creation Date:";
                    gameResult.text = "Game Result:";
                    timeControll.text = "Time Control:";
                    movesPlayed.text = "Moves Played:";
                    return;
                }
            }

            review.interactable = true;
            button.GetComponent<Animator>().SetTrigger("GreenOn");
            currentlySelectedItem = button;
            FinishedGameItem finishedGameItem = button.GetComponent<FinishedGameItem>();
            LoadPositionPreview(finishedGameItem.gameID);

            // Gets data for info panel
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.GameRecord gameRecord = reader.ReadGameRecord(finishedGameItem.gameID);
            reader.CloseDB();
            // Updates info panel
            gameName.text = $"Game Name: {gameRecord.gameTitle}";
            aiStrenght.text = $"AI Strength: {FormateAiName(gameRecord.AIStrength).Replace("AI", "")}";
            creationDate.text = $"Creation Date: {gameRecord.startDate.Split(" ")[0]}";
            gameResult.text = $"Game Result: {gameRecord.gameResult.Replace(":", " ")}";
            string[] timeControlParts = gameRecord.timeControll.Split("+");
            timeControll.text = $"Time Control: {int.Parse(timeControlParts[0]) / 60 + "|" + timeControlParts[1]}";
            movesPlayed.text = $"Moves Played: {(gameRecord.moves != "" ? gameRecord.moves.Split(":").Length : 0)}";
        }

        // Load position preview
        private void LoadPositionPreview(int gameID)
        {
            // Reads the game record
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.GameRecord gameRecord = reader.ReadGameRecord(gameID);
            reader.CloseDB();

            // Loads board settings
            if (bool.Parse(gameRecord.isHumanWhite))
            {
                previewBoard.whiteHumman = true;
                previewBoard.whiteBottom = true;
            }
            else
            {
                previewBoard.whiteHumman = false;
                previewBoard.whiteBottom = false;
            }

            // Loads AI name
            dataGameDisplay.SetAiName(gameRecord.AIStrength);

            // Calculate the position FEN
            EngineUtility.Position position = new EngineUtility.Position();
            string[] moves = gameRecord.moves.Split(":");
            if (moves.Length != 0)
            {
                for (int i = moves.Length - 1; i >= 0; i--)
                {
                    if (moves[i] != "")
                    {
                        position.MakeMove(ushort.Parse(moves[i]));
                    }
                }
            }

            // Calculates time
            string[] timeControll = gameRecord.timeControll.Split("+");
            float timeIncrement = int.Parse(timeControll[1]) * 1000;
            float whiteTime = int.Parse(timeControll[0]) * 1000;
            float blackTime = whiteTime;
            bool whitesTurn = true;
            string[] timeUsage = gameRecord.timeUsage.Split(":");
            if (moves.Length != 0)
            {
                foreach (string timeUse in timeUsage)
                {
                    if (timeUse != "")
                    {
                        if (whitesTurn)
                        {
                            whiteTime -= (int.Parse(timeUse) - timeIncrement);
                            whitesTurn = false;
                        }
                        else
                        {
                            blackTime -= (int.Parse(timeUse) - timeIncrement);
                            whitesTurn = true;
                        }
                    }
                }
            }
            dataGameDisplay.SetTime(whiteTime, blackTime);

            // Loads the position on the chess board
            previewBoard.whiteHumman = bool.Parse(gameRecord.isHumanWhite);
            previewBoard.whiteBottom = bool.Parse(gameRecord.isHumanWhite);
            previewBoard.LoadFEN(new EngineUtility.FEN(position.GetFEN()).GetPiecePlacment(), true);
        }

        private string FormateAiName(string code)
        {
            if (code == "1")
            {
                return "Week AI";
            }
            else if (code == "2")
            {
                return "Normal AI";
            }
            else if (code == "3")
            {
                return "Strong AI";
            }
            else
            {
                return "AI";
            }
        }

        public void ReviewBtn()
        {
            // Reads the game record
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.GameRecord gameRecord = reader.ReadGameRecord(currentlySelectedItem.GetComponent<FinishedGameItem>().gameID);
            reader.CloseDB();

            // Gets game data
            bool loadGame = true;
            bool newGame = false;
            bool saved = true;
            string moves = gameRecord.moves;
            string AiStrength = gameRecord.AIStrength;
            string timeUsage = gameRecord.timeUsage;
            string[] timeControll = gameRecord.timeControll.Split("+");
            int initialTime = int.Parse(timeControll[0]);
            int timeIncrement = int.Parse(timeControll[1]);
            int unmakesLimit = gameRecord.unmakesLimit;
            int unmakesMade = gameRecord.unmakesMade;
            string startDate = gameRecord.startDate;
            string endDate = gameRecord.endDate;
            string gameResult = gameRecord.gameResult;
            string gameTitle = gameRecord.gameTitle;
            bool isHumanWhite = bool.Parse(gameRecord.isHumanWhite);

            // Saves game data
            chessGameDataManager.chessGameData = new Common.ChessGameDataManager.ChessGameData()
            {
                loadGame = loadGame,
                newGame = newGame,
                saved = saved,
                moves = moves,
                AiStrength = AiStrength,
                timeUsage = timeUsage,
                initialTime = initialTime,
                timeIncrement = timeIncrement,
                unmakesLimit = unmakesLimit,
                unmakesMade = unmakesMade,
                startDate = startDate,
                endDate = endDate,
                gameResult = gameResult.Split(":")[0],
                gameResultCode = gameResult.Split(":")[1],
                gameTitle = gameTitle,
                isHumanWhite = isHumanWhite,
            };

            // Loads next scene after the opening book has loaded
            StartCoroutine(nameof(WaitForDB));
        }

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
        }

        public void ChangeProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("ProfileSelectionScene");
        }

        public IEnumerator WaitForDB()
        {
            if (!EngineUtility.OpeningBook.hasLoaded)
            {
                FindObjectOfType<DBLoadingProgress>().Show();
            }
            while (!EngineUtility.OpeningBook.hasLoaded)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            chessGameDataManager.chessGameData.loadGame = false;
            // Loads the game
            FindObjectOfType<SceneLoader>().LoadScene("GameReviewScene");
        }
    }
}