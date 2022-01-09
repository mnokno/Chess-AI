using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class HomeUI : MonoBehaviour
    {
        public void StartNewGameBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("GameCreationScene");
        }

        public void LoadGameBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("LoadGameScene");
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
