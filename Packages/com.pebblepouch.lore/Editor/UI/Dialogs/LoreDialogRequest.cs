using System;

namespace PebblePouch.Lore.Editor
{
    public enum LoreDialogMode
    {
        Confirmation,
        Destructive,
        Warning
    }

    public sealed class LoreDialogRequest
    {
        #region Properties
        public string Title { get; }
        public string Message { get; }
        public string ConfirmLabel { get; }
        public string CancelLabel { get; }
        public LoreDialogMode Mode { get; }
        public Action Confirmed { get; }
        public Action Cancelled { get; }
        public bool HasCancel => !string.IsNullOrWhiteSpace(CancelLabel);
        #endregion

        #region Constructor
        public LoreDialogRequest(
            string title,
            string message,
            string confirmLabel,
            string cancelLabel,
            LoreDialogMode mode,
            Action confirmed,
            Action cancelled = null)
        {
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
            ConfirmLabel = confirmLabel ?? LoreText.Common.OkButton;
            CancelLabel = cancelLabel ?? string.Empty;
            Mode = mode;
            Confirmed = confirmed;
            Cancelled = cancelled;
        }
        #endregion
    }
}