using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class BasicMenu : MonoBehaviour
    {
        // Class variables
        public Animator animator;
        public UnityEngine.UI.Toggle fullScreenToggle;
        private bool isMenuOn = false;

        // Class functions

        public virtual void Start()
        {
            // Sets correct initial state to the fullscreen toggle
            fullScreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        }

        public virtual void QuitBtn()
        {
            Application.Quit();
        }

        public virtual void ReturnBtn()
        {
            isMenuOn = false;
            animator.SetTrigger("Hide");
        }

        public void BackGroundBtn()
        {
            ReturnBtn();
        }

        public virtual void SettingsBtn()
        {
            if (isMenuOn)
            {
                ReturnBtn();
            }
            else
            {
                animator.SetTrigger("Show");
                isMenuOn = true;
            }
        }

        public void FullscreenTog(bool value)
        {
            if (value)
            {
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreen = false;
            }
        }
    }
}