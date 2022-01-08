using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI 
{
    public class GameMenu : ExtendedMenu
    {
        [HideInInspector]
        public Common.ChessGameDataManager chessGameDataManager;
        private BoardInputManager inputManager;

        public SaveAs saveAs;
        public PopUpMessage gmaeSaveSuccessfully; 
        public PopUpMessage invalidSaveTime;
        public PopUpYesNo gameNotSaved;

        public override void Start()
        {
            base.Start();
            // Finds chessGameDataManager & inputManager
            chessGameDataManager = FindObjectOfType<Common.ChessGameDataManager>();
            inputManager = FindObjectOfType<BoardInputManager>();
        }

        public override void SettingsBtn()
        {
            inputManager.takeHumanInpuit = false;
            base.SettingsBtn();
        }

        public override void ReturnBtn()
        {
            base.ReturnBtn();
            if (chessGameDataManager.chessGameData.gameResultCode == null || chessGameDataManager.chessGameData.gameResultCode == "")
            {
                inputManager.takeHumanInpuit = true;
            }
        }

        public void StartNewGameBtn()
        {
            if (chessGameDataManager.chessGameData.saved)
            {
                FindObjectOfType<SceneLoader>().LoadScene("GameCreationScene");
            }
            else
            {
                void Action(PopUpYesNo.Anwser anwser)
                {
                    if (anwser == PopUpYesNo.Anwser.Yes)
                    {
                        FindObjectOfType<SceneLoader>().LoadScene("GameCreationScene");
                    }
                }
                gameNotSaved.SetAction(Action);
                gameNotSaved.Show();
            }
        }

        public void SaveBtn()
        {
            if (inputManager.parrentBoard.whiteHumman == inputManager.parrentBoard.whiteToMove)
            {
                Debug.Log("SAVE");
            }
            else
            {
                invalidSaveTime.Show();
            }
        }

        public void SaveAsBtn()
        {
            if (inputManager.parrentBoard.whiteHumman == inputManager.parrentBoard.whiteToMove)
            {
                saveAs.Show();
            }
            else
            {
                invalidSaveTime.Show();
            }
        }

        public void GoHomeBtn()
        {
            if (chessGameDataManager.chessGameData.saved)
            {
                FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
            }
            else
            {
                void Action(PopUpYesNo.Anwser anwser)
                {
                    if (anwser == PopUpYesNo.Anwser.Yes)
                    {
                        FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
                    }
                }
                gameNotSaved.SetAction(Action);
                gameNotSaved.Show();
            }
        }

        public override void LogOutBtn()
        {
            if (chessGameDataManager.chessGameData.saved)
            {
                base.LogOutBtn();
            }
            else
            {
                void Action(PopUpYesNo.Anwser anwser)
                {
                    if (anwser == PopUpYesNo.Anwser.Yes)
                    {
                        base.LogOutBtn();
                    }
                }
                gameNotSaved.SetAction(Action);
                gameNotSaved.Show();
            }
        }

        public override void QuitBtn()
        {
            if (chessGameDataManager.chessGameData.saved)
            {
                base.QuitBtn();
            }
            else
            {
                void Action(PopUpYesNo.Anwser anwser)
                {
                    if (anwser == PopUpYesNo.Anwser.Yes)
                    {
                        base.QuitBtn();
                    }
                }
                gameNotSaved.SetAction(Action);
                gameNotSaved.Show();
            }
        }
    }
}