namespace PebblePouch.Lore.Editor
{
    public static class ContentBrowserFilterContext
    {
        #region Properties
        public static Field MatchingField { get; private set; }
        #endregion

        #region Public API
        public static void SetMatchingField(
            Field field)
        {
            MatchingField =
                field;
        }

        public static bool IsMatchingField(
            Field field)
        {
            return field != null &&
                   MatchingField != null &&
                   field == MatchingField;
        }

        public static void Clear()
        {
            MatchingField =
                null;
        }
        #endregion
    }
}