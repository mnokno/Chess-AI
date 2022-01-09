using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class FinishedGameItem : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI gameNameText;
        public TMPro.TextMeshProUGUI timeControllText;
        public TMPro.TextMeshProUGUI endDateText;
        public TMPro.TextMeshProUGUI aiNameText;
        public TMPro.TextMeshProUGUI gameResultText;
        public int gameID;

        public void SetGameName(string gameName)
        {
            gameNameText.text = gameName;
        }

        public void SetTimeControll(string timeControll)
        {
            timeControllText.text = timeControll;
        }

        public void SetEndDate(string startDate)
        {
            endDateText.text = startDate;
        }

        public void SetAiName(string aiName)
        {
            aiNameText.text = aiName;
        }

        public void SetGameResult(string gameResult)
        {
            gameResultText.text = gameResult;
        }
    }
}