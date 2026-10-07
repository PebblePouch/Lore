using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryEntryGroup
    {
        public Category Category { get; }

        public string Title => Category.Title;

        public Color Color => Category.Color;

        public string Glyph => Category.Glyph;

        public int Count => Entries.Count;

        public IReadOnlyList<Entry> Entries { get; }

        public CategoryEntryGroup(Category category, IEnumerable<Entry> entries)
        {
            Category = category;
            Entries = entries.ToList();
        }
    }
}