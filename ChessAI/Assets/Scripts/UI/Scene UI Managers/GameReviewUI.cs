using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.UI
{
    public class GameReviewUI : MonoBehaviour
    {
        // Class variables
        public Image gameButtonImage;
        public Image engineDetailsButtonImage;
        public Color selected;
        public Color deselected;
        public CanvasGroup gameDisplayCG;
        public CanvasGroup engineDetailsDisplayCG;
        public TMPro.TextMeshProUGUI onOffAutoPlayText;

        private bool gameDisplayActive;
        private Board board;
        private Common.ChessGameDataManager chessGameDataManager;
        private GameDataDisplay gameDataDisplay;
        private GamePlayBack gamePlayBack;

        public Transform content;
        public GameObject contentItemPrefab;
        private GameObject currentItem;

        // Class functions

        // Start is called before the first frame update
        void Start()
        {
            // Finds objects
            board = FindObjectOfType<Board>();
            chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
            gameDataDisplay = FindObjectOfType<GameDataDisplay>();

            // Sets initiates state
            gameDisplayCG.alpha = 1;
            gameDisplayCG.blocksRaycasts = true;
            engineDetailsDisplayCG.alpha = 0;
            engineDetailsDisplayCG.blocksRaycasts = false;
            gameButtonImage.color = selected;
            engineDetailsButtonImage.color = deselected;
            gameDisplayActive = true;
            gameDataDisplay.SetAiName(chessGameDataManager.chessGameData.AiStrength);

            // Creates game play back
            gamePlayBack = new GamePlayBack(chessGameDataManager.chessGameData.moves, 
                chessGameDataManager.chessGameData.timeUsage, 
                chessGameDataManager.chessGameData.initialTime,
                chessGameDataManager.chessGameData.timeIncrement,
                chessGameDataManager.chessGameData.AiStrength,
                board,
                gameDataDisplay,
                FindObjectOfType<BoardInputManager>(),
                true);

            // Creates game history
            EngineUtility.Position localPlayBackPosition = new EngineUtility.Position();
            string[] timeUsage = chessGameDataManager.chessGameData.timeUsage.Split(":");
            string[] moves = chessGameDataManager.chessGameData.moves.Split(":");
            if (moves.Length != 0)
            {
                for (int i = moves.Length - 1; i >= 0; i--)
                {
                    if (moves[i] != "")
                    {
                        ushort move = ushort.Parse(moves[i]);
                        int turnNumer = (int)System.Math.Ceiling(localPlayBackPosition.historicMoveData.Count / 2d);
                        LogMove(turnNumer, EngineUtility.Move.ConvertUshortToPNG(move, localPlayBackPosition), int.Parse(timeUsage[timeUsage.Length - i - 1]));
                        localPlayBackPosition.MakeMove(move);
                    }
                }
            }
            Canvas.ForceUpdateCanvases();
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

        public void FirstBtn()
        {
            gamePlayBack.First();
            StopAutoPlay();
        }

        public void PreviousBtn()
        {
            gamePlayBack.Previous();
            StopAutoPlay();
        }

        public void NextBtn()
        {
            gamePlayBack.Next();
            StopAutoPlay();
        }

        public void LastBtn()
        {
            gamePlayBack.Last();
            StopAutoPlay();
        }

        public void AutoReplayBtn()
        {
            if (onOffAutoPlayText.text == "On")
            {
                StopCoroutine(nameof(AutoPlayBack));
                onOffAutoPlayText.text = "Off";
            }
            else
            {
                if (gamePlayBack.moveLeft > 0)
                {
                    StartCoroutine(nameof(AutoPlayBack));
                    onOffAutoPlayText.text = "On";
                }
            }
        }

        private void StopAutoPlay()
        {
            StopCoroutine(nameof(AutoPlayBack));
            onOffAutoPlayText.text = "Off";
        }

        public IEnumerator AutoPlayBack()
        {
            while (gamePlayBack.moveLeft > 0)
            {
                gamePlayBack.Next();
                yield return new WaitForSecondsRealtime(1f);
            }
            onOffAutoPlayText.text = "Off";
        }

        public void GoHomeBtn()
        {
            chessGameDataManager.ClearData();
            FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
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
        }
    }
}

