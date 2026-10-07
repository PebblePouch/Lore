using System.Collections.Generic;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class RelationshipIntegrityAnalyzer
    {
        #region Fields
        private readonly LoreRepository _repository;
        #endregion

        #region Constructor
        public RelationshipIntegrityAnalyzer(
            LoreRepository repository)
        {
            _repository =
                repository;
        }
        #endregion

        #region Public API
        public IReadOnlyList<RelationshipIntegrityIssue> Analyze()
        {
            var issues =
                new List<RelationshipIntegrityIssue>();

            foreach (Entry entry in
                     _repository.Entries)
            {
                AnalyzeEntry(
                    entry,
                    issues);
            }

            return issues;
        }

        public IReadOnlyList<RelationshipIntegrityIssue> Analyze(
            Entry entry)
        {
            var issues =
                new List<RelationshipIntegrityIssue>();

            if (entry == null)
                return issues;

            AnalyzeEntry(
                entry,
                issues);

            return issues;
        }

        public bool HasMissingReference(
            Entry entry,
            FieldValue fieldValue)
        {
            if (entry == null || fieldValue?.Field == null)
            {
                return false;
            }

            if (!FieldValueUtility.IsEntryReference(fieldValue.Field.Type))
            {
                return false;
            }

            int fieldIndex =
                FindFieldIndex(
                    entry,
                    fieldValue);

            if (fieldIndex < 0)
                return false;

            var serializedEntry = new SerializedObject(entry);

            SerializedProperty fieldsProperty = serializedEntry.FindProperty("_fields");

            if (fieldsProperty == null || fieldIndex >= fieldsProperty.arraySize)
            {
                return false;
            }

            SerializedProperty fieldValueProperty =
                fieldsProperty.GetArrayElementAtIndex(
                    fieldIndex);

            return HasMissingEntry(
                fieldValueProperty,
                fieldValue.Field.Type);
        }
        #endregion

        #region Helpers
        private static void AnalyzeEntry(
            Entry entry,
            List<RelationshipIntegrityIssue> issues)
        {
            if (entry == null)
                return;

            var serializedEntry =
                new SerializedObject(
                    entry);

            SerializedProperty fieldsProperty =
                serializedEntry.FindProperty(
                    "_fields");

            if (fieldsProperty == null ||
                !fieldsProperty.isArray)
            {
                return;
            }

            for (int i = 0;
                 i < fieldsProperty.arraySize;
                 i++)
            {
                SerializedProperty fieldValueProperty =
                    fieldsProperty.GetArrayElementAtIndex(
                        i);

                SerializedProperty fieldProperty =
                    fieldValueProperty.FindPropertyRelative(
                        "_field");

                if (fieldProperty == null)
                    continue;

                Field field =
                    fieldProperty.objectReferenceValue
                    as Field;

                if (field == null ||
                    !FieldValueUtility.IsEntryReference(field.Type))
                {
                    continue;
                }

                if (!HasMissingEntry(
                        fieldValueProperty,
                        field.Type))
                {
                    continue;
                }

                issues.Add(
                    new RelationshipIntegrityIssue(
                        entry,
                        field));
            }
        }

        // Entry fields: the reference is missing. Entries fields: at least one listed Entry is missing
        // (one issue per Field, like Entry fields).
        private static bool HasMissingEntry(
            SerializedProperty fieldValueProperty,
            FieldType type)
        {
            if (type == FieldType.Entry)
            {
                SerializedProperty entryValueProperty =
                    fieldValueProperty.FindPropertyRelative(
                        "_entryValue");

                return entryValueProperty != null &&
                       SerializedReferenceUtility.IsMissingReference(entryValueProperty);
            }

            SerializedProperty entriesProperty =
                fieldValueProperty.FindPropertyRelative(
                    "_entriesValue");

            if (entriesProperty == null ||
                !entriesProperty.isArray)
            {
                return false;
            }

            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                if (SerializedReferenceUtility.IsMissingReference(
                        entriesProperty.GetArrayElementAtIndex(i)))
                {
                    return true;
                }
            }

            return false;
        }

        private static int FindFieldIndex(
            Entry entry,
            FieldValue fieldValue)
        {
            for (int i = 0;
                 i < entry.Fields.Count;
                 i++)
            {
                if (ReferenceEquals(
                        entry.Fields[i],
                        fieldValue))
                {
                    return i;
                }
            }

            return -1;
        }
        #endregion
    }
}