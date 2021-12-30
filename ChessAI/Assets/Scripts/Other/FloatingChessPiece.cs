using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class FloatingChessPiece : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public FloatingChessPieceManager floatingChessPieceManager;

        // Start is called before the first frame update
        void Start()
        {
            this.gameObject.AddComponent<PolygonCollider2D>();
            StartCoroutine("CheckForDispawn");
        }

        // Couroutine for dispowing
        public IEnumerator CheckForDispawn()
        {
            while (true)
            {
                if (transform.localPosition.x > 0.61 || transform.localPosition.y > 0.61 || transform.localPosition.x < -0.61 || transform.localPosition.y < -0.61)
                {
                    floatingChessPieceManager.pieceCount--;
                    Destroy(this.gameObject);
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }
}

