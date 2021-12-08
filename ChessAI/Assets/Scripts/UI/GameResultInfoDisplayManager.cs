using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class GameResultInfoDisplayManager : MonoBehaviour
    {
        #region Class variables

        // Initial alpha value
        public float initialAlpha = 0f;
        // Speed at which the display will fade in and out
        public float fadeSpeed = 0.5f;
        // References to texts
        public TMPro.TextMeshProUGUI mainText;
        public TMPro.TextMeshProUGUI subText;
        // Reference to the canvas component
        CanvasGroup canvasGroup;

        #endregion

        #region Start

        public void Start()
        {
            // Gets component from parent
            this.canvasGroup = this.GetComponentInParent<CanvasGroup>();
            // Sets initial alpha
            this.canvasGroup.alpha = initialAlpha;
        }

        #endregion

        #region Display update

        // Updates the main display
        public void UpdateMainDisplay(string newMainDisplayText, bool showDisplay)
        {
            // Updates display info
            mainText.text = newMainDisplayText;
            // Shows the display
            if (showDisplay)
            {
                Fade(true);
            }
        }

        // Updates the sub display
        public void UpdateSubDisplay(string newSubDisplayText, bool showDisplay)
        {
            // Updates display info
            subText.text = newSubDisplayText;
            // Shows the display
            if (showDisplay)
            {
                Fade(true);
            }
        }

        // Updates both displays
        public void UpdateDisplay(string newMainDisplayText, string newSubDisplayText, bool showDisplay)
        {
            // Updates display info
            mainText.text = newMainDisplayText;
            subText.text = newSubDisplayText;
            // Shows the display
            if (showDisplay)
            {
                Fade(true);
            }
        }

        #endregion

        #region Button events

        // If the OK button is pressed it fades out
        public void ButtonOKPressed()
        {
            Fade(false);
        }

        #endregion

        #region Fade in and out animations and functions

        // Fades in the display 
        public void Fade(bool fadeIn)
        {
            StopAllCoroutines(); // Prevents two animations being playing at the same time
            if (fadeIn)
            {
                StartCoroutine(FadeIn());
            }
            else
            {
                StartCoroutine(FadeOut());
            }
        }

        // Fades out the display 

        // Fades in the display (animation)
        public IEnumerator FadeIn()
        {
            float t = 0; // Timer
            // Has away this piece
            while (t <= 1)
            {
                yield return null; // Waits till next update
                t += Time.deltaTime / fadeSpeed; // Updates time       
                canvasGroup.alpha += Time.deltaTime / fadeSpeed; // Updates alpha
            }
            canvasGroup.alpha = 1; // Ensures the alpha is 1 in case there was a rounding error
            canvasGroup.blocksRaycasts = true; // Enables this display from interacting
        }

        // Fades out the display (animation)
        public IEnumerator FadeOut()
        {
            canvasGroup.blocksRaycasts = false; // Prevents this display from interacting
            float t = 0; // Timer
            // Has away this piece
            while (t <= 1)
            {
                yield return null; // Waits till next update
                t += Time.deltaTime / fadeSpeed; // Updates time
                canvasGroup.alpha -= Time.deltaTime / fadeSpeed; // Updates alpha
            }
            canvasGroup.alpha = 0; // Ensures the alpha is 0 in case there was a rounding error
        }

        #endregion
    }
}

