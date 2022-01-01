using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chess.UI
{
    public class PopUpMessage : MonoBehaviour
    {
        // Class variables
        public Animator animator;
        public TMPro.TextMeshProUGUI messageText;
        private Action action;

        // Called when the background button is pressed
        public void BackGroundBtn()
        {
            animator.SetTrigger("Pouls");
        }

        // Called when the ok button is pressed
        public void OkBtn()
        {
            animator.SetTrigger("Hide");
            if (action != null)
            {
                action();
            }
        }

        // Shows the popup message
        public void Show()
        {
            animator.SetTrigger("Show");
        }

        // Sets action that is executed when the ok button is pressed
        public void SetAction(Action action)
        {
            this.action = action;
        }

        // Sets message in the massage box
        public void SetMessage(string message)
        {
            messageText.text = message;
        }
    }
}