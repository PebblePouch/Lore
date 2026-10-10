using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Folder value: an object field for project folders (folders are DefaultAssets; other DefaultAssets, e.g. unknown
    // files, are ignored). Stores the folder and its path (see FieldValue.SetFolder).
    public sealed class FieldValueFolderElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            if (!DrawField(
                    rect,
                    _fieldValue.FolderValue,
                    out DefaultAsset folder))
            {
                return;
            }

            RecordChange();

            _fieldValue.SetFolder(
                folder,
                GetPath(folder));

            Save();
        }
        #endregion

        #region Helpers
        // Returns true when a folder (or none) was picked; picks that are no folder are ignored.
        internal static bool DrawField(
            Rect rect,
            Object current,
            out DefaultAsset folder)
        {
            EditorGUI.BeginChangeCheck();

            folder =
                (DefaultAsset)EditorGUI.ObjectField(
                    rect,
                    GUIContent.none,
                    current as DefaultAsset,
                    typeof(DefaultAsset),
                    false);

            if (!EditorGUI.EndChangeCheck())
                return false;

            return folder == null ||
                   IsFolder(folder);
        }

        internal static bool IsFolder(Object asset) =>
            asset != null &&
            AssetDatabase.IsValidFolder(
                AssetDatabase.GetAssetPath(
                    asset));

        internal static string GetPath(Object folder) =>
            folder != null
                ? AssetDatabase.GetAssetPath(folder)
                : string.Empty;
        #endregion
    }
}
