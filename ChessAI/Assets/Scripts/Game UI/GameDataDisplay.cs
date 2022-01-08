using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.UI
{
    public class GameDataDisplay : MonoBehaviour
    {
        #region Class variables

        public bool autoUpdateTimeDisplay = false;
        public Color whiteTimeDisplayColor;
        public Color blackTimeDisplayColor;
        public Color whiteTimeDisplayFontColor;
        public Color blackTimeDisplayFontColor;
        public Image upperTimeDisplayImage;
        public Image lowerTimeDisplayImage;
        public TMPro.TextMeshProUGUI upperTimeDisplay;
        public TMPro.TextMeshProUGUI lowerTimeDisplay;
        public TMPro.TextMeshProUGUI upperUsername;
        public TMPro.TextMeshProUGUI lowerUsername;

        private Board board;
        private EngineUtility.Clock clock;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            board = FindObjectOfType<Board>();
            clock = board.engineManager.chessEngine.centralPosition.clock;
            string username = PlayerPrefs.GetString("username");
            string aiName = "AI";

            if (board.whiteBottom)
            {
                upperTimeDisplayImage.color = blackTimeDisplayColor;
                upperTimeDisplay.color = blackTimeDisplayFontColor;
                lowerTimeDisplayImage.color = whiteTimeDisplayColor;
                lowerTimeDisplay.color = whiteTimeDisplayFontColor;
            }
            else
            {
                upperTimeDisplayImage.color = whiteTimeDisplayColor;
                upperTimeDisplay.color = whiteTimeDisplayFontColor;
                lowerTimeDisplayImage.color = blackTimeDisplayColor;
                lowerTimeDisplay.color = blackTimeDisplayFontColor;
            }

            if (board.whiteHumman == board.whiteBottom)
            {
                upperUsername.text = aiName;
                lowerUsername.text = username;
            }
            else
            {
                upperUsername.text = username;
                lowerUsername.text = aiName;
            }

            if (autoUpdateTimeDisplay)
            {
                int initialTime = board.engineManager.chessEngine.centralPosition.clock.initialTime;
                SetTime(initialTime, initialTime);
                StartCoroutine("UpdateTimeDisplay");
            }
        }

        /// <summary>
        /// All time should be given in milliseconds
        /// </summary>
        /// <param name="whitesTime"></param>
        /// <param name="balckTime"></param>
        public void SetTime(float whitesTime, float balckTime)
        {
            if (board.whiteBottom)
            {
                upperTimeDisplay.text = FormatTime(balckTime);
                lowerTimeDisplay.text = FormatTime(whitesTime);
            }
            else
            {
                upperTimeDisplay.text = FormatTime(whitesTime);
                lowerTimeDisplay.text = FormatTime(balckTime);
            }
        }

        public void SetAiName(string code)
        {
            string aiName;
            if (code == "1")
            {
                aiName = "Week AI";
            }
            else if (code == "2")
            {
                aiName = "Normal AI";
            }
            else
            {
                aiName = "Strong AI";
            }

            if (board.whiteHumman == board.whiteBottom)
            {
                upperUsername.text = aiName;
            }
            else
            {
                lowerUsername.text = aiName;
            }
        }

        private IEnumerator UpdateTimeDisplay()
        {
            while (true)
            {
                Vector2 times = clock.GetCurrentTimes();
                SetTime(times.x, times.y);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        /// <summary>
        /// Take a time in milliseconds and formats it to min:sec format
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private string FormatTime(float time)
        {
            float secTotal = time / 1000f;
            int min = (int)(secTotal / 60);
            int sec = Mathf.RoundToInt(secTotal - min * 60);
            if (sec == 60)
            {
                min++;
                sec = 0;
            }
            return $"{min}:{sec.ToString("D2")}";
        }
    }
}
