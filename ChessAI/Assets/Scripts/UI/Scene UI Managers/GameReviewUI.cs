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

        private bool gameDisplayActive;
        private Board board;
        private Common.ChessGameDataManager chessGameDataManager;
        private GameDataDisplay gameDataDisplay;
        private GamePlayBack gamePlayBack;

        // Class functions

        // Start is called before the first frame update
        void Start()
        {
            board = FindObjectOfType<Board>();
            chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
            gameDataDisplay = FindObjectOfType<GameDataDisplay>();

            gameDisplayCG.alpha = 1;
            gameDisplayCG.blocksRaycasts = true;
            engineDetailsDisplayCG.alpha = 0;
            engineDetailsDisplayCG.blocksRaycasts = false;
            gameButtonImage.color = selected;
            engineDetailsButtonImage.color = deselected;
            gameDisplayActive = true;

            gameDataDisplay.SetAiName(chessGameDataManager.chessGameData.AiStrength);
            gamePlayBack = new GamePlayBack(chessGameDataManager.chessGameData.moves, 
                chessGameDataManager.chessGameData.timeUsage, 
                chessGameDataManager.chessGameData.initialTime,
                chessGameDataManager.chessGameData.timeIncrement,
                board,
                gameDataDisplay);
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

        public void PreviousBtn()
        {
            gamePlayBack.Previous();
        }

        public void NextBtn()
        {
            gamePlayBack.Next();
        }

        public void AutoReplayBtn()
        {

        }

        public void GoHomeBtn()
        {
            chessGameDataManager.ClearData();
            FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
        }
    }
}

