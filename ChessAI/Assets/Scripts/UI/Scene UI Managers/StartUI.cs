using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.DB;

namespace Chess.UI
{
    public class StartUI : MonoBehaviour
    {
        private void Awake()
        {
            string username = PlayerPrefs.GetString("username");
            if (username != "")
            {
                PlayerDbReader reader = new PlayerDbReader();
                reader.OpenDB();
                PlayerDb.PlayerRecord playerRecord = reader.TryGetPlayersRecord(username);
                reader.CloseDB();
                if (playerRecord.isValid)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("HomeScene");
                }
            }
        }

        public void SelectProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("ProfileSelectionScene");
        }

        public void CreateProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("ProfileCreationScene");
        }

        public void ProfileManagerBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("ProfileManagerScene");
        }

        public void QuitBtn()
        {
            Application.Quit();
        }
    }
}