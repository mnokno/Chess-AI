using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class OnGoingGameItem : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI gameNameText;
        public TMPro.TextMeshProUGUI timeControllText;
        public TMPro.TextMeshProUGUI startDateText;
        public TMPro.TextMeshProUGUI aiNameText;
        public int gameID;

        public void SetGameName(string gameName)
        {
            gameNameText.text = gameName;
        }

        public void SetTimeControll(string timeControll)
        {
            timeControllText.text = timeControll;
        }

        public void SetStartDate(string startDate)
        {
            startDateText.text = startDate;
        }

        public void SetAiName(string aiName)
        {
            aiNameText.text = aiName;
        }
    }
}