using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public readonly struct IntegrityCalloutPiece
    {
        #region Properties
        public string Text { get; }
        public Action Action { get; }

        public bool IsLink =>
            Action != null;
        #endregion

        #region Constructor
        public IntegrityCalloutPiece(
            string text,
            Action action = null)
        {
            Text = text;
            Action = action;
        }
        #endregion
    }

    public sealed class IntegrityCalloutCard
    {
        #region Fields
        private float _availableWidth;
        #endregion

        #region Public API
        // Adds the separator before list item [index] of [count], for "A", "A and B", "A, B and C".
        public static void AddListSeparator(
            List<IntegrityCalloutPiece> pieces,
            int index,
            int count)
        {
            if (index <= 0)
                return;

            pieces.Add(
                new IntegrityCalloutPiece(
                    index == count - 1
                        ? LoreText.Common.ListLastSeparator
                        : LoreText.Common.ListSeparator));
        }

        // Adds a sentence template, inserting the pieces added by fill at its {0} placeholder,
        // so translations can place clickable parts anywhere in the sentence.
        public static void AddTemplate(
            List<IntegrityCalloutPiece> pieces,
            string template,
            Action fill)
        {
            const string placeholder = "{0}";

            int index =
                template.IndexOf(
                    placeholder,
                    StringComparison.Ordinal);

            if (index < 0)
            {
                pieces.Add(
                    new IntegrityCalloutPiece(
                        template));

                return;
            }

            if (index > 0)
            {
                pieces.Add(
                    new IntegrityCalloutPiece(
                        template.Substring(0, index)));
            }

            fill?.Invoke();

            string rest =
                template.Substring(
                    index + placeholder.Length);

            if (rest.Length > 0)
            {
                pieces.Add(
                    new IntegrityCalloutPiece(
                        rest));
            }
        }

        // Styles: null = integrity (red card, link color); InsightsStyle.NoticeCard + NoticeLink for neutral notices.
        public void Draw(
            IReadOnlyList<IntegrityCalloutPiece> pieces,
            GUIStyle cardStyle = null,
            GUIStyle linkStyle = null)
        {
            if (pieces == null ||
                pieces.Count == 0)
            {
                return;
            }

            SectionCard.Draw(
                () =>
                    DrawTextFlow(
                        pieces,
                        linkStyle ?? InsightsStyle.IntegrityLink),
                cardStyle ?? InsightsStyle.IntegrityCard);
        }
        #endregion

        #region Draw
        private void DrawTextFlow(
            IReadOnlyList<IntegrityCalloutPiece> pieces,
            GUIStyle linkStyle)
        {
            float availableWidth =
                _availableWidth > 0f
                    ? _availableWidth
                    : 300f;

            float lineHeight =
                InsightsStyle.IntegrityText.lineHeight;

            float x = 0f;
            float y = 0f;

            foreach (IntegrityCalloutPiece piece in pieces)
            {
                if (string.IsNullOrEmpty(
                        piece.Text))
                {
                    continue;
                }

                GUIStyle style =
                    piece.IsLink
                        ? linkStyle
                        : InsightsStyle.IntegrityText;

                float pieceWidth =
                    style
                        .CalcSize(
                            new GUIContent(
                                piece.Text))
                        .x;

                if (x > 0f &&
                    x + pieceWidth > availableWidth)
                {
                    x = 0f;

                    y += lineHeight;
                }

                x +=
                    Mathf.Min(
                        pieceWidth,
                        availableWidth);
            }

            float totalHeight =
                y +
                lineHeight;

            Rect flowRect =
                GUILayoutUtility.GetRect(
                    0f,
                    totalHeight,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(
                        totalHeight));

            if (Event.current.type ==
                EventType.Repaint)
            {
                _availableWidth =
                    flowRect.width;
            }

            x = 0f;
            y = 0f;

            foreach (IntegrityCalloutPiece piece in pieces)
            {
                if (string.IsNullOrEmpty(
                        piece.Text))
                {
                    continue;
                }

                GUIStyle style =
                    piece.IsLink
                        ? linkStyle
                        : InsightsStyle.IntegrityText;

                float pieceWidth =
                    style
                        .CalcSize(
                            new GUIContent(
                                piece.Text))
                        .x;

                if (x > 0f &&
                    x + pieceWidth > flowRect.width)
                {
                    x = 0f;

                    y += lineHeight;
                }

                float drawWidth =
                    Mathf.Min(
                        pieceWidth,
                        flowRect.width);

                Rect pieceRect =
                    new(
                        flowRect.x + x,
                        flowRect.y + y,
                        drawWidth,
                        lineHeight);

                GUI.Label(
                    pieceRect,
                    piece.Text,
                    style);

                if (piece.IsLink)
                {
                    EditorGUIUtility.AddCursorRect(
                        pieceRect,
                        MouseCursor.Link);

                    if (Event.current.type ==
                            EventType.MouseDown &&
                        Event.current.button == 0 &&
                        pieceRect.Contains(
                            Event.current.mousePosition))
                    {
                        piece.Action.Invoke();

                        Event.current.Use();
                    }
                }

                x += drawWidth;
            }
        }
        #endregion
    }
}