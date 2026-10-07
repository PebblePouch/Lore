using System.Linq;
using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldIntegrityRepairService
    {
        #region Public API
        public bool RemoveMissingField(
            Category category,
            int fieldIndex)
        {
            if (category == null)
                return false;

            RecordRepair(category, LoreText.Undo.RemoveBrokenField);

            var serializedCategory =
                new SerializedObject(
                    category);

            SerializedProperty fieldsProperty =
                serializedCategory.FindProperty(
                    "_fields");

            if (fieldsProperty == null ||
                !fieldsProperty.isArray)
            {
                return false;
            }

            if (fieldIndex < 0 ||
                fieldIndex >= fieldsProperty.arraySize)
            {
                return false;
            }

            SerializedProperty fieldProperty =
                fieldsProperty.GetArrayElementAtIndex(
                    fieldIndex);

            if (!SerializedReferenceUtility.IsMissingReference(
                    fieldProperty))
            {
                return false;
            }

            int previousSize =
                fieldsProperty.arraySize;

            fieldsProperty.DeleteArrayElementAtIndex(
                fieldIndex);

            if (fieldsProperty.arraySize ==
                previousSize)
            {
                fieldsProperty.DeleteArrayElementAtIndex(
                    fieldIndex);
            }

            serializedCategory.ApplyModifiedPropertiesWithoutUndo();

            LoreChangeTracker.MarkModified(
                category);

            new CategoryFieldSyncService()
                .SyncCategory(
                    category);

            LoreRefreshService.Refresh();

            return true;
        }

        public bool ReplaceMissingField(
    Category category,
    int fieldIndex,
    Field replacement)
        {
            if (category == null ||
                replacement == null)
            {
                return false;
            }

            RecordRepair(category, LoreText.Undo.ReplaceBrokenField);

            var serializedCategory =
                new SerializedObject(
                    category);

            SerializedProperty fieldsProperty =
                serializedCategory.FindProperty(
                    "_fields");

            if (fieldsProperty == null ||
                !fieldsProperty.isArray)
            {
                return false;
            }

            if (fieldIndex < 0 ||
                fieldIndex >= fieldsProperty.arraySize)
            {
                return false;
            }

            SerializedProperty fieldProperty =
                fieldsProperty.GetArrayElementAtIndex(
                    fieldIndex);

            if (!SerializedReferenceUtility.IsMissingReference(
                    fieldProperty))
            {
                return false;
            }

            if (category.Fields.Contains(
                    replacement))
            {
                return false;
            }

            fieldProperty.objectReferenceValue =
                replacement;

            serializedCategory.ApplyModifiedPropertiesWithoutUndo();

            LoreChangeTracker.MarkModified(
                category);

            new CategoryFieldSyncService()
                .SyncCategory(
                    category);

            LoreRefreshService.Refresh();

            return true;
        }
        #endregion

        #region Helpers
        // The repair changes the Category and syncs its Entries' field values: one undo step.
        private static void RecordRepair(
            Category category,
            string action)
        {
            LoreUndo.Record(
                new UnityEngine.Object[] { category }
                    .Concat(
                        LoreRepository.Instance.Entries
                            .Where(entry => entry.Category == category)),
                action);
        }
        #endregion
    }
}