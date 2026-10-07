using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryFieldRow
    {
        #region Fields
        private Entry _entry;
        private FieldValue _fieldValue;

        private readonly EntryFieldValueElement _valueElement = new();
        private readonly EntryFieldValueCard _missingValueCard = new();
        #endregion

        #region Public API
        public void SetEntry(
            Entry entry)
        {
            _entry =
                entry;
        }

        public void SetFieldValue(
            FieldValue fieldValue)
        {
            _fieldValue =
                fieldValue;

            _valueElement.SetEntry(
                _entry);

            _valueElement.SetFieldValue(
                fieldValue);
        }

        public void Draw(
            Rect rowRect,
            Action clearAction)
        {
            if (_fieldValue?.Field == null)
                return;

            GetRowRects(
                rowRect,
                out Rect leftRect,
                out Rect valueRect,
                out Rect buttonRect);

            DrawLeft(
                leftRect);

            // Entry cards and Entries chips use the whole value cell; other controls are one centered line.
            Rect fieldValueRect =
                FieldValueUtility.IsEntryReference(_fieldValue.Field.Type)
                    ? valueRect
                    : GetCenteredControlRect(
                        valueRect);

            _valueElement.Draw(
                fieldValueRect);

            Rect centeredButtonRect =
                GetCenteredControlRect(
                    buttonRect);

            centeredButtonRect.y +=
                InspectorStyle.FieldEntryButtonVerticalOffset;

            ButtonDrawer.Clear(
                centeredButtonRect,
                clearAction,
                ButtonDrawer.DisplayMode.Glyph,
                InspectorStyle.FieldButton);
        }

        public void DrawMissing(
            Rect rowRect)
        {
            GetRowRects(
                rowRect,
                out Rect leftRect,
                out Rect valueRect,
                out _);

            DrawMissingLeft(
                leftRect);

            bool clicked =
                _missingValueCard.DrawMissing(
                    valueRect);

            if (!clicked)
                return;

            NavigateToCategory();

            Event.current.Use();
        }
        #endregion

        #region Draw
        private void DrawLeft(
            Rect rect)
        {
            GetLeftRects(
                rect,
                out Rect glyphRect,
                out Rect titleRect,
                out Rect typeRect);

            GUI.Label(
                glyphRect,
                _fieldValue.Field.Glyph,
                InspectorStyle.FieldGlyph);

            if (titleRect.width > 0f)
            {
                bool required = IsRequired();

                // Long names clip before the ✱, so the mark stays visible.
                Rect nameRect = titleRect;

                if (required)
                {
                    nameRect.width =
                        Mathf.Max(
                            0f,
                            nameRect.width
                            - InspectorStyle.FieldRequiredGlyphWidth
                            - InspectorStyle.FieldRequiredTitleSpacing);
                }

                GUI.Label(
                    nameRect,
                    _fieldValue.Field.name,
                    InspectorStyle.FieldTitle);

                if (required)
                    DrawRequiredMark(titleRect);
            }

            if (typeRect.width > 0f)
            {
                GUI.Label(
                    typeRect,
                    ScriptableObjectTypeService.GetFieldTypeContent(_fieldValue.Field),
                    InspectorStyle.FieldTypeLabel);
            }
        }

        // "Age ✱": the glyph right after the (possibly clipped) name.
        private void DrawRequiredMark(Rect titleRect)
        {
            float nameWidth =
                Mathf.Min(
                    InspectorStyle.FieldTitle.CalcSize(new GUIContent(_fieldValue.Field.name)).x,
                    titleRect.width - InspectorStyle.FieldRequiredGlyphWidth - InspectorStyle.FieldRequiredTitleSpacing);

            GUI.Label(
                new Rect(
                    titleRect.x + Mathf.Max(0f, nameWidth) + InspectorStyle.FieldRequiredTitleSpacing,
                    titleRect.y,
                    InspectorStyle.FieldRequiredGlyphWidth,
                    titleRect.height),
                new GUIContent(
                    InspectorStyle.FieldRequiredGlyph,
                    LoreText.Inspector.FieldRequiredTooltip),
                InspectorStyle.FieldRequiredOn);
        }

        private bool IsRequired() =>
            _entry != null &&
            _entry.Category != null &&
            _entry.Category.IsRequired(_fieldValue.Field);

        private static void DrawMissingLeft(
            Rect rect)
        {
            GetLeftRects(
                rect,
                out _,
                out Rect titleRect,
                out _);

            if (titleRect.width <= 0f)
                return;

            GUI.Label(
                titleRect,
                LoreText.Inspector.MissingFieldTitle,
                InspectorStyle.MissingFieldTitleStyle);
        }
        #endregion

        #region Commands
        private void NavigateToCategory()
        {
            LoreNavigation.NavigateToCategory(
                _entry?.Category);
        }
        #endregion

        #region Helpers
        // Width of the value cell in a row of this width (EntryFieldTable: height of Entries rows).
        public static float GetValueWidth(float rowWidth)
        {
            GetRowRects(
                new Rect(0f, 0f, rowWidth, InspectorStyle.FieldTableRowHeight),
                out _,
                out Rect valueRect,
                out _);

            return valueRect.width;
        }

        // Value cell of a row (EntryFieldTable: health mark on the value only).
        public static Rect GetValueRect(Rect rowRect)
        {
            GetRowRects(
                rowRect,
                out _,
                out Rect valueRect,
                out _);

            return valueRect;
        }

        private static void GetRowRects(
            Rect rowRect,
            out Rect leftRect,
            out Rect valueRect,
            out Rect buttonRect)
        {
            float spacing =
                InspectorStyle.FieldTableColumnSpacing;

            float buttonWidth =
                InspectorStyle.FieldButtonColumnWidth;

            float contentWidth =
                Mathf.Max(
                    0f,
                    rowRect.width -
                        spacing * 2f -
                        buttonWidth);

            float leftWidth =
                Mathf.Floor(
                    contentWidth *
                    InspectorStyle.FieldEntryLeftTabRatio);

            float valueWidth =
                Mathf.Max(
                    0f,
                    contentWidth -
                        leftWidth);

            leftRect =
                new Rect(
                    rowRect.x,
                    rowRect.y,
                    leftWidth,
                    rowRect.height);

            valueRect =
                new Rect(
                    leftRect.xMax +
                        spacing,
                    rowRect.y,
                    valueWidth,
                    rowRect.height);

            buttonRect =
                new Rect(
                    valueRect.xMax +
                        spacing,
                    rowRect.y,
                    buttonWidth,
                    rowRect.height);
        }

        private static void GetLeftRects(
            Rect rect,
            out Rect glyphRect,
            out Rect titleRect,
            out Rect typeRect)
        {
            float spacing =
                InspectorStyle.FieldTableColumnSpacing;

            float glyphWidth =
                Mathf.Min(
                    InspectorStyle.FieldGlyphColumnWidth,
                    rect.width);

            // Name and type share what is left next to the glyph (FieldTypeColumnRatio).
            float typeWidth =
                Mathf.Floor(
                    Mathf.Max(
                        0f,
                        rect.width
                            - glyphWidth
                            - spacing * 2f)
                    * InspectorStyle.FieldTypeColumnRatio);

            glyphRect =
                new Rect(
                    rect.x,
                    rect.y,
                    glyphWidth,
                    rect.height);

            typeRect =
                new Rect(
                    rect.xMax -
                        typeWidth,
                    rect.y,
                    typeWidth,
                    rect.height);

            float titleX =
                glyphRect.xMax +
                spacing;

            float titleXMax =
                typeRect.x -
                spacing;

            titleRect =
                new Rect(
                    titleX,
                    rect.y,
                    Mathf.Max(
                        0f,
                        titleXMax -
                            titleX),
                    rect.height);
        }

        private static Rect GetCenteredControlRect(
            Rect rect)
        {
            float height =
                Mathf.Min(
                    EditorGUIUtility.singleLineHeight,
                    rect.height);

            return
                new Rect(
                    rect.x,
                    rect.center.y -
                        height * 0.5f,
                    rect.width,
                    height);
        }
        #endregion
    }
}