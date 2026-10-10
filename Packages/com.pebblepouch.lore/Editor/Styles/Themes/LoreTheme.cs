using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    [CreateAssetMenu(
        menuName = "Lore/Editor/Theme")]
    public class LoreTheme : ScriptableObject
    {
        [Header("Actions")]
        public ButtonDefinition Menu;

        public ButtonDefinition Create;
        public ButtonDefinition Delete;

        public ButtonDefinition Add;
        public ButtonDefinition Remove;

        public ButtonDefinition Edit;
        public ButtonDefinition Clear;

        public ButtonDefinition Duplicate;
        public ButtonDefinition Save;

        public ButtonDefinition Open;

        // Field / Tag Library menus: show the Entries that use the item.
        public ButtonDefinition References;

        // Audio Fields: preview playback.
        public ButtonDefinition Play;
        public ButtonDefinition Stop;

        public ButtonDefinition ExportCsv;
        public ButtonDefinition ImportCsv;

        // Tabular Editing: bulk actions for the selected rows.
        public ButtonDefinition AddTag;
        public ButtonDefinition RemoveTag;
        public ButtonDefinition ClearTags;
        public ButtonDefinition DeleteSelected;

        // Welcome page (no Lore Project yet).
        public ButtonDefinition StartLore;
        public ButtonDefinition ImportProject;
        public ButtonDefinition CreateDemo;

        [Header("Badges")]
        public ButtonDefinition Added;
    }
}