using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Target of an Entry Library pick: one Entry's Field value (Entry page, Tabular Editing cells), or one Field on
    // several Entries (bulk: Tabular Editing selection row; picks apply to all of them as one undo step).
    public static class EntryAssignmentContext
    {
        #region Fields
        public static Entry TargetEntry;
        public static FieldValue TargetFieldValue;

        // Bulk target (null when not bulk).
        private static List<Entry> _bulkEntries;
        private static Field _bulkField;

        public static bool IsBulk => _bulkEntries != null && _bulkField != null;

        public static bool IsActive => (TargetEntry != null && TargetFieldValue != null) || IsBulk;
        public static bool IsTarget(FieldValue fieldValue) => TargetFieldValue == fieldValue;
        public static bool IsBulkTarget(Field field) => IsBulk && _bulkField == field;

        public static Field TargetField => IsBulk ? _bulkField : TargetFieldValue?.Field;

        // Entries fields: the Library adds to the list and stays open, offering only Entries not listed yet
        // (bulk: not listed in all target Entries).
        public static bool IsListTarget =>
            IsActive &&
            TargetField != null &&
            TargetField.Type == FieldType.Entries;

        // Pickable for the target: from an allowed Category (Field restriction) and, for lists, not listed yet.
        public static bool CanAdd(Entry entry) =>
            entry != null &&
            (TargetField == null || TargetField.AllowsCategory(entry.Category)) &&
            (!IsListTarget || !IsListedEverywhere(entry));

        // The target Field limits the Categories (the Library then shows a filter).
        public static bool IsRestricted =>
            IsActive &&
            TargetField != null &&
            TargetField.AllowedCategories.Any(category => category != null);

        // Current value of an Entry field target (bulk: the Entry all targets share), for the Library preselection.
        public static Entry CurrentEntry
        {
            get
            {
                if (!IsBulk)
                    return TargetFieldValue?.EntryValue;

                Entry shared = null;
                bool first = true;

                foreach (FieldValue value in GetBulkValues())
                {
                    if (first)
                        shared = value.EntryValue;
                    else if (value.EntryValue != shared)
                        return null;

                    first = false;
                }

                return shared;
            }
        }
        #endregion

        #region Public API
        public static void BeginAssignment(Entry entry, FieldValue fieldValue)
        {
            LoreGUIFocus.Clear();

            _bulkEntries = null;
            _bulkField = null;

            TargetEntry = entry;
            TargetFieldValue = fieldValue;
        }

        public static void BeginBulkAssignment(IEnumerable<Entry> entries, Field field)
        {
            LoreGUIFocus.Clear();

            TargetEntry = null;
            TargetFieldValue = null;

            _bulkEntries = entries.Where(entry => entry != null).ToList();
            _bulkField = field;
        }

        public static bool Assign(Entry entry)
        {
            if (!IsActive)
                return false;

            if (entry == null)
                return false;

            bool isList = IsListTarget;

            bool assigned =
                IsBulk
                    ? AssignBulk(entry, isList)
                    : AssignSingle(entry, isList);

            if (!assigned)
                return false;

            LoreRelationshipIndex.Instance.Refresh();

            LoreRepaintService.Repaint();

            // Lists: keep adding until the Library is closed (Esc, click elsewhere).
            if (!isList)
                EndAssignment();

            return true;
        }

        public static void EndAssignment()
        {
            LoreGUIFocus.Clear();

            TargetEntry = null;
            TargetFieldValue = null;

            _bulkEntries = null;
            _bulkField = null;

            LibraryService.CloseLibrary?.Invoke();
        }

        public static void CheckInteraction(Rect rect)
        {
            if (!IsActive)
                return;

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown)
                return;

            if (!rect.Contains(Event.current.mousePosition))
                return;

            EndAssignment();

            Event.current.Use();
        }
        #endregion

        #region Helpers
        private static bool AssignSingle(
            Entry entry,
            bool isList)
        {
            LoreUndo.Record(
                TargetEntry,
                LoreText.Undo.EditFieldValue);

            if (isList)
            {
                if (!TargetFieldValue.AddListEntry(entry))
                    return false;
            }
            else
            {
                TargetFieldValue.SetEntry(entry);
            }

            LoreChangeTracker.MarkModified(TargetEntry);

            return true;
        }

        // Entry fields: all targets get the Entry. Lists: added where it is not listed yet.
        private static bool AssignBulk(
            Entry entry,
            bool isList)
        {
            List<Entry> targets =
                _bulkEntries
                    .Where(target =>
                        target != null &&
                        target.TryGetValue(_bulkField, out FieldValue value) &&
                        (!isList || !value.EntriesValue.Contains(entry)))
                    .ToList();

            if (targets.Count == 0)
                return false;

            LoreUndo.Record(
                targets,
                LoreText.Undo.EditFieldValue);

            foreach (Entry target in targets)
            {
                target.TryGetValue(_bulkField, out FieldValue value);

                if (isList)
                    value.AddListEntry(entry);
                else
                    value.SetEntry(entry);

                LoreChangeTracker.MarkModified(target);
            }

            return true;
        }

        private static bool IsListedEverywhere(Entry entry)
        {
            if (!IsBulk)
                return TargetFieldValue.EntriesValue.Contains(entry);

            return GetBulkValues().All(value => value.EntriesValue.Contains(entry));
        }

        // Field values of the bulk targets (targets without a value for the Field are left out).
        private static IEnumerable<FieldValue> GetBulkValues()
        {
            foreach (Entry entry in _bulkEntries)
            {
                if (entry != null &&
                    entry.TryGetValue(_bulkField, out FieldValue value))
                {
                    yield return value;
                }
            }
        }
        #endregion
    }
}
