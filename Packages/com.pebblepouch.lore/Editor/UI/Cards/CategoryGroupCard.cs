using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public class CategoryGroupCard
    {
        #region Fields
        private readonly WorkspaceState _state;

        private readonly EntryService _entryService = new();

        private readonly BrowserNavigation _navigation;

        private Category _pendingExpandCategory;

        private readonly Action<Entry> _entryCreated;
        #endregion

        #region Callbacks
        private readonly Action<Category> _categoryEditRequested;
        private readonly Action<Entry> _entryEditRequested;

        private readonly Action<Category> _categoryDeleteRequested;
        private readonly Action<Entry> _entryDeleteRequested;
        #endregion

        #region Constructor
        public CategoryGroupCard(
            WorkspaceState state,
            BrowserNavigation navigation,
            Action<Entry> entryCreated,
            Action<Category> categoryEditRequested,
            Action<Entry> entryEditRequested,
            Action<Category> categoryDeleteRequested,
            Action<Entry> entryDeleteRequested)
        {
            _state = state;
            _navigation = navigation;
            _entryCreated = entryCreated;
            _categoryEditRequested = categoryEditRequested;
            _entryEditRequested = entryEditRequested;
            _categoryDeleteRequested = categoryDeleteRequested;
            _entryDeleteRequested = entryDeleteRequested;
        }
        #endregion

        #region Public API
        public void Draw(
            CategoryEntryGroup group,
            Action<Category, Rect, Rect> registerCategoryHandle,
            Action<Entry, Rect, Rect> registerEntryHandle,
            Action<Category, Rect> categoryDrawn = null,
            Action<Entry, Rect> entryDrawn = null)
        {
            bool expanded =
                _state.IsExpanded(
                    group.Category);

            GUILayout.Space(
                BrowserStyle.GroupTopSpacing);

            string categoryId =
                $"Category:{group.Category.Id}";

            bool categoryHighlighted =
                _navigation.IsHighlighted(
                    categoryId);

            Rect categoryRect;

            expanded =
                CategoryCard.Draw(
                    group,
                    expanded,
                    categoryHighlighted,
                    () => CreateEntry(
                        group.Category),
                    category =>
                        SelectCategory(
                            category,
                            categoryId),
                    registerCategoryHandle,
                    out categoryRect);

            if (_pendingExpandCategory ==
                group.Category)
            {
                expanded = true;

                _pendingExpandCategory = null;
            }

            categoryDrawn?.Invoke(
                group.Category,
                categoryRect);

            _navigation.Register(
                categoryId,
                categoryRect,
                () => ActivateCategory(group.Category),
                () => CollapseCategory(group.Category),
                () => ExpandCategory(group.Category),
                edit:
                () => _categoryEditRequested?.Invoke(group.Category),
                delete:
                () => _categoryDeleteRequested?.Invoke(group.Category),
                createChild:
                () => CreateEntry(group.Category));

            _state.SetExpanded(
                group.Category,
                expanded);

            GUILayout.Space(
                BrowserStyle.CategoryCardBottomSpacing);

            if (!expanded)
                return;

            foreach (var entry in group.Entries)
            {
                GUILayout.Space(
                    BrowserStyle.EntryCardTopSpacing);

                string entryId =
                    $"Entry:{entry.Id}";

                bool entryHighlighted =
                    _navigation.IsHighlighted(
                        entryId);

                Rect entryRect =
                    EntryCard.Draw(
                        entry,
                        entryHighlighted,
                        selectedEntry =>
                            SelectEntry(
                                selectedEntry,
                                entryId),
                        registerEntryHandle);

                entryDrawn?.Invoke(
                    entry,
                    entryRect);

                _navigation.Register(
                    entryId,
                    entryRect,
                    () => ActivateEntry(entry),
                    edit:
                    () => _entryEditRequested?.Invoke(entry),
                    delete:
                    () => _entryDeleteRequested?.Invoke(entry));

                GUILayout.Space(
                    BrowserStyle.EntryCardBottomSpacing);
            }

            GUILayout.Space(
                BrowserStyle.GroupBottomSpacing);
        }
        #endregion

        #region Draw
        #endregion

        #region Commands
        private void SelectCategory(
            Category category,
            string navigationId)
        {
            if (category == null)
                return;

            SelectionService.SelectCategory(
                category);

            LoreKeyboardFocus.SetOwner(
                LoreKeyboardOwner.Content);

            _navigation.Highlight(
                navigationId);

            LoreRepaintService.Repaint();
        }

        private void SelectEntry(
            Entry entry,
            string navigationId)
        {
            if (entry == null)
                return;

            SelectionService.SelectEntry(
                entry);

            LoreKeyboardFocus.SetOwner(
                LoreKeyboardOwner.Content);

            _navigation.Highlight(
                navigationId);

            LoreRepaintService.Repaint();
        }

        private static void ActivateCategory(Category category)
        {
            if (category == null)
                return;

            bool alreadySelected =
                SelectionService.CurrentCategory ==
                    category &&
                SelectionService.CurrentEntry ==
                    null;

            if (alreadySelected)
            {
                LoreKeyboardFocus.SetOwner(
                    LoreKeyboardOwner.Inspector);

                LoreRepaintService.Repaint();

                return;
            }

            SelectionService.SelectCategory(
                category);

            LoreKeyboardFocus.SetOwner(
                LoreKeyboardOwner.Content);

            LoreRepaintService.Repaint();
        }

        private static void ActivateEntry(Entry entry)
        {
            if (entry == null)
                return;

            bool alreadySelected =
                SelectionService.CurrentEntry ==
                    entry;

            if (alreadySelected)
            {
                LoreKeyboardFocus.SetOwner(
                    LoreKeyboardOwner.Inspector);

                LoreRepaintService.Repaint();

                return;
            }

            SelectionService.SelectEntry(
                entry);

            LoreKeyboardFocus.SetOwner(
                LoreKeyboardOwner.Content);

            LoreRepaintService.Repaint();
        }

        private void CreateEntry(
            Category category)
        {
            Entry entry =
                _entryService.CreateEntry(
                    category,
                    LoreText.Defaults.NewEntry);

            if (entry == null)
                return;

            _pendingExpandCategory =
                category;

            _entryCreated?.Invoke(
                entry);
        }

        private void CollapseCategory(
            Category category)
        {
            if (!_state.IsExpanded(
                    category))
            {
                return;
            }

            _state.SetExpanded(
                category,
                false);

            LoreRepaintService.Repaint();
        }

        private void ExpandCategory(
            Category category)
        {
            if (_state.IsExpanded(
                    category))
            {
                return;
            }

            _state.SetExpanded(
                category,
                true);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}
