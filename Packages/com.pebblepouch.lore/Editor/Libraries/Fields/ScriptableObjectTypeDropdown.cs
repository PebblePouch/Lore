using System;

using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Searchable list of ScriptableObject types for ScriptableObject Fields (see ScriptableObjectTypeService).
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
            }
        }
        #endregion

        #region Fields
        private readonly Action<Type> _selected;
        #endregion

        #region Constructor
        public ScriptableObjectTypeDropdown(Action<Type> selected)
            : base(new AdvancedDropdownState())
        {
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
            var root =
                new AdvancedDropdownItem(
                    LoreText.Library.ObjectTypeDropdownTitle);

            foreach (Type type in ScriptableObjectTypeService.GetCandidates())
            {
                root.AddChild(
                    new TypeItem(
                        type));
            }

            if (ScriptableObjectTypeService.GetCandidates().Count == 0)
            {
                root.AddChild(
                    new AdvancedDropdownItem(
                        LoreText.Library.ObjectTypeEmpty)
                    {
                        enabled = false
                    });
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeItem typeItem)
                _selected?.Invoke(typeItem.Type);
        }
        #endregion
    }
}
