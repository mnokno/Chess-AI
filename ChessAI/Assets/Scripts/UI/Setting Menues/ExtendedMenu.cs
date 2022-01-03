using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class ExtendedMenu : BasicMenu
    {
        // Class variables
        public TMPro.TextMeshProUGUI usernameText;

        // Class functions

        public override void Start()
        {
            // Sets correct initial state to the fullscreen toggle
            fullScreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            // Updates username
            usernameText.text = $"Username: {PlayerPrefs.GetString("username")}";
        }

        public void LogOutBtn()
        {
            PlayerPrefs.SetString("username", "");
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }
    }
}