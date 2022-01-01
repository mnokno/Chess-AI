using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chess.DB;

namespace Chess.UI
{
    public class ProfileManagerUI : MonoBehaviour
    {
        // Class variables
        public RectTransform content;
        public GameObject listItemPrefab;
        public Button deleteButton;
        private GameObject currentlySelectedListItem;
        public PopUpYesNo deletionConfirmation;
        public CanvasGroup updatePanel;

        public PopUpMessage invalidUserName;
        public PopUpMessage invalidDifficulty;
        public PopUpMessage usernameIsRequired;
        public TMPro.TMP_Dropdown dropdown;
        public TMPro.TMP_InputField inputFieldOldUsername;
        public TMPro.TMP_InputField inputFieldNewUsername;

        // Start is called before the first frame update
        void Start()
        {
            deleteButton.interactable = false;
            updatePanel.interactable = false;
            deletionConfirmation.SetAction(Delete);
            PopulateListBox();
        }

        public void PopulateListBox()
        {
            // Gets all player records
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.PlayerRecord[] playerRecords = reader.ReadAllPlayers();
            reader.CloseDB();

            // Displays all read player records
            foreach (PlayerDb.PlayerRecord record in playerRecords)
            {
                GameObject listBoxItem = Instantiate(listItemPrefab, content);
                listBoxItem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = record.username;
                listBoxItem.GetComponent<Button>().onClick.AddListener(() => ListItemBtn(listBoxItem));
            }
        }

        public void ListItemBtn(GameObject button)
        {
            if (currentlySelectedListItem != null)
            {
                currentlySelectedListItem.GetComponent<Animator>().SetTrigger("GreenOff");
                if (currentlySelectedListItem == button)
                {
                    currentlySelectedListItem = null;
                    inputFieldOldUsername.text = "";
                    dropdown.SetValueWithoutNotify(0);
                    deleteButton.interactable = false;
                    updatePanel.interactable = false;
                    return;
                }
            }

            button.GetComponent<Animator>().SetTrigger("GreenOn");
            currentlySelectedListItem = button;
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.PlayerRecord playerRecord = reader.TryGetRecord(currentlySelectedListItem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text);
            reader.CloseDB();
            inputFieldOldUsername.text = playerRecord.username;
            dropdown.SetValueWithoutNotify(int.Parse(playerRecord.defaultDifficulty));
            updatePanel.interactable = true;
            deleteButton.interactable = true;
        }

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public void Delete(PopUpYesNo.Anwser anwser)
        {
            if (anwser == PopUpYesNo.Anwser.Yes)
            {
                // Delets the profile
                PlayerDbWriter writer = new PlayerDbWriter();
                writer.OpenDB();
                writer.DeleteFromPlayers(currentlySelectedListItem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text);
                writer.CloseDB();

                // Delets the list item corresponding to the deleted profile
                Destroy(currentlySelectedListItem);
                currentlySelectedListItem = null;
                // Disables buttons
                deleteButton.interactable = false;
            }
        }

        public void DeleteBtn()
        {
            deletionConfirmation.SetMessage($"Are you shure you want to delete '{currentlySelectedListItem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text}' profile, this action can not be undone.");
            deletionConfirmation.Show();
        }

        public void UpdateBtn()
        {
            string usernameNoSpaces = inputFieldNewUsername.text;

            if (dropdown.value == 0) // Invalid AI difficulty
            {
                invalidDifficulty.Show();
            }
            else if (IsUsernameEmpty(inputFieldNewUsername.text)) // Invalid username
            {
                usernameIsRequired.Show();
            }
            else if (inputFieldOldUsername.text != inputFieldNewUsername.text && IsUsernameTaken(inputFieldNewUsername.text)) // Invalid username
            {
                invalidUserName.SetMessage($"Username '{inputFieldNewUsername.text}' is already taken, please chouse a different username.");
                invalidUserName.Show();
            }
            else // Updates user profile
            {
                PlayerDbWriter writer = new PlayerDbWriter();
                writer.OpenDB();
                writer.UpdatePlayers(inputFieldOldUsername.text, inputFieldNewUsername.text, dropdown.value.ToString());
                writer.CloseDB();
                currentlySelectedListItem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = inputFieldNewUsername.text;
                inputFieldOldUsername.text = inputFieldNewUsername.text;
                inputFieldNewUsername.text = "";
            }
        }

        private bool IsUsernameTaken(string username)
        {
            PlayerDbReader reader = new PlayerDbReader();
            reader.OpenDB();
            PlayerDb.PlayerRecord playerRecord = reader.TryGetRecord(username);
            reader.CloseDB();
            return playerRecord.isValid;
        }

        private bool IsUsernameEmpty(string username)
        {
            while (username.Contains(" "))
            {
                username = username.Replace(" ", "");
            }
            return username == "";
        }
    }
}