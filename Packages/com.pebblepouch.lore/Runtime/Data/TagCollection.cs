using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore
{
    public sealed class TagCollection : ScriptableObject
    {
        #region Fields
        [SerializeField, HideInInspector]
        private List<TagDefinition> _tags = new();
        #endregion

        #region Properties
        public IReadOnlyList<TagDefinition> Tags => _tags;
        #endregion

        #region Public API
        internal TagDefinition CreateTag(
            string title,
            Color color)
        {
            var tag = new TagDefinition();

            tag.Initialize(
                title,
                color);

            _tags.Add(tag);

            return tag;
        }

        public TagDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (TagDefinition tag in _tags)
            {
                if (tag != null 
                    && tag.Id == id)
                {
                    return tag;
                }
            }

            return null;
        }

        // Tag with this title (case-insensitive).
        public TagDefinition FindByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            string normalized = title.Trim();

            foreach (TagDefinition tag in _tags)
            {
                if (tag != null &&
                    string.Equals(
                        tag.Title,
                        normalized,
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    return tag;
                }
            }

            return null;
        }

        public bool ContainsTitle(
            string title,
            TagDefinition ignore = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            string normalized = title.Trim();

            foreach (TagDefinition tag in _tags)
            {
                if (tag == null 
                    || tag == ignore)
                {
                    continue;
                }

                if (string.Equals(
                        tag.Title,
                        normalized,
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        internal bool RemoveTag(TagDefinition tag)
        {
            if (tag == null)
                return false;

            return _tags.Remove(
                tag);
        }
        #endregion
    }
}