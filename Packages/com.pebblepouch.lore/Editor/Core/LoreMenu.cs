namespace PebblePouch.Lore.Editor
{
    // Tools > Lore menu items: paths and order (consts: MenuItem attributes need them, so not in LoreText).
    // Unity sorts by priority and draws a separator between items whose priorities differ by more than 10.
    public static class LoreMenu
    {
        #region Items
        public const string Open = "Tools/Lore/Open Lore";
        public const int OpenPriority = 0;

        public const string GenerateFieldConstants = "Tools/Lore/Generate Field Constants";
        public const int GenerateFieldConstantsPriority = 20;

        public const string ExportProject = "Tools/Lore/Export Project…";
        public const int ExportProjectPriority = 40;

        public const string ImportProject = "Tools/Lore/Import Project…";
        public const int ImportProjectPriority = 41;
        #endregion
    }
}
