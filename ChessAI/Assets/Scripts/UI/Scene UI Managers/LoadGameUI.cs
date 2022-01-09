using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.DB;
using UnityEngine.UI;

namespace Chess.UI
{
    public class LoadGameUI : MonoBehaviour
    {
        public RectTransform content;
        public GameObject contentItemPrefab;
        private GameObject currentlySelectedItem;
        private Common.ChessGameDataManager chessGameDataManager;
        public GameDataDisplay dataGameDisplay;
        public Board previewBoard;
        public Button load;
        private PlayerDb.PlayerRecord currentPlayerRecord;

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
            PlayerDb.SavedGameRecord[] savedGames = reader.ReadSavedGames(currentPlayerRecord.playerID);
            reader.CloseDB();

            // Generate all games
            foreach(PlayerDb.SavedGameRecord savedGame in savedGames)
            {
                GameObject contentItem = Instantiate(contentItemPrefab, content);
                SaveGameItem saveGameItem = contentItem.GetComponent<SaveGameItem>();
                saveGameItem.SetGameName(savedGame.gameTitle);
                string[] timeControll = savedGame.timeControll.Split("+");
                saveGameItem.SetTimeControll(int.Parse(timeControll[0])/60 + "|" + timeControll[1]);
                saveGameItem.SetStartDate(savedGame.startDate.Split(" ")[0]);
                saveGameItem.gameID = savedGame.gameID;
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
                    load.interactable = false;
                    currentlySelectedItem = null;
                    previewBoard.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
                    dataGameDisplay.SetAiName("AI");
                    dataGameDisplay.SetTime(600000, 600000);
                    return;
                }
            }

            load.interactable = true;
            button.GetComponent<Animator>().SetTrigger("GreenOn");
            currentlySelectedItem = button;
            LoadPositionPreview(button.GetComponent<SaveGameItem>().gameID);
        }

        // Load position preview
        private void LoadPositionPreview(int gameID)
        {
            // Reads the game record
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.SavedGameRecord savedGameRecord = reader.ReadSavedGame(gameID);
            reader.CloseDB();

            // Loads board settings
            if (bool.Parse(savedGameRecord.isHumanWhite))
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
            dataGameDisplay.SetAiName(savedGameRecord.AIStrength);

            // Calculate the position FEN
            EngineUtility.Position position = new EngineUtility.Position();
            string[] moves = savedGameRecord.moves.Split(":");
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
            string[] timeControll = savedGameRecord.timeControll.Split("+");
            float timeIncrement = int.Parse(timeControll[1]) * 1000;
            float whiteTime = int.Parse(timeControll[0]) * 1000;
            float blackTime = whiteTime;
            bool whitesTurn = true;
            string[] timeUsage = savedGameRecord.timeUsage.Split(":");
            if (moves.Length != 0)
            {
                foreach(string timeUse in timeUsage)
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
            previewBoard.LoadFEN(new EngineUtility.FEN(position.GetFEN()).GetPiecePlacment());
        }

        public void LoadBtn()
        {
            // Reads the game record
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.SavedGameRecord savedGameRecord = reader.ReadSavedGame(currentlySelectedItem.GetComponent<SaveGameItem>().gameID);
            reader.CloseDB();

            // Gets game data
            bool loadGame = true;
            bool newGame = false;
            bool saved = true;
            string moves = savedGameRecord.moves;
            string AiStrength = savedGameRecord.AIStrength;
            string timeUsage = savedGameRecord.timeUsage;
            string[] timeControll = savedGameRecord.timeControll.Split("+");
            int initialTime = int.Parse(timeControll[0]);
            int timeIncrement = int.Parse(timeControll[1]);
            int unmakesLimit = savedGameRecord.unmakesLimit;
            int unmakesMade = savedGameRecord.unmakesMade;
            string startDate = savedGameRecord.startDate;
            string gameTitle = savedGameRecord.gameTitle;
            bool isHumanWhite = bool.Parse(savedGameRecord.isHumanWhite);

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
                gameTitle = gameTitle,
                isHumanWhite = isHumanWhite,
            };

            // Loads next scene after the opening book has loaded
            StartCoroutine(nameof(WaitForDB));
        }

        public void GoBack()
        {
            FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
        }

        public IEnumerator WaitForDB()
        {
            while (!EngineUtility.OpeningBook.hasLoaded)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            // Loads the game
            FindObjectOfType<SceneLoader>().LoadScene("GameScene");
        }
    }
}
