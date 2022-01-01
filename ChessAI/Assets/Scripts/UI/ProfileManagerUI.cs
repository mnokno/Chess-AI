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

        // Start is called before the first frame update
        void Start()
        {
            deleteButton.interactable = false;
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
                    deleteButton.interactable = false;
                    return;
                }
            }

            button.GetComponent<Animator>().SetTrigger("GreenOn");
            currentlySelectedListItem = button;
            deleteButton.interactable = true;
        }

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public void DeleteBtn()
        {
            // Delets the profile
            PlayerDbWriter writer = new PlayerDbWriter();
            writer.OpenDB();
            writer.DeleteFromPlayers(currentlySelectedListItem.GetComponent<TMPro.TextMeshProUGUI>().text);
            writer.CloseDB();

            // Delets the list item corresponding to the deleted profile
            Destroy(currentlySelectedListItem);
            currentlySelectedListItem = null;
            // Disables buttons
            deleteButton.interactable = false;
        }
    }
}