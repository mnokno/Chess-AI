using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.DB;

namespace Chess.UI 
{
    public class GameMenu : ExtendedMenu
    {
        // Class variables
        [HideInInspector]
        public Common.ChessGameDataManager chessGameDataManager;
        private BoardInputManager inputManager;

        public SaveAs saveAs;
        public PopUpMessage gameSaveSuccessfully; 
        public PopUpMessage invalidSaveTime;
        public PopUpYesNo gameNotSaved;

        // Class functions

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
                if (chessGameDataManager.chessGameData.gameResultCode != null && chessGameDataManager.chessGameData.gameResultCode != "") // The game has ended
                {
                    ArchiveGame();
                }
                else
                {
                    SaveGame();
                }
                gameSaveSuccessfully.Show();
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

        /// <summary>
        /// Can only by called in humans turn
        /// </summary>
        public void SaveGame()
        {
            // Gets central position, it contains all information that need to be saved regarding the chess game
            EngineUtility.Position position = inputManager.parrentBoard.engineManager.chessEngine.centralPosition;

            // Helper functions
            string GetMoves()
            {
                string moves = "";
                foreach (ushort move in position.moves)
                {
                    moves += move.ToString() + ":";
                }
                if (moves.Length == 0)
                {
                    return moves;
                }
                else
                {
                    return moves.Remove(moves.Length - 1);
                }
            }
            string GetTimeUsage()
            {
                string timeUsage = "";
                foreach (float time in position.timeTakenPerMove)
                {
                    timeUsage += time.ToString() + ":";
                }
                if (timeUsage.Length == 0)
                {
                    return timeUsage;
                }
                else
                {
                    return timeUsage.Remove(timeUsage.Length - 1);
                }
            }

            // Gathers all data that needs to be saved
            int playersID = saveAs.currentPlayerRecord.playerID;
            string moves = GetMoves();
            string timeUsage = GetTimeUsage();
            string aiStrenght = chessGameDataManager.chessGameData.AiStrength;
            string isHumanWhite = inputManager.parrentBoard.whiteHumman.ToString();
            string startDate = chessGameDataManager.chessGameData.startDate;
            string gameTitle = chessGameDataManager.chessGameData.gameTitle;
            int unmakesLimit = chessGameDataManager.chessGameData.unmakesLimit;
            int unmakesMade = chessGameDataManager.chessGameData.unmakesMade;
            string timeControll = chessGameDataManager.chessGameData.initialTime.ToString() + "+" + chessGameDataManager.chessGameData.timeIncrement.ToString();

            PlayerDb.SavedGameRecord savedGameRecord = new PlayerDb.SavedGameRecord()
            {
                playerID = playersID,
                moves = moves,
                timeUsage = timeUsage,
                AIStrength = aiStrenght,
                isHumanWhite = isHumanWhite,
                startDate = startDate,
                gameTitle = gameTitle,
                unmakesLimit = unmakesLimit,
                unmakesMade = unmakesMade,
                timeControll = timeControll
            };

            // Saves the game
            bool isGameNameTaken = IsGameNameTaken(gameTitle);
            PlayerDbWriter writer = new PlayerDbWriter();
            writer.OpenDB();
            if (isGameNameTaken)
            {
                writer.UpdateSavedGames(savedGameRecord, gameTitle, playersID);
            }
            else
            {
                writer.WriteToSavedGames(savedGameRecord);
            }
            writer.CloseDB();

            // Show a message
            chessGameDataManager.chessGameData.saved = true;
        }

        /// <summary>
        /// Can only by called in humans turn
        /// </summary>
        public void ArchiveGame()
        {
            // Gets central position, it contains all information that need to be saved regarding the chess game
            EngineUtility.Position position = inputManager.parrentBoard.engineManager.chessEngine.centralPosition;

            // Helper functions
            string GetMoves()
            {
                string moves = "";
                foreach (ushort move in position.moves)
                {
                    moves += move.ToString() + ":";
                }
                if (moves.Length == 0)
                {
                    return moves;
                }
                else
                {
                    return moves.Remove(moves.Length - 1);
                }
            }
            string GetTimeUsage()
            {
                string timeUsage = "";
                foreach (float time in position.timeTakenPerMove)
                {
                    timeUsage += time.ToString() + ":";
                }
                if (timeUsage.Length == 0)
                {
                    return timeUsage;
                }
                else
                {
                    return timeUsage.Remove(timeUsage.Length - 1);
                }
            }

            // Gathers all data that needs to be saved
            int playerID = saveAs.currentPlayerRecord.playerID;
            string moves = GetMoves();
            string timeUsage = GetTimeUsage();
            string aiStrenght = chessGameDataManager.chessGameData.AiStrength;
            string isHumanWhite = inputManager.parrentBoard.whiteHumman.ToString();
            string startDate = chessGameDataManager.chessGameData.startDate;
            string endDate = System.DateTime.Today.ToString();
            string gameTitle = chessGameDataManager.chessGameData.gameTitle;
            int unmakesLimit = chessGameDataManager.chessGameData.unmakesLimit;
            int unmakesMade = chessGameDataManager.chessGameData.unmakesMade;
            string timeControll = chessGameDataManager.chessGameData.initialTime.ToString() + "+" + chessGameDataManager.chessGameData.timeIncrement.ToString();
            string gameResult = chessGameDataManager.chessGameData.gameResult + ":" + chessGameDataManager.chessGameData.gameResultCode;

            PlayerDb.GameRecord gameRecord = new PlayerDb.GameRecord()
            {
                playerID = playerID,
                moves = moves,
                timeUsage = timeUsage,
                AIStrength = aiStrenght,
                isHumanWhite = isHumanWhite,
                startDate = startDate,
                endDate = endDate,
                gameTitle = gameTitle,
                unmakesLimit = unmakesLimit,
                unmakesMade = unmakesMade,
                timeControll = timeControll,
                gameResult = gameResult
            };

            // Saves the game
            bool isGameNameTaken = IsGameRecordNameTaken(gameTitle);
            PlayerDbWriter writer = new PlayerDbWriter();
            writer.OpenDB();
            if (isGameNameTaken)
            {
                writer.UpdateGameRecord(gameRecord, gameTitle, playerID);
            }
            else
            {
                writer.WriteToGamesRecords(gameRecord);
            }
            writer.CloseDB();

            // Deletes the saved game since its finished
            writer.OpenDB();
            writer.DeleteFromSavedGames(gameTitle, playerID);
            writer.CloseDB();

            // Show a message
            chessGameDataManager.chessGameData.saved = true;
        }

        private bool IsGameNameTaken(string gameName)
        {
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            bool isTaken = reader.IsGameNameTaken(gameName, saveAs.currentPlayerRecord.playerID);
            reader.CloseDB();
            return isTaken;
        }

        private bool IsSavedNameTaken(string gameName)
        {
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            bool isTaken = reader.IsSavedGameNameTaken(gameName, saveAs.currentPlayerRecord.playerID);
            reader.CloseDB();
            return isTaken;
        }

        private bool IsGameRecordNameTaken(string gameName)
        {
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            bool isTaken = reader.IsGameRecordNameTaken(gameName, saveAs.currentPlayerRecord.playerID);
            reader.CloseDB();
            return isTaken;
        }
    }
}