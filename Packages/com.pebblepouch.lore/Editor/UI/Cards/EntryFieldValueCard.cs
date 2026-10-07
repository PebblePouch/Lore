using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryFieldValueCard
    {
        #region Public API
        // Compact (Tabular Editing): glyph, name and Category share the width (name left, Category right aligned);
        // tooltips show the full texts.
        public void DrawCompact(
            Rect rect,
            Entry entry)
        {
            if (entry == null)
            {
                Draw(rect, null);
                return;
            }

            float spacing = InspectorStyle.FieldValueEntryCompactSpacing;

            float glyphWidth = Mathf.Min(InspectorStyle.FieldGlyphColumnWidth, rect.width);

            float textWidth = Mathf.Max(0f, rect.width - glyphWidth - spacing * 2f);
            float titleWidth = Mathf.Floor(textWidth * InspectorStyle.FieldValueEntryCompactTitleRatio);
            float categoryWidth = Mathf.Max(0f, textWidth - titleWidth);

            Rect glyphRect = new(rect.x, rect.y, glyphWidth, rect.height);
            Rect titleRect = new(glyphRect.xMax + spacing, rect.y, titleWidth, rect.height);
            Rect categoryRect = new(titleRect.xMax + spacing, rect.y, categoryWidth, rect.height);

            string category =
                entry.Category != null
                    ? entry.Category.Title
                    : string.Empty;

            GUI.Label(
                glyphRect,
                entry.Glyph,
                InspectorStyle.FieldValueEntryGlyph);

            if (titleRect.width > 0f)
            {
                GUI.Label(
                    titleRect,
                    new GUIContent(entry.Title, entry.Title),
                    InspectorStyle.FieldValueEntryTitle);
            }

            if (categoryRect.width > 0f)
            {
                GUI.Label(
                    categoryRect,
                    new GUIContent(category, category),
                    InspectorStyle.FieldValueEntryCategoryCompact);
            }
        }

        public void Draw(
            Rect rect,
            Entry entry)
        {
            Rect contentRect =
                new(
                    rect.x +
                        InspectorStyle.FieldTableEdgeMargin,
                    rect.y,
                    Mathf.Max(
                        0f,
                        rect.width -
                            InspectorStyle.FieldTableEdgeMargin),
                    rect.height);

            if (entry == null)
            {
                GUI.Label(
                    contentRect,
                    LoreText.Inspector.BrowseEntries,
                    InspectorStyle.FieldValueEntryEmpty);

                return;
            }

            float glyphWidth =
                Mathf.Min(
                    InspectorStyle.FieldGlyphColumnWidth,
                    contentRect.width);

            float categoryWidth =
                Mathf.Min(
                    InspectorStyle.FieldCategoryColumnWidth,
                    Mathf.Max(
                        0f,
                        contentRect.width -
                            glyphWidth -
                            InspectorStyle.FieldTableColumnSpacing));

            float titleStart =
                contentRect.x +
                glyphWidth +
                InspectorStyle.FieldTableColumnSpacing;

            float categoryX =
                contentRect.xMax -
                categoryWidth;

            float titleWidth =
                Mathf.Max(
                    0f,
                    categoryX -
                        InspectorStyle.FieldTableColumnSpacing -
                        titleStart);

            Rect glyphRect =
                new(
                    contentRect.x,
                    contentRect.y,
                    glyphWidth,
                    contentRect.height);

            Rect titleRect =
                new(
                    titleStart,
                    contentRect.y,
                    titleWidth,
                    contentRect.height);

            Rect categoryRect =
                new(
                    categoryX,
                    contentRect.y,
                    categoryWidth,
                    contentRect.height);

            GUI.Label(
                glyphRect,
                entry.Glyph,
                InspectorStyle.FieldValueEntryGlyph);

            if (titleRect.width > 0f)
            {
                GUI.Label(
                    titleRect,
                    entry.Title,
                    InspectorStyle.FieldValueEntryTitle);
            }

            if (categoryRect.width > 0f)
            {
                GUI.Label(
                    categoryRect,
                    entry.Category != null
                        ? entry.Category.Title
                        : string.Empty,
                    InspectorStyle.FieldValueEntryCategory);
            }
        }

        public bool DrawMissing(
            Rect rect)
        {
            Rect contentRect =
                new(
                    rect.x +
                        InspectorStyle.FieldTableEdgeMargin,
                    rect.y,
                    Mathf.Max(
                        0f,
                        rect.width -
                            InspectorStyle.FieldTableEdgeMargin),
                    rect.height);

            GUI.Label(
                contentRect,
                LoreText.Inspector.MissingFieldFix,
                InspectorStyle.MissingFieldFixStyle);

            EditorGUIUtility.AddCursorRect(
                contentRect,
                MouseCursor.Link);

            return
                Event.current.type ==
                    EventType.MouseDown &&
                Event.current.button == 0 &&
                contentRect.Contains(
                    Event.current.mousePosition);
        }
        #endregion
    }
}