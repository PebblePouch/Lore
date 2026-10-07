using System.Collections.Generic;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryIntegrityAnalyzer
    {
        #region Public API
        public IReadOnlyList<CategoryIntegrityIssue> Analyze(
            Category category)
        {
            var issues =
                new List<CategoryIntegrityIssue>();

            if (category == null)
                return issues;

            var serializedCategory =
                new SerializedObject(
                    category);

            SerializedProperty fieldsProperty =
                serializedCategory.FindProperty(
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
                SerializedProperty fieldProperty =
                    fieldsProperty.GetArrayElementAtIndex(
                        i);

                if (!SerializedReferenceUtility.IsMissingReference(
                        fieldProperty))
                {
                    continue;
                }

                issues.Add(
                    new CategoryIntegrityIssue(
                        category,
                        i));
            }

            return issues;
        }
        #endregion
    }
}