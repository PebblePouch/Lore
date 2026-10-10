using System;

using UnityEngine;

namespace PebblePouch.Lore
{
    public class Field : LoreAsset
    {
        #region Fields
        [SerializeField, HideInInspector]
        private FieldType _type;

        [SerializeField, HideInInspector]
        private string _glyph;

        // Object fields (ScriptableObject, Object): the allowed type; subclasses are allowed too.
        // Stored as "Namespace.Type, Assembly" (no version), so it survives recompiles and Unity upgrades.
        [SerializeField, HideInInspector]
        private string _objectTypeName;

        // GUID of the type's script asset (editor): lets the editor repair the type name after the class was
        // renamed together with its file. Empty for types without a script asset of their own.
        [SerializeField, HideInInspector]
        private string _objectScriptGuid;

        // Resolved type, cached per type name.
        [NonSerialized]
        private System.Type _objectType;

        [NonSerialized]
        private string _resolvedObjectTypeName;

        // Enum fields: the options, in dropdown order.
        [SerializeField, HideInInspector]
        private System.Collections.Generic.List<EnumOption> _enumOptions = new();

        // Entry / Entries fields: the Categories whose Entries can be picked (empty = all Categories).
        [SerializeField, HideInInspector]
        private System.Collections.Generic.List<Category> _allowedCategories = new();

        // Vector fields: 2 (Vector2) or 3 (Vector3).
        [SerializeField, HideInInspector]
        private int _vectorDimension = 3;
        #endregion

        #region Properties
        public string Title => name;

        public FieldType Type => _type;

        public string Glyph => _glyph;

        // Allowed type of object fields; null when none is set or the type no longer exists.
        public System.Type ObjectType
        {
            get
            {
                if (_resolvedObjectTypeName != _objectTypeName)
                {
                    _resolvedObjectTypeName = _objectTypeName;

                    _objectType =
                        string.IsNullOrEmpty(_objectTypeName)
                            ? null
                            : System.Type.GetType(
                                _objectTypeName,
                                false);
                }

                return _objectType;
            }
        }

        // Stored type name (also when the type cannot be resolved), e.g. for messages.
        public string ObjectTypeName => _objectTypeName ?? string.Empty;

        internal string ObjectScriptGuid => _objectScriptGuid ?? string.Empty;

        // Enum fields: the options in dropdown order (empty for other types).
        public System.Collections.Generic.IReadOnlyList<EnumOption> EnumOptions => _enumOptions;

        // Entry / Entries fields: Categories whose Entries can be picked; empty = all Categories.
        public System.Collections.Generic.IReadOnlyList<Category> AllowedCategories => _allowedCategories ??= new();

        // Vector fields: 2 (Vector2) or 3 (Vector3).
        public int VectorDimension => _vectorDimension == 2 ? 2 : 3;

        // True when Entries of this Category can be picked (no restriction, or the Category is allowed).
        public bool AllowsCategory(Category category)
        {
            bool restricted = false;

            foreach (Category allowed in AllowedCategories)
            {
                if (allowed == null)
                    continue;

                restricted = true;

                if (allowed == category)
                    return true;
            }

            return !restricted;
        }
        #endregion

        #region Public API
        internal void Initialize(FieldType type, string glyph)
        {
            _type = type;
            _glyph = glyph ?? string.Empty;
        }

        internal void SetType(FieldType type) => _type = type;

        internal void SetVectorDimension(int dimension) => _vectorDimension = dimension == 2 ? 2 : 3;

        internal void SetGlyph(string glyph)
        {
            _glyph = glyph ?? string.Empty;
        }

        // typeName in GetTypeName form; empty clears the constraint (and the script GUID).
        internal void SetObjectType(
            string typeName,
            string scriptGuid)
        {
            _objectTypeName = typeName ?? string.Empty;
            _objectScriptGuid = string.IsNullOrEmpty(_objectTypeName)
                ? string.Empty
                : scriptGuid ?? string.Empty;
        }

        // Type name only (rename repair keeps the script GUID). Returns true if it changed.
        internal bool SetObjectTypeName(string typeName)
        {
            typeName ??= string.Empty;

            if (_objectTypeName == typeName)
                return false;

            _objectTypeName = typeName;

            return true;
        }

        // Enum option by ID; null if it does not exist (anymore).
        public EnumOption FindEnumOption(string optionId)
        {
            if (string.IsNullOrEmpty(optionId))
                return null;

            foreach (EnumOption option in _enumOptions)
            {
                if (option != null &&
                    option.Id == optionId)
                {
                    return option;
                }
            }

            return null;
        }

        // Position of the option in the dropdown; -1 if it does not exist.
        public int GetEnumOptionIndex(string optionId)
        {
            EnumOption option =
                FindEnumOption(
                    optionId);

            return option != null
                ? _enumOptions.IndexOf(option)
                : -1;
        }

        internal EnumOption AddEnumOption(string label)
        {
            var option = new EnumOption(label);

            _enumOptions.Add(option);

            return option;
        }

        internal bool RemoveEnumOption(EnumOption option) => _enumOptions.Remove(option);

        // insertionIndex is the gap index (0 = before the first option, Count = after the last one).
        internal bool MoveEnumOption(
            EnumOption option,
            int insertionIndex)
        {
            int oldIndex = _enumOptions.IndexOf(option);

            if (oldIndex < 0 ||
                insertionIndex == oldIndex ||
                insertionIndex == oldIndex + 1)
            {
                return false;
            }

            _enumOptions.RemoveAt(oldIndex);

            if (insertionIndex > oldIndex)
                insertionIndex--;

            _enumOptions.Insert(
                Mathf.Clamp(insertionIndex, 0, _enumOptions.Count),
                option);

            return true;
        }

        // Replaces the options with the edited list (dropdown order). Options keep their ID when Id is set (renames
        // keep assigned values); entries without Id become new options; options missing in the list are removed.
        internal void ApplyEnumOptions(System.Collections.Generic.IReadOnlyList<(string Id, string Label)> options)
        {
            var applied = new System.Collections.Generic.List<EnumOption>();

            foreach ((string id, string label) in options)
            {
                EnumOption option =
                    FindEnumOption(
                        id);

                if (option != null)
                    option.SetLabel(label);
                else
                    option = new EnumOption(label);

                applied.Add(option);
            }

            _enumOptions = applied;
        }

        // Replaces the allowed Categories (empty = all); deleted Categories are dropped.
        internal void SetAllowedCategories(System.Collections.Generic.IEnumerable<Category> categories)
        {
            _allowedCategories = new System.Collections.Generic.List<Category>();

            foreach (Category category in categories)
            {
                if (category != null &&
                    !_allowedCategories.Contains(category))
                {
                    _allowedCategories.Add(category);
                }
            }
        }

        // Duplicating a Field: same labels, new option IDs (values of the copy are independent).
        internal void CopyEnumOptionsFrom(Field source)
        {
            _enumOptions.Clear();

            if (source == null)
                return;

            foreach (EnumOption option in source._enumOptions)
            {
                if (option != null)
                    AddEnumOption(option.Label);
            }
        }

        // "Namespace.Type, Assembly": the form stored in _objectTypeName.
        internal static string GetTypeName(System.Type type)
        {
            return type != null
                ? $"{type.FullName}, {type.Assembly.GetName().Name}"
                : string.Empty;
        }
        #endregion
    }
}
