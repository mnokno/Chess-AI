using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class StartUI : MonoBehaviour
    {
        public void SelectProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("ProfileSelectScene");
        }

        public void CreateProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("ProfileCreationScene");
        }

        public void DeleteProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("ProfileDeleteScene");
        }

        public void QuitBtn()
        {
            Application.Quit();
        }
    }
}

