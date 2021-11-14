using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class Piece
    {
        // Class variables
        #region Variables

        private Board parrentBoard; // Reference to parent chess board
        public PieceType pieceType = new PieceType(); // Store color and type of this piece
        public int file; // File of the square on the chess board in reference to the parent board --- NOTE: only intended to be set between 0 and 7
        public int rank; // Rank of the square on the chess board in reference to the parent board --- NOTE: only intended to be set between 0 and 7
        public GameObject piece; // Used to store reference to a game object representing this chess piece

        #endregion

        // Responsible for square initialization
        #region Initialization

        // Class constructor
        public Piece(Board parrentBoard, PieceType pieceType, int file, int rank)
        {
            // Sets variables
            this.parrentBoard = parrentBoard;
            this.pieceType = pieceType;
            this.file = file;
            this.rank = rank;

            GeneratePiece(); // Create this piece
        }

        // Generates piece
        private void GeneratePiece()
        {
            piece = new GameObject($"Piece File:{file} Rank:{rank}"); // Create new empty game object
            SpriteRenderer spriteRenderer = piece.AddComponent<SpriteRenderer>(); // Adds sprite renderer component
            spriteRenderer.sprite = GetSprite(pieceType); // Sets the correct sprite
            spriteRenderer.sortingOrder = 20; // Sets sprite sorting oder (layer)
            piece.transform.SetParent(parrentBoard.pieceContainer); // Set parent
            piece.transform.localPosition = GetLocalCenter(file, rank); // Sets local position
            piece.transform.localScale = new Vector2(parrentBoard.pieceScale, parrentBoard.pieceScale); // Sets scale
        }

        #endregion

        // Responsible for modifying state and position of the piece
        #region Modifier functions

        // Resets position back to its current file and rank
        public void ResetPosition()
        {
            piece.transform.localPosition = GetLocalCenter(file, rank); // Resets position
        }

        // Changes sprite of this piece
        public void Promote(PieceType to, bool animate = true, float animationDuration = 0.2f)
        {
            if (animate)
            {
                parrentBoard.StartCoroutine(AnimatedPromote(to, animationDuration)); // Fades between two sprites
            }
            else
            {
                pieceType = to; // Updates piece type
                piece.GetComponent<SpriteRenderer>().sprite = GetSprite(pieceType); // Updates sprite
            }
        }

        // Changes location of this piece
        public void Move(int file, int rank, bool animate = true, float animationDuration = 0.2f)
        {
            if (animate)
            {
                parrentBoard.StartCoroutine(AnimatedMove(file, rank, animationDuration)); // Slide this piece between its current location and destination
            }
            else
            {
                piece.transform.localPosition = GetLocalCenter(file, rank); // Updates this pieces position
                this.file = file; // Updates file
                this.rank = rank; // Updates rank
            }
        }

        // Destroys this piece
        public void Destroy(bool animate = true, float animationDuration = 0.2f)
        {
            if (animate)
            {
                parrentBoard.StartCoroutine(AnimatedDestroy(animationDuration)); // Animates (fades ways) then destroys this piece game object
            }
            else
            {
                parrentBoard.DestoryGO(piece); // Destroys this piece game object
            }
        }

        #endregion

        // Responsible for animating modifier functions
        #region Modifier functions animations

        // Fade between current piece and promotion piece
        public IEnumerator AnimatedPromote(PieceType to, float animationDuration)
        {
            float t = 0; // Timer
            float a = 1; // Alpha value counter 

            // Fades away this piece
            while (t <= 1)
            {
                yield return null; // Waits till next update
                t += Time.deltaTime / animationDuration * 2; // Updates time       
                a -= Time.deltaTime / animationDuration * 2; // Updates alpha value counter 

                UnityEngine.Color currentColor = piece.GetComponent<SpriteRenderer>().color; // Gets current color
                currentColor.a = a; // Updates alpha
                piece.GetComponent<SpriteRenderer>().color = currentColor; // Updates the color
            }

            this.pieceType = to; // Updates piece type
            this.piece.GetComponent<SpriteRenderer>().sprite = GetSprite(pieceType); // Updates sprite
            t = 0; // Resets timer
            a = 0; // Resets alpha value counter 

            // Fades in this piece
            while (t <= 1)
            {
                yield return null; // Waits till next update
                t += Time.deltaTime / animationDuration * 2; // Updates time       
                a += Time.deltaTime / animationDuration * 2; // Updates alpha value counter 

                UnityEngine.Color currentColor = piece.GetComponent<SpriteRenderer>().color; // Gets current color
                currentColor.a = a; // Updates alpha
                piece.GetComponent<SpriteRenderer>().color = currentColor; // Updates the color
            }

            // Ensures the alpha is back to 1
            UnityEngine.Color color = piece.GetComponent<SpriteRenderer>().color; // Gets current color
            color.a = 1; // Updates alpha
            piece.GetComponent<SpriteRenderer>().color = color; // Updates the color
        }

        // Slides this pees between start and end location *rank and file)
        public IEnumerator AnimatedMove(int file, int rank, float animationDuration)
        {
            float t = 0; // Timer
            Vector2 startPosition = piece.transform.localPosition; // Starting coordinates
            Vector2 endPosition = GetLocalCenter(file, rank); // End coordinates

            // Has away this piece
            while (t <= 1)
            {
                yield return null; // Waits till next update
                t += Time.deltaTime / animationDuration; // Updates time   
                piece.transform.localPosition = Vector2.Lerp(startPosition, endPosition, t); // Linearly interpolates between start and end position by t
            }

            piece.transform.localPosition = endPosition; // Ensures this piece is exactly at its end location
            this.file = file; // Updates file
            this.rank = rank; // Updates rank
        }

        // Fades the piece away before destroying the piece
        public IEnumerator AnimatedDestroy(float animationDuration)
        {
            float t = 0; // Timer
            float a = 1; // Alpha value counter 

            // Has away this piece
            while (t <= 1)
            {
                yield return null; // Waits till next update
                t += Time.deltaTime / animationDuration; // Updates time       
                a -= Time.deltaTime / animationDuration; // Updates alpha value counter 

                UnityEngine.Color currentColor = piece.GetComponent<SpriteRenderer>().color; // Gets current color
                currentColor.a = a; // Updates alpha
                piece.GetComponent<SpriteRenderer>().color = currentColor; // Updates the color
            }

            parrentBoard.DestoryGO(piece); // Destroys this piece
        }

        #endregion

        // Responsible from helping other functions
        #region Helper functions

        // Returns vector representing the local center of a given square
        private Vector2 GetLocalCenter(int file, int rank)
        {
            // Calculate local scale
            float localScale = 1f / 8f;

            // Calculates local center
            float offset = 3.5f;
            if (parrentBoard.whiteBottom)
            {
                return new Vector2((file - offset) * localScale, (offset - rank) * -localScale);
            }
            else
            {
                return new Vector2((file - offset) * -localScale, (offset - rank) * localScale);
            }
        }

        // Returns sprite corresponding to the piece type
        private Sprite GetSprite(PieceType pieceType)
        {
            return parrentBoard.pieceAtlas.GetSprite(pieceType.type.ToString() + " " + pieceType.color.ToString().Replace("White", "W").Replace("Black", "B"));
        }

        #endregion

        // Structures and enumerations used by this class
        #region Structures and enumerations

        // Combines Color and Type into Piece Type 
        public struct PieceType
        {
            public Color color;
            public Type type;
        }

        // Used to chose piece color
        public enum Color
        {
            Black,
            White
        }

        // Used to chose piece type
        public enum Type
        {
            Pawn,
            Knight,
            Bishop,
            Rook,
            Queen,
            King
        }

        #endregion
    }
}
