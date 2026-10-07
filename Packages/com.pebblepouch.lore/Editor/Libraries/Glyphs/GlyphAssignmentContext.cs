using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class GlyphAssignmentContext
    {
        #region Fields
        private static Action<string> _onAssigned;
        #endregion

        #region Properties
        public static bool IsActive => _onAssigned != null;
        #endregion

        #region Public API
        public static void BeginAssignment(Action<string> onAssigned)
        {
            LoreGUIFocus.Clear();

            _onAssigned = onAssigned;
        }

        public static bool Assign(string glyph)
        {
            if (_onAssigned == null)
                return false;

            Action<string> onAssigned = _onAssigned;

            EndAssignment();

            onAssigned.Invoke(glyph);

            return true;
        }

        public static void EndAssignment()
        {
            LoreGUIFocus.Clear();

            _onAssigned = null;
        }
        #endregion
    }
}