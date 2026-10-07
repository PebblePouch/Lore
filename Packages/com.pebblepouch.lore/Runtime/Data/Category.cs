using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore
{
    public class Category : LoreAsset
    {
        #region Fields
        [SerializeField, HideInInspector]
        private string _glyph;

        [SerializeField, HideInInspector]
        private Color _color = Color.black;

        [SerializeField, HideInInspector]
        private string _description;

        [SerializeField, HideInInspector]
        private int _sortOrder;

        [SerializeField, HideInInspector]
        private List<Field> _fields = new();

        // Fields of this Category that every Entry should fill (Health: "missing required values").
        [SerializeField, HideInInspector]
        private List<Field> _requiredFields = new();
        #endregion

        #region Properties
        public string Title => name;
        
        public string Glyph => _glyph;
        
        public Color Color => _color;
        
        // Stored form with [[id|Title]] links; games use LoreProject.GetPlainText / FormatDescription.
        public string Description => _description;
        
        public int SortOrder => _sortOrder;
        
        public IReadOnlyList<Field> Fields => _fields;

        public IReadOnlyList<Field> RequiredFields => _requiredFields ??= new();
        #endregion

        #region Public API
        internal void SetGlyph(string glyph)
        {
            _glyph = glyph ?? string.Empty;
        }
        
        internal void SetColor(Color color) => _color = color;

        internal void SetDescription(string description)
        {
            _description = description ?? string.Empty;
        }

        internal void SetSortOrder(int sortOrder) => _sortOrder = sortOrder;

        // True when Entries of this Category should fill this Field.
        public bool IsRequired(Field field)
        {
            return field != null &&
                   _requiredFields != null &&
                   _requiredFields.Contains(field);
        }

        // Returns true if it changed.
        internal bool SetRequired(
            Field field,
            bool required)
        {
            if (field == null)
                return false;

            _requiredFields ??= new List<Field>();

            if (required)
            {
                if (_requiredFields.Contains(field) ||
                    !_fields.Contains(field))
                {
                    return false;
                }

                _requiredFields.Add(field);

                return true;
            }

            return _requiredFields.Remove(field);
        }

        public int GetFieldIndex(Field field)
        {
            return _fields.IndexOf(field);
        }
        
        internal void AddField(Field field)
        {
            if (field == null)
                return;

            if (_fields.Contains(field))
                return;

            _fields.Add(field);
        }

        internal bool MoveField(Field field, int insertionIndex)
        {
            if (field == null)
                return false;

            int oldIndex = _fields.IndexOf(field);

            if (oldIndex < 0)
                return false;

            if (insertionIndex == oldIndex ||
                insertionIndex == oldIndex + 1)
            {
                return false;
            }

            _fields.RemoveAt(oldIndex);

            if (insertionIndex > oldIndex)
                insertionIndex--;

            insertionIndex = Mathf.Clamp(
                insertionIndex,
                0,
                _fields.Count);

            _fields.Insert(insertionIndex, field);

            return true;
        }

        internal void RemoveField(Field field)
        {
            if (field == null)
                return;

            _fields.Remove(field);

            _requiredFields?.Remove(field);
        }
        #endregion
    }
}