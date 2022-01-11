using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Chess.UI
{
    public class FloatingChessPieceManager : MonoBehaviour
    {
        public static FloatingChessPieceManager floatingChessPieceManager;
        public float torqueRange = 3.5f;
        public int forceRange = 200;
        public SpriteAtlas spriteAtlas;
        public GameObject floatingPiecePrefab;
        public byte pieceCount = 0;
        public byte maxPieceCount = 10;
        [HideInInspector]
        public Sprite[] sprites;
        private float saftyRadiusX2 = 0.3f;

        // To provide overlapping when spawning
        [HideInInspector]
        public List<Transform> transforms = new List<Transform>();


        // Start is called before the first frame update
        void Start()
        {
            //if (floatingChessPieceManager == null)
            //{
            //    floatingChessPieceManager = this;
            //}
            //else
            //{
            //    Destroy(this.gameObject);
            //    return;
            //}
            //DontDestroyOnLoad(this.gameObject);
            sprites = new Sprite[12];
            spriteAtlas.GetSprites(sprites);
            StartCoroutine("SpawnPieces");
        }

        // Spawns new floating chess pieces, and adds random forces
        public IEnumerator SpawnPieces()
        {
            while (true)
            {
                if (pieceCount < maxPieceCount)
                {
                    SpawnPiece();
                }
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }

        // Spawns new floating chess pieces, and adds random forces
        private void SpawnPiece()
        {
            // Choues a random edge to spawn at
            int randomEdge = Random.Range(0, 4);
            Vector2 spawnPosition;
            switch (randomEdge)
            {
                case 0:
                    spawnPosition = new Vector2(0.6f, Random.Range(-0.5f, 0.5f));
                    break;
                case 1:
                    spawnPosition = new Vector2(-0.6f, Random.Range(-0.5f, 0.5f));
                    break;
                case 2:
                    spawnPosition = new Vector2(Random.Range(-0.5f, 0.5f), 0.6f);
                    break;
                default:
                    spawnPosition = new Vector2(Random.Range(-0.5f, 0.5f), -0.6f);
                    break;
            }

            if (!DoesOverlap(spawnPosition))
            {
                // Create new floating piece
                GameObject newFloatingPiece = Instantiate(floatingPiecePrefab, this.transform);
                newFloatingPiece.transform.localPosition = spawnPosition;
                // Adds reference to this manager
                FloatingChessPiece floatingChessPiece = newFloatingPiece.GetComponent<FloatingChessPiece>();
                floatingChessPiece.floatingChessPieceManager = this;
                floatingChessPiece.spriteRenderer.sprite = sprites[Random.Range(0, 12)];
                // Adds random forces
                Rigidbody2D rigidbody2D = newFloatingPiece.GetComponent<Rigidbody2D>();
                rigidbody2D.AddForce(new Vector2(Random.Range(-forceRange, forceRange), Random.Range(-forceRange, forceRange)));
                rigidbody2D.AddTorque(Random.Range(-torqueRange, torqueRange));
                // Adds this transfrom to transforms list
                transforms.Add(newFloatingPiece.transform);
                // Updates piece count
                pieceCount++;
            }
        }

        // Checks fro overplaying
        private bool DoesOverlap(Vector2 pos)
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i] == null)
                {
                    transforms.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (Vector2.Distance(transforms[i].localPosition, pos) < saftyRadiusX2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Hide the floating chess pieces
        public void Hide()
        {

        }

        // Shows the floating chess pieces
        public void Show()
        {

        }
    }
}
