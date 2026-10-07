using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    [CustomEditor(typeof(GlyphLibraryCollection))]
    public sealed class GlyphLibraryCollectionEditor : UnityEditor.Editor
    {
        #region Public API
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(LibraryStyle.CollectionEditorSpacing);

            if (GUILayout.Button(
                    LoreText.Library.GenerateDefaultGlyphLibrary))
            {
                new GlyphLibraryService()
                    .GenerateDefaultCollection();
            }
        }
        #endregion
    }
}