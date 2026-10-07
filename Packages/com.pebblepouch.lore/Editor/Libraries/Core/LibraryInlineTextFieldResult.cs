namespace PebblePouch.Lore.Editor
{
    public enum LibraryInlineTextFieldAction
    {
        None,
        Commit,
        Cancel
    }

    public readonly struct LibraryInlineTextFieldResult
    {
        #region Constructor
        public LibraryInlineTextFieldResult(string value, LibraryInlineTextFieldAction action)
        {
            Value = value;
            Action = action;
        }
        #endregion

        #region Properties
        public string Value { get; }

        public LibraryInlineTextFieldAction Action { get; }

        public bool DidCommit => Action == LibraryInlineTextFieldAction.Commit;

        public bool DidCancel => Action == LibraryInlineTextFieldAction.Cancel;
        #endregion
    }
}