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
                    currentlySelectedItem = null;
                    return;
                }
            }

            button.GetComponent<Animator>().SetTrigger("GreenOn");
            currentlySelectedItem = button;
        }

        // Load position preview
        private void LoadPositionPreview(int gameID)
        {
            // Reads the game record
        }
    }
}
