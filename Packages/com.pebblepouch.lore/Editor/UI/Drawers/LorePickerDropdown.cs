using System;
using System.Collections.Generic;

using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Object = UnityEngine.Object;

namespace PebblePouch.Lore.Editor
{
    // Searchable picker for Lore fields in the user's own Inspectors (LorePropertyDrawer): "None", then the items,
    // in groups (e.g. Entries per Category) or flat (a single group).
    public sealed class LorePickerDropdown : AdvancedDropdown
    {
        #region Types
        public readonly struct Item
        {
            public readonly string Label;
            public readonly Object Value;

            public Item(
                string label,
                Object value)
            {
                Label = label;
                Value = value;
            }
        }

        public readonly struct Group
        {
            public readonly string Label;
            public readonly IReadOnlyList<Item> Items;

            public Group(
                string label,
                IReadOnlyList<Item> items)
            {
                Label = label;
                Items = items;
            }
        }

        private sealed class ValueItem : AdvancedDropdownItem
        {
            public Object Value { get; }

            public ValueItem(
                string name,
                Object value)
                : base(name)
            {
                Value = value;
            }
        }
        #endregion

        #region Fields
        private readonly string _title;
        private readonly IReadOnlyList<Group> _groups;
        private readonly Action<Object> _selected;
        #endregion

        #region Constructor
        public LorePickerDropdown(
            string title,
            IReadOnlyList<Group> groups,
            Action<Object> selected)
            : base(new AdvancedDropdownState())
        {
            _title = title;
            _groups = groups;
            _selected = selected;

            minimumSize =
                new Vector2(
                    DrawerStyle.DropdownWidth,
                    DrawerStyle.DropdownHeight);
        }
        #endregion

        #region AdvancedDropdown
        protected override AdvancedDropdownItem BuildRoot()
        {
            var root =
                new AdvancedDropdownItem(
                    _title);

            root.AddChild(
                new ValueItem(
                    LoreText.Drawers.PickerNone,
                    null));

            bool flat = _groups.Count == 1;
            bool hasItems = false;

            foreach (Group group in _groups)
            {
                if (group.Items.Count == 0)
                    continue;

                AdvancedDropdownItem parent = root;

                if (!flat)
                {
                    parent =
                        new AdvancedDropdownItem(
                            group.Label);

                    root.AddChild(parent);
                }

                foreach (Item item in group.Items)
                {
                    parent.AddChild(
                        new ValueItem(
                            item.Label,
                            item.Value));

                    hasItems = true;
                }
            }

            if (!hasItems)
            {
                root.AddChild(
                    new AdvancedDropdownItem(
                        LoreText.Drawers.PickerEmpty)
                    {
                        enabled = false
                    });
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is ValueItem valueItem)
                _selected?.Invoke(valueItem.Value);
        }
        #endregion
    }
}
