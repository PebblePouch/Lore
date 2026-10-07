namespace PebblePouch.Lore.Editor
{
    public static class LoreKeyboardFocus
    {
        #region Fields
        private static LoreKeyboardOwner _owner =
            LoreKeyboardOwner.Content;
        #endregion

        #region Properties
        public static LoreKeyboardOwner Owner =>
            _owner;
        #endregion

        #region Public API
        public static bool IsOwner(
            LoreKeyboardOwner owner)
        {
            return _owner == owner;
        }

        public static void SetOwner(
            LoreKeyboardOwner owner)
        {
            _owner = owner;
        }
        #endregion
    }
}