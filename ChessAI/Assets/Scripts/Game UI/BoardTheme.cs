using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI
{
    public class BoardTheme
    {
        // Responsible for representing different parts of a chess board theme
        #region Board themes

        // Theme is used to store and pass color theme of the chess board
        [System.Serializable]
        public struct SquareTheme
        {
            public Color lightNormal;
            public Color lightHighlighted;
            public Color darkNormal;
            public Color darkHighlighted;
        }

        // LegalMovesSprites is used to store and pass color theme and sprites used to show legal moves
        [System.Serializable]
        public struct LegalMoveTheme
        {
            public Sprite legalCaptureSprite;
            public Sprite legalNonCaptureSprite;
            public Color legalCaptureColor;
            public Color legalNonCaptureColor;
        }

        // AnnotationTheme is used to store and pass color and font sued to annotate the board
        [System.Serializable]
        public struct AnnotationTheme
        {
            public Font font;
            public Color lightNormal;
            public Color lightHighlighted;
            public Color darkNormal;
            public Color darkHighlighted;
        }

        #endregion
    }
}