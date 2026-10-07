using System.Collections.Generic;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryIntegrityAnalyzer
    {
        #region Public API
        public IReadOnlyList<EntryIntegrityIssue> Analyze(
            Entry entry)
        {
            var issues =
                new List<EntryIntegrityIssue>();

            if (entry == null)
                return issues;

            var serializedEntry =
                new SerializedObject(
                    entry);

            SerializedProperty fieldsProperty =
                serializedEntry.FindProperty(
                    "_fields");

            if (fieldsProperty == null ||
                !fieldsProperty.isArray)
            {
                return issues;
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

                if (!SerializedReferenceUtility.IsMissingReference(
                        fieldProperty))
                {
                    continue;
                }

                issues.Add(
                    new EntryIntegrityIssue(
                        entry,
                        i));
            }

            return issues;
        }
        #endregion
    }
}