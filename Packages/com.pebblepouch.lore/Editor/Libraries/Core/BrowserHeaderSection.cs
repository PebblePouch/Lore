using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class BrowserHeaderSection
    {
        #region Fields
        private string _title;
        private readonly Action _create;
        #endregion

        #region Constructor
        public BrowserHeaderSection(
            string title,
            Action create = null)
        {
            _title = title;
            _create = create;
        }
        #endregion

        #region Public API
        public void Draw()
        {
            EditorGUILayout.BeginHorizontal(
                BrowserStyle.BrowserHeaderLayout);

            GUILayout.Label(
                _title,
                BrowserStyle.BrowserHeaderContent,
                GUILayout.Height(
                    BrowserStyle.BrowserHeaderHeight));

            GUILayout.FlexibleSpace();

            DrawCreateButton();

            EditorGUILayout.EndHorizontal();
        }

        public void SetTitle(string title)
        {
            _title = title ?? string.Empty;
        }
        #endregion

        #region Draw
        private void DrawCreateButton()
        {
            if (_create == null)
                return;

            ButtonDrawer.Create(
                _create,
                ButtonDrawer.DisplayMode.Glyph,
                BrowserStyle.BrowserHeaderButton);
        }
        #endregion
    }
}