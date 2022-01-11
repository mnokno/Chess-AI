using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.EngineUtility;

namespace Chess.UI
{
    public class DBLoadingProgress : MonoBehaviour
    {
        // Class variables
        public Animator animator;
        public RectTransform loadingBar;
        public TMPro.TextMeshProUGUI subText;
        private const int totalLoadingBarLenght = 180;

        private void Start()
        {
            Show();
        }
        // Class utilities
        public void Show()
        {
            animator.SetTrigger("Show");
            StartCoroutine(nameof(UpdateData));
        }
        public void Hide()
        {
            animator.SetTrigger("Hide");
            StopAllCoroutines();
        }

        private IEnumerator UpdateData()
        {
            while (true)
            {
                // Calculates progress
                float progress = (float)OpeningBook.readEntries / (float)OpeningBook.totalEntries;
                // Updates sub display
                subText.text = (progress * 100).ToString("0.0") + "%";
                // Updates progress bar
                loadingBar.sizeDelta = new Vector2(totalLoadingBarLenght * progress, loadingBar.sizeDelta.y);
                // Waits till next update
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
    }
}
