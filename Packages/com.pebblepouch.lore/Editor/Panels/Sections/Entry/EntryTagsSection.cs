using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryTagsSection
    {
        #region Fields
        private readonly TagLibraryService _tagService = new();
        // Asked on use (cached by the service): never in the constructor, which runs before a Project may exist.
        private TagCollection TagCollection => _tagService.GetCollection();

        private readonly EntryService _entryService = new();
        private readonly EntryTagsHeaderSection _header = new();
        private readonly WorkspaceState _state;

        private Entry _entry;

        // Technical start value until the first Repaint measures the real width (then corrected right away).
        private float _availableTagWidth = 300f;
        #endregion

        #region Callbacks
        private readonly Action _repaint;
        private readonly Action<Entry> _browseRequested;
        private readonly Action<TagDefinition> _editRequested;
        #endregion

        #region Constructor
        public EntryTagsSection(
            WorkspaceState state,
            Action repaint,
            Action<Entry> browseRequested,
            Action<TagDefinition> editRequested)
        {
            _state = state;
            _repaint = repaint;
            _browseRequested = browseRequested;
            _editRequested = editRequested;
        }
        #endregion

        #region Public API
        public void SetEntry(
            Entry entry)
        {
            _entry =
                entry;

            if (_entry != null)
            {
                int removed =
                    _entry.RemoveInvalidTags(
                        TagCollection);

                if (removed > 0)
                {
                    LoreChangeTracker.MarkModified(
                        _entry);
                }
            }

            _header.SetEntry(
                entry,
                _browseRequested,
                CreateTag);
        }
        public void Draw()
        {
            if (_entry == null)
                return;

            bool expanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.EntryTags);

            SectionCard.Draw(
                () =>
                {
                    _header.Draw(
                        expanded,
                        () =>
                            _state.ToggleInspectorSection(
                                InspectorSection.EntryTags));

                    if (expanded)
                    {
                        DrawContent();
                    }
                });
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            // Spacing and width line are also drawn without tags, so the empty card has the same
            // height as an empty Fields table.
            GUILayout.Space(
                InspectorStyle.FieldTableTopSpacing);

            Rect widthRect =
                EditorGUILayout.GetControlRect(
                    false,
                    0f,
                    GUILayout.ExpandWidth(true));

            float layoutWidth =
                _availableTagWidth;

            // Layout wraps the tags with this width, so a new width needs one more repaint.
            if (Event.current.type == EventType.Repaint &&
                !Mathf.Approximately(
                    _availableTagWidth,
                    widthRect.width))
            {
                _availableTagWidth =
                    widthRect.width;

                LoreRepaintService.Repaint();
            }

            if (_entry.TagIds.Count == 0)
                return;

            float currentWidth =
                0f;

            bool rowOpen =
                false;

            foreach (string tagId in
                     _entry.TagIds.ToArray())
            {
                TagDefinition tag =
                    TagCollection.FindById(
                        tagId);

                if (tag == null)
                    continue;

                float itemWidth =
                    GetTagItemWidth(
                        tag);

                bool needsNewRow =
                    rowOpen &&
                    currentWidth +
                        itemWidth >
                    layoutWidth;

                if (needsNewRow)
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    GUILayout.Space(
                        InspectorStyle.TagRowSpacing);

                    rowOpen =
                        false;

                    currentWidth =
                        0f;
                }

                if (!rowOpen)
                {
                    GUILayout.BeginHorizontal();

                    rowOpen =
                        true;
                }

                DrawTag(
                    tag);

                GUILayout.Space(
                    InspectorStyle.TagSpacing);

                currentWidth +=
                    itemWidth +
                    InspectorStyle.TagSpacing;
            }

            if (rowOpen)
            {
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
        }

        private void DrawTag(
            TagDefinition tag)
        {
            float itemWidth =
                GetTagItemWidth(
                    tag);

            // MaxWidth instead of Width: a fixed width would make each tag row a minimum width that
            // IMGUI passes up to the whole Inspector (it could not get narrower than the last width).
            Rect tagRect =
                GUILayoutUtility.GetRect(
                    itemWidth,
                    InspectorStyle.TagHeight,
                    GUILayout.MinWidth(0f),
                    GUILayout.MaxWidth(
                        itemWidth),
                    GUILayout.ExpandWidth(false),
                    GUILayout.Height(
                        InspectorStyle.TagHeight));

            EditorGUI.DrawRect(
                tagRect,
                tag.Color);

            float buttonWidth =
                InspectorStyle.FieldButtonColumnWidth;

            Rect removeRect =
                new(
                    tagRect.xMax
                        - InspectorStyle.TagButtonHorizontalPadding
                        - buttonWidth,
                    tagRect.y
                        + InspectorStyle.TagButtonVerticalOffset,
                    buttonWidth,
                    tagRect.height);

            Rect titleRect =
                new(
                    tagRect.x
                        + InspectorStyle.TagHorizontalPadding,
                    tagRect.y,
                    tagRect.width
                        - InspectorStyle.TagHorizontalPadding * 2f
                        - buttonWidth
                        - InspectorStyle.TagButtonHorizontalPadding,
                    tagRect.height);

            Color previousContentColor = GUI.contentColor;

            GUI.contentColor =
                LoreColorUtility.GetContrastTextColor(
                    tag.Color);

            GUI.Label(
                titleRect,
                tag.Title,
                InspectorStyle.TagTitle);

            GUI.contentColor = previousContentColor;

            ButtonDrawer.Remove(
                removeRect,
                () => RemoveTag(
                    tag),
                ButtonDrawer.DisplayMode.Glyph,
                InspectorStyle.FieldButton);
        }
        #endregion

        #region Commands
        private void CreateTag(
            Entry entry)
        {
            if (entry == null)
                return;

            TagDefinition tag = _tagService.CreateTag(LoreText.Defaults.NewTag);

            if (tag == null)
                return;

            if (!_entryService.AddTag(
                    entry,
                    tag))
            {
                return;
            }

            _editRequested?.Invoke(tag);

            _repaint?.Invoke();
        }

        private void RemoveTag(TagDefinition tag)
        {
            if (_entry == null ||
                tag == null)
            {
                return;
            }

            if (!_entryService.RemoveTag(
                _entry,
                tag))
            {
                return;
            }

            _repaint?.Invoke();
        }
        #endregion

        #region Helpers
        private static float GetTagItemWidth(
            TagDefinition tag)
        {
            float textWidth =
                InspectorStyle.TagTitle
                    .CalcSize(
                        new GUIContent(
                            tag.Title))
                    .x;

            return
                InspectorStyle.TagHorizontalPadding
                + textWidth
                + InspectorStyle.TagHorizontalPadding
                + InspectorStyle.FieldButtonColumnWidth
                + InspectorStyle.TagButtonHorizontalPadding;
        }
        #endregion
    }
}