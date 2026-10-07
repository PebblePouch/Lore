using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryInspectorPanel
    {
        #region Fields
        private readonly Action _repaint;

        private readonly WorkspaceState _state;
        private readonly EntryHeaderSection _headerSection;
        private readonly DescriptionSection<Entry> _descriptionSection;
        private readonly EntryFieldSection _fieldsSection;
        private readonly EntryTagsSection _tagsSection;
        private readonly EntryInsightsSection _insightsSection;

        private UnityEditor.Editor _cachedEditor;

        private Rect? _pendingFieldRevealRect;

        private bool _wasHeaderEditing;
        private bool _pendingHeaderReveal;
        #endregion

        #region Properties
        public bool IsEditing => _headerSection.IsEditing || _descriptionSection.IsEditing;
        #endregion

        #region Constructor
        public EntryInspectorPanel(
            WorkspaceState state,
            Action repaint,
            Action<Entry> browseTagsRequested,
            Action<TagDefinition> editTagRequested)
        {
            _state = state;
            _repaint = repaint;

            _headerSection = new EntryHeaderSection();
            _descriptionSection = DescriptionSections.ForEntry(state);
            _fieldsSection =
                new EntryFieldSection(
                    state,
                    RevealField);

            _tagsSection =
                new EntryTagsSection(
                    state,
                    repaint,
                    browseTagsRequested,
                    editTagRequested);

            _insightsSection = new EntryInsightsSection(state);

            SelectionService.EntryChanged += SelectEntry;

            SelectEntry(SelectionService.CurrentEntry);
        }
        #endregion

        #region Public API
        public void BeginTitleEditing()
        {
            _headerSection.BeginTitleEditing();
        }

        public void Duplicate()
        {
            _headerSection.Duplicate();
        }

        public void Delete()
        {
            _headerSection.Delete();
        }

        public void Dispose()
        {
            SelectionService.EntryChanged -= SelectEntry;

            if (_cachedEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(_cachedEditor);
                _cachedEditor = null;
            }
        }

        public void Draw()
        {
            if (Event.current.type ==
                EventType.MouseDown)
            {
                if (!_headerSection.HandleOutsideClick(
                        Event.current.mousePosition))
                {
                    Event.current.Use();
                }
                else
                {
                    _descriptionSection.HandleOutsideClick(
                        Event.current.mousePosition);
                }
            }

            if (SelectionService.CurrentEntry == null 
                || _cachedEditor == null)
            {
                GUILayout.Label(LoreText.Inspector.NothingSelected);
                return;
            }

            _state.RightScroll =
                EditorLayout.ScrollView(
                    _state.RightScroll,
                    () =>
                    {
                        DrawHeader();
                        _insightsSection.DrawIntegrity();

                        _descriptionSection.Draw();
                        _fieldsSection.Draw();
                        _tagsSection.Draw();
                        _insightsSection.Draw();

                        HandlePendingFieldReveal();

                        Rect contentEndRect =
                            GUILayoutUtility.GetRect(
                                0f,
                                0f,
                                GUILayout.ExpandWidth(true));

                        if (Event.current.type ==
                            EventType.Repaint)
                        {
                            _state.RightContentHeight =
                                contentEndRect.yMax;
                        }
                    });
        }
        #endregion

        #region Draw
        private void DrawHeader()
        {
            bool wasEditing =
                _wasHeaderEditing;

            _headerSection.Draw();

            bool isEditing =
                _headerSection.IsEditing;

            if (!wasEditing &&
                isEditing)
            {
                _pendingHeaderReveal =
                    true;

                _repaint?.Invoke();
            }

            _wasHeaderEditing =
                isEditing;

            HandlePendingHeaderReveal();
        }
        #endregion

        #region Helpers
        private void SelectEntry(Entry entry)
        {
            _headerSection.CommitEditing();
            _descriptionSection.CommitEditing();

            GUI.FocusControl(null);
            EditorGUI.FocusTextInControl(null);

            if (_cachedEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(_cachedEditor);
                _cachedEditor = null;
            }

            _headerSection.SetEntry(entry);
            
            _wasHeaderEditing = false;
            _pendingHeaderReveal = false;

            _descriptionSection.SetTarget(entry);
            _fieldsSection.SetEntry(entry);
            _tagsSection.SetEntry(entry);
            _insightsSection.SetEntry(entry);

            if (entry != null)
                _cachedEditor = UnityEditor.Editor.CreateEditor(entry);

            _repaint?.Invoke();
        }

        private void HandlePendingHeaderReveal()
        {
            if (!_pendingHeaderReveal)
                return;

            if (Event.current.type !=
                EventType.Repaint)
            {
                return;
            }

            GUI.ScrollTo(
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f));

            _pendingHeaderReveal =
                false;

            _repaint?.Invoke();
        }

        private void RevealField(
            Rect rect)
        {
            _state.SetInspectorSectionExpanded(
                InspectorSection.EntryFields,
                true);

            _pendingFieldRevealRect =
                rect;

            _repaint?.Invoke();
        }

        private void HandlePendingFieldReveal()
        {
            if (!_pendingFieldRevealRect.HasValue)
                return;

            GUI.ScrollTo(
                _pendingFieldRevealRect.Value);

            _pendingFieldRevealRect =
                null;

            _repaint?.Invoke();
        }
        #endregion
    }
}