using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    /// <summary>
    /// CategoryInspector > CategoryFieldSection > CategoryFieldTable
    /// </summary>
    public sealed class CategoryFieldRow
    {
        #region Fields
        private float _availableWidth;
        #endregion

        #region Public API
        public void SetAvailableWidth(float availableWidth) => _availableWidth = availableWidth;

        // required / toggleRequired: the Category's Required flag of this Field (✱ before the remove button).
        public void Draw(
            Field field,
            Action removeAction,
            bool required,
            Action toggleRequired)
        {
            float leftWidth = _availableWidth * InspectorStyle.FieldEntryLeftTabRatio;
            float rightWidth = _availableWidth * InspectorStyle.FieldEntryRightTabRatio;

            // Handle is drawn in CategoryFieldTable

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            DrawCentered(
                InspectorStyle.FieldGlyphColumnWidth,
                () => DrawGlyph(field));

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            DrawTitleAndType(
                field.name,
                InspectorStyle.FieldTitle,
                ScriptableObjectTypeService.GetFieldTypeContent(field),
                FieldTypeIntegrityAnalyzer.HasMissingType(field));

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            DrawCentered(
                InspectorStyle.FieldButtonColumnWidth,
                () => DrawRequiredToggle(required, toggleRequired));

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            DrawCentered(
                InspectorStyle.FieldButtonColumnWidth,
                () => DrawButton(removeAction));
        }

        public void DrawMissing(Action removeAction)
        {
            float leftWidth = _availableWidth * InspectorStyle.FieldEntryLeftTabRatio;
            float rightWidth = _availableWidth * InspectorStyle.FieldEntryRightTabRatio;

            // Handle is drawn in CategoryFieldTable

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            DrawCentered(
                InspectorStyle.FieldGlyphColumnWidth,
                null);

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            DrawTitleAndType(
                LoreText.Inspector.MissingFieldTitle,
                InspectorStyle.MissingFieldTitleStyle,
                GUIContent.none,
                false);

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            // No Required toggle for a broken Field (keeps the columns aligned).
            DrawCentered(
                InspectorStyle.FieldButtonColumnWidth,
                null);

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            DrawCentered(
                InspectorStyle.FieldButtonColumnWidth,
                () => DrawButton(removeAction));
        }
        #endregion

        #region Draw
        private void DrawGlyph(Field field)
        {
            GUILayout.Label(
                string.IsNullOrEmpty(field.Glyph)
                    ? "?"
                    : field.Glyph,
                InspectorStyle.FieldGlyph,
                GUILayout.Width(
                    InspectorStyle.FieldGlyphColumnWidth));
        }

        // Name and type share one rect (type: FieldTypeColumnRatio), so long type names get room as the table grows.
        private static void DrawTitleAndType(
            string title,
            GUIStyle titleStyle,
            GUIContent type,
            bool typeBroken)
        {
            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    InspectorStyle.FieldTableRowHeight,
                    GUILayout.ExpandWidth(true));

            float spacing =
                InspectorStyle.FieldTableColumnSpacing;

            float typeWidth =
                Mathf.Floor(
                    Mathf.Max(
                        0f,
                        rect.width - spacing)
                    * InspectorStyle.FieldTypeColumnRatio);

            Rect typeRect =
                new(
                    rect.xMax - typeWidth,
                    rect.y,
                    typeWidth,
                    rect.height);

            Rect titleRect =
                new(
                    rect.x,
                    rect.y,
                    Mathf.Max(
                        0f,
                        typeRect.x - spacing - rect.x),
                    rect.height);

            GUI.Label(
                titleRect,
                title,
                titleStyle);

            // Missing type (integrity): red value cell, like broken values in the Entry Fields table.
            if (typeBroken &&
                Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(
                    typeRect,
                    InspectorStyle.FieldBrokenBackground);
            }

            GUI.Label(
                typeRect,
                type,
                typeBroken
                    ? InspectorStyle.FieldTypeLabelBroken
                    : InspectorStyle.FieldTypeLabel);
        }

        private static void DrawRequiredToggle(
            bool required,
            Action toggleRequired)
        {
            var content =
                new GUIContent(
                    InspectorStyle.FieldRequiredGlyph,
                    required
                        ? LoreText.Inspector.FieldRequiredTooltip
                        : LoreText.Inspector.FieldOptionalTooltip);

            if (GUILayout.Button(
                    content,
                    required
                        ? InspectorStyle.FieldRequiredOn
                        : InspectorStyle.FieldRequiredOff,
                    GUILayout.Width(InspectorStyle.FieldButtonColumnWidth)))
            {
                toggleRequired?.Invoke();
            }
        }

        private void DrawButton(Action removeAction)
        {
            ButtonDrawer.Remove(
                removeAction,
                ButtonDrawer.DisplayMode.Glyph,
                InspectorStyle.FieldButton);
        }
        #endregion

        #region Helper
        private static void DrawCentered(
            float? width,
            Action draw,
            bool expandWidth = false)
        {
            if (width.HasValue)
            {
                GUILayout.BeginVertical(
                    GUILayout.Width(width.Value),
                    GUILayout.Height(InspectorStyle.FieldTableRowHeight));
            }
            else
            {
                GUILayout.BeginVertical(
                    GUILayout.ExpandWidth(expandWidth),
                    GUILayout.Height(InspectorStyle.FieldTableRowHeight));
            }

            GUILayout.FlexibleSpace();

            draw?.Invoke();

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();
        }
        #endregion
    }
}