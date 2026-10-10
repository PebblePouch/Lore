using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PebblePouch.Lore
{
    public class Entry : LoreAsset
    {
        #region Fields
        [SerializeField, HideInInspector]
        private Category _category;
        
        [SerializeField, HideInInspector]
        private string _glyph;
        
        [SerializeField, HideInInspector]
        private string _subtitle;
        
        [SerializeField, HideInInspector]
        private string _description;
        
        [SerializeField, HideInInspector]
        private int _sortOrder;
        
        [SerializeField, HideInInspector]
        private List<FieldValue> _fields = new();

        [SerializeField, HideInInspector]
        private List<string> _tagIds = new();
        #endregion

        #region Properties
        public Category Category => _category;
        
        public string Glyph =>
            string.IsNullOrEmpty(_glyph)
            ? _category != null
            ? _category.Glyph
            : "?"
            : _glyph;
        
        public bool HasGlyphOverride => !string.IsNullOrEmpty(_glyph);
        
        public string Title => name;
        
        public string Subtitle => _subtitle;
        
        // Stored form with [[id|Title]] links; games use LoreProject.GetPlainText / FormatDescription.
        public string Description => _description;
        
        public int SortOrder => _sortOrder;
        
        public IReadOnlyList<FieldValue> Fields => _fields;

        public IReadOnlyList<string> TagIds => _tagIds;
        #endregion

        #region Field Access
        // Values by Field reference or Field name (see FieldKey). Missing Fields and wrong types never throw:
        // TryGetValue returns false, GetValue returns null and the Get… shortcuts return a default value
        // (both with a warning in the editor/development builds).

        public bool TryGetValue(
            FieldKey field,
            out FieldValue value)
        {
            foreach (FieldValue fieldValue in _fields)
            {
                if (fieldValue != null &&
                    field.Matches(fieldValue.Field))
                {
                    value = fieldValue;

                    return true;
                }
            }

            value = null;

            return false;
        }

        public FieldValue GetValue(FieldKey field)
        {
            if (TryGetValue(
                    field,
                    out FieldValue value))
            {
                return value;
            }

            LoreRuntimeLog.WarnOnce(
                $"Entry '{Title}' has no Field '{field}'.",
                this);

            return null;
        }

        public bool HasField(FieldKey field) => TryGetValue(field, out _);

        public string GetText(FieldKey field) => GetValue(field)?.GetText() ?? string.Empty;

        public string GetUrl(FieldKey field) => GetValue(field)?.GetUrl() ?? string.Empty;

        public bool GetBool(FieldKey field) => GetValue(field)?.GetBool() ?? false;

        public int GetInt(FieldKey field) => GetValue(field)?.GetInt() ?? 0;

        public float GetFloat(FieldKey field) => GetValue(field)?.GetFloat() ?? 0f;

        public Entry GetEntry(FieldKey field) => GetValue(field)?.GetEntry();

        public GameObject GetGameObject(FieldKey field) => GetValue(field)?.GetGameObject();

        public Sprite GetSprite(FieldKey field) => GetValue(field)?.GetSprite();

        public AudioClip GetAudioClip(FieldKey field) => GetValue(field)?.GetAudioClip();

        public string GetScenePath(FieldKey field) => GetValue(field)?.GetScenePath() ?? string.Empty;

        public string GetSceneName(FieldKey field) => GetValue(field)?.GetSceneName() ?? string.Empty;

        public string GetEnum(FieldKey field) => GetValue(field)?.GetEnum() ?? string.Empty;

        public int GetEnumIndex(FieldKey field) => GetValue(field)?.GetEnumIndex() ?? -1;

        public IReadOnlyList<Entry> GetEntries(FieldKey field) =>
            GetValue(field)?.GetEntries() ?? (IReadOnlyList<Entry>)System.Array.Empty<Entry>();

        public ScriptableObject GetScriptableObject(FieldKey field) => GetValue(field)?.GetScriptableObject();

        public T GetScriptableObject<T>(FieldKey field)
            where T : ScriptableObject
        {
            return GetValue(field)?.GetScriptableObject<T>();
        }
        #endregion

        #region Public API
        internal void Initialize(Category category)
        {
            _category = category;

            _glyph = string.Empty;

            SyncFields();
        }

        internal void SetCategory(Category category)
        {
            if (category == null)
                return;

            if (_category == category)
                return;

            _category = category;

            SyncFields();
        }

        internal void SetGlyph(string glyph)
        {
            _glyph = glyph ?? string.Empty;
        }

        internal void SetSubtitle(string subtitle)
        {
            _subtitle = subtitle ?? string.Empty;
        }

        internal void SetDescription(string description)
        {
            _description = description ?? string.Empty;
        }

        internal void SetSortOrder(int sortOrder) => _sortOrder = sortOrder;

        // Matches the field values to the Category's Fields (add missing, remove obsolete, same order).
        // Returns true if values were added or removed; a pure reorder returns false.
        internal bool SyncFields()
        {
            if (_category == null)
                return false;

            int removedCount =
                _fields.RemoveAll(field =>
                    field.Field == null ||
                    !_category.Fields.Contains(field.Field));

            bool added = false;

            foreach (var definition in _category.Fields)
            {
                bool exists =
                    _fields.Any(
                        field =>
                            field.Field ==
                            definition);

                if (exists)
                    continue;

                var fieldValue = new FieldValue();

                fieldValue.Initialize(definition);

                _fields.Add(fieldValue);

                added = true;
            }

            var orderedFields = new List<FieldValue>();

            foreach (var definition in _category.Fields)
            {
                var field = _fields.First(f => f.Field == definition);
                orderedFields.Add(field);
            }

            _fields = orderedFields;

            return removedCount > 0 || added;
        }

        internal bool AddTag(string tagId)
        {
            if (string.IsNullOrEmpty(tagId))
                return false;

            if (_tagIds.Contains(tagId))
                return false;

            _tagIds.Add(tagId);

            return true;
        }

        public bool HasTag(string tagId)
        {
            if (string.IsNullOrEmpty(tagId))
                return false;

            return _tagIds.Contains(tagId);
        }

        internal bool RemoveTag(string tagId)
        {
            if (string.IsNullOrEmpty(tagId))
                return false;

            return _tagIds.Remove(tagId);
        }

        internal int RemoveInvalidTags(TagCollection collection)
        {
            if (collection == null)
                return 0;

            int removed =
                _tagIds.RemoveAll(
                    tagId =>
                        collection.FindById(
                            tagId) == null);

            return removed;
        }
        #endregion
    }
}