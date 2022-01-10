using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chess.DB;

namespace Chess.UI
{
    public class GameManagerUI : MonoBehaviour
    {
        #region Class variables

        // Class variables
        public Button deleteButton;
        public TMPro.TMP_Dropdown dropdown;
        public TMPro.TMP_InputField oldGameNameInputField;
        public TMPro.TMP_InputField newGameNameInputField;
        public CanvasGroup savedGamesScrollViewCG;
        public CanvasGroup finishedGamesScrollViewCG;
        public CanvasGroup updateGameNameCG;
        public RectTransform savedGamesContent;
        public RectTransform finishedGamesContent;
        public GameObject savedGameItemPerfab;
        public GameObject finishedGameItemPerfab;
        public Board previewBoard;
        public GameDataDisplay dataGameDisplay;
        public PopUpYesNo deletionQuestion;
        public PopUpMessage invalidGameNameMessage;
        public PopUpMessage gameNameIsRequiredMessage;
        private GameObject currentlySelectedItem;
        private PlayerDb.PlayerRecord currentPlayerRecord;
        private bool savedGames = true;

        #endregion

        #region Initialization & support utilities

        // Start is called before the first frame update
        void Start()
        {
            // Updates staring configuration canvas groups
            savedGamesScrollViewCG.alpha = 1;
            savedGamesScrollViewCG.blocksRaycasts = true;
            finishedGamesScrollViewCG.alpha = 0;
            finishedGamesScrollViewCG.blocksRaycasts = false;
            updateGameNameCG.alpha = 1;
            updateGameNameCG.interactable = false;
            // Gets the current players profile
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            currentPlayerRecord = reader.TryGetPlayersRecord(PlayerPrefs.GetString("username"));
            reader.CloseDB();
            // Populates scroll views
            PopulateSavedGames();
            PopulateFinishedSavedGames();
        }

        // Populates saved game
        private void PopulateSavedGames()
        {
            // Gets all saved game for the current player
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.SavedGameRecord[] savedGames = reader.ReadSavedGames(currentPlayerRecord.playerID);
            reader.CloseDB();

            // Generate all games
            foreach (PlayerDb.SavedGameRecord savedGame in savedGames)
            {
                GameObject contentItem = Instantiate(savedGameItemPerfab, savedGamesContent);
                OnGoingGameItem onGoingGameItem = contentItem.GetComponent<OnGoingGameItem>();
                onGoingGameItem.SetGameName(savedGame.gameTitle);
                string[] timeControll = savedGame.timeControll.Split("+");
                onGoingGameItem.SetTimeControll(int.Parse(timeControll[0]) / 60 + "|" + timeControll[1]);
                onGoingGameItem.SetStartDate(savedGame.startDate.Split(" ")[0]);
                onGoingGameItem.SetAiName(FormateAiName(savedGame.AIStrength));
                onGoingGameItem.gameID = savedGame.gameID;
                contentItem.GetComponent<Button>().onClick.AddListener(() => ScrollViewItemBtn(contentItem));
            }
        }

        // Populates finished games
        private void PopulateFinishedSavedGames()
        {
            // Gets all saved game for the current player
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.GameRecord[] gameRecords = reader.ReadGameRecords(currentPlayerRecord.playerID);
            reader.CloseDB();

            // Generate all games
            foreach (PlayerDb.GameRecord gameRecord in gameRecords)
            {
                GameObject contentItem = Instantiate(finishedGameItemPerfab, finishedGamesContent);
                FinishedGameItem finishedGameItem = contentItem.GetComponent<FinishedGameItem>();
                finishedGameItem.SetGameName(gameRecord.gameTitle);
                string[] timeControll = gameRecord.timeControll.Split("+");
                finishedGameItem.SetTimeControll(int.Parse(timeControll[0]) / 60 + "|" + timeControll[1]);
                finishedGameItem.SetEndDate(gameRecord.endDate.Split(" ")[0]);
                finishedGameItem.SetAiName(FormateAiName(gameRecord.AIStrength));
                finishedGameItem.SetGameResult(gameRecord.gameResult.Split(":")[0]);
                finishedGameItem.gameID = gameRecord.gameID;
                contentItem.GetComponent<Button>().onClick.AddListener(() => ScrollViewItemBtn(contentItem));
            }
        }

