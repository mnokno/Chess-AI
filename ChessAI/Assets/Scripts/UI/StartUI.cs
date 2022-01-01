using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class StartUI : MonoBehaviour
    {
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

