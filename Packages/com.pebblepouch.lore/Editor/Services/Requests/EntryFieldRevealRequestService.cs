namespace PebblePouch.Lore.Editor
{
    public static class EntryFieldRevealRequestService
    {
        #region Fields
        private static Entry _entry;
        private static Field _field;
        #endregion

        #region Public API
        public static void Request(
            Entry entry,
            Field field)
        {
            _entry = entry;
            _field = field;
        }

        public static bool Consume(
            Entry entry,
            Field field)
        {
            if (entry == null ||
                field == null)
            {
                return false;
            }

            if (_entry != entry ||
                _field != field)
            {
                return false;
            }

            _entry = null;
            _field = null;

            return true;
        }
        #endregion
    }
}