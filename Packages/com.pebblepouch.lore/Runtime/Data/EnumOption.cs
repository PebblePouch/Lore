using System;

using UnityEngine;

namespace PebblePouch.Lore
{
    // An option of an Enum Field ("Common", "Rare", "Epic"). Values store the ID, so renaming an option keeps every
    // assigned value. The order of the options is the order in the dropdown (and GetEnumIndex).
    [Serializable]
    public sealed class EnumOption
    {
        #region Fields
        [SerializeField, HideInInspector]
        private string _id;

        [SerializeField, HideInInspector]
        private string _label;
        #endregion

        #region Properties
        public string Id => _id;

        public string Label => _label;
        #endregion

        #region Constructor
        internal EnumOption(string label)
        {
            _id = Guid.NewGuid().ToString();
            _label = label ?? string.Empty;
        }
        #endregion

        #region Editor API
        internal void SetLabel(string label)
        {
            _label = label ?? string.Empty;
        }
        #endregion
    }
}
