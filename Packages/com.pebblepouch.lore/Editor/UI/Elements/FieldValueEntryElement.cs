using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueEntryElement : FieldValueElement
    {
        #region Fields
        private readonly EntryFieldValueCard _card =
            new();

        // Tabular Editing: compact card (see EntryFieldValueCard.DrawCompact).
        public bool Compact { get; set; }

        private readonly RelationshipIntegrityAnalyzer _integrityAnalyzer =
            new(LoreRepository.Instance);
        #endregion

        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            bool isActive =
                EntryAssignmentContext.IsTarget(
                    _fieldValue);

            bool isBroken =
                _integrityAnalyzer.HasMissingReference(
                    _entry,
                    _fieldValue);

            // Disabled (selected row in Tabular Editing): no marks over the row's selection color, no clicks.
            bool enabled = GUI.enabled;

            if (enabled && isActive)
            {
                EditorGUI.DrawRect(
                    rect,
                    InspectorStyle.FieldSelectBackground);
            }
            else if (enabled && isBroken)
            {
                EditorGUI.DrawRect(
                    rect,
                    InspectorStyle.FieldBrokenBackground);
            }

            if (Compact)
                _card.DrawCompact(rect, _fieldValue.EntryValue);
            else
                _card.Draw(rect, _fieldValue.EntryValue);

            if (!enabled)
                return;

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type !=
                    EventType.MouseDown ||
                Event.current.button != 0 ||
                !rect.Contains(
                    Event.current.mousePosition))
            {
                return;
            }

            EntryAssignmentContext.BeginAssignment(
                _entry,
                _fieldValue);

            LibraryService
                .OpenEntryLibrary?
                .Invoke();

            Event.current.Use();
        }
        #endregion
    }
}