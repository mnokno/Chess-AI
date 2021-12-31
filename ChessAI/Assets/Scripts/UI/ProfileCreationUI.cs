using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class ProfileCreationUI : MonoBehaviour
    {
        // Class variables
        public PopUpMessage invalidUserName;
        public PopUpMessage invalidDifficulty;
        public TMPro.TMP_Dropdown dropdown;
        public TMPro.TMP_InputField inputField;

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public void CreateBtn()
        {
            // Invalid AI difficulty
            if (dropdown.value == 0)
            {
                invalidDifficulty.Show();
            }
        }
    }

}
