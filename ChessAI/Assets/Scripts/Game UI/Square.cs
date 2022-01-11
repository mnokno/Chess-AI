using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.UI
{
    public class Square
    {
        // Class variables
        #region Variables

        private Board parrentBoard; // Reference to parent chess board
        private int file; // File of the square on the chess board in reference to the parent board --- NOTE: only intended to be set between 0 and 7
        private int rank; // Rank of the square on the chess board in reference to the parent board --- NOTE: only intended to be set between 0 and 7
        private int ID; // ID of this square in reference to the parent board --- NOTE: only intended to be set between 0 and 63

        private Vector2 localCenter; // Local coordinates of the center of this square
        private float localScale; // Local scale (used to scale assets respectively to the square size)

        private ColorSet squareColours; // Stores color set for this square
        private ColorSet annotationColors; // Stores color set for this squares annotation
        public GameObject quad; // Stores reference to a quad that represents this square
        public GameObject legalCaptureSprite; // Stores reference to a legalCaptureSprite game object (sprite)
        public GameObject legalNonCaptureSprite; // Stores reference to a legalNonCaptureSprite game object (sprite)
        public List<GameObject> annotations = new List<GameObject>(); // Stores references to all annotations on this square

        #endregion

        // Responsible for square initialization
        #region Initialization

        // Class constructor
        public Square(Board parrentBoard, int file, int rank, int ID)
        {
            // Sets variables
            this.parrentBoard = parrentBoard;
            this.file = file;
            this.rank = rank;
            this.ID = ID;

            VarInit(); // Calculates private variables
            SelectColorSet(); // Selects color set for this square
            GenerateSquare(); // Generates this square
            GenerateLegalCaptureSprite(); // Generate legal capture sprite (initially hidden)
            GenerateLegalNonCaptureSprite(); // Generate legal non-capture sprite (initially hidden)
            Annotate(); // Annotates this square
        }

        // Calculates values of private variables
        private void VarInit()
        {
            // Calculate local scale
            localScale = 1f / 8f;

            // Calculates local center
            float offset = 3.5f;
            if (parrentBoard.whiteBottom)
            {
                localCenter = new Vector2((file - offset) * localScale, (offset - rank) * -localScale);
            }
            else
            {
                localCenter = new Vector2((file - offset) * -localScale, (offset - rank) * localScale);
            }

        }

        // Selects color set for this square
        private void SelectColorSet()
        {
            if (rank % 2 == 0)
            {
                if (file % 2 == 0)
                {
                    squareColours = new ColorSet() { normal = parrentBoard.squareTheme.darkNormal, highlighted = parrentBoard.squareTheme.darkHighlighted };
                    annotationColors = new ColorSet() { normal = parrentBoard.annotationTheme.lightNormal, highlighted = parrentBoard.annotationTheme.lightHighlighted };
                }
                else
                {
                    squareColours = new ColorSet() { normal = parrentBoard.squareTheme.lightNormal, highlighted = parrentBoard.squareTheme.lightHighlighted };
                    annotationColors = new ColorSet() { normal = parrentBoard.annotationTheme.darkNormal, highlighted = parrentBoard.annotationTheme.darkHighlighted };
                }
            }
            else
            {
                if (file % 2 == 0)
                {
                    squareColours = new ColorSet() { normal = parrentBoard.squareTheme.lightNormal, highlighted = parrentBoard.squareTheme.lightHighlighted };
                    annotationColors = new ColorSet() { normal = parrentBoard.annotationTheme.darkNormal, highlighted = parrentBoard.annotationTheme.darkHighlighted };
                }
                else
                {
                    squareColours = new ColorSet() { normal = parrentBoard.squareTheme.darkNormal, highlighted = parrentBoard.squareTheme.darkHighlighted };
                    annotationColors = new ColorSet() { normal = parrentBoard.annotationTheme.lightNormal, highlighted = parrentBoard.annotationTheme.lightHighlighted };
                }
            }
        }

        // Create square as a PrimitiveType.Quad
        private void GenerateSquare()
        {
            GameObject square = GameObject.CreatePrimitive(PrimitiveType.Quad); // Create primitive game object of type quad
            quad = square; // Enables to reference square at a later point
            square.name = $"Quad File:{file} Rank:{rank} ID:{ID}"; // Sets name of this object
            square.transform.SetParent(parrentBoard.squareParentContainer); // Sets parent for this game object
            square.transform.localScale = new Vector2(localScale, localScale); // Sets local scale for this game object
            square.transform.localPosition = localCenter; // Sets local position to local center position
            square.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color"); // Sets correct shader for this game object
            square.GetComponent<Renderer>().material.color = squareColours.normal; // Sets color for this game object
        }

        // Adds annotations to this square Annotate
        private void Annotate()
        {
            if (parrentBoard.whiteBottom)
            {
                if (file == 0)
                {
                    // Adds rank annotation
                    GenerateAnnotation(parrentBoard.rankAnnotations[rank], Alligment.Rank);
                }

                if (rank == 0)
                {
                    // Adds file annotation
                    GenerateAnnotation(parrentBoard.fileAnnotations[file], Alligment.File);
                }
            }
            else
            {
                if (file == 7)
                {
                    // Adds rank annotation
                    GenerateAnnotation(parrentBoard.rankAnnotations[rank], Alligment.Rank);
                }

                if (rank == 7)
                {
                    // Adds file annotation
                    GenerateAnnotation(parrentBoard.fileAnnotations[file], Alligment.File);
                }
            }
        }

        // Generate new annotation
        private void GenerateAnnotation(string content, Alligment alignment)
        {

            GameObject annatation = new GameObject($"Annotation File:{file} Rank:{rank} ID:{ID} Content:{content}"); // Creates new empty game object
            annatation.transform.SetParent(parrentBoard.boardAnnotationCanvasTransform); // Sets parent 
            annatation.transform.localScale = new Vector2(0.00125f, 0.00125f); // Sets scale

            Text text = annatation.AddComponent<Text>(); // Adds text component to the object
            text.text = content; // Sets text of the text element
            text.font = parrentBoard.annotationTheme.font; // Sets font
            text.fontSize = 22; // Sets font size
            text.fontStyle = FontStyle.Normal; // Sets font style
            text.color = annotationColors.normal; // Sets color


            if (alignment == Alligment.Rank)
            {
                annatation.transform.localPosition = localCenter + new Vector2(0.01f, -0.003f); // Sets position, the offset varies depending on alignment type
                text.alignment = TextAnchor.UpperLeft; // Sets text alignment
            }
            else if (alignment == Alligment.File)
            {
                annatation.transform.localPosition = localCenter + new Vector2(-0.01f, 0.003f); // Sets position, the offset varies depending on alignment type
                text.alignment = TextAnchor.LowerRight; // Sets text alignment
            }

            annotations.Add(annatation); // Stores reference to this annotation in a list
        }

        // Create instance of hollow circle, initially not visible
        private void GenerateLegalCaptureSprite()
        {
            legalCaptureSprite = new GameObject($"Legal Capture Sprite File:{file} Rank:{rank} ID:{ID}"); // Create new empty game object
            legalCaptureSprite.SetActive(false); // Initial sates is set to hidden
            legalCaptureSprite.transform.SetParent(quad.transform); // Sets parent transform to this quad
            legalCaptureSprite.transform.localPosition = new Vector2(0, 0); // Sets local position to the center of this quad
            legalCaptureSprite.transform.localScale = new Vector2(0.3f, 0.3f); // Sets local scale

            SpriteRenderer spriteRenderer = legalCaptureSprite.AddComponent<SpriteRenderer>(); // Adds sprites renderer
            spriteRenderer.sprite = parrentBoard.legalMoveTheme.legalCaptureSprite; // Sets sprite
            spriteRenderer.color = parrentBoard.legalMoveTheme.legalCaptureColor; // Sets color
            spriteRenderer.sortingOrder = 10; // Sets sprite sorting oder (layer)
        }

        // Create instance of filled circle, initially not visible
        private void GenerateLegalNonCaptureSprite()
        {
            legalNonCaptureSprite = new GameObject($"Legal Non-Capture Sprite File:{file} Rank:{rank} ID:{ID}"); // Create new empty game object
            legalNonCaptureSprite.SetActive(false); // Initial sates is set to hidden
            legalNonCaptureSprite.transform.SetParent(quad.transform); // Sets parent transform to this quad
            legalNonCaptureSprite.transform.localPosition = new Vector2(0, 0); // Sets local position to the center of this quad
            legalNonCaptureSprite.transform.localScale = new Vector2(0.1f, 0.1f); // Sets local scale

            SpriteRenderer spriteRenderer = legalNonCaptureSprite.AddComponent<SpriteRenderer>(); // Adds sprites renderer
            spriteRenderer.sprite = parrentBoard.legalMoveTheme.legalNonCaptureSprite; // Sets sprite
            spriteRenderer.color = parrentBoard.legalMoveTheme.legalNonCaptureColor; // Sets color
            spriteRenderer.sortingOrder = 10; // Sets sprite sorting oder (layer)
        }

        #endregion

        // Responsible for changing appearance of this square
        #region Modifier functions

        // Used to show and hide hollow circle on this tile 
        public void LegalCaptureSpriteVisible(bool visible)
        {
            // Enables or disable hollow circle
            legalCaptureSprite.SetActive(visible);
        }

        // Used to show and hide filled circle on this tile
        public void LegalNonCaptureSpriteVisible(bool visible)
        {
            // Enables or disable filled circle
            legalNonCaptureSprite.SetActive(visible);
        }

        // Used to highlight and dehighlight this square, also changes annotation color
        public void Highlighted(bool highlighted)
        {
            if (highlighted)
            {
                quad.GetComponent<Renderer>().material.color = squareColours.highlighted; // Changes color to highlighted
                foreach (GameObject annotation in annotations) // Changes color of each annotation to highlighted
                {
                    annotation.GetComponent<Text>().color = annotationColors.highlighted;
                }
            }
            else
            {
                quad.GetComponent<Renderer>().material.color = squareColours.normal; // Changes color to normal
                foreach (GameObject annotation in annotations) // Changes color of each annotation to normal
                {
                    annotation.GetComponent<Text>().color = annotationColors.normal;
                }
            }
        }

        // Resets the square colors
        public void ResetColor()
        {
            quad.GetComponent<Renderer>().material.color = squareColours.normal; // Sets color for this game object
        }

        #endregion

        // Structures and enumerations used by this class
        #region Structures and enumerations

        // Used to store colors for his square
        private struct ColorSet
        {
            public Color normal;
            public Color highlighted;
        }

        // Used to chose alignment type of annotations
        private enum Alligment
        {
            File,
            Rank
        }

        #endregion
    }
}
