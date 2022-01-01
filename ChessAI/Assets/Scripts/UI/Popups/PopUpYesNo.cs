using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chess.UI
{
    public class PopUpYesNo : MonoBehaviour
    {
        // Class variables
        public Animator animator;
        public TMPro.TextMeshProUGUI messageText;
        private Action<Anwser> action;

        // Called when the background button is pressed
        public void BackGroundBtn()
        {
            animator.SetTrigger("Pouls");
        }

        // Called when the ok button is pressed
        public void YesBtn()
        {
            animator.SetTrigger("Hide");
            if (action != null)
            {
                action(Anwser.Yes);
            }
        }

        // Called when the ok button is pressed
        public void NoBtn()
        {
            animator.SetTrigger("Hide");
            if (action != null)
            {
                action(Anwser.No);
            }
        }

        // Shows the popup message
        public void Show()
        {
            animator.SetTrigger("Show");
        }

        // Sets action that is executed when the ok button is pressed
        public void SetAction(Action<Anwser> action)
        {
            this.action = action;
        }

        // Sets message in the massage box
        public void SetMessage(string message)
        {
            messageText.text = message;
        }

        // Enums
        public enum Anwser
        {
            Yes = 0,
            No = 1
        }
    }
}