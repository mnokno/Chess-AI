using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class GameReviewMenu : ExtendedMenu
    {
        // Class variables
        [HideInInspector]
        public Common.ChessGameDataManager chessGameDataManager;

        // Class variables
        public override void Start()
        {
            base.Start();
            // Finds chessGameDataManager
            chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
        }

        public override void GoHomeBtn()
        {
            chessGameDataManager.ClearData();
            base.GoHomeBtn();
        }

        public override void LogOutBtn()
        {
            chessGameDataManager.ClearData();
            base.LogOutBtn();
        }
    }
}
