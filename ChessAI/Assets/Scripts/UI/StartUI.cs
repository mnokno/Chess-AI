using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class StartUI : MonoBehaviour
    {
        public void SelectProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("SelectProfileScene");
        }

        public void CreateProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("CreateProfileScene");
        }

        public void DeleteProfileBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("DeleteProfileScene");
        }

        public void QuitBtn()
        {
            Application.Quit();
        }
    }
}

