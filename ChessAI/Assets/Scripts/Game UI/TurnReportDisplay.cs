using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class TurnReportDisplay : MonoBehaviour
    {
        // Class variables
        public TMPro.TextMeshProUGUI turnNumberText;
        public TMPro.TextMeshProUGUI moveWhiteText;
        public TMPro.TextMeshProUGUI moveBalckText;
        public TMPro.TextMeshProUGUI timeWhiteText;
        public TMPro.TextMeshProUGUI timeBlackText;
        public RectTransform whiteTimeImageRectTransform;
        public RectTransform blackTimeImageRectTransform;

        public Color light;
        public Color dark;

        private const int MaxImageSize = 195;
        private float totalTime;

        // Class functions

        public void SetTrunNumber(int turnNumber)
        {
            turnNumberText.text = turnNumber.ToString();
            totalTime = FindObjectOfType<Common.ChessGameDataManager>().chessGameData.initialTime;
        }

        public void SetMove(bool white, string move)
        {
            if (white)
            {
                moveWhiteText.text = move;
            }
            else
            {
                moveBalckText.text = move;
            }
        }

        public void SetTime(bool white, float time)
        {
            if (white)
            {
                timeWhiteText.text = FormatTime(time);
                float imageSize = MaxImageSize * (totalTime / time) * 3;
                if (imageSize > MaxImageSize)
                {
                    imageSize = MaxImageSize;
                }
                whiteTimeImageRectTransform.sizeDelta = new Vector2(imageSize, whiteTimeImageRectTransform.sizeDelta.y);
            }
            else
            {
                timeBlackText.text = FormatTime(time);
                float imageSize = MaxImageSize * (totalTime / time) * 3;
                if (imageSize > MaxImageSize)
                {
                    imageSize = MaxImageSize;
                }
                blackTimeImageRectTransform.sizeDelta = new Vector2(imageSize, blackTimeImageRectTransform.sizeDelta.y);
            }
        }

        /// <summary>
        /// Take a time in milliseconds and formats it to min:sec format
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private string FormatTime(float time)
        {
            double sec = time / 1000d;
            return $"{sec.ToString("0.0")}";
        }
    }
}