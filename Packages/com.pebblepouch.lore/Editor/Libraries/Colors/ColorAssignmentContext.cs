using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class ColorAssignmentContext
    {
        #region Fields
        private static Action<Color> _onAssigned;
        #endregion

        #region Properties
        public static bool IsActive =>
            _onAssigned != null;
        #endregion

        #region Public API
        public static void BeginAssignment(
            Action<Color> onAssigned)
        {
            LoreGUIFocus.Clear();

            _onAssigned = onAssigned;
        }

        public static bool Assign(
            Color color)
        {
            if (_onAssigned == null)
                return false;

            Action<Color> onAssigned =
                _onAssigned;

            EndAssignment();

            onAssigned.Invoke(color);

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