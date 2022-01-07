using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.DB;
using Chess.Common;

namespace Chess.UI
{
    public class GameCreationUI : MonoBehaviour
    {
        // Class variables
        public TMPro.TMP_InputField gameNameInputField;
        public TMPro.TMP_InputField initialTimeInputField;
        public TMPro.TMP_InputField timeIncrementInputField;
        public TMPro.TMP_InputField moveUnamkeLimitInputField;
        public TMPro.TMP_Dropdown gameDifficultyDropdown;
        public TMPro.TMP_Dropdown startingColorDropdown;

        public PopUpMessage invalidGameName;
        public PopUpMessage invalidDifficulty;
        public PopUpMessage initialTimeIsRequired;

        private PlayerDb.PlayerRecord currentPlayerRecord;

        // Class functions

        void Start()
        {
            // Gets the current players profile
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.PlayerRecord playerRecord = reader.TryGetRecord(PlayerPrefs.GetString("username"));
            reader.CloseDB();

            // Sets the game difficulty to their default AI difficulty
            gameDifficultyDropdown.SetValueWithoutNotify(int.Parse(playerRecord.defaultDifficulty));
            // Saves players record for later usage
            currentPlayerRecord = playerRecord;
        }

        public void PlayBtn()
        {
            string gameName = gameNameInputField.text;

            if (gameDifficultyDropdown.value == 0) // Invalid game difficulty
            {
                invalidDifficulty.Show();
            }
            else if (initialTimeInputField.text.Replace("-", "") == "") // Invalid time control
            {
                initialTimeIsRequired.Show();
            }
            else if (IsGameNameTaken(gameName)) // Invalid game name
            {
                invalidGameName.SetMessage($"Game name '{gameName}' is already taken, please chose a different game name.");
                invalidGameName.Show();
            }
            else
            {
                // Gets game data
                bool loadGame = true;
                bool newGame = true;
                bool saved = false;
                string AiStrength = gameDifficultyDropdown.value.ToString();
                int timeLeft = Mathf.Abs(int.Parse(initialTimeInputField.text)) * 60;
                int initialTime = Mathf.Abs(int.Parse(initialTimeInputField.text)) * 60;
                int timeIncrement = timeIncrementInputField.text.Replace("-", "") == "" ? 0 : int.Parse(timeIncrementInputField.text);
                int unmakesLimit = moveUnamkeLimitInputField.text.Replace("-", "") == "" ? 0 : int.Parse(moveUnamkeLimitInputField.text);
                int unmakesMade = 0;
                bool whereUnamkesEnabled = unmakesLimit == 0 ? false : true;
                string startDate = System.DateTime.Today.ToString();
                string gameTitle = gameName == "" ? AutoGenerateGameName() : gameName;
                bool isHumanWhite = GetIsHumanWhite();

                // Saves game data
                ChessGameDataManager chessGameDataManager = FindObjectOfType<ChessGameDataManager>();
                chessGameDataManager.chessGameData = new ChessGameDataManager.ChessGameData()
                {
                    loadGame = loadGame,
                    newGame = newGame,
                    saved = saved,
                    AiStrength = AiStrength,
                    timeLeft = timeLeft,
                    initialTime = initialTime,
                    timeIncrement = timeIncrement,
                    unmakesLimit = unmakesLimit,
                    unmakesMade = unmakesMade,
                    whereUnamkesEnabled = whereUnamkesEnabled,
                    startDate = startDate,
                    gameTitle = gameTitle,
                    isHumanWhite = isHumanWhite
                };

                // Loads next scene after the opening book has loaded
                StartCoroutine(nameof(WaitForDB));
            }
        }

        public bool GetIsHumanWhite()
        {
            if (startingColorDropdown.value == 1) // White
            {
                return true;
            }
            else if (startingColorDropdown.value == 2) // Black
            {
                return false;
            }
            else // Random
            {
                return Random.value > 0.5f;
            }
        }

        public string AutoGenerateGameName()
        {
            int nameIteration = 1;
            string name = currentPlayerRecord.username + "-" + nameIteration;
            while (IsGameNameTaken(name))
            {
                nameIteration++;
                name = currentPlayerRecord.username + "-" + nameIteration;
            }
            return name;
        }

        public bool IsGameNameTaken(string gameName)
        {
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            bool isTaken = reader.IsGameNameTaken(gameName, currentPlayerRecord.playerID);
            reader.CloseDB();
            return isTaken;
        }

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
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