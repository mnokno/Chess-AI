using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class HomeUI : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log(PlayerPrefs.GetString("username"));
        }

        public void StartNewGameBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("GameCreationScene");
        }

        public void LoadGameBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("SavedGameSelectionScene");
        }

        public void ReviewGameBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("ReviewGameSelectionScene");
        }

        public void LogOutBtn()
        {
            PlayerPrefs.SetString("username", "");
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public void QuitBtn()
        {
            Application.Quit();
        }
    }
}
