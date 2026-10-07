using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LibraryEditingService
    {
        #region Fields
        private static Type _ownerType;

        private static Func<bool> _commit;
        private static Action _cancel;
        #endregion

        #region Properties
        public static string EditingId { get; private set; }

        public static bool IsActive =>
            !string.IsNullOrEmpty(EditingId);
        #endregion

        #region Public API
        public static bool IsEditing(string id)
        {
            return EditingId == id;
        }

        public static bool IsEditing<T>() where T : LibraryItem
        {
            return IsActive &&
                _ownerType == typeof(T);
        }

        public static bool BeginEditing<T>(
            string id,
            Func<bool> commit,
            Action cancel)
            where T : LibraryItem
        {
            if (IsActive &&
                EditingId != id)
            {
                if (!CommitEditing())
                    return false;
            }

            _ownerType = typeof(T);

            EditingId = id;

            _commit = commit;
            _cancel = cancel;

            return true;
        }

        public static bool HandleEscape<T>()
            where T : LibraryItem
        {
            if (!IsActive)
                return false;

            if (_ownerType != typeof(T))
                return false;

            if (Event.current.type != EventType.KeyDown)
                return false;

            if (Event.current.keyCode != KeyCode.Escape)
                return false;

            CancelEditing();

            Event.current.Use();

            return true;
        }

        public static void HandleOutsideClick<T>(
            string id,
            Rect rect)
            where T : LibraryItem
        {
            if (_ownerType != typeof(T))
                return;

            if (!IsEditing(id))
                return;

            if (Event.current.type != EventType.MouseDown)
                return;

            if (rect.Contains(
                    Event.current.mousePosition))
            {
                return;
            }

            CommitEditing();
        }

        public static bool CommitEditing()
        {
            if (!IsActive)
                return true;

            if (_commit != null &&
                !_commit.Invoke())
            {
                return false;
            }

            Clear();

            return true;
        }

        public static bool HandleCommit<T>() where T : LibraryItem
        {
            if (!IsActive)
                return false;

            if (_ownerType != typeof(T))
                return false;

            if (Event.current.type != EventType.KeyDown)
                return false;

            if (Event.current.keyCode != KeyCode.Return &&
                Event.current.keyCode != KeyCode.KeypadEnter)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(
                    GUI.GetNameOfFocusedControl()))
            {
                return false;
            }

            CommitEditing();

            Event.current.Use();

            return true;
        }

        public static void CancelEditing()
        {
            if (!IsActive)
                return;

            _cancel?.Invoke();

            Clear();
        }
        #endregion

        #region Helpers
        private static void Clear()
        {
            EditingId = null;

            _ownerType = null;

            _commit = null;
            _cancel = null;
        }
        #endregion
    }
}