using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chess.EngineUtility;


namespace Chess.UI
{
    public class EvalDisplay : MonoBehaviour
    {
        // Class variables
        public RectTransform upperBar;
        public RectTransform lowerBar;
        public Color whiteBarColor;
        public Color blackBarColor;
        public Color whiteEvalTextColor;
        public Color blackEvalTextColor;
        public Image upperBarImage;
        public Image lowerBarImage;
        public TMPro.TextMeshProUGUI upperEvalText;
        public TMPro.TextMeshProUGUI lowerEvalText;
        public TMPro.TextMeshProUGUI bestMoveText;
        private const int barMaxHeight = 364;
        private Engine.ChessEngineManager engineManager;
        private Board board;
        private ushort currentBestMove;
        private int currentEval;
        private float upperTarget;
        private float lowerTarget;

        // Class functions

        // Start is called before the first frame update
        void Start()
        {
            // Finds required objects
            board = FindObjectOfType<Board>();
            engineManager = board.engineManager;

            // Sets correct colors to UI elements
            if (board.whiteBottom)
            {
                upperBarImage.color = blackBarColor;
                lowerBarImage.color = whiteBarColor;
                upperEvalText.color = blackEvalTextColor;
                lowerEvalText.color = whiteEvalTextColor;
            }
            else
            {
                upperBarImage.color = whiteBarColor;
                lowerBarImage.color = blackBarColor;
                upperEvalText.color = whiteEvalTextColor;
                lowerEvalText.color = blackEvalTextColor;
            }

            // Starts checking for updates
            StartCoroutine(nameof(CheckForChanges));
            // Start animator
            StartCoroutine(nameof(AnimateBar));
        }

        // Checks for display updates
        private IEnumerator CheckForChanges()
        {
            while (true)
            {
                if (engineManager.chessEngine.bestMove != currentBestMove || engineManager.chessEngine.eval != currentEval)
                {
                    UpdateDisplay(engineManager.chessEngine.eval, 
                        ((engineManager.chessEngine.bestMove == 65535) || (engineManager.chessEngine.bestMove == 0)) ? "" : Move.ConvertUshortToPNG(engineManager.chessEngine.bestMove, board.inputManager.localPosition), 
                        engineManager.chessEngine.bestMove, 
                        board.inputManager.localPosition.sideToMove);
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        // Updates the display
        private void UpdateDisplay(int eval, string bestMove, ushort move, bool whiteToMove)
        {
            // Updates recommender move
            bestMoveText.text = $"Recommender Move: {bestMove}";

            // Updates eval bar & eval text
            eval *= whiteToMove ? -1 : 1;
            UpdateEvalBar(eval);
            string sEval = FormatEval(eval);
            upperEvalText.text = sEval;
            lowerEvalText.text = sEval;
            if (board.whiteBottom)
            {
                if (eval < 0)
                {
                    upperEvalText.enabled = true;
                    lowerEvalText.enabled = false;
                }
                else
                {
                    upperEvalText.enabled = false;
                    lowerEvalText.enabled = true;
                }
            }
            else
            {
                if (eval > 0)
                {
                    upperEvalText.enabled = true;
                    lowerEvalText.enabled = false;
                }
                else
                {
                    upperEvalText.enabled = false;
                    lowerEvalText.enabled = true;
                }
            }

            // Sets current values
            currentEval = eval;
            currentBestMove = move;
        }

        // Updates eval bar
        private void UpdateEvalBar(int eval)
        {
            float height = (barMaxHeight / 2f) * ((eval / 100f) / 10f);
            if (eval < 0)
            {
                height = height * 2 < -barMaxHeight ? -barMaxHeight / 2f : height;
            }
            else
            {
                height = height * 2 > barMaxHeight ? barMaxHeight / 2f : height;
            }

            if (board.whiteBottom)
            {
                upperTarget = barMaxHeight / 2f - height;
                lowerTarget = barMaxHeight / 2f + height;
            }
            else
            {
                upperTarget = barMaxHeight / 2f + height;
                lowerTarget = barMaxHeight / 2f - height;
            }
        }

        // Formats eval
        private string FormatEval(int eval)
        {
            // Updates info
            if (Constants.negativeInfinity + 1000 > eval)
            {
                return $"M{Mathf.Abs(Constants.negativeInfinity - eval)}";
            }
            else if (Constants.positiveInfinity - 1000 < eval)
            {
                return $"M{(Constants.positiveInfinity - eval)}";
            }
            else
            {
                float lEval = Mathf.Abs(eval) / 100f;
                string sEval = lEval.ToString() + ".0";
                if (lEval < 10)
                {
                    return sEval[0].ToString() + sEval[1].ToString() + sEval[2].ToString();
                }
                else
                {
                    return sEval[0].ToString() + sEval[1].ToString();
                }
            }
        }
        
        // Animates the evaluation bar
        private IEnumerator AnimateBar()
        {
            while (true)
            {
                if (upperBarImage.rectTransform.sizeDelta.y != upperTarget || lowerBarImage.rectTransform.sizeDelta.y != lowerTarget)
                {
                    // Upper
                    float changeRequired = upperTarget - upperBarImage.rectTransform.sizeDelta.y;
                    float change = changeRequired * 0.05f;
                    change = changeRequired < 0 ? (change > -1 ? -1 : change) : (change < 1 ? 1 : change);
                    change = changeRequired < 0 ? (change < changeRequired ? changeRequired : change) : (change > changeRequired ? changeRequired : change);
                    upperBarImage.rectTransform.sizeDelta = new Vector2(upperBarImage.rectTransform.sizeDelta.x, upperBarImage.rectTransform.sizeDelta.y + change);
                    // Lower
                    lowerBarImage.rectTransform.sizeDelta = new Vector2(lowerBarImage.rectTransform.sizeDelta.x, lowerBarImage.rectTransform.sizeDelta.y - change);
                }
                yield return new WaitForSecondsRealtime(1f / 60f);
            }
        }
    }
}