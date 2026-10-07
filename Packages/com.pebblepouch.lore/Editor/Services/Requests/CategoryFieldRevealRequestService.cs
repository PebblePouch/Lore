namespace PebblePouch.Lore.Editor
{
    public static class CategoryFieldRevealRequestService
    {
        #region Fields
        private static Category _category;
        private static int _fieldIndex = -1;
        #endregion

        #region Public API
        public static void Request(
            Category category,
            int fieldIndex)
        {
            if (category == null ||
                fieldIndex < 0)
            {
                return;
            }

            _category =
                category;

            _fieldIndex =
                fieldIndex;
        }

        public static bool Consume(
            Category category,
            int fieldIndex)
        {
            if (category == null ||
                fieldIndex < 0)
            {
                return false;
            }

            if (_category != category ||
                _fieldIndex != fieldIndex)
            {
                return false;
            }

            _category =
                null;

            _fieldIndex =
                -1;

            return true;
        }
        #endregion
    }
}