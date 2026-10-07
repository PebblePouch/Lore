using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class WorkspaceState
    {
        #region Fields
        private const string InspectorSectionPrefsPrefix = "Lore.InspectorSection.";

        private readonly Dictionary<Category, bool> _foldouts = new();
        private readonly Dictionary<InspectorSection, bool> _inspectorSections = new();

        // Sections that start collapsed until the user expands them once.
        private static readonly HashSet<InspectorSection> CollapsedByDefault = new() { InspectorSection.CategoryTable };

        public BrowserState ContentBrowser { get; } = new();

        public Vector2 RightScroll { get; set; }
        public float RightViewportHeight { get; set; }
        public float RightContentHeight { get; set; }
        #endregion

        #region Public API
        public bool IsExpanded(Category category)
        {
            if (!_foldouts.ContainsKey(category))
            {
                _foldouts[category] = true;
            }

            return _foldouts[category];
        }

        public void SetExpanded(
            Category category,
            bool expanded)
        {
            _foldouts[category] = expanded;
        }

        public bool IsInspectorSectionExpanded(
            InspectorSection section)
        {
            if (_inspectorSections.TryGetValue(
                    section,
                    out bool expanded))
            {
                return expanded;
            }

            expanded =
                EditorPrefs.GetBool(
                    GetInspectorSectionPrefsKey(
                        section),
                    !CollapsedByDefault.Contains(section));

            _inspectorSections[section] =
                expanded;

            return expanded;
        }

        public void SetInspectorSectionExpanded(
            InspectorSection section,
            bool expanded)
        {
            _inspectorSections[section] =
                expanded;

            EditorPrefs.SetBool(
                GetInspectorSectionPrefsKey(
                    section),
                expanded);
        }

        public void ToggleInspectorSection(
            InspectorSection section)
        {
            SetInspectorSectionExpanded(
                section,
                !IsInspectorSectionExpanded(
                    section));
        }
        #endregion

        #region Helpers
        private static string GetInspectorSectionPrefsKey(
            InspectorSection section)
        {
            return
                $"{InspectorSectionPrefsPrefix}{section}";
        }
        #endregion
    }
}