using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Searchable list of types for ScriptableObject and Object Fields (see ScriptableObjectTypeService).
    // ScriptableObject types are a flat list; Object types come in groups by function (see ObjectTypeGroups), after
    // the types that Object Fields already use. The search covers all groups.
    public sealed class ScriptableObjectTypeDropdown : AdvancedDropdown
    {
        #region Types
        private sealed class TypeItem : AdvancedDropdownItem
        {
            public Type Type { get; }

            public TypeItem(Type type)
                : base(ScriptableObjectTypeService.GetCandidateLabel(type))
            {
                Type = type;

                icon = AssetPreview.GetMiniTypeThumbnail(type);
            }
        }
        #endregion

        #region Fields
        private readonly FieldType _fieldType;

        private readonly Action<Type> _selected;
        #endregion

        #region Constructor
        public ScriptableObjectTypeDropdown(
            FieldType fieldType,
            Action<Type> selected)
            : base(new AdvancedDropdownState())
        {
            _fieldType = fieldType;
            _selected = selected;

            minimumSize =
                new Vector2(
                    LibraryStyle.ObjectTypeDropdownWidth,
                    LibraryStyle.ObjectTypeDropdownHeight);
        }
        #endregion

        #region AdvancedDropdown
        protected override AdvancedDropdownItem BuildRoot()
        {
            bool objectField = _fieldType == FieldType.Object;

            var root =
                new AdvancedDropdownItem(
                    objectField
                        ? LoreText.Library.ObjectTypeDropdownTitleObject
                        : LoreText.Library.ObjectTypeDropdownTitle);

            IReadOnlyList<Type> candidates =
                ScriptableObjectTypeService.GetCandidates(
                    _fieldType);

            if (candidates.Count == 0)
            {
                root.AddChild(
                    new AdvancedDropdownItem(
                        objectField
                            ? LoreText.Library.ObjectTypeEmptyObject
                            : LoreText.Library.ObjectTypeEmpty)
                    {
                        enabled = false
                    });

                return root;
            }

            if (!objectField)
            {
                AddTypes(
                    root,
                    candidates);

                return root;
            }

            List<Type> usedTypes =
                GetUsedObjectTypes(
                    candidates);

            if (usedTypes.Count > 0)
            {
                root.AddChild(
                    CreateGroup(
                        LoreText.Library.ObjectTypeGroupUsed,
                        usedTypes));

                root.AddSeparator();
            }

            foreach (ObjectTypeGroups.Group group in ObjectTypeGroups.GetGroups())
            {
                root.AddChild(
                    CreateGroup(
                        group.Label,
                        group.Types));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeItem typeItem)
                _selected?.Invoke(typeItem.Type);
        }
        #endregion

        #region Helpers
        private static AdvancedDropdownItem CreateGroup(
            string label,
            IReadOnlyList<Type> types)
        {
            var group =
                new AdvancedDropdownItem(
                    label);

            AddTypes(
                group,
                types);

            return group;
        }

        private static void AddTypes(
            AdvancedDropdownItem parent,
            IReadOnlyList<Type> types)
        {
            foreach (Type type in types)
            {
                parent.AddChild(
                    new TypeItem(
                        type));
            }
        }

        // Types of the existing Object Fields (still candidates), by name. Read per opening: Fields change meanwhile.
        private static List<Type> GetUsedObjectTypes(IReadOnlyList<Type> candidates)
        {
            var candidateSet = new HashSet<Type>(candidates);

            return AssetDatabase
                .FindAssets("t:Field")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<Field>(path))
                .Where(field => field != null && field.Type == FieldType.Object)
                .Select(field => field.ObjectType)
                .Where(type => type != null && candidateSet.Contains(type))
                .Distinct()
                .OrderBy(type => type.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        #endregion
    }
}
