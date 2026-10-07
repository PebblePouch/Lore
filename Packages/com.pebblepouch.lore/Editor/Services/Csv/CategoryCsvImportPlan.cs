using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    // Result of analyzing a Category CSV (CategoryCsvImportAnalyzer): what an import would change, without having
    // changed anything. Shown in the preview; applied as a whole.
    public sealed class CategoryCsvImportPlan
    {
        #region Types
        public enum ColumnKind
        {
            Id,
            Title,
            Subtitle,
            Glyph,
            Tags,
            Description,
            Field,
        }

        public sealed class Column
        {
            public int Index { get; }
            public ColumnKind Kind { get; }
            public string Header { get; }

            // Field columns only.
            public Field Field { get; }

            public Column(
                int index,
                ColumnKind kind,
                string header,
                Field field = null)
            {
                Index = index;
                Kind = kind;
                Header = header;
                Field = field;
            }
        }

        // An Entry reference in a cell: an existing Entry, or an Entry this import creates.
        public readonly struct EntryReference
        {
            public Entry Existing { get; }
            public Row NewRow { get; }

            public EntryReference(
                Entry existing,
                Row newRow)
            {
                Existing = existing;
                NewRow = newRow;
            }
        }

        // A parsed cell; only the members of its column's kind / Field type are used.
        public sealed class Value
        {
            // Text, URL, Subtitle, Glyph, Description (editable link form; converted when applied).
            public string Text;
            public bool Bool;
            public int Int;
            public float Float;
            public string EnumOptionId;
            public UnityEngine.Object Asset;
            public List<EntryReference> Entries;
            public List<string> TagIds;

            // Description: editable label -> ID of links whose Entry is missing (kept when applied).
            public Dictionary<string, string> BrokenLinkIds;
        }

        public sealed class Change
        {
            public Column Column { get; }
            public Value Value { get; }

            public Change(
                Column column,
                Value value)
            {
                Column = column;
                Value = value;
            }
        }

        public sealed class Row
        {
            public int Number { get; }

            // Null: new Entry.
            public Entry Entry { get; }

            // Title after the import (new name, renamed name or unchanged).
            public string Title { get; set; }

            public bool IsNew => Entry == null;
            public bool Renames => Entry != null && Title != Entry.Title;

            public List<Change> Changes { get; } = new();

            public Row(
                int number,
                Entry entry,
                string title)
            {
                Number = number;
                Entry = entry;
                Title = title;
            }
        }
        #endregion

        #region Properties
        public Category Category { get; }
        public string Path { get; }

        // Set when the file cannot be imported at all (unreadable, no known columns).
        public string Error { get; set; }

        public List<Column> Columns { get; } = new();
        public List<Row> Rows { get; } = new();
        public List<string> Warnings { get; } = new();

        // Entries of the Category without a row in the file (stay as they are).
        public int UntouchedEntryCount { get; set; }

        public int CreatedCount => Rows.Count(row => row.IsNew);
        public int RenamedCount => Rows.Count(row => row.Renames);
        public int UpdatedCount => Rows.Count(row => !row.IsNew && (row.Changes.Count > 0 || row.Renames));
        public int UnchangedCount => Rows.Count(row => !row.IsNew && row.Changes.Count == 0 && !row.Renames);
        public int ChangedValueCount => Rows.Where(row => !row.IsNew).Sum(row => row.Changes.Count);

        public bool HasChanges => Error == null && (CreatedCount > 0 || UpdatedCount > 0);
        #endregion

        #region Constructor
        public CategoryCsvImportPlan(
            Category category,
            string path)
        {
            Category = category;
            Path = path;
        }
        #endregion
    }
}
