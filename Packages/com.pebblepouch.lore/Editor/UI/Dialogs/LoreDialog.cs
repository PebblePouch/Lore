using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LoreDialog
    {
        #region Fields
        private static LoreDialogRequest _current;
        #endregion

        #region Properties
        public static bool IsOpen =>
            _current != null;

        public static LoreDialogRequest Current =>
            _current;
        #endregion

        #region Public API
        // Label defaults are null (optional parameters require constants) and resolved from LoreText.
        public static void Confirm(
            string title,
            string message,
            string confirmLabel,
            Action confirmed,
            string cancelLabel = null,
            Action cancelled = null)
        {
            Show(
                new LoreDialogRequest(
                    title,
                    message,
                    confirmLabel,
                    cancelLabel ?? LoreText.Common.CancelButton,
                    LoreDialogMode.Confirmation,
                    confirmed,
                    cancelled));
        }

        public static void ConfirmDestructive(
            string title,
            string message,
            string confirmLabel,
            Action confirmed,
            string cancelLabel = null,
            Action cancelled = null)
        {
            Show(
                new LoreDialogRequest(
                    title,
                    message,
                    confirmLabel,
                    cancelLabel ?? LoreText.Common.CancelButton,
                    LoreDialogMode.Destructive,
                    confirmed,
                    cancelled));
        }

        public static void Warning(
            string title,
            string message,
            string confirmLabel = null,
            Action acknowledged = null)
        {
            Show(
                new LoreDialogRequest(
                    title,
                    message,
                    confirmLabel ?? LoreText.Common.OkButton,
                    string.Empty,
                    LoreDialogMode.Warning,
                    acknowledged));
        }

        public static void Confirm()
        {
            if (_current == null)
                return;

            Action action =
                _current.Confirmed;

            Close();

            action?.Invoke();
        }

        public static void Cancel()
        {
            if (_current == null)
                return;

            Action action =
                _current.Cancelled;

            Close();

            action?.Invoke();
        }

        public static void Close()
        {
            if (_current == null)
                return;

            _current =
                null;

            LoreRepaintService.Repaint();
        }
        #endregion

        #region Helpers
        private static void Show(
            LoreDialogRequest request)
        {
            if (request == null)
                return;

            _current =
                request;

            GUI.FocusControl(
                null);

            EditorGUI.FocusTextInControl(
                null);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}