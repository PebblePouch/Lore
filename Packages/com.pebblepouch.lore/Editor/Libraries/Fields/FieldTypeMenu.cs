using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    // Order of the Field type menu: groups with separators between them. The FieldType numbers are serialized and
    // never change, so the menu order lives here; types missing here are listed at the end.
    public static class FieldTypeMenu
    {
        #region Properties
        public static readonly IReadOnlyList<IReadOnlyList<FieldType>> Groups =
            new[]
            {
                // Basic
                new[]
                {
                    FieldType.Text,
                    FieldType.Url,
                    FieldType.Boolean,
                    FieldType.Integer,
                    FieldType.Float,
                    FieldType.Vector,
                    FieldType.Color
                },

                // Choice
                new[]
                {
                    FieldType.Enum,
                    FieldType.Enums,
                    FieldType.Entry,
                    FieldType.Entries
                },

                // Unity
                new[]
                {
                    FieldType.GameObject,
                    FieldType.Sprite,
                    FieldType.Audio,
                    FieldType.Scene,
                    FieldType.Folder,
                    FieldType.ScriptableObject,
                    FieldType.Object
                }
            };
        #endregion
    }
}
