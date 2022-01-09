using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class SaveGameItem : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI gameNameText;
        public TMPro.TextMeshProUGUI timeControllText;
        public TMPro.TextMeshProUGUI startDateText;
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
    }
}
