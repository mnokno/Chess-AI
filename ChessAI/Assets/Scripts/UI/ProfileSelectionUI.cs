using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chess.DB;

namespace Chess.UI
{
    public class ProfileSelectionUI : MonoBehaviour
    {
        // Class variables
        public RectTransform content;
        public GameObject listItemPrefab;
        public Button selectButton;
        private GameObject currentlySelectedListItem;

        // Start, called before update
        private void Start()
        {
            selectButton.interactable = false;
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

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public void ListItemBtn(GameObject button)
        {
            if (currentlySelectedListItem != null)
            {
                currentlySelectedListItem.GetComponent<Animator>().SetTrigger("GreenOff");
                if (currentlySelectedListItem == button)
                {
                    currentlySelectedListItem = null;
                    selectButton.interactable = false;
                    return;
                }
            }

            button.GetComponent<Animator>().SetTrigger("GreenOn");
            currentlySelectedListItem = button;
            selectButton.interactable = true;
        }

        public void SelectBtn()
        {
            PlayerPrefs.SetString("username", currentlySelectedListItem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text);
            FindObjectOfType<SceneLoader>().LoadScene("HomeScene");
        }
    }
}

