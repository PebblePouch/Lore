using System.Collections.Generic;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    // Finds GameObject/Sprite/Scene/Folder/ScriptableObject/Audio/Object field values that still store a reference,
    // but whose Unity asset was deleted ("Missing" in Unity's object fields).
    // Mirrors RelationshipIntegrityAnalyzer, which does the same for Entry references.
    public sealed class UnityReferenceIntegrityAnalyzer
    {
        #region Fields
        private readonly LoreRepository _repository;
        #endregion

        #region Constructor
        public UnityReferenceIntegrityAnalyzer(
            LoreRepository repository)
        {
            _repository =
                repository;
        }
        #endregion

        #region Public API
        public IReadOnlyList<UnityReferenceIntegrityIssue> Analyze()
        {
            var issues =
                new List<UnityReferenceIntegrityIssue>();

            foreach (Entry entry in
                     _repository.Entries)
            {
                AnalyzeEntry(
                    entry,
                    issues);
            }

            return issues;
        }

        public IReadOnlyList<UnityReferenceIntegrityIssue> Analyze(
            Entry entry)
        {
            var issues =
                new List<UnityReferenceIntegrityIssue>();

            if (entry == null)
                return issues;

            AnalyzeEntry(
                entry,
                issues);

            return issues;
        }
        #endregion

        #region Helpers
        private static void AnalyzeEntry(
            Entry entry,
            List<UnityReferenceIntegrityIssue> issues)
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

                Field field =
                    fieldProperty?.objectReferenceValue
                    as Field;

                if (field == null)
                    continue;

                string valuePropertyName =
                    GetValuePropertyName(
                        field.Type);

                if (valuePropertyName == null)
                    continue;

                SerializedProperty valueProperty =
                    fieldValueProperty.FindPropertyRelative(
                        valuePropertyName);

                if (valueProperty == null ||
                    !SerializedReferenceUtility.IsMissingReference(
                        valueProperty))
                {
                    continue;
                }

                issues.Add(
                    new UnityReferenceIntegrityIssue(
                        entry,
                        field));
            }
        }

        // Serialized FieldValue member holding the Unity reference for this field type.
        private static string GetValuePropertyName(FieldType type)
        {
            return type switch
            {
                FieldType.GameObject => "_gameObjectValue",
                FieldType.Sprite => "_spriteValue",
                FieldType.Scene => "_sceneValue",
                FieldType.Folder => "_folderValue",
                FieldType.ScriptableObject => "_objectValue",
                FieldType.Object => "_objectValue",
                FieldType.Audio => "_objectValue",
                _ => null
            };
        }
        #endregion
    }
}
