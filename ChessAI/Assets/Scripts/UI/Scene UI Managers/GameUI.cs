using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.UI
{
    public class GameUI : MonoBehaviour
    {
        // Class variables
        public Image gameButtonImage;
        public Image engineDetailsButtonImage;
        public Color selected;
        public Color deselected;
        public CanvasGroup gameDisplayCG;
        public CanvasGroup engineDetailsDisplayCG;

        private bool gameDisplayActive;

        // Class functions

        void Start()
        {
            gameDisplayCG.alpha = 1;
            gameDisplayCG.blocksRaycasts = true;
            engineDetailsDisplayCG.alpha = 0;
            engineDetailsDisplayCG.blocksRaycasts = false;
            gameButtonImage.color = selected;
            engineDetailsButtonImage.color = deselected;
            gameDisplayActive = true;
        }

        public void GameBtn()
        {
            if (!gameDisplayActive)
            {
                gameDisplayCG.alpha = 1;
                gameDisplayCG.blocksRaycasts = true;
                engineDetailsDisplayCG.alpha = 0;
                engineDetailsDisplayCG.blocksRaycasts = false;
                gameButtonImage.color = selected;
                engineDetailsButtonImage.color = deselected;
                gameDisplayActive = true;
            }
        }

        public void EngineDetailsBtn()
        {
            if (gameDisplayActive)
            {
                gameDisplayCG.alpha = 0;
                gameDisplayCG.blocksRaycasts = false;
                engineDetailsDisplayCG.alpha = 1;
                engineDetailsDisplayCG.blocksRaycasts = true;
                gameButtonImage.color = deselected;
                engineDetailsButtonImage.color = selected;
                gameDisplayActive = false;
            }
        }
    }
}
