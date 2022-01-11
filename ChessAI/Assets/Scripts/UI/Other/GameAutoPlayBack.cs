using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.DB;

namespace Chess.UI
{
    public class GameAutoPlayBack : MonoBehaviour
    {
        public bool useUserIDConstraint;
        private GamePlayBack gamePlayBack;
        private PlayerDb.GameRecord gameRecord;
        private string playersName;

        private void Awake()
        {
            // Reads the game record
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            if (useUserIDConstraint)
            {
                PlayerDb.PlayerRecord playerRecord  = reader.TryGetPlayersRecord(PlayerPrefs.GetString("username"));
                playersName = playerRecord.username;
                gameRecord = reader.ReadRandomGameRecord(playerRecord.playerID);
            }
            else
            {
                gameRecord = reader.ReadRandomGameRecord();
                playersName = reader.TryGetPlayersRecord(gameRecord.playerID).username;

            }
            reader.CloseDB();
            // Gets references
            Board board = FindObjectOfType<Board>();
            // Loads board settings
            board.whiteBottom = bool.Parse(gameRecord.isHumanWhite);
            board.whiteHumman = bool.Parse(gameRecord.isHumanWhite);
        }

        private void Start()
        {
            StartAutoPlayBack();
        }

        private void StartAutoPlayBack()
        {
            // Gets references
            Board board = FindObjectOfType<Board>();
            GameDataDisplay gameDataDisplay = FindObjectOfType<GameDataDisplay>();
            BoardInputManager boardInputManager = FindObjectOfType<BoardInputManager>();


            // Checks if the game record is valid
            if (gameRecord.isValid)
            {
                // Loads all required date
                gamePlayBack = new GamePlayBack(
                    gameRecord.moves,
                    gameRecord.timeUsage,
                    int.Parse(gameRecord.timeControll.Split("+")[0]),
                    int.Parse(gameRecord.timeControll.Split("+")[1]),
                    gameRecord.AIStrength,
                    board,
                    gameDataDisplay,
                    boardInputManager
                    );

                // Updates labels
                gameDataDisplay.SetHumanName(playersName);
                gameDataDisplay.SetAiName(gameRecord.AIStrength);

                // Started auto playback
                StartCoroutine(nameof(AutoPlayBack), gameRecord.moves.Split(":").Length.ToString() + ":" + gameRecord.gameResult);
            }
        }

        public IEnumerator AutoPlayBack(string data)
        {
            string[] parts = data.Split(":");
            string gameResult;
            Debug.Log(data);
            for (int i = 0; i < int.Parse(parts[0]); i++)
            {
                yield return new WaitForSecondsRealtime(1f);
                gamePlayBack.Next();
            }

            if (parts[1] == "0-1")
            {
                gameResult = "Black Won";
            }
            else if (parts[1] == "1-0")
            {
                gameResult = "White Won";
            }
            else
            {
                gameResult = "Draw";
            }

            if (parts[2] == EngineUtility.Position.GameState.OutOfTime.ToString())
            {
                yield return new WaitForSecondsRealtime(1f);
                FindObjectOfType<GameDataDisplay>().SetTime((parts[1] == "0-1" ? false : true), 0);
                FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay(gameResult, parts[2], true);
            }
            else
            {
                FindObjectOfType<GameResultInfoDisplayManager>().UpdateDisplay(gameResult, parts[2], true);
            }
        }
    }
}