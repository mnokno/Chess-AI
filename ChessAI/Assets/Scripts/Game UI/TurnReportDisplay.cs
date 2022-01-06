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
        public UnityEngine.UI.Image imageBackground;

        public Color lightColor;
        public Color darkColor;

        private const float MaxImageSize = 182f;
        private float totalTime;
        private static bool wasPrevLight = true;

        // Class functions

        private void Awake()
        {
            turnNumberText.text = "";
            moveWhiteText.text = "";
            moveBalckText.text = "";
            timeWhiteText.text = "";
            timeBlackText.text = "";
            whiteTimeImageRectTransform.sizeDelta = new Vector2(0, whiteTimeImageRectTransform.sizeDelta.y);
            blackTimeImageRectTransform.sizeDelta = new Vector2(0, blackTimeImageRectTransform.sizeDelta.y);
            if (wasPrevLight)
            {
                imageBackground.color = darkColor;
                wasPrevLight = false;
            }
            else
            {
                imageBackground.color = lightColor;
                wasPrevLight = true;
            }
            totalTime = FindObjectOfType<Common.ChessGameDataManager>().chessGameData.initialTime;
            Debug.Log("EJRE");
        }

        public void SetTrunNumber(int turnNumber)
        {
            turnNumberText.text = turnNumber.ToString() + '.';
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
                float imageSize = MaxImageSize * ((time / 1000f) / totalTime) * 4;
                if (imageSize > MaxImageSize)
                {
                    imageSize = MaxImageSize;
                }
                whiteTimeImageRectTransform.sizeDelta = new Vector2(imageSize, whiteTimeImageRectTransform.sizeDelta.y);
            }
            else
            {
                timeBlackText.text = FormatTime(time);
                float imageSize = MaxImageSize * ((time / 1000f) / totalTime) * 4;
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