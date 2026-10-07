using System;

using UnityEngine;

namespace PebblePouch.Lore
{
    [Serializable]
    public sealed class TagDefinition
    {
        #region Fields
        [SerializeField, HideInInspector]
        private string _id;

        [SerializeField, HideInInspector]
        private string _title;

        [SerializeField, HideInInspector]
        private Color _color;
        #endregion

        #region Properties
        public string Id => _id;

        public string Title => _title;

        public Color Color => _color;
        #endregion

        #region Public API
        internal void Initialize(
            string title,
            Color color)
        {
            _id =
                System.Guid
                    .NewGuid()
                    .ToString();

            _title = title ?? string.Empty;

            _color = color;
        }

        internal void SetTitle(string title)
        {
            _title = title ?? string.Empty;
        }

        internal void SetColor(Color color) => _color = color;
        #endregion
    }
}