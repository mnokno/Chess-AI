using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.DB;

namespace Chess.UI
{
    public class ProfileCreationUI : MonoBehaviour
    {
        // Class variables
        public PopUpMessage invalidUserName;
        public PopUpMessage invalidDifficulty;
        public PopUpMessage usernameIsRequired;
        public TMPro.TMP_Dropdown dropdown;
        public TMPro.TMP_InputField inputField;

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public void CreateBtn()
        {
            string usernameNoSpaces = inputField.text;

            if (dropdown.value == 0) // Invalid AI difficulty
            {
                invalidDifficulty.Show();
            }
            else if (IsUsernameEmpty(inputField.text)) // Invalid username
            {
                usernameIsRequired.Show();
            }
            else if (IsUsernameTaken(inputField.text)) // Invalid username
            {
                invalidUserName.SetMessage($"Username '{inputField.text}' is already taken, please chouse a different username.");
                invalidUserName.Show();
            }
            else // Creates user
            {
                PlayerDbWriter writer = new PlayerDbWriter();
                writer.OpenDB();
                writer.WriteToPlayers(new PlayerDb.PlayerRecord(0, inputField.text, dropdown.value.ToString(), true));
                writer.CloseDB();
                PlayerPrefs.SetString("username", inputField.text);
                FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
            }
        }

        private bool IsUsernameTaken(string username)
        {
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.PlayerRecord playerRecord = reader.TryGetRecord(username);
            reader.CloseDB();
            return playerRecord.isValid;
        }

        private bool IsUsernameEmpty(string username)
        {
            while (username.Contains(" "))
            {
                username = username.Replace(" ", "");
            }
            return username == "";
        }
    }
}
