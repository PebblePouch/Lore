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

        // Folder fields: the folder asset (editor-only DefaultAsset, like scenes) and its path ("Assets/Art/Wolves"),
        // kept in sync by the editor (also when the folder is moved/renamed), so games can read it in builds.
        [SerializeField, HideInInspector]
        private UnityEngine.Object _folderValue;

        [SerializeField, HideInInspector]
        private string _folderPath;

        // ScriptableObject and Object fields: any object of the Field's ObjectType (or a subclass). Audio fields: the
        // AudioClip.
        [SerializeField, HideInInspector]
        private UnityEngine.Object _objectValue;

        // Entries fields: the referenced Entries, in order (no duplicates).
        [SerializeField, HideInInspector]
        private System.Collections.Generic.List<Entry> _entriesValue = new();

        // Enum fields: the ID of the selected option (see Field.EnumOptions).
        [SerializeField, HideInInspector]
        private string _enumValue;

        // Enums fields: the IDs of the selected options (no duplicates). Read in the Field's option order.
        [SerializeField, HideInInspector]
        private System.Collections.Generic.List<string> _enumValues = new();

        [SerializeField, HideInInspector]
        private bool _boolValue;

        [SerializeField, HideInInspector]
        private int _intValue;

        [SerializeField, HideInInspector]
        private float _floatValue;

        [SerializeField, HideInInspector]
        private Color _colorValue = Color.black;

        // Vector fields: always a Vector3; 2D Fields (Field.VectorDimension) use x and y (z is kept but not shown).
        [SerializeField, HideInInspector]
        private Vector3 _vectorValue;
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

        internal UnityEngine.Object FolderValue => _folderValue;

        internal string FolderPath => _folderPath ?? string.Empty;

        internal UnityEngine.Object ObjectValue => _objectValue;

        internal AudioClip AudioClipValue => _objectValue as AudioClip;

        internal string EnumValue => _enumValue ?? string.Empty;

        // Raw list (may contain removed options, stored order); games use GetEnums.
        internal IReadOnlyList<string> EnumValues => _enumValues ??= new();

        // Raw list (may contain missing Entries); games use GetEntries.
        internal System.Collections.Generic.IReadOnlyList<Entry> EntriesValue => _entriesValue ??= new();

        internal bool BoolValue => _boolValue;

        internal int IntValue => _intValue;

        internal float FloatValue => _floatValue;

        internal Color ColorValue => _colorValue;

        // The value as the Field shows it: z is 0 for 2D Fields.
        internal Vector3 VectorValue =>
            _field != null && _field.VectorDimension == 2
                ? new Vector3(_vectorValue.x, _vectorValue.y, 0f)
                : _vectorValue;
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

        public Color GetColor() => Get(FieldType.Color, _colorValue, Color.black);

        public bool TryGetColor(out Color value) => TryGet(FieldType.Color, _colorValue, out value);

        // Vector fields of both dimensions: GetVector2 drops z, GetVector3 of a 2D Field has z = 0.
        public Vector2 GetVector2() => Get(FieldType.Vector, (Vector2)VectorValue, Vector2.zero);

        public bool TryGetVector2(out Vector2 value) => TryGet(FieldType.Vector, (Vector2)VectorValue, out value);

        public Vector3 GetVector3() => Get(FieldType.Vector, VectorValue, Vector3.zero);

        public bool TryGetVector3(out Vector3 value) => TryGet(FieldType.Vector, VectorValue, out value);

        public Entry GetEntry() => Get(FieldType.Entry, AsReference(_entryValue), null);

        public bool TryGetEntry(out Entry value) => TryGetReference(FieldType.Entry, _entryValue, out value);

        public GameObject GetGameObject() => Get(FieldType.GameObject, AsReference(_gameObjectValue), null);

        public bool TryGetGameObject(out GameObject value) => TryGetReference(FieldType.GameObject, _gameObjectValue, out value);

        public Sprite GetSprite() => Get(FieldType.Sprite, AsReference(_spriteValue), null);

        public bool TryGetSprite(out Sprite value) => TryGetReference(FieldType.Sprite, _spriteValue, out value);

        public AudioClip GetAudioClip() => Get(FieldType.Audio, AsReference(AudioClipValue), null);

        public bool TryGetAudioClip(out AudioClip value) => TryGetReference(FieldType.Audio, AudioClipValue, out value);

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

        // Folder path ("Assets/Art/Wolves"); empty when no folder is set.
        public string GetFolderPath() => Get(FieldType.Folder, FolderPath, string.Empty);

        public bool TryGetFolderPath(out string value)
        {
            value = string.Empty;

            return TryGet(FieldType.Folder, FolderPath, out value) &&
                   !string.IsNullOrEmpty(value);
        }

        // ScriptableObject fields. T can be the Field's type or any base/derived type: a value that is not a T
        // returns null (Get… with a warning, TryGet… false).
        public ScriptableObject GetScriptableObject() => GetScriptableObject<ScriptableObject>();

        public T GetScriptableObject<T>()
            where T : ScriptableObject
        {
            return GetTypedObject<T>(FieldType.ScriptableObject);
        }

        // Object fields (Texture2D, Material, a component on a prefab, ...). T works like for ScriptableObject fields.
        public UnityEngine.Object GetObject() => GetObject<UnityEngine.Object>();

        public T GetObject<T>()
            where T : UnityEngine.Object
        {
            return GetTypedObject<T>(FieldType.Object);
        }

        public bool TryGetObject<T>(out T value)
            where T : UnityEngine.Object
        {
            value = IsType(FieldType.Object)
                ? AsReference(_objectValue) as T
                : null;

            return value != null;
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

        // Enums fields: the labels of the selected options in the Field's option order, without removed options
        // (new list per call).
        public IReadOnlyList<string> GetEnums()
        {
            if (!IsType(FieldType.Enums))
            {
                WarnWrongType(FieldType.Enums);

                return Array.Empty<string>();
            }

            return GetSelectedEnumLabels();
        }

        // False on a wrong type and when nothing is selected.
        public bool TryGetEnums(out IReadOnlyList<string> labels)
        {
            labels =
                IsType(FieldType.Enums)
                    ? GetSelectedEnumLabels()
                    : Array.Empty<string>();

            return labels.Count > 0;
        }

        // Enums fields: true when the option with this label is selected (labels are matched ignoring case).
        public bool HasEnum(string label)
        {
            if (!IsType(FieldType.Enums))
            {
                WarnWrongType(FieldType.Enums);

                return false;
            }

            foreach (string selected in GetSelectedEnumLabels())
            {
                if (string.Equals(selected, label, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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

        internal void SetFolder(
            UnityEngine.Object value,
            string path)
        {
            _folderValue = value;

            _folderPath =
                value != null
                    ? path ?? string.Empty
                    : string.Empty;
        }

        // Folder moved/renamed: updates the stored path. Returns true if it changed.
        internal bool SetFolderPath(string path)
        {
            path ??= string.Empty;

            if (FolderPath == path)
                return false;

            _folderPath = path;

            return true;
        }

        internal void SetObject(UnityEngine.Object value) => _objectValue = value;

        internal void SetAudioClip(AudioClip value) => _objectValue = value;

        internal void SetEnum(string optionId) => _enumValue = optionId ?? string.Empty;

        // Enums fields. Returns true if the list changed.
        internal bool AddEnumValue(string optionId)
        {
            _enumValues ??= new();

            if (string.IsNullOrEmpty(optionId) ||
                _enumValues.Contains(optionId))
            {
                return false;
            }

            _enumValues.Add(optionId);

            return true;
        }

        internal bool RemoveEnumValue(string optionId) => _enumValues != null && _enumValues.Remove(optionId);

        internal void SetEnumValues(IEnumerable<string> optionIds)
        {
            _enumValues = new List<string>();

            foreach (string optionId in optionIds)
                AddEnumValue(optionId);
        }

        // Removes the given options (removed from the Field). Returns the number removed.
        internal int RemoveEnumValues(ICollection<string> optionIds) =>
            _enumValues?.RemoveAll(optionIds.Contains) ?? 0;

        // Field type change Enum ↔ Enums or Entry ↔ Entries: moves the value (broken references too). Values that
        // do not fit (several values into a single-value type) and all other type changes clear the value.
        // Returns true if the value was kept.
        internal bool ConvertValue(
            FieldType from,
            FieldType to)
        {
            if (from == FieldType.Enum && to == FieldType.Enums)
            {
                string optionId = EnumValue;

                Clear();

                AddEnumValue(optionId);

                return true;
            }

            if (from == FieldType.Enums && to == FieldType.Enum && EnumValues.Count <= 1)
            {
                string optionId =
                    EnumValues.Count == 1
                        ? EnumValues[0]
                        : string.Empty;

                Clear();

                _enumValue = optionId;

                return true;
            }

            if (from == FieldType.Entry && to == FieldType.Entries)
            {
                Entry entry = _entryValue;

                Clear();

                // Deleted Entries stay as broken references (not null in C#).
                if (!ReferenceEquals(entry, null))
                    _entriesValue.Add(entry);

                return true;
            }

            if (from == FieldType.Entries && to == FieldType.Entry && EntriesValue.Count <= 1)
            {
                Entry entry =
                    EntriesValue.Count == 1
                        ? EntriesValue[0]
                        : null;

                Clear();

                _entryValue = entry;

                return true;
            }

            Clear();

            return false;
        }

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

        internal void SetColor(Color value) => _colorValue = value;

        // 2D Fields keep the stored z (switching the Field back to 3D shows it again).
        internal void SetVector(Vector3 value)
        {
            _vectorValue =
                _field != null && _field.VectorDimension == 2
                    ? new Vector3(value.x, value.y, _vectorValue.z)
                    : value;
        }

        internal void Clear()
        {
            _stringValue = string.Empty;

            _entryValue = null;

            _gameObjectValue = null;

            _spriteValue = null;

            _sceneValue = null;

            _scenePath = string.Empty;

            _folderValue = null;

            _folderPath = string.Empty;

            _objectValue = null;

            _enumValue = string.Empty;

            _enumValues ??= new();
            _enumValues.Clear();

            _entriesValue ??= new();
            _entriesValue.Clear();

            _boolValue = false;

            _intValue = 0;

            _floatValue = 0f;

            _colorValue = Color.black;

            _vectorValue = Vector3.zero;
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

        private IReadOnlyList<string> GetSelectedEnumLabels()
        {
            var labels = new List<string>();

            if (_field == null ||
                _enumValues == null ||
                _enumValues.Count == 0)
            {
                return labels;
            }

            foreach (EnumOption option in _field.EnumOptions)
            {
                if (option != null &&
                    _enumValues.Contains(option.Id))
                {
                    labels.Add(option.Label);
                }
            }

            return labels;
        }

        private EnumOption GetEnumOption() =>
            _field != null
                ? _field.FindEnumOption(_enumValue)
                : null;

        internal bool IsType(FieldType type) =>
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

        // ScriptableObject and Object fields: the value as T; null (with a warning) when it is no T.
        private T GetTypedObject<T>(FieldType type)
            where T : UnityEngine.Object
        {
            if (!IsType(type))
            {
                WarnWrongType(type);

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

        internal void WarnWrongType(FieldType expected)
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
