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
            base.Start();
            // Updates username
            usernameText.text = $"Username: {PlayerPrefs.GetString("username")}";
        }

        public virtual void LogOutBtn()
        {
            PlayerPrefs.SetString("username", "");
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public virtual void GoHomeBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
        }
    }
}