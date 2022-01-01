using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.UI
{
    public class ProfileSelectionUI : MonoBehaviour
    {
        // Class variables
        public Transform content;
        public GameObject listItemPrefab;
        public Button selectButton;

        // Start, called before update
        private void Start()
        {
            selectButton.interactable = false;
        }

        public void PopulateListBox()
        {

        }

        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public void ListItemBtn()
        {

        }

        public void SelectBtn()
        {

        }
    }
}

