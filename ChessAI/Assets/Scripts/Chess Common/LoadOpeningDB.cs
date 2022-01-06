using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Common
{
    public class LoadOpeningDB : MonoBehaviour
    {
        public bool loadAsync = true;

        private void Awake()
        {
            EngineUtility.OpeningBook.LoadBookFromCSV(!loadAsync);
        }
    }
}

