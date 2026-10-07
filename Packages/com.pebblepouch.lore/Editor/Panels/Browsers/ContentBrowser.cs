using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ContentBrowser
    {
        #region Fields
        public const string SearchControlName = "ContentBrowserSearch";

        private readonly Action _repaint;
        private readonly Func<bool> _isLibraryOpen;

        private readonly WorkspaceState _state;
        private readonly ContentBrowserModel _model;

        private readonly BrowserShell _shell;
        private readonly BrowserNavigation _navigation;
        private readonly CategoryGroupCard _groupCard;

        private readonly CategoryService _categoryService = new();
        private readonly EntryService _entryService = new();

        private readonly DragListInteraction<Category> _categoryDrag;
        private readonly DragListInteraction<Entry> _entryDrag;

        private Category _pendingRevealCategory;
        private Rect? _pendingRevealRect;

        private Entry _pendingRevealEntry;

        private readonly CategoryDeleteCommand _categoryDeleteCommand = new();
        private readonly EntryDeleteCommand _entryDeleteCommand = new();

        private Category _deletePreviousCategory;
        private Entry _deletePreviousEntry;

        private int? _pendingDeleteRecoveryIndex;

        private Category _renamePreviousCategory;
        private Entry _renamePreviousEntry;

        // "Restore previous selection" mode: history before the rename (its select + restore are not navigation).
        private NavigationHistoryService.Snapshot _renameHistorySnapshot;

        private bool _pendingRenameCompletion;
        private string _pendingRenameTargetId;

        private string _activeFilterLabel;
        private Entry _matchingFieldRevealEntry;
        #endregion

        #region Constructor
        public ContentBrowser(
            WorkspaceState state,
            ContentBrowserModel model,
            Action repaint,
            Func<bool> isLibraryOpen)
        {
            _state = state;
            _model = model;
            _repaint = repaint;
            _isLibraryOpen = isLibraryOpen;

            _categoryDrag =
                new DragListInteraction<Category>(
                    repaint);

            _entryDrag =
                new DragListInteraction<Entry>(
                    repaint,
                    (entry, draggedEntry) =>
                        entry.Category ==
                        draggedEntry.Category);

            _navigation =
                new BrowserNavigation(
                    _state.ContentBrowser,
                    resetOnActivate: false);

            _groupCard =
                new CategoryGroupCard(
                    state,
                    _navigation,
                    HandleEntryCreated,
                    BeginCategoryRename,
                    BeginEntryRename,
                    DeleteCategory,
                    DeleteEntry);

            _shell =
                new BrowserShell(
                    LoreText.Browser.ContentsTitle,
                    LoreText.Browser.SearchPlaceholder,
                    SearchControlName,
                    _state.ContentBrowser,
                    _model,
                    CreateCategory,
                    _navigation.Reset,
                    ActivateSearch,
                    drawTools: DrawFilter,
                    getToolsHeight: GetFilterHeight);
        }
        #endregion

        #region Public API
        public void SetAvailableHeight(float height)
        {
            _shell.SetAvailableHeight(height);
        }

        public void Draw()
        {
            HandlePendingRenameCompletion();

            HandleFilterRequests();

            HandleMatchingFieldReveal();

            HandleRevealRequests();

            HandlePendingRevealScroll();

            HandleKeyboardNavigation();

            _navigation.BeginFrame();

            EditorGUILayout.BeginVertical(
                BrowserStyle.BrowserLayout,
                GUILayout.Width(
                    BrowserStyle.BrowserWidth));

            if (LoreKeyboardFocus.IsOwner(LoreKeyboardOwner.Content))
            {
                HandleEscape();

                EnsureSearchFocus();
            }

            _shell.Draw(DrawContent);

            EditorGUILayout.EndVertical();
        }

        public void FocusNavigationOnCurrentSelection()
        {
            _shell.FocusSearch();

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Content);

            HighlightCurrentSelection();

            _repaint?.Invoke();
        }

        public void ReleaseSearchFocus()
        {
            _shell.ReleaseSearchFocus();
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            var groups = _model.GetGroups();

            GUILayout.Space(BrowserStyle.ListTopSpacing);

            if (groups.Count == 0)
            {
                DrawEmpty();

                HandlePendingDeleteHighlightRecovery();

                return;
            }

            DrawGroups(groups);

            HandlePendingDeleteHighlightRecovery();

            GUILayout.Space(BrowserStyle.ListBottomSpacing);
        }

        private void DrawEmpty()
        {
            if (string.IsNullOrWhiteSpace(_model.Search))
            {
                EmptyStateCard.Draw(LoreText.Browser.SearchNoContents);
            }
            else
            {
                EmptyStateCard.Draw(LoreText.Browser.SearchNoMatches);
            }
        }

        private void DrawGroups(IReadOnlyList<CategoryEntryGroup> groups)
        {
            var snapshot = groups.ToArray();

            _categoryDrag.BeginFrame();
            _entryDrag.BeginFrame();

            foreach (var group in snapshot)
            {
                _groupCard.Draw(
                    group,
                    RegisterCategoryHandle,
                    RegisterEntryHandle,
                    HandleCategoryDrawn,
                    HandleEntryDrawn);
            }

            _categoryDrag.HandleEvents(
                (category, insertionIndex) =>
                    _categoryService.MoveCategory(
                        category,
                        insertionIndex));

            _categoryDrag.DrawDropLine(DropLineDrawer.Draw);

            _entryDrag.HandleEvents(
                (entry, insertionIndex) =>
                    _entryService.MoveEntry(
                        entry,
                        insertionIndex));

            _entryDrag.DrawDropLine(DropLineDrawer.Draw);
        }

        private void DrawFilter()
        {
            if (!_model.HasFilter)
                return;

            BrowserFilterCard.Draw(
                _activeFilterLabel,
                ClearFilter);
        }
        #endregion

        #region Commands
        private void CreateCategory()
        {
            Category category = _categoryService.CreateCategory(LoreText.Defaults.NewCategory);

            if (category == null)
                return;

            SelectionService.SelectCategory(category);

            InspectorEditRequestService.RequestCategoryTitle(category);

            _pendingRevealCategory = category;

            _repaint?.Invoke();
        }

        private void ActivateSearch()
        {
            if (LoreKeyboardFocus.IsOwner(LoreKeyboardOwner.Library))
            {
                return;
            }

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Content);

            HighlightCurrentSelection();
        }

        private void DeleteCategory(Category category)
        {
            if (category == null)
                return;

            BeginBrowserDelete();

            SelectionService.SelectCategory(category);

            int targetIndex = _navigation.HighlightedIndex;

            bool wasPreviouslySelected = _deletePreviousCategory == category;

            _categoryDeleteCommand.Execute(
                category,
                deleted =>
                {
                    EndBrowserDelete(
                        deleted,
                        wasPreviouslySelected,
                        targetIndex,
                        $"Category:{category.Id}");
                });
        }

        private void DeleteEntry(Entry entry)
        {
            if (entry == null)
                return;

            BeginBrowserDelete();

            SelectionService.SelectEntry(entry);

            int targetIndex = _navigation.HighlightedIndex;

            bool wasPreviouslySelected = _deletePreviousEntry == entry;

            _entryDeleteCommand.Execute(
                entry,
                deleted =>
                {
                    EndBrowserDelete(
                        deleted,
                        wasPreviouslySelected,
                        targetIndex,
                        $"Entry:{entry.Id}");
                });
        }

        public void SetCategoryFilter(
            Func<Category, bool> predicate,
            string label)
        {
            _model.SetCategoryFilter(predicate);

            ContentBrowserFilterContext.Clear();

            _matchingFieldRevealEntry = null;

            _activeFilterLabel = label ?? string.Empty;

            _navigation.Reset();

            _state.ContentBrowser.Scroll = Vector2.zero;

            _repaint?.Invoke();
        }

        public void SetEntryFilter(
            Func<Entry, bool> predicate,
            string label,
            Field matchingField = null)
        {
            _model.SetEntryFilter(predicate);

            ContentBrowserFilterContext.SetMatchingField(matchingField);

            _matchingFieldRevealEntry = null;

            _activeFilterLabel = label ?? string.Empty;

            _navigation.Reset();

            _state.ContentBrowser.Scroll = Vector2.zero;

            _repaint?.Invoke();
        }

        public void SetCombinedFilter(
            Func<Category, bool> categoryPredicate,
            Func<Entry, bool> entryPredicate,
            string label)
        {
            _model.SetCombinedFilter(
                categoryPredicate,
                entryPredicate);

            ContentBrowserFilterContext.Clear();

            _matchingFieldRevealEntry = null;

            _activeFilterLabel = label ?? string.Empty;

            _navigation.Reset();

            _state.ContentBrowser.Scroll = Vector2.zero;

            _repaint?.Invoke();
        }

        public void ClearFilter()
        {
            _model.ClearFilter();

            ContentBrowserFilterContext.Clear();

            _matchingFieldRevealEntry = null;

            _activeFilterLabel = string.Empty;

            _navigation.Reset();

            _state.ContentBrowser.Scroll = Vector2.zero;

            _repaint?.Invoke();
        }
        #endregion

        #region Helpers
        private void HighlightCurrentSelection()
        {
            if (SelectionService.CurrentEntry != null)
            {
                string entryId = $"Entry:{SelectionService.CurrentEntry.Id}";

                _navigation.Highlight(entryId);

                return;
            }

            if (SelectionService.CurrentCategory != null)
            {
                string categoryId = $"Category:{SelectionService.CurrentCategory.Id}";

                _navigation.Highlight(categoryId);
            }
        }

        private void HandleRevealRequests()
        {
            Category category =ContentBrowserRequestService.ConsumeCategoryReveal();

            if (category != null)
            {
                if (!IsCategoryVisible(category))
                {
                    ClearSearchAndFilter();
                }

                if (IsCategoryVisible(category))
                {
                    _pendingRevealCategory = category;
                }

                _repaint?.Invoke();
            }

            Entry entry = ContentBrowserRequestService.ConsumeEntryReveal();

            if (entry == null)
                return;

            // An explicit reveal must always land: clear search/filter only when they hide the target.
            if (!IsEntryVisible(entry))
            {
                ClearSearchAndFilter();
            }

            // Never leave a reveal pending for a row that will not be drawn.
            if (!IsEntryVisible(entry))
            {
                _repaint?.Invoke();

                return;
            }

            if (entry.Category != null)
            {
                _state.SetExpanded(entry.Category, true);
            }

            _pendingRevealEntry = entry;

            _repaint?.Invoke();
        }

        private bool IsCategoryVisible(Category category)
        {
            return _model
                .GetGroups()
                .Any(group => group.Category == category);
        }

        private bool IsEntryVisible(Entry entry)
        {
            return _model
                .GetGroups()
                .Any(group => group.Entries.Contains(entry));
        }

        private void ClearSearchAndFilter()
        {
            _shell.ClearSearch();

            if (_model.HasFilter)
            {
                ClearFilter();
            }
        }

        private void HandleKeyboardNavigation()
        {
            if (!LoreKeyboardFocus.IsOwner(
                    LoreKeyboardOwner.Content))
            {
                return;
            }

            if (Event.current.type == EventType.KeyDown
                && Event.current.keyCode == KeyCode.Insert)
            {
                CreateCategory();

                Event.current.Use();

                return;
            }

            _navigation.HandleKeyboard();

            if (!LoreKeyboardFocus.IsOwner(
                    LoreKeyboardOwner.Content))
            {
                _shell.ReleaseSearchFocus();
            }
        }

        private void HandlePendingRevealScroll()
        {
            if (!_pendingRevealRect.HasValue)
                return;

            _navigation.Reveal(_pendingRevealRect.Value);

            _pendingRevealRect = null;

            LoreRepaintService.Repaint();
        }

        private void HandleCategoryDrawn(
            Category category,
            Rect rect)
        {
            if (_pendingRevealCategory == null)
                return;

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (_pendingRevealCategory != category)
            {
                return;
            }

            _pendingRevealRect = rect;

            _pendingRevealCategory = null;

            LoreRepaintService.Repaint();
        }

        private void HandleEntryDrawn(
            Entry entry,
            Rect rect)
        {
            if (_pendingRevealEntry == null)
                return;

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (_pendingRevealEntry != entry)
            {
                return;
            }

            _pendingRevealRect = rect;

            _pendingRevealEntry = null;

            LoreRepaintService.Repaint();
        }

        private void HandleEntryCreated(
            Entry entry)
        {
            if (entry == null)
                return;

            SelectionService.SelectEntry(entry);

            InspectorEditRequestService.RequestEntryTitle(entry);

            ContentBrowserRequestService.RequestEntryReveal(entry);

            _repaint?.Invoke();
        }

        private void BeginCategoryRename(Category category)
        {
            if (category == null)
                return;

            _renamePreviousCategory = SelectionService.CurrentCategory;
            _renamePreviousEntry = SelectionService.CurrentEntry;

            _renameHistorySnapshot = CreateRenameHistorySnapshot();

            SelectionService.SelectCategory(category);

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Inspector);

            InspectorEditRequestService
                .RequestCategoryTitle(
                    category,
                    () => EndBrowserRename(
                        $"Category:{category.Id}"));

            _repaint?.Invoke();
        }

        private void BeginEntryRename(Entry entry)
        {
            if (entry == null)
                return;

            _renamePreviousCategory = SelectionService.CurrentCategory;
            _renamePreviousEntry = SelectionService.CurrentEntry;

            _renameHistorySnapshot = CreateRenameHistorySnapshot();

            SelectionService.SelectEntry(entry);

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Inspector);

            InspectorEditRequestService
                .RequestEntryTitle(
                    entry,
                    () => EndBrowserRename(
                        $"Entry:{entry.Id}"));

            _repaint?.Invoke();
        }

        private void EndBrowserRename(string targetId)
        {
            _pendingRenameTargetId = targetId;
            _pendingRenameCompletion = true;

            _repaint?.Invoke();
        }

        private void BeginBrowserDelete()
        {
            _deletePreviousCategory = SelectionService.CurrentCategory;
            _deletePreviousEntry = SelectionService.CurrentEntry;
        }

        private void EndBrowserDelete(
            bool deleted,
            bool wasPreviouslySelected,
            int targetIndex,
            string targetId)
        {
            RestoreSelectionAfterDelete(
                deleted,
                wasPreviouslySelected);

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Content);

            _shell.FocusSearch();

            if (deleted)
            {
                _pendingDeleteRecoveryIndex = targetIndex;
            }
            else
            {
                _navigation.Highlight(targetId);
            }

            _deletePreviousCategory = null;
            _deletePreviousEntry = null;

            _repaint?.Invoke();
        }

        private void RestoreSelectionAfterDelete(
            bool deleted,
            bool wasPreviouslySelected)
        {
            if (deleted &&
                wasPreviouslySelected)
            {
                SelectionService.Clear();

                return;
            }

            if (_deletePreviousEntry != null)
            {
                SelectionService.SelectEntry(_deletePreviousEntry);

                return;
            }

            if (_deletePreviousCategory != null)
            {
                SelectionService.SelectCategory(_deletePreviousCategory);

                return;
            }

            SelectionService.Clear();
        }

        private void HandlePendingDeleteHighlightRecovery()
        {
            if (!_pendingDeleteRecoveryIndex.HasValue)
                return;

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            _navigation.HighlightAtIndex(_pendingDeleteRecoveryIndex.Value);

            _pendingDeleteRecoveryIndex = null;

            LoreRepaintService.Repaint();
        }

        private static NavigationHistoryService.Snapshot CreateRenameHistorySnapshot()
        {
            return LorePreferences.BrowserRenameSelectionMode
                == BrowserRenameSelectionMode.RestorePreviousSelection
                    ? NavigationHistoryService.CreateSnapshot()
                    : null;
        }

        private void RestoreSelectionAfterRename()
        {
            if (_renamePreviousEntry != null)
            {
                SelectionService.SelectEntry(_renamePreviousEntry);

                return;
            }

            if (_renamePreviousCategory != null)
            {
                SelectionService.SelectCategory(_renamePreviousCategory);

                return;
            }

            SelectionService.Clear();
        }

        private void HandlePendingRenameCompletion()
        {
            if (!_pendingRenameCompletion)
                return;

            _pendingRenameCompletion = false;

            LoreGUIFocus.Clear();

            if (LorePreferences.BrowserRenameSelectionMode
                == BrowserRenameSelectionMode.RestorePreviousSelection)
            {
                RestoreSelectionAfterRename();

                // Drops the rename's select + restore steps (A -> B -> A stays A).
                NavigationHistoryService.Restore(
                    _renameHistorySnapshot);
            }

            _renameHistorySnapshot = null;

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Content);

            _shell.FocusSearch();

            if (!string.IsNullOrEmpty(_pendingRenameTargetId))
            {
                _navigation.Highlight(_pendingRenameTargetId);
            }

            _pendingRenameTargetId = null;

            _renamePreviousCategory = null;
            _renamePreviousEntry = null;

            _repaint?.Invoke();
        }

        private float GetFilterHeight()
        {
            return _model.HasFilter
                ? BrowserStyle.FilterHeight
                : 0f;
        }

        private void HandleFilterRequests()
        {
            ContentBrowserFilterRequest request =
                ContentBrowserRequestService
                    .ConsumeFilter();

            if (request == null)
                return;

            if (request.EntryPredicate != null &&
                request.CategoryPredicate != null)
            {
                SetCombinedFilter(
                    request.CategoryPredicate,
                    request.EntryPredicate,
                    request.Label);
            }
            else if (request.EntryPredicate != null)
            {
                SetEntryFilter(
                    request.EntryPredicate,
                    request.Label,
                    request.MatchingField);
            }
            else if (request.CategoryPredicate != null)
            {
                SetCategoryFilter(
                    request.CategoryPredicate, 
                    request.Label);
            }
            else
            {
                return;
            }

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Content);

            _shell.FocusSearch();
        }

        // While the Content browser owns keyboard focus, its search always holds the cursor (type-to-search),
        // regardless of whether focus arrived by click, keyboard, or history navigation.
        private void EnsureSearchFocus()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            // Dialogs and Quick Search own the keyboard while they are open.
            if (LoreDialog.IsOpen ||
                QuickSearchWindow.IsOpen)
            {
                return;
            }

            if (GUI.GetNameOfFocusedControl() == SearchControlName)
                return;

            _shell.FocusSearch();

            _repaint?.Invoke();
        }

        private void HandleEscape()
        {
            if (Event.current.type !=
                EventType.KeyDown)
            {
                return;
            }

            if (Event.current.keyCode !=
                KeyCode.Escape)
            {
                return;
            }

            if (_shell.HandleSearchEscape())
                return;

            if (!_model.HasFilter)
            {
                // Nothing to clear: the search keeps focus (type-to-search) instead of the text field dropping it.
                // An open Library still gets Escape (it closes).
                if (_isLibraryOpen?.Invoke() != true &&
                    GUI.GetNameOfFocusedControl() == SearchControlName)
                {
                    Event.current.Use();
                }

                return;
            }

            ClearFilter();

            _shell.FocusSearch();

            Event.current.Use();
        }

        private void HandleMatchingFieldReveal()
        {
            Field matchingField =
                ContentBrowserFilterContext.MatchingField;

            if (matchingField == null)
            {
                _matchingFieldRevealEntry =
                    null;

                return;
            }

            Entry entry =
                SelectionService.CurrentEntry;

            if (entry == null)
            {
                _matchingFieldRevealEntry =
                    null;

                return;
            }

            if (_matchingFieldRevealEntry ==
                entry)
            {
                return;
            }

            bool hasMatchingField =
                entry.Fields.Any(
                    fieldValue =>
                        fieldValue?.Field ==
                        matchingField);

            if (!hasMatchingField)
                return;

            _matchingFieldRevealEntry =
                entry;

            EntryFieldRevealRequestService.Request(
                entry,
                matchingField);

            _repaint?.Invoke();
        }
        #endregion

        #region Drag'n'Drop
        private void RegisterCategoryHandle(
            Category category,
            Rect handleRect,
            Rect cardRect)
        {
            _categoryDrag.Register(
                category,
                handleRect,
                cardRect);
        }

        private void RegisterEntryHandle(
            Entry entry,
            Rect handleRect,
            Rect cardRect)
        {
            _entryDrag.Register(
                entry,
                handleRect,
                cardRect);
        }
        #endregion
    }
}