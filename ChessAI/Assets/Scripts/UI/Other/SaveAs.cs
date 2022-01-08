using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chess.DB;

namespace Chess.UI
{
    public class SaveAs : MonoBehaviour
    {
        // Class variables
        public GameMenu gameMenu;
        public TMPro.TMP_InputField inputField; 
        public PopUpMessage invalidGameName;
        public PopUpMessage gameNameIsRequired;
        public CanvasGroup canvasGroup;
        [HideInInspector]
        public PlayerDb.PlayerRecord currentPlayerRecord;

        // Class functions

        void Start()
        {
            // Gets the current players profile
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            currentPlayerRecord = reader.TryGetRecord(PlayerPrefs.GetString("username"));
            reader.CloseDB();
        }

        public void Show()
        {
            inputField.text = "";
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }

        public void BackGroundBtn()
        {
            Hide();
        }

        public void BackBtn()
        {
            Hide();
        }

        public void Save()
        {
            string gameName = inputField.text;

            if (IsGameNameEmpty(gameName)) // Invalid game name (empty)
            {
                gameNameIsRequired.Show();
            }
            else if (IsGameNameTaken(gameName)) // Invalid game name (taken)
            {
                invalidGameName.SetMessage($"Game name '{gameName}' is already taken, please chose a different game name.");
                invalidGameName.Show();
            }
            else
            {
                gameMenu.chessGameDataManager.chessGameData.gameTitle = gameName;
                Debug.Log("SAVE AS");
                Hide();
                gameMenu.SaveBtn();
            }
        }

        public bool IsGameNameTaken(string gameName)
        {
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            bool isTaken = reader.IsGameNameTaken(gameName, currentPlayerRecord.playerID);
            reader.CloseDB();
            return isTaken;
        }

        private bool IsGameNameEmpty(string gameName)
        {
            while (gameName.Contains(" "))
            {
                gameName = gameName.Replace(" ", "");
            }
            return gameName == "";
        }
    }
}
