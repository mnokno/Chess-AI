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
        public Button startNewGame;
        public Button reviewButton;
        public Button surrenerButton;
        public TMPro.TextMeshProUGUI takeBackText;

        public PopUpMessage invalidTakeback;
        public PopUpYesNo surrender;
        public PopUpYesNo gameNotSaved;

        private bool gameDisplayActive;
        private GameObject currentItem;
        private Stack<GameObject> items = new Stack<GameObject>();
        private Common.ChessGameDataManager chessGameDataManager;
        private Board board;

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
            board = FindObjectOfType<Board>();
            StartCoroutine(nameof(CheckGameState));
            UpdateTaleBacksSubText();
            FindObjectOfType<GameDataDisplay>().SetAiName(chessGameDataManager.chessGameData.AiStrength);
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
                items.Push(currentItem);
                currentItem = null;
            }
            // Scrolls to the bottom of the scroll rec
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }

        public void UnLogTurn()
        {
            // Unlogs the move
            if (currentItem != null)
            {
                DestroyImmediate(currentItem);
                currentItem = items.Pop();
                TurnReportDisplay turnReportDisplay = currentItem.GetComponent<TurnReportDisplay>();
                turnReportDisplay.SetMove(false, "");
                turnReportDisplay.SetTime(false, 0);
            }
            else 
            {
                DestroyImmediate(items.Pop());
            }
            // Scrolls to the bottom of the scroll rec
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }

        public void TabkeBackBtn()
        {
            if (board.whiteHumman == board.whiteToMove)
            {
                if (board.engineManager.chessEngine.centralPosition.moves.Count >= 2)
                {
                    chessGameDataManager.chessGameData.unmakesMade++;
                    UpdateTaleBacksSubText();
                    board.TakeBack();
                    UnLogTurn();
                }
            }
            else
            {
                board.inputManager.takeHumanInpuit = false;
                invalidTakeback.SetAction(() => board.inputManager.takeHumanInpuit = true);
                invalidTakeback.Show();
            }
        }

        public void ReviewGameBtn()
        {
            if (chessGameDataManager.chessGameData.saved)
            {
                chessGameDataManager.chessGameData.loadGame = false;
                StopAllCoroutines();
                LoadReviewData();
                FindObjectOfType<SceneLoader>().LoadScene("GameReviewScene");
            }
            else
            {
                void Action(PopUpYesNo.Anwser anwser)
                {
                    if (anwser == PopUpYesNo.Anwser.Yes)
                    {
                        chessGameDataManager.chessGameData.loadGame = false;
                        StopAllCoroutines();
                        LoadReviewData();
                        FindObjectOfType<SceneLoader>().LoadScene("GameReviewScene");
                    }
                }
                gameNotSaved.SetAction(Action);
                gameNotSaved.Show();
            }
        }

        private void LoadReviewData()
        {
            // Helper functions
            string GetMoves()
            {
                string moves = "";
                foreach (ushort move in board.inputManager.parrentBoard.engineManager.chessEngine.centralPosition.moves)
                {
                    moves += move.ToString() + ":";
                }
                if (moves.Length == 0)
                {
                    return moves;
                }
                else
                {
                    return moves.Remove(moves.Length - 1);
                }
            }
            string GetTimeUsage()
            {
                string timeUsage = "";
                foreach (float time in board.inputManager.parrentBoard.engineManager.chessEngine.centralPosition.moves)
                {
                    timeUsage += time.ToString() + ":";
                }
                if (timeUsage.Length == 0)
                {
                    return timeUsage;
                }
                else
                {
                    return timeUsage.Remove(timeUsage.Length - 1);
                }
            }

            chessGameDataManager.chessGameData.moves = GetMoves();
            chessGameDataManager.chessGameData.timeUsage = GetTimeUsage();
        }

        public void StartNewGameBtn()
        {
            if (chessGameDataManager.chessGameData.saved)
            {
                chessGameDataManager.ClearData();
                StopAllCoroutines();
                FindObjectOfType<SceneLoader>().LoadScene("GameCreationScene");
            }
            else
            {
                void Action(PopUpYesNo.Anwser anwser)
                {
                    if (anwser == PopUpYesNo.Anwser.Yes)
                    {
                        chessGameDataManager.ClearData();
                        StopAllCoroutines();
                        FindObjectOfType<SceneLoader>().LoadScene("GameCreationScene");
                    }
                }
                gameNotSaved.SetAction(Action);
                gameNotSaved.Show();
            } 
        }

        public void SurrenderBtn()
        {
            void Action(PopUpYesNo.Anwser anwser)
            {
                if (anwser == PopUpYesNo.Anwser.Yes)
                {
                    FindObjectOfType<Engine.ChessEngineManager>().SurrenderHuman();
                    chessGameDataManager.chessGameData.gameResultCode = EngineUtility.Position.GameState.Surrender.ToString();
                    if (board.whiteHumman)
                    {
                        chessGameDataManager.chessGameData.gameResult = "0-1";
                    }
                    else
                    {
                        chessGameDataManager.chessGameData.gameResult = "1-0";
                    }
                }
                else
                {
                    board.inputManager.takeHumanInpuit = true;
                }
            }
            
            board.inputManager.takeHumanInpuit = false;
            surrender.SetAction(Action);
            surrender.Show();
        }

        public IEnumerator CheckGameState()
        {
            while (true)
            {
                if (chessGameDataManager.chessGameData.gameResultCode != null && chessGameDataManager.chessGameData.gameResultCode != "")
                {
                    int turnNumer = (int)System.Math.Ceiling(board.engineManager.chessEngine.centralPosition.historicMoveData.Count / 2d);
                    LogMove(turnNumer, chessGameDataManager.chessGameData.gameResult, 0);
                    takeBackButton.gameObject.SetActive(false);
                    startNewGame.gameObject.SetActive(true);
                    reviewButton.interactable = true;
                    surrenerButton.interactable = false;
                    chessGameDataManager.chessGameData.saved = false;
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