        // Scroll view item on pressed event
        private void ScrollViewItemBtn(GameObject button)
        {
            if (currentlySelectedItem != null)
            {
                currentlySelectedItem.GetComponent<Animator>().SetTrigger("GreenOff");
                if (currentlySelectedItem == button)
                {
                    updateGameNameCG.interactable = false;
                    oldGameNameInputField.text = "";
                    newGameNameInputField.text = "";
                    deleteButton.interactable = false;
                    currentlySelectedItem = null;
                    previewBoard.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
                    dataGameDisplay.SetAiName("AI");
                    dataGameDisplay.SetTime(600000, 600000);
                    return;
                }
            }

            updateGameNameCG.interactable = true;
            oldGameNameInputField.text = savedGames ? button.GetComponent<OnGoingGameItem>().gameNameText.text : button.GetComponent<FinishedGameItem>().gameNameText.text;
            newGameNameInputField.text = "";
            deleteButton.interactable = true;
            button.GetComponent<Animator>().SetTrigger("GreenOn");
            currentlySelectedItem = button;
            LoadPositionPreview(savedGames ? button.GetComponent<OnGoingGameItem>().gameID : button.GetComponent<FinishedGameItem>().gameID);
        }

        // Load position preview
        private void LoadPositionPreview(int gameID)
        {
            // Data that need to be read
            bool isHumanWhite;
            string AiStreght;
            string[] moves;
            string[] timeControll;
            float timeIncrement;
            float whiteTime;
            float blackTime;
            bool whitesTurn;
            string[] timeUsage;

            // Reads the game record
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            if (savedGames)
            {
                PlayerDb.SavedGameRecord savedGameRecord = reader.ReadSavedGame(gameID);
                isHumanWhite = bool.Parse(savedGameRecord.isHumanWhite);
                AiStreght = savedGameRecord.AIStrength;
                moves = savedGameRecord.moves.Split(":");
                timeControll = savedGameRecord.timeControll.Split("+");
                timeIncrement = int.Parse(timeControll[1]) * 1000;
                whiteTime = int.Parse(timeControll[0]) * 1000;
                blackTime = whiteTime;
                whitesTurn = true;
                timeUsage = savedGameRecord.timeUsage.Split(":");
            }
            else
            {
                PlayerDb.GameRecord gameRecord = reader.ReadGameRecord(gameID);
                isHumanWhite = bool.Parse(gameRecord.isHumanWhite);
                AiStreght = gameRecord.AIStrength;
                moves = gameRecord.moves.Split(":");
                timeControll = gameRecord.timeControll.Split("+");
                timeIncrement = int.Parse(timeControll[1]) * 1000;
                whiteTime = int.Parse(timeControll[0]) * 1000;
                blackTime = whiteTime;
                whitesTurn = true;
                timeUsage = gameRecord.timeUsage.Split(":");
            }
            reader.CloseDB();

            // Loads board settings
            if (isHumanWhite)
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
            dataGameDisplay.SetAiName(AiStreght);

            // Calculate the position FEN
            EngineUtility.Position position = new EngineUtility.Position();
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
            previewBoard.LoadFEN(new EngineUtility.FEN(position.GetFEN()).GetPiecePlacment());
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

        #endregion

        #region Main panel management

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
        }

