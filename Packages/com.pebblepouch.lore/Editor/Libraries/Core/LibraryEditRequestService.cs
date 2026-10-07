namespace PebblePouch.Lore.Editor
{
    public static class LibraryEditRequestService
    {
        #region Fields
        private static Field _requestedField;
        private static TagDefinition _requestedTag;
        #endregion

        #region Public API
        public static void RequestField(Field field) => _requestedField = field;

        public static void RequestTag(TagDefinition tag) => _requestedTag = tag;

        public static bool ConsumeField(Field field)
        {
            if (field == null || _requestedField != field)
            {
                return false;
            }

            _requestedField = null;

            return true;
        }

        public static bool ConsumeTag(TagDefinition tag)
        {
            if (tag == null || _requestedTag != tag)
            {
                return false;
            }

            _requestedTag = null;

            return true;
        }
        #endregion
    }
}