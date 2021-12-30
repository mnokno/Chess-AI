using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class ProfileCreationUI : MonoBehaviour
    {
        public void GoBackBtn()
        {
            FindObjectOfType<SceneLoader>().LoadScene("StartScene");
        }

        public void CreateBtn()
        {

        }
    }

}
