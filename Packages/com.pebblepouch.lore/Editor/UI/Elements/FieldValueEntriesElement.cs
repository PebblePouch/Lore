using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Entries value: one chip per listed Entry ("🐺 Wolf ✕", deleted ones "Missing (Entry) ✕" in red), then "+".
    // Chip click opens the Entry, ✕ removes it from the list, "+" opens the Entry Library in add mode (it stays
    // open and only offers Entries that are not listed yet). Chips wrap; EntryFieldTable gives the row the height
    // of CalcHeight.
    public sealed class FieldValueEntriesElement : FieldValueElement
    {
        #region Types
        private readonly struct Chip
        {
            public readonly Rect Rect;
            public readonly int Index;

            public Chip(
                Rect rect,
                int index)
            {
                Rect = rect;
                Index = index;
            }
        }
        #endregion

        #region Public API
        // Height of the chips at the given value width (at least the normal row height).
        public static float CalcHeight(
            FieldValue fieldValue,
            float width)
        {
            Layout(
                fieldValue,
                new Rect(0f, 0f, width, 0f),
                out float height);

            return Mathf.Max(
                InspectorStyle.FieldTableRowHeight,
                height);
        }
        #endregion

        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                // Active pick: selection color. Empty list: health color (unfinished, nothing added yet).
                if (EntryAssignmentContext.IsTarget(_fieldValue))
                {
                    EditorGUI.DrawRect(
                        rect,
                        InspectorStyle.FieldSelectBackground);
                }
                else if (_fieldValue.EntriesValue.Count == 0)
                {
                    EditorGUI.DrawRect(
                        rect,
                        InspectorStyle.FieldHealthBackground);
                }
            }

            List<Chip> chips =
                Layout(
                    _fieldValue,
                    rect,
                    out _);

            IReadOnlyList<Entry> entries =
                _fieldValue.EntriesValue;

            Entry removed = null;
            bool removeRequested = false;

            foreach (Chip chip in chips)
            {
                if (chip.Index < 0)
                {
                    DrawAddChip(chip.Rect);

                    continue;
                }

                Entry entry = entries[chip.Index];

                if (DrawEntryChip(chip.Rect, entry))
                {
                    removed = entry;
                    removeRequested = true;
                }
            }

            // After drawing: the list must not change while its chips are drawn.
            if (removeRequested)
                RemoveEntry(removed);
        }

        // Returns true when ✕ was clicked.
        private static bool DrawEntryChip(
            Rect rect,
            Entry entry)
        {
            bool missing = entry == null;

            Rect removeRect =
                new(
                    rect.xMax - InspectorStyle.EntriesChipRemoveWidth,
                    rect.y,
                    InspectorStyle.EntriesChipRemoveWidth,
                    rect.height);

            Rect textRect =
                new(
                    rect.x + InspectorStyle.EntriesChipHorizontalPadding,
                    rect.y,
                    removeRect.x - rect.x - InspectorStyle.EntriesChipHorizontalPadding,
                    rect.height);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(
                    rect,
                    missing
                        ? InspectorStyle.EntriesChipMissingColor
                        : InspectorStyle.EntriesChipColor);
            }

            GUI.Label(
                textRect,
                GetChipText(entry),
                missing
                    ? InspectorStyle.EntriesChipMissingText
                    : InspectorStyle.EntriesChipText);

            if (GUI.Button(removeRect, InspectorStyle.EntriesRemoveGlyph, InspectorStyle.EntriesChipButton))
                return true;

            if (!missing)
            {
                EditorGUIUtility.AddCursorRect(textRect, MouseCursor.Link);

                if (Event.current.type == EventType.MouseDown &&
                    Event.current.button == 0 &&
                    textRect.Contains(Event.current.mousePosition))
                {
                    LoreNavigation.NavigateToEntry(entry);

                    Event.current.Use();
                }
            }

            return false;
        }

        private void DrawAddChip(Rect rect)
        {
            // Same button as the row's Clear button (style, size, vertical offset), so both sit on one line.
            Rect buttonRect = rect;

            buttonRect.y +=
                InspectorStyle.FieldEntryButtonVerticalOffset;

            ButtonDrawer.Add(
                buttonRect,
                BeginAdding,
                ButtonDrawer.DisplayMode.Glyph,
                InspectorStyle.FieldButton);
        }

        private void BeginAdding()
        {
            EntryAssignmentContext.BeginAssignment(
                _entry,
                _fieldValue);

            LibraryService
                .OpenEntryLibrary?
                .Invoke();
        }
        #endregion

        #region Helpers
        private void RemoveEntry(Entry entry)
        {
            if (_entry == null)
                return;

            LoreUndo.Record(
                _entry,
                LoreText.Undo.EditFieldValue);

            if (entry == null)
                _fieldValue.RemoveMissingListEntries();
            else
                _fieldValue.RemoveListEntry(entry);

            LoreChangeTracker.MarkModified(_entry);

            LoreRelationshipIndex.Instance.Refresh();

            LoreRepaintService.Repaint();
        }

        // Chip positions in rect (wrapping), the add chip last (Index -1); height = total height used.
        private static List<Chip> Layout(
            FieldValue fieldValue,
            Rect rect,
            out float height)
        {
            var chips = new List<Chip>();

            IReadOnlyList<Entry> entries = fieldValue.EntriesValue;

            float x = rect.x;
            float y = rect.y + InspectorStyle.EntriesVerticalPadding;

            void Place(float width, int index)
            {
                width = Mathf.Min(width, rect.width);

                if (x > rect.x &&
                    x + width > rect.xMax)
                {
                    x = rect.x;
                    y += InspectorStyle.EntriesChipHeight + InspectorStyle.EntriesLineSpacing;
                }

                chips.Add(
                    new Chip(
                        new Rect(x, y, width, InspectorStyle.EntriesChipHeight),
                        index));

                x += width + InspectorStyle.EntriesChipSpacing;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                float textWidth =
                    (entries[i] == null
                        ? InspectorStyle.EntriesChipMissingText
                        : InspectorStyle.EntriesChipText)
                    .CalcSize(new GUIContent(GetChipText(entries[i])))
                    .x;

                Place(
                    InspectorStyle.EntriesChipHorizontalPadding
                    + textWidth
                    + InspectorStyle.EntriesChipRemoveWidth,
                    i);
            }

            Place(
                InspectorStyle.EntriesAddWidth,
                -1);

            height =
                y + InspectorStyle.EntriesChipHeight + InspectorStyle.EntriesVerticalPadding
                - rect.y;

            // A single line is centered in the normal row height.
            float rowHeight = InspectorStyle.FieldTableRowHeight;

            if (height < rowHeight)
            {
                float offset = (rowHeight - height) * 0.5f;

                for (int i = 0; i < chips.Count; i++)
                {
                    Rect chipRect = chips[i].Rect;
                    chipRect.y += offset;

                    chips[i] = new Chip(chipRect, chips[i].Index);
                }
            }

            return chips;
        }

        private static string GetChipText(Entry entry)
        {
            return entry != null
                ? string.Format(LoreText.Drawers.GlyphTitle, entry.Glyph, entry.Title)
                : string.Format(LoreText.Drawers.Missing, LoreText.Drawers.EntryKind);
        }
        #endregion
    }
}
