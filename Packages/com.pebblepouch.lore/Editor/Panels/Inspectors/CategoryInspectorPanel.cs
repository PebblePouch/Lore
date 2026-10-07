using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryInspectorPanel
    {
        #region Fields
        private readonly Action _repaint;

        private readonly WorkspaceState _state;

        private readonly CategoryHeaderSection _headerSection;
        private readonly DescriptionSection<Category> _descriptionSection;
        private readonly CategoryFieldSection _fieldsSection;
        private readonly CategoryInsightsSection _insightsSection;
        private readonly CategoryTableSection _tableSection;

        private UnityEditor.Editor _cachedEditor;

        private Rect? _pendingFieldRevealRect;

        private bool _wasHeaderEditing;
        private bool _pendingHeaderReveal;
        #endregion

        #region Properties
        public bool IsEditing => _headerSection.IsEditing || _descriptionSection.IsEditing;
        #endregion

        #region Constructor
        public CategoryInspectorPanel(
            WorkspaceState state,
            Action repaint,
            Action<Category> browseRequested,
            Action<Field> editRequested)
        {
            _state = state;
            _repaint = repaint;

            _headerSection = new CategoryHeaderSection();
            _descriptionSection = DescriptionSections.ForCategory(state);

            _fieldsSection =
                new CategoryFieldSection(
                    state,
                    repaint,
                    browseRequested,
                    editRequested,
                    RevealField);

            _insightsSection = new CategoryInsightsSection(state);
            _tableSection = new CategoryTableSection(state);

            SelectionService.CategoryChanged += SelectCategory;

            SelectCategory(SelectionService.CurrentCategory);
        }
        #endregion

        #region Public API
        public void BeginTitleEditing()
        {
            _headerSection.BeginTitleEditing();
        }

        public void AddEntry()
        {
            _headerSection.AddEntry();
        }

        public void Delete()
        {
            _headerSection.Delete();
        }

        public void Dispose()
        {
            SelectionService.CategoryChanged -= SelectCategory;

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

            if (SelectionService.CurrentCategory == null 
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
                        _insightsSection.Draw();
                        _tableSection.Draw();

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
        private void SelectCategory(Category category)
        {
            _descriptionSection.CommitEditing();

            GUI.FocusControl(null);
            EditorGUI.FocusTextInControl(null);

            if (_cachedEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(_cachedEditor);
                _cachedEditor = null;
            }

            _headerSection.SetCategory(category);

            _wasHeaderEditing = false;
            _pendingHeaderReveal = false;

            _descriptionSection.SetTarget(category);
            _fieldsSection.SetCategory(category);
            _insightsSection.SetCategory(category);
            _tableSection.SetCategory(category);

            if (category != null)
            {
                _cachedEditor = UnityEditor.Editor.CreateEditor(category);
            }

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
                InspectorSection.CategoryFields,
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