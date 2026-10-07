using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore
{
    [Serializable]
    public sealed class FieldValue
    {
        #region Fields
        [SerializeField, HideInInspector]
        private Field _field;

        [SerializeField, HideInInspector]
        private string _stringValue;

        [SerializeField, HideInInspector]
        private Entry _entryValue;

        [SerializeField, HideInInspector]
        private GameObject _gameObjectValue;

        [SerializeField, HideInInspector]
        private Sprite _spriteValue;

        // SceneAsset is editor-only, so the scene is stored as a plain Object reference.
        // The editor restricts it to SceneAsset; in player builds it is not available.
        [SerializeField, HideInInspector]
        private UnityEngine.Object _sceneValue;

        // Asset path of the scene ("Assets/Scenes/Level 1.unity"), kept in sync by the editor
        // (also when the scene is moved/renamed), so games can load the scene in builds.
        [SerializeField, HideInInspector]
        private string _scenePath;

        // ScriptableObject fields: any object of the Field's ObjectType (or a subclass).
        [SerializeField, HideInInspector]
        private UnityEngine.Object _objectValue;

        // Entries fields: the referenced Entries, in order (no duplicates).
        [SerializeField, HideInInspector]
        private System.Collections.Generic.List<Entry> _entriesValue = new();

        // Enum fields: the ID of the selected option (see Field.EnumOptions).
        [SerializeField, HideInInspector]
        private string _enumValue;

        [SerializeField, HideInInspector]
        private bool _boolValue;

        [SerializeField, HideInInspector]
        private int _intValue;

        [SerializeField, HideInInspector]
        private float _floatValue;
        #endregion

        #region Properties
        public Field Field => _field;

        // Type of the Field (Text if the Field is missing).
        public FieldType Type =>
            _field != null
                ? _field.Type
                : FieldType.Text;

        // Raw storage (editor): games use the typed getters below, which also check the Field type.
        internal string StringValue => _stringValue;

        internal Entry EntryValue => _entryValue;

        internal GameObject GameObjectValue => _gameObjectValue;

        internal Sprite SpriteValue => _spriteValue;

        internal UnityEngine.Object SceneValue => _sceneValue;

        internal string ScenePath => _scenePath;

        internal UnityEngine.Object ObjectValue => _objectValue;

        internal string EnumValue => _enumValue ?? string.Empty;

        // Raw list (may contain missing Entries); games use GetEntries.
        internal System.Collections.Generic.IReadOnlyList<Entry> EntriesValue => _entriesValue ??= new();

        internal bool BoolValue => _boolValue;

        internal int IntValue => _intValue;

        internal float FloatValue => _floatValue;
        #endregion

        #region Typed Access
        // Get…: the value, or a default (with a warning in the editor/development builds) when the Field has
        // another type. TryGet…: false when the Field has another type; for references and scenes also when empty.

        public string GetText() => Get(FieldType.Text, _stringValue ?? string.Empty, string.Empty);

        public bool TryGetText(out string value) => TryGet(FieldType.Text, _stringValue ?? string.Empty, out value);

        public string GetUrl() => Get(FieldType.Url, _stringValue ?? string.Empty, string.Empty);

        public bool TryGetUrl(out string value) => TryGet(FieldType.Url, _stringValue ?? string.Empty, out value);

        public bool GetBool() => Get(FieldType.Boolean, _boolValue, false);

        public bool TryGetBool(out bool value) => TryGet(FieldType.Boolean, _boolValue, out value);

        public int GetInt() => Get(FieldType.Integer, _intValue, 0);

        public bool TryGetInt(out int value) => TryGet(FieldType.Integer, _intValue, out value);

        public float GetFloat() => Get(FieldType.Float, _floatValue, 0f);

        public bool TryGetFloat(out float value) => TryGet(FieldType.Float, _floatValue, out value);

        public Entry GetEntry() => Get(FieldType.Entry, AsReference(_entryValue), null);

        public bool TryGetEntry(out Entry value) => TryGetReference(FieldType.Entry, _entryValue, out value);

        public GameObject GetGameObject() => Get(FieldType.GameObject, AsReference(_gameObjectValue), null);

        public bool TryGetGameObject(out GameObject value) => TryGetReference(FieldType.GameObject, _gameObjectValue, out value);

        public Sprite GetSprite() => Get(FieldType.Sprite, AsReference(_spriteValue), null);

        public bool TryGetSprite(out Sprite value) => TryGetReference(FieldType.Sprite, _spriteValue, out value);

        // Scene path for SceneManager.LoadScene (the scene must be in the build's scene list).
        public string GetScenePath() => Get(FieldType.Scene, _scenePath ?? string.Empty, string.Empty);

        public bool TryGetScenePath(out string value)
        {
            value = string.Empty;

            return TryGet(FieldType.Scene, _scenePath ?? string.Empty, out value) &&
                   !string.IsNullOrEmpty(value);
        }

        // Scene name ("Level 1"), also accepted by SceneManager.LoadScene.
        public string GetSceneName() => ToSceneName(GetScenePath());

        public bool TryGetSceneName(out string value)
        {
            bool found = TryGetScenePath(out string path);

            value = ToSceneName(path);

            return found;
        }

        // ScriptableObject fields. T can be the Field's type or any base/derived type: a value that is not a T
        // returns null (Get… with a warning, TryGet… false).
        public ScriptableObject GetScriptableObject() => GetScriptableObject<ScriptableObject>();

        public T GetScriptableObject<T>()
            where T : ScriptableObject
        {
            if (!IsType(FieldType.ScriptableObject))
            {
                WarnWrongType(FieldType.ScriptableObject);

                return null;
            }

            UnityEngine.Object value = AsReference(_objectValue);

            if (value == null)
                return null;

            if (value is T typed)
                return typed;

            LoreRuntimeLog.WarnOnce(
                $"Field '{_field.Title}' holds a {value.GetType().Name}, not a {typeof(T).Name}.",
                _field);

            return null;
        }

        // Enum fields: the label of the selected option ("Rare"); empty when nothing (or a removed option) is set.
        public string GetEnum()
        {
            if (!IsType(FieldType.Enum))
            {
                WarnWrongType(FieldType.Enum);

                return string.Empty;
            }

            EnumOption option = GetEnumOption();

            return option != null
                ? option.Label
                : string.Empty;
        }

        public bool TryGetEnum(out string label)
        {
            EnumOption option =
                IsType(FieldType.Enum)
                    ? GetEnumOption()
                    : null;

            label = option != null
                ? option.Label
                : string.Empty;

            return option != null;
        }

        // Position of the selected option in the Field's option list (e.g. for sorting or ranks); -1 when nothing
        // is set or the Field is no Enum field (with a warning in the editor/development builds).
        public int GetEnumIndex()
        {
            if (!IsType(FieldType.Enum))
            {
                WarnWrongType(FieldType.Enum);

                return -1;
            }

            return _field.GetEnumOptionIndex(
                _enumValue);
        }

        public bool TryGetEnumIndex(out int index)
        {
            index =
                IsType(FieldType.Enum)
                    ? _field.GetEnumOptionIndex(_enumValue)
                    : -1;

            return index >= 0;
        }

        // Entries fields: the referenced Entries in order, without deleted/missing ones (new list per call).
        public IReadOnlyList<Entry> GetEntries()
        {
            if (!IsType(FieldType.Entries))
            {
                WarnWrongType(FieldType.Entries);

                return Array.Empty<Entry>();
            }

            return GetExistingEntries();
        }

        // False on a wrong type and when the list is empty.
        public bool TryGetEntries(out IReadOnlyList<Entry> entries)
        {
            entries =
                IsType(FieldType.Entries)
                    ? GetExistingEntries()
                    : Array.Empty<Entry>();

            return entries.Count > 0;
        }

        public bool TryGetScriptableObject<T>(out T value)
            where T : ScriptableObject
        {
            value = IsType(FieldType.ScriptableObject)
                ? AsReference(_objectValue) as T
                : null;

            return value != null;
        }
        #endregion

        #region Editor API
        internal void Initialize(Field field) => _field = field;

        internal void SetText(string value)
        {
            _stringValue = value ?? string.Empty;
        }

        internal void SetEntry(Entry value) => _entryValue = value;

        internal void SetGameObject(GameObject value) => _gameObjectValue = value;

        internal void SetSprite(Sprite value) => _spriteValue = value;

        internal void SetScene(
            UnityEngine.Object value,
            string path)
        {
            _sceneValue = value;

            _scenePath =
                value != null
                    ? path ?? string.Empty
                    : string.Empty;
        }

        // Scene moved/renamed: updates the stored path. Returns true if it changed.
        internal bool SetScenePath(string path)
        {
            path ??= string.Empty;

            if (_scenePath == path)
                return false;

            _scenePath = path;

            return true;
        }

        internal void SetObject(UnityEngine.Object value) => _objectValue = value;

        internal void SetEnum(string optionId) => _enumValue = optionId ?? string.Empty;

        // Entries fields. Returns true if the list changed.
        internal bool AddListEntry(Entry entry)
        {
            _entriesValue ??= new();

            if (entry == null ||
                _entriesValue.Contains(entry))
            {
                return false;
            }

            _entriesValue.Add(entry);

            return true;
        }

        internal bool RemoveListEntry(Entry entry) => _entriesValue != null && _entriesValue.Remove(entry);

        // Replaces the Entries that still exist; deleted/missing ones stay at the end (broken references are never
        // removed silently).
        internal void SetEntriesKeepingMissing(System.Collections.Generic.IEnumerable<Entry> entries)
        {
            var missing = new System.Collections.Generic.List<Entry>();

            if (_entriesValue != null)
            {
                foreach (Entry entry in _entriesValue)
                {
                    if (entry == null && !ReferenceEquals(entry, null))
                        missing.Add(entry);
                }
            }

            SetEntries(entries);

            _entriesValue.AddRange(missing);
        }

        internal void SetEntries(System.Collections.Generic.IEnumerable<Entry> entries)
        {
            _entriesValue = new System.Collections.Generic.List<Entry>();

            foreach (Entry entry in entries)
                AddListEntry(entry);
        }

        // Removes deleted/missing Entries. Returns the number removed.
        internal int RemoveMissingListEntries() => _entriesValue?.RemoveAll(entry => entry == null) ?? 0;

        internal void SetBoolean(bool value) => _boolValue = value;

        internal void SetInteger(int value) => _intValue = value;

        internal void SetFloat(float value) => _floatValue = value;

        internal void Clear()
        {
            _stringValue = string.Empty;

            _entryValue = null;

            _gameObjectValue = null;

            _spriteValue = null;

            _sceneValue = null;

            _scenePath = string.Empty;

            _objectValue = null;

            _enumValue = string.Empty;

            _entriesValue?.Clear();

            _boolValue = false;

            _intValue = 0;

            _floatValue = 0f;
        }
        #endregion

        #region Helpers
        private IReadOnlyList<Entry> GetExistingEntries()
        {
            var entries = new List<Entry>();

            if (_entriesValue == null)
                return entries;

            foreach (Entry entry in _entriesValue)
            {
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private EnumOption GetEnumOption() =>
            _field != null
                ? _field.FindEnumOption(_enumValue)
                : null;

        private bool IsType(FieldType type) =>
            _field != null &&
            _field.Type == type;

        private T Get<T>(
            FieldType type,
            T value,
            T fallback)
        {
            if (IsType(type))
                return value;

            WarnWrongType(type);

            return fallback;
        }

        private bool TryGet<T>(
            FieldType type,
            T source,
            out T value)
        {
            bool matches = IsType(type);

            value = matches
                ? source
                : default;

            return matches;
        }

        private bool TryGetReference<T>(
            FieldType type,
            T source,
            out T value)
            where T : UnityEngine.Object
        {
            value = IsType(type)
                ? AsReference(source)
                : null;

            return value != null;
        }

        // Destroyed/missing assets compare equal to null in Unity but are not null in C#:
        // return real null, so callers can use ?. and ?? safely.
        private static T AsReference<T>(T value)
            where T : UnityEngine.Object
        {
            return value != null
                ? value
                : null;
        }

        private static string ToSceneName(string path)
        {
            return string.IsNullOrEmpty(path)
                ? string.Empty
                : System.IO.Path.GetFileNameWithoutExtension(path);
        }

        private void WarnWrongType(FieldType expected)
        {
            if (_field == null)
            {
                LoreRuntimeLog.WarnOnce(
                    $"Field value without Field (expected {expected}).");

                return;
            }

            LoreRuntimeLog.WarnOnce(
                $"Field '{_field.Title}' is {_field.Type}, not {expected}.",
                _field);
        }
        #endregion
    }
}
