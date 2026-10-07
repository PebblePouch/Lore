using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore
{
    // Limits what the Lore pickers offer for a field in your own scripts (editor only, no effect at runtime):
    //   [LoreFilter(Category = "Characters")] public Entry hero;
    //   [LoreFilter(Category = "Items|Weapons")] public List<Entry> loot;
    // Several values in one criterion are separated by "|" (= or; "|" is not allowed in Lore names).
    //   [LoreFilter(Category = "Characters", FieldType = FieldType.Integer)] public Field healthField;
    // Criteria are combined (= and).
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LoreFilterAttribute : PropertyAttribute
    {
        #region Constants
        public const char ValueSeparator = '|';
        #endregion

        #region Fields
        private FieldType _fieldType;
        #endregion

        #region Properties
        // Category titles (case-insensitive). Entry fields: Entries of these Categories.
        // Field fields: Fields used by these Categories. Category fields: these Categories.
        public string Category { get; set; }

        // Field fields only: Fields of this type (one type; leave out for any type).
        public FieldType FieldType
        {
            get => _fieldType;
            set
            {
                _fieldType = value;
                HasFieldType = true;
            }
        }

        // Attribute arguments cannot be nullable: tells "FieldType = Text" apart from no FieldType criterion.
        public bool HasFieldType { get; private set; }
        #endregion

        #region Public API
        // The Category titles of the filter (empty: no Category filter).
        public IReadOnlyList<string> GetCategories() => Split(Category);
        #endregion

        #region Helpers
        private static IReadOnlyList<string> Split(string value)
        {
            var values = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
                return values;

            foreach (string part in value.Split(ValueSeparator))
            {
                string trimmed = part.Trim();

                if (trimmed.Length > 0)
                    values.Add(trimmed);
            }

            return values;
        }
        #endregion
    }
}
