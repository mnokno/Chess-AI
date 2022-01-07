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
        public Transform content;
        public GameObject contentItemPrefab;
        public ScrollRect scrollRect;
        public Button takeBackButton;
        public Button reviewButton;
        public Button surrenerButton;
        public TMPro.TextMeshProUGUI takeBackText;

        private bool gameDisplayActive;
        private GameObject currentItem;
        private Common.ChessGameDataManager chessGameDataManager;

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
            chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
            StartCoroutine(nameof(CheckGameState));
            UpdateTaleBacksSubText();
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

        public void LogMove(int turnNumber, string move, float time)
        {
            // Logs the move
            if (currentItem == null)
            {
                currentItem = Instantiate(contentItemPrefab, content);
                TurnReportDisplay turnReportDisplay = currentItem.GetComponent<TurnReportDisplay>();
                turnReportDisplay.SetTrunNumber(turnNumber);
                turnReportDisplay.SetMove(true, move);
                turnReportDisplay.SetTime(true, time);
            }
            else
            {
                TurnReportDisplay turnReportDisplay = currentItem.GetComponent<TurnReportDisplay>();
                turnReportDisplay.SetTrunNumber(turnNumber);
                turnReportDisplay.SetMove(false, move);
                turnReportDisplay.SetTime(false, time);
                currentItem = null;
            }
            // Scrolls to the bottom of the scroll rec
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }

        public void TabkeBackBtn()
        {
            chessGameDataManager.chessGameData.unmakesMade++;
            UpdateTaleBacksSubText();
        }

        public void ReviewGameBtn()
        {

        }

        public void SurrenderBtn()
        {
            Surrender();
        }

        public void Surrender()
        {
            FindObjectOfType<Engine.ChessEngineManager>().SurrenderHuman();
        }

        public IEnumerator CheckGameState()
        {
            while (true)
            {
                if (chessGameDataManager.chessGameData.gameResultCode != null)
                {
                    takeBackButton.interactable = false;
                    reviewButton.interactable = true;
                    surrenerButton.interactable = false;
                    break;
                }
                else
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }
            }
        }

        public void UpdateTaleBacksSubText()
        {
            if (chessGameDataManager.chessGameData.unmakesLimit - chessGameDataManager.chessGameData.unmakesMade <= 0)
            {
                takeBackButton.interactable = false;
                takeBackText.text = $"You have {0} takebacks";
            }
            else if (chessGameDataManager.chessGameData.unmakesLimit - chessGameDataManager.chessGameData.unmakesMade == 1)
            {
                takeBackText.text = $"You have {1} takeback";
            }
            else
            {
                takeBackText.text = $"You have {chessGameDataManager.chessGameData.unmakesLimit - chessGameDataManager.chessGameData.unmakesMade} takebacks";
            }
        }
    }
}