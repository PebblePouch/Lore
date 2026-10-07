using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueSceneElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            EditorGUI.BeginChangeCheck();

            SceneAsset value =
                (SceneAsset)EditorGUI.ObjectField(
                    rect,
                    GUIContent.none,
                    _fieldValue.SceneValue as SceneAsset,
                    typeof(SceneAsset),
                    false);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetScene(
                    value,
                    value != null
                        ? AssetDatabase.GetAssetPath(value)
                        : string.Empty);

                Save();
            }
        }
        #endregion
    }
}
