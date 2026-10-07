namespace PebblePouch.Lore.Editor
{
    public readonly struct LoreOperationResult
    {
        #region Properties
        public bool Success { get; }
        public string Message { get; }
        #endregion

        #region Constructor
        private LoreOperationResult(
            bool success,
            string message)
        {
            Success = success;
            Message = message ?? string.Empty;
        }
        #endregion

        #region Public API
        public static LoreOperationResult Succeeded()
        {
            return new LoreOperationResult(
                true,
                string.Empty);
        }

        public static LoreOperationResult Failed(
            string message)
        {
            return new LoreOperationResult(
                false,
                message);
        }
        #endregion
    }
}