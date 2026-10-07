using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    [Serializable]
    public sealed class ColorDefinition
    {
        #region Fields
        [SerializeField, HideInInspector]
        private Color _color = Color.white;

        [SerializeField, HideInInspector]
        private string _title = string.Empty;

        [SerializeField, HideInInspector]
        private string _category = string.Empty;
        #endregion

        #region Properties
        public Color Color => _color;

        public string Title => _title;

        public string Category => _category;
        #endregion

        #region Constructor
        public ColorDefinition(
            Color color,
            string title,
            string category)
        {
            _color = color;
            _title = title;
            _category = category;
        }
        #endregion
    }
}