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

        // Writable types (see Runtime Changes): the changed value, otherwise the original.
        public string GetText(FieldKey field) =>
            TryGetChanged(field, FieldType.Text, out LoreStateValue changed)
                ? changed.Text
                : GetOriginalText(field);

        public string GetUrl(FieldKey field) => GetValue(field)?.GetUrl() ?? string.Empty;

        public bool GetBool(FieldKey field) =>
            TryGetChanged(field, FieldType.Boolean, out LoreStateValue changed)
                ? changed.Bool
                : GetOriginalBool(field);

        public int GetInt(FieldKey field) =>
            TryGetChanged(field, FieldType.Integer, out LoreStateValue changed)
                ? changed.Int
                : GetOriginalInt(field);

        public float GetFloat(FieldKey field) =>
            TryGetChanged(field, FieldType.Float, out LoreStateValue changed)
                ? changed.Float
                : GetOriginalFloat(field);

        public Color GetColor(FieldKey field) =>
            TryGetChanged(field, FieldType.Color, out LoreStateValue changed)
                ? changed.Color
                : GetOriginalColor(field);

        public Vector2 GetVector2(FieldKey field) =>
            TryGetChanged(field, FieldType.Vector, out LoreStateValue changed)
                ? (Vector2)changed.Vector
                : GetOriginalVector2(field);

        public Vector3 GetVector3(FieldKey field) =>
            TryGetChanged(field, FieldType.Vector, out LoreStateValue changed)
                ? changed.Vector
                : GetOriginalVector3(field);

        public Entry GetEntry(FieldKey field) =>
            TryGetChanged(field, FieldType.Entry, out LoreStateValue changed)
                ? ResolveChangedEntry(field, changed)
                : GetOriginalEntry(field);

        public GameObject GetGameObject(FieldKey field) => GetValue(field)?.GetGameObject();

        public Sprite GetSprite(FieldKey field) => GetValue(field)?.GetSprite();

        public AudioClip GetAudioClip(FieldKey field) => GetValue(field)?.GetAudioClip();

        public string GetScenePath(FieldKey field) => GetValue(field)?.GetScenePath() ?? string.Empty;

        public string GetFolderPath(FieldKey field) => GetValue(field)?.GetFolderPath() ?? string.Empty;

        public string GetSceneName(FieldKey field) => GetValue(field)?.GetSceneName() ?? string.Empty;

        // A changed option that was removed from the Field counts as not set, like in Lore.
        public string GetEnum(FieldKey field) =>
            TryGetChanged(field, FieldType.Enum, out LoreStateValue changed, out Field definition)
                ? definition.FindEnumOption(changed.Text)?.Label ?? string.Empty
                : GetOriginalEnum(field);

        public int GetEnumIndex(FieldKey field) =>
            TryGetChanged(field, FieldType.Enum, out LoreStateValue changed, out Field definition)
                ? definition.GetEnumOptionIndex(changed.Text)
                : GetOriginalEnumIndex(field);

        public IReadOnlyList<string> GetEnums(FieldKey field) =>
            GetValue(field)?.GetEnums() ?? (IReadOnlyList<string>)System.Array.Empty<string>();

        public bool HasEnum(FieldKey field, string label) => GetValue(field)?.HasEnum(label) ?? false;

        public IReadOnlyList<Entry> GetEntries(FieldKey field) =>
            GetValue(field)?.GetEntries() ?? (IReadOnlyList<Entry>)System.Array.Empty<Entry>();

        public ScriptableObject GetScriptableObject(FieldKey field) => GetValue(field)?.GetScriptableObject();

        public T GetScriptableObject<T>(FieldKey field)
            where T : ScriptableObject
        {
            return GetValue(field)?.GetScriptableObject<T>();
        }

        public UnityEngine.Object GetObject(FieldKey field) => GetValue(field)?.GetObject();

        public T GetObject<T>(FieldKey field)
            where T : UnityEngine.Object
        {
            return GetValue(field)?.GetObject<T>();
        }
        #endregion

        #region Runtime Changes
        // Writable field types (Text, Boolean, Integer, Float, Vector, Color, Enum, Entry) can be changed at runtime.
        // Changes live in LoreState, never in the asset; the getters above return them, GetOriginal… the value from
        // Lore. Set… warns like Get… on a missing Field, wrong type or invalid value and changes nothing; TrySet…
        // returns false instead. Setting the original value again removes the change.

        public string GetOriginalText(FieldKey field) => GetValue(field)?.GetText() ?? string.Empty;

        public bool GetOriginalBool(FieldKey field) => GetValue(field)?.GetBool() ?? false;

        public int GetOriginalInt(FieldKey field) => GetValue(field)?.GetInt() ?? 0;

        public float GetOriginalFloat(FieldKey field) => GetValue(field)?.GetFloat() ?? 0f;

        public Vector2 GetOriginalVector2(FieldKey field) => GetValue(field)?.GetVector2() ?? Vector2.zero;

        public Vector3 GetOriginalVector3(FieldKey field) => GetValue(field)?.GetVector3() ?? Vector3.zero;

        public Color GetOriginalColor(FieldKey field) => GetValue(field)?.GetColor() ?? Color.black;

        public string GetOriginalEnum(FieldKey field) => GetValue(field)?.GetEnum() ?? string.Empty;

        public int GetOriginalEnumIndex(FieldKey field) => GetValue(field)?.GetEnumIndex() ?? -1;

        public Entry GetOriginalEntry(FieldKey field) => GetValue(field)?.GetEntry();

        public void SetText(FieldKey field, string value) => TrySet(field, LoreStateValue.FromText(value), true);

        public bool TrySetText(FieldKey field, string value) => TrySet(field, LoreStateValue.FromText(value), false);

        public void SetBool(FieldKey field, bool value) => TrySet(field, LoreStateValue.FromBool(value), true);

        public bool TrySetBool(FieldKey field, bool value) => TrySet(field, LoreStateValue.FromBool(value), false);

        public void SetInt(FieldKey field, int value) => TrySet(field, LoreStateValue.FromInt(value), true);

        public bool TrySetInt(FieldKey field, int value) => TrySet(field, LoreStateValue.FromInt(value), false);

        public void SetFloat(FieldKey field, float value) => TrySet(field, LoreStateValue.FromFloat(value), true);

        public bool TrySetFloat(FieldKey field, float value) => TrySet(field, LoreStateValue.FromFloat(value), false);

        // Vector fields of both dimensions: SetVector2 keeps the current z, 2D Fields always keep z = 0.
        public void SetVector2(FieldKey field, Vector2 value) => TrySetVector(field, value, null, true);

        public bool TrySetVector2(FieldKey field, Vector2 value) => TrySetVector(field, value, null, false);

        public void SetVector3(FieldKey field, Vector3 value) => TrySetVector(field, value, value.z, true);

        public bool TrySetVector3(FieldKey field, Vector3 value) => TrySetVector(field, value, value.z, false);

        public void SetColor(FieldKey field, Color value) => TrySet(field, LoreStateValue.FromColor(value), true);

        public bool TrySetColor(FieldKey field, Color value) => TrySet(field, LoreStateValue.FromColor(value), false);

        // By option label (case-insensitive); null or empty = no option.
        public void SetEnum(FieldKey field, string label) => TrySetEnum(field, label, true);

        public bool TrySetEnum(FieldKey field, string label) => TrySetEnum(field, label, false);

        // null = no Entry. The Entry must be of a Category the Field allows.
        public void SetEntry(FieldKey field, Entry value) => TrySetEntry(field, value, true);

        public bool TrySetEntry(FieldKey field, Entry value) => TrySetEntry(field, value, false);

        // True when the value was not changed at runtime (also for missing Fields, with a warning).
        public bool IsOriginal(FieldKey field)
        {
            FieldValue value = GetValue(field);

            return value == null ||
                   !LoreState.TryGet(this, value.Field, out LoreStateValue changed) ||
                   !value.IsType(changed.Type);
        }

        public void ResetToOriginal(FieldKey field)
        {
            FieldValue value = GetValue(field);

            if (value != null)
                LoreState.Reset(this, value.Field);
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

        #region Unity
        // Loaded Entries are known to LoreState, which resolves Entry values loaded from JSON by ID.
        protected override void OnEnable()
        {
            base.OnEnable();

            LoreState.RegisterEntry(this);
        }

        private void OnDisable() => LoreState.UnregisterEntry(this);
        #endregion

        #region Helpers
        // The runtime change of a Field of the given type; no warnings (the original getters warn).
        private bool TryGetChanged(
            FieldKey field,
            FieldType type,
            out LoreStateValue changed)
        {
            return TryGetChanged(field, type, out changed, out _);
        }

        private bool TryGetChanged(
            FieldKey field,
            FieldType type,
            out LoreStateValue changed,
            out Field definition)
        {
            changed = default;
            definition = null;

            if (!LoreState.HasChanges ||
                !TryGetValue(field, out FieldValue value) ||
                !value.IsType(type) ||
                !LoreState.TryGet(this, value.Field, out changed) ||
                changed.Type != type)
            {
                return false;
            }

            definition = value.Field;

            return true;
        }

        // A changed Entry loaded from JSON is found by ID among the loaded Entries.
        private Entry ResolveChangedEntry(
            FieldKey field,
            LoreStateValue changed)
        {
            Entry entry = LoreState.ResolveEntry(changed);

            if (entry == null &&
                !string.IsNullOrEmpty(changed.Text))
            {
                LoreRuntimeLog.WarnOnce(
                    $"Entry '{Title}': the changed value of Field '{field}' is an Entry that is not loaded or was " +
                    $"deleted (ID {changed.Text}). Reference the Lore Project to load all Entries.",
                    this);
            }

            return entry;
        }

        private bool TryResolveWritable(
            FieldKey field,
            FieldType type,
            bool warn,
            out FieldValue value)
        {
            if (warn)
                value = GetValue(field);
            else
                TryGetValue(field, out value);

            if (value == null)
                return false;

            if (value.IsType(type))
                return true;

            if (warn)
                value.WarnWrongType(type);

            return false;
        }

        private bool TrySet(
            FieldKey field,
            LoreStateValue changed,
            bool warn)
        {
            if (!TryResolveWritable(field, changed.Type, warn, out FieldValue value))
                return false;

            Store(value, changed);

            return true;
        }

        private bool TrySetEnum(
            FieldKey field,
            string label,
            bool warn)
        {
            if (!TryResolveWritable(field, FieldType.Enum, warn, out FieldValue value))
                return false;

            string optionId = string.Empty;

            if (!string.IsNullOrEmpty(label))
            {
                EnumOption option = null;

                foreach (EnumOption candidate in value.Field.EnumOptions)
                {
                    if (candidate != null &&
                        string.Equals(candidate.Label, label, System.StringComparison.OrdinalIgnoreCase))
                    {
                        option = candidate;

                        break;
                    }
                }

                if (option == null)
                {
                    if (warn)
                    {
                        LoreRuntimeLog.WarnOnce(
                            $"Field '{value.Field.Title}' has no option '{label}'.",
                            value.Field);
                    }

                    return false;
                }

                optionId = option.Id;
            }

            Store(value, LoreStateValue.FromEnum(optionId));

            return true;
        }

        private bool TrySetEntry(
            FieldKey field,
            Entry entry,
            bool warn)
        {
            if (!TryResolveWritable(field, FieldType.Entry, warn, out FieldValue value))
                return false;

            if (entry != null &&
                !value.Field.AllowsCategory(entry.Category))
            {
                if (warn)
                {
                    LoreRuntimeLog.WarnOnce(
                        $"Field '{value.Field.Title}' does not allow Entries of Category " +
                        $"'{(entry.Category != null ? entry.Category.Title : "(none)")}' ('{entry.Title}').",
                        value.Field);
                }

                return false;
            }

            Store(value, LoreStateValue.FromEntry(entry));

            return true;
        }

        private void Store(
            FieldValue value,
            LoreStateValue changed)
        {
            LoreStateValue original = changed.Type switch
            {
                FieldType.Text => LoreStateValue.FromText(value.StringValue),
                FieldType.Boolean => LoreStateValue.FromBool(value.BoolValue),
                FieldType.Integer => LoreStateValue.FromInt(value.IntValue),
                FieldType.Float => LoreStateValue.FromFloat(value.FloatValue),
                FieldType.Color => LoreStateValue.FromColor(value.ColorValue),
                FieldType.Enum => LoreStateValue.FromEnum(value.EnumValue),
                FieldType.Entry => LoreStateValue.FromEntry(value.EntryValue),
                _ => LoreStateValue.FromVector(value.VectorValue),
            };

            LoreState.Set(this, value.Field, changed, original);
        }

        // z == null keeps the current z (SetVector2).
        private bool TrySetVector(
            FieldKey field,
            Vector2 xy,
            float? z,
            bool warn)
        {
            if (!TryResolveWritable(field, FieldType.Vector, warn, out FieldValue value))
                return false;

            Vector3 original = value.VectorValue;

            Vector3 current =
                LoreState.TryGet(this, value.Field, out LoreStateValue changed) &&
                changed.Type == FieldType.Vector
                    ? changed.Vector
                    : original;

            float newZ =
                value.Field.VectorDimension == 2
                    ? 0f
                    : z ?? current.z;

            LoreState.Set(
                this,
                value.Field,
                LoreStateValue.FromVector(new Vector3(xy.x, xy.y, newZ)),
                LoreStateValue.FromVector(original));

            return true;
        }
        #endregion
    }
}