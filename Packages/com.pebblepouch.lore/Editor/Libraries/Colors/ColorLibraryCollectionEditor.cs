using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    [CustomEditor(typeof(ColorLibraryCollection))]
    public sealed class ColorLibraryCollectionEditor : UnityEditor.Editor
    {
        #region Public API
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(LibraryStyle.CollectionEditorSpacing);

            if (GUILayout.Button(
                    LoreText.Library.GenerateDefaultColorLibrary))
            {
                new ColorLibraryService()
                    .GenerateDefaultCollection();
            }
        }
        #endregion
    }
}
