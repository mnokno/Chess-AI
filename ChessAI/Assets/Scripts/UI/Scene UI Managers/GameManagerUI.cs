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
        private void FinishedSavedGames()
        {

        }

        // Scroll view item on pressed event
        // On clicked list box item event
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

            updateGameNameCG.interactable = false;
            oldGameNameInputField.text = savedGames ? button.GetComponent<OnGoingGameItem>().gameNameText.text : button.GetComponent<FinishedGameItem>().gameNameText.text;
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
                isHumanWhite = false;
                AiStreght = "";
                moves = new string[0];
                timeControll = new string[0];
                timeIncrement = 0;
                whiteTime = 0;
                blackTime = 0;
                whitesTurn = false;
                timeUsage = new string[0];
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
    }
}

