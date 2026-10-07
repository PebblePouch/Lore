using System;

using UnityEngine;

namespace PebblePouch.Lore
{
    // A project-level link (e.g. repository, board, documentation) shown on the Project page.
    [Serializable]
    public sealed class ProjectLink
    {
        #region Fields
        [SerializeField, HideInInspector]
        private string _label;

        [SerializeField, HideInInspector]
        private string _url;
        #endregion

        #region Properties
        public string Label => _label;

        public string Url => _url;
        #endregion

        #region Constructor
        internal ProjectLink(
            string label,
            string url)
        {
            _label = label ?? string.Empty;
            _url = url ?? string.Empty;
        }
        #endregion

        #region Public API
        internal void SetLabel(string label)
        {
            _label = label ?? string.Empty;
        }

        internal void SetUrl(string url)
        {
            _url = url ?? string.Empty;
        }
        #endregion
    }
}