        public void DeleteBtn()
        {
            void Action(PopUpYesNo.Anwser anwser)
            {
                if (anwser == PopUpYesNo.Anwser.Yes)
                {
                    PlayerDbWriter writer = new PlayerDbWriter();
                    writer.OpenDB();
                    if (savedGames)
                    {
                        writer.DeleteFromSavedGames(currentlySelectedItem.GetComponent<OnGoingGameItem>().gameID);
                    }
                    else
                    {
                        writer.DeleteFromGameRecord(currentlySelectedItem.GetComponent<FinishedGameItem>().gameID);
                    }
                    writer.CloseDB();
                    DestroyImmediate(currentlySelectedItem);
                    currentlySelectedItem = null;
                    updateGameNameCG.interactable = false;
                    oldGameNameInputField.text = "";
                    newGameNameInputField.text = "";
                    deleteButton.interactable = false;
                    previewBoard.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
                    dataGameDisplay.SetAiName("AI");
                    dataGameDisplay.SetTime(600000, 600000);
                }
            }
            deletionQuestion.SetAction(Action);
            deletionQuestion.Show();
        }

        public void TableViewDrp(int value)
        {
            if (value == 0) // Saved games
            {
                if (!savedGames)
                {
                    if (currentlySelectedItem != null)
                    {
                        currentlySelectedItem.GetComponent<Animator>().SetTrigger("GreenOff");
                        updateGameNameCG.interactable = false;
                        oldGameNameInputField.text = "";
                        newGameNameInputField.text = "";
                        deleteButton.interactable = false;
                        currentlySelectedItem = null;
                        previewBoard.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
                        dataGameDisplay.SetAiName("AI");
                        dataGameDisplay.SetTime(600000, 600000);
                    }
                }
                savedGames = true;
                savedGamesScrollViewCG.alpha = 1;
                savedGamesScrollViewCG.blocksRaycasts = true;
                finishedGamesScrollViewCG.alpha = 0;
                finishedGamesScrollViewCG.blocksRaycasts = false;
            }
            else // Finished game
            {
                if (savedGames)
                {
                    if (currentlySelectedItem != null)
                    {
                        currentlySelectedItem.GetComponent<Animator>().SetTrigger("GreenOff");
                        updateGameNameCG.interactable = false;
                        oldGameNameInputField.text = "";
                        newGameNameInputField.text = "";
                        deleteButton.interactable = false;
                        currentlySelectedItem = null;
                        previewBoard.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
                        dataGameDisplay.SetAiName("AI");
                        dataGameDisplay.SetTime(600000, 600000);
                    }
                }
                savedGames = false;
                savedGamesScrollViewCG.alpha = 0;
                savedGamesScrollViewCG.blocksRaycasts = false;
                finishedGamesScrollViewCG.alpha = 1;
                finishedGamesScrollViewCG.blocksRaycasts = true;
            }
        }

        #endregion

        #region Update panel management

        public void UpdateGameNameBtn()
        {
            string gameName = newGameNameInputField.text;
            if (IsGameNameEmpty(gameName))
            {
                gameNameIsRequiredMessage.Show();
            }
            else if (IsGameNameTaken(gameName))
            {
                invalidGameNameMessage.SetMessage($"Game name {gameName} is already taken, please chose a different game name.");
                invalidGameNameMessage.Show();
            }
            else
            {
                PlayerDbWriter writer = new PlayerDbWriter();
                writer.OpenDB();
                if (savedGames)
                {
                    OnGoingGameItem onGoingGameItem = currentlySelectedItem.GetComponent<OnGoingGameItem>();
                    writer.UpdateSavedGames(onGoingGameItem.gameID, gameName);
                    onGoingGameItem.SetGameName(gameName);
                }
                else
                {
                    FinishedGameItem finishedGameItem = currentlySelectedItem.GetComponent<FinishedGameItem>();
                    writer.UpdateGameRecord(finishedGameItem.gameID, gameName);
                    finishedGameItem.SetGameName(gameName);
                }
                writer.CloseDB();
            }
        }

        private bool IsGameNameEmpty(string username)
        {
            while (username.Contains(" "))
            {
                username = username.Replace(" ", "");
            }
            return username == "";
        }

        public bool IsGameNameTaken(string gameName)
        {
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            bool isTaken = reader.IsGameNameTaken(gameName, currentPlayerRecord.playerID);
            reader.CloseDB();
            return isTaken;
        }

        #endregion
    }
}