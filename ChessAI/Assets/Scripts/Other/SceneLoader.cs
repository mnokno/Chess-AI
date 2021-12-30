using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chess.UI
{
    public class SceneLoader : MonoBehaviour
    {
        public Animator transition;

        public void LoadScene(int scene)
        {
            StartCoroutine(Load(scene));
        }

        IEnumerator Load(int scene)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSecondsRealtime(1f);
            SceneManager.LoadScene(scene);
        }

        public void LoadScene(string scene)
        {
            StartCoroutine(Load(scene));
        }

        IEnumerator Load(string scene)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSecondsRealtime(1f);
            Time.timeScale = 1f;
            SceneManager.LoadScene(scene);
        }
    }
}
