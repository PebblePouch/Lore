using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryHost : IDisposable
    {
        private enum LibraryInteractionMode
        {
            Pick,
            Manage
        }

        #region Types
        private enum LibraryMode
        {
            None,
            Fields,
            Entries,
            Tags,
            Glyphs,
            Colors
        }
        #endregion

        #region Fields
        private readonly Action _repaint;

        private readonly EntryService _entryService = new();
        private readonly FieldIntegrityRepairService _fieldIntegrityRepairService = new();

        private readonly ItemBrowser<FieldLibraryItem> _fieldLibrary;
        private readonly ItemBrowser<EntryLibraryItem> _entryLibrary;
        private readonly ItemBrowser<TagLibraryItem> _tagLibrary;
        private readonly ItemBrowser<GlyphLibraryItem> _glyphLibrary;
        private readonly ItemBrowser<ColorLibraryItem> _colorLibrary;

        private Entry _glyphEntryContext;

        private LibraryInteractionMode _interactionMode = LibraryInteractionMode.Pick;
        private LibraryInteractionMode _previousInteractionMode = LibraryInteractionMode.Pick;

        private LibraryMode _activeLibrary;
        private LibraryMode _previousLibrary;

        private TagDefinition _tagCloseAfterEdit;
        #endregion

        #region Properties
        public bool IsOpen =>
            _activeLibrary != LibraryMode.None;
        #endregion

        #region Constructor
        public LibraryHost(Action repaint)
        {
            _repaint = repaint;
            
            _entryLibrary =
                LibraryFactory.CreateEntryLibrary(
                    AssignEntry);

            _fieldLibrary =
                LibraryFactory.CreateFieldLibrary(
                    repaint,
                    AssignField,
                    () =>
                        _interactionMode ==
                        LibraryInteractionMode.Pick,
                    () =>
                        _interactionMode ==
                        LibraryInteractionMode.Manage);

            _tagLibrary =
                LibraryFactory.CreateTagLibrary(
                    repaint,
                    AssignTag,
                    HandleTagEditingCommitted,
                    () =>
                        _interactionMode ==
                        LibraryInteractionMode.Pick);

            _glyphLibrary =
                LibraryFactory.CreateGlyphLibrary(
                    AssignGlyph,
                    () => _glyphEntryContext,
                    UseCategoryGlyph,
                    () =>
                        _interactionMode ==
                        LibraryInteractionMode.Pick);

            _colorLibrary =
                LibraryFactory.CreateColorLibrary(
                    AssignColor,
                    () =>
                        _interactionMode ==
                        LibraryInteractionMode.Pick);

            Subscribe();
        }
        #endregion

        #region Public API
        public void Draw(float availableHeight)
        {
            HandleEscape();

            SetAvailableHeight(availableHeight);

            switch (_activeLibrary)
            {
                case LibraryMode.Fields:
                    _fieldLibrary.Draw();
                    break;

                case LibraryMode.Entries:
                    _entryLibrary.Draw();
                    break;

                case LibraryMode.Tags:
                    _tagLibrary.Draw();
                    break;

                case LibraryMode.Glyphs:
                    _glyphLibrary.Draw();
                    break;

                case LibraryMode.Colors:
                    _colorLibrary.Draw();
                    break;
            }
        }

        public void ShowFields()
        {
            _fieldLibrary.SetTitle(LoreText.Library.FieldPickerTitle);

            _fieldLibrary.ClearFilter();

            _fieldLibrary.Refresh();
            _fieldLibrary.FocusSearch();

            Show(LibraryMode.Fields);
        }

        public void ShowFields(Category category)
        {
            ShowFields();
        }

        public void ShowFieldForEdit(Field field)
        {
            if (field == null)
                return;

            _fieldLibrary.ClearSearch();
            _fieldLibrary.Refresh();

            _fieldLibrary.Reveal(
                item => item.Field == field);

            LibraryEditRequestService.RequestField(
                field);

            Show(LibraryMode.Fields);
        }

        public void RevealField(Field field)
        {
            if (field == null)
                return;

            _fieldLibrary.SetTitle(LoreText.Library.FieldManagerTitle);

            _fieldLibrary.ClearFilter();
            _fieldLibrary.ClearSearch();
            _fieldLibrary.Refresh();

            _fieldLibrary.Reveal(
                item => item.Field == field);

            _fieldLibrary.Highlight(
                field.Id);

            Show(LibraryMode.Fields, LibraryInteractionMode.Manage);
        }

        public void ManageUnusedFields()
        {
            var analyzer = new LibraryUsageAnalyzer(LoreRepository.Instance);

            _fieldLibrary.SetTitle(LoreText.Library.FieldManagerTitle);
            _fieldLibrary.ClearSearch();

            _fieldLibrary.SetFilter(
                item =>
                    item?.Field != null &&
                    !analyzer.IsFieldUsed(
                        item.Field),
                LoreText.Library.FieldUnusedLabel);

            _fieldLibrary.Refresh();
            _fieldLibrary.FocusSearch();

            Show(LibraryMode.Fields, LibraryInteractionMode.Manage);
        }

        public void ManageUnusedTags()
        {
            var analyzer = new LibraryUsageAnalyzer(LoreRepository.Instance);

            _tagLibrary.SetTitle(LoreText.Library.TagManagerTitle);
            _tagLibrary.ClearSearch();

            _tagLibrary.SetFilter(
                item =>
                    item?.Tag != null &&
                    !analyzer.IsTagUsed(
                        item.Tag),
                LoreText.Library.TagUnusedLabel);

            _tagLibrary.Refresh();
            _tagLibrary.FocusSearch();

            Show(LibraryMode.Tags, LibraryInteractionMode.Manage);
        }

        public void ManageUnusedGlyphs()
        {
            var analyzer = new LibraryUsageAnalyzer(LoreRepository.Instance);

            _glyphEntryContext = null;

            _glyphLibrary.SetTitle(LoreText.Library.GlyphManagerTitle);
            _glyphLibrary.ClearSearch();

            _glyphLibrary.SetFilter(
                item =>
                    item != null &&
                    !analyzer.IsGlyphUsed(
                        item.Glyph),
                LoreText.Library.GlyphUnusedLabel);

            _glyphLibrary.Refresh();
            _glyphLibrary.FocusSearch();

            Show(LibraryMode.Glyphs, LibraryInteractionMode.Manage);
        }

        public void ManageUnusedColors()
        {
            var analyzer = new LibraryUsageAnalyzer(LoreRepository.Instance);

            _colorLibrary.SetTitle(LoreText.Library.ColorManagerTitle);
            _colorLibrary.ClearSearch();

            _colorLibrary.SetFilter(
                item =>
                    item != null &&
                    !analyzer.IsColorUsed(
                        item.Color),
                LoreText.Library.ColorUnusedLabel);

            _colorLibrary.Refresh();
            _colorLibrary.FocusSearch();

            Show(LibraryMode.Colors, LibraryInteractionMode.Manage);
        }

        public void ShowEntries()
        {
            // Adding to an Entries list (only Entries not listed yet, re-evaluated while adding) or picking for a
            // Field limited to some Categories: only pickable Entries.
            if (EntryAssignmentContext.IsListTarget ||
                EntryAssignmentContext.IsRestricted)
            {
                _entryLibrary.SetFilter(
                    item => EntryAssignmentContext.CanAdd(item.Entry),
                    EntryAssignmentContext.IsListTarget
                        ? string.Format(
                            LoreText.Library.EntryAddToListFilter,
                            EntryAssignmentContext.TargetField.Title)
                        : string.Format(
                            LoreText.Library.EntryAllowedCategoriesFilter,
                            LoreText.Common.JoinList(
                                EntryAssignmentContext.TargetField.AllowedCategories
                                    .Where(category => category != null)
                                    .Select(category => category.Title)
                                    .ToList())));
            }
            else
            {
                _entryLibrary.ClearFilter();
            }

            PreselectAssignedEntry();

            _entryLibrary.Refresh();
            _entryLibrary.FocusSearch();

            Show(LibraryMode.Entries);
        }

        // Entry field with a value (Entry page, Tabular Editing): the Library opens on the current Entry, like the
        // Glyph and Color Libraries. Not for Entries lists (they add) or values the Field no longer allows.
        private void PreselectAssignedEntry()
        {
            _entryLibrary.ClearHighlight();

            if (!EntryAssignmentContext.IsActive ||
                EntryAssignmentContext.IsListTarget)
            {
                return;
            }

            Entry current = EntryAssignmentContext.CurrentEntry;

            if (!EntryAssignmentContext.CanAdd(current))
                return;

            // The current Entry must not be hidden by an old search.
            _entryLibrary.ClearSearch();

            _entryLibrary.Preselect(
                item =>
                    item.Entry ==
                    current);
        }

        public void ShowEntries(Entry entry)
        {
            ShowEntries();
        }

        public void ShowTags()
        {
            _tagLibrary.SetTitle(LoreText.Library.TagPickerTitle);
            _tagLibrary.ClearFilter();

            _tagLibrary.Refresh();
            _tagLibrary.FocusSearch();

            Show(LibraryMode.Tags);
        }

        public void ShowTags(Entry entry)
        {
            ShowTags();
        }

        public void ShowTagForEdit(TagDefinition tag)
        {
            if (tag == null)
                return;

            _tagCloseAfterEdit = tag;

            _tagLibrary.SetTitle(LoreText.Library.TagPickerTitle);
            _tagLibrary.ClearFilter();

            _tagLibrary.ClearSearch();
            _tagLibrary.Refresh();

            _tagLibrary.Reveal(item => item.Tag == tag);

            LibraryEditRequestService.RequestTag(tag);

            Show(LibraryMode.Tags);
        }

        public void ShowGlyphs()
        {
            PrepareGlyphPicker();

            _glyphEntryContext = null;

            Show(LibraryMode.Glyphs);
        }

        public void ShowGlyphs(string glyph)
        {
            PrepareGlyphPicker();

            _glyphEntryContext = null;

            _glyphLibrary.ClearSearch();
            _glyphLibrary.ClearHighlight();

            if (!string.IsNullOrWhiteSpace(
                    glyph))
            {
                _glyphLibrary.Preselect(
                    item =>
                        item.Glyph ==
                        glyph);
            }

            Show(LibraryMode.Glyphs);
        }

        public void ShowEntryGlyphs(Entry entry)
        {
            if (entry == null)
                return;

            PrepareGlyphPicker();

            _glyphEntryContext = entry;

            _glyphLibrary.ClearSearch();
            _glyphLibrary.ClearHighlight();

            if (entry.HasGlyphOverride &&
                !string.IsNullOrWhiteSpace(
                    entry.Glyph))
            {
                _glyphLibrary.Preselect(
                    item =>
                        item.Glyph ==
                        entry.Glyph);
            }

            Show(LibraryMode.Glyphs);
        }

        public void ShowNestedGlyphs()
        {
            PrepareGlyphPicker();

            _glyphEntryContext = null;

            ShowNested(LibraryMode.Glyphs);
        }

        public void ShowNestedGlyphs(string glyph)
        {
            PrepareGlyphPicker();

            _glyphEntryContext = null;

            _glyphLibrary.ClearSearch();
            _glyphLibrary.ClearHighlight();

            if (!string.IsNullOrWhiteSpace(glyph))
            {
                _glyphLibrary.Preselect(
                    item =>
                        item.Glyph ==
                        glyph);
            }

            ShowNested(LibraryMode.Glyphs);
        }

        public void ShowColors()
        {
            PrepareColorPicker();

            Show(LibraryMode.Colors);
        }

        public void ShowColors(Color color)
        {
            PrepareColorPicker();

            _colorLibrary.ClearSearch();
            _colorLibrary.ClearHighlight();
            _colorLibrary.Preselect(
                item =>
                    item.Color ==
                    color);

            Show(LibraryMode.Colors);
        }

        public void ShowNestedColors()
        {
            PrepareColorPicker();

            ShowNested(LibraryMode.Colors);
        }

        public void ShowNestedColors(Color color)
        {
            PrepareColorPicker();

            _colorLibrary.ClearSearch();
            _colorLibrary.ClearHighlight();
            _colorLibrary.Preselect(
                item =>
                    item.Color ==
                    color);

            ShowNested(LibraryMode.Colors);
        }

        public bool TryClose()
        {
            if (LibraryEditingService.IsActive 
                && !LibraryEditingService.CommitEditing())
            {
                return false;
            }

            _glyphEntryContext = null;
            _tagCloseAfterEdit = null;

            LoreGUIFocus.Clear();

            MissingFieldAssignmentContext.EndAssignment();
            TagAssignmentContext.End();

            Show(LibraryMode.None);

            return true;
        }

        public void Close()
        {
            TryClose();
        }

        public void ClearFilters()
        {
            _fieldLibrary.ClearFilter();
            _entryLibrary.ClearFilter();
            _tagLibrary.ClearFilter();
            _glyphLibrary.ClearFilter();
            _colorLibrary.ClearFilter();
        }

        public void Dispose()
        {
            Unsubscribe();

            _fieldLibrary.Dispose();
            _entryLibrary.Dispose();
            _tagLibrary.Dispose();
            _glyphLibrary.Dispose();
            _colorLibrary.Dispose();
        }
        #endregion

        #region Navigation
        private void Show(LibraryMode mode)
        {
            Show(mode, LibraryInteractionMode.Pick);
        }

        private void Show(
            LibraryMode mode,
            LibraryInteractionMode interactionMode)
        {
            _previousLibrary = LibraryMode.None;

            _previousInteractionMode = LibraryInteractionMode.Pick;

            _activeLibrary = mode;

            _interactionMode = interactionMode;

            SetKeyboardOwner(mode);

            _repaint?.Invoke();
        }

        private void ShowNested(LibraryMode mode)
        {
            _previousLibrary = _activeLibrary;

            _previousInteractionMode = _interactionMode;

            _activeLibrary = mode;

            _interactionMode = LibraryInteractionMode.Pick;

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Library);

            _repaint?.Invoke();
        }

        private void Return()
        {
            if (_previousLibrary == LibraryMode.None)
            {
                Close();

                return;
            }

            LibraryMode previousLibrary = _previousLibrary;

            LibraryInteractionMode previousInteractionMode = _previousInteractionMode;

            _previousLibrary = LibraryMode.None;

            _previousInteractionMode = LibraryInteractionMode.Pick;

            _activeLibrary = previousLibrary;

            _interactionMode = previousInteractionMode;

            SetKeyboardOwner(previousLibrary);

            _repaint?.Invoke();
        }

        private void SetAvailableHeight(float height)
        {
            switch (_activeLibrary)
            {
                case LibraryMode.Fields:
                    _fieldLibrary.SetAvailableHeight(
                        height);
                    break;

                case LibraryMode.Entries:
                    _entryLibrary.SetAvailableHeight(
                        height);
                    break;

                case LibraryMode.Tags:
                    _tagLibrary.SetAvailableHeight(
                        height);
                    break;

                case LibraryMode.Glyphs:
                    _glyphLibrary.SetAvailableHeight(
                        height);
                    break;

                case LibraryMode.Colors:
                    _colorLibrary.SetAvailableHeight(
                        height);
                    break;
            }
        }

        private static void SetKeyboardOwner(LibraryMode mode)
        {
            if (mode != LibraryMode.None)
            {
                LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Library);

                return;
            }

            bool hasSelection =
                SelectionService.CurrentCategory != null ||
                SelectionService.CurrentEntry != null;

            LoreKeyboardFocus.SetOwner(
                hasSelection
                    ? LoreKeyboardOwner.Inspector
                    : LoreKeyboardOwner.Content);
        }

        private void HandleEscape()
        {
            if (_activeLibrary == LibraryMode.None)
                return;

            if (Event.current.type != EventType.KeyDown)
                return;

            if (Event.current.keyCode != KeyCode.Escape)
                return;

            if (HandleLibraryEscape())
                return;

            CancelAssignment();

            if (_previousLibrary != LibraryMode.None)
            {
                Return();
            }
            else
            {
                Close();
            }

            Event.current.Use();
        }

        private bool HandleLibraryEscape()
        {
            return _activeLibrary switch
            {
                LibraryMode.Fields => _fieldLibrary.HandleEscape(),

                LibraryMode.Entries => _entryLibrary.HandleEscape(),

                LibraryMode.Tags => _tagLibrary.HandleEscape(),

                LibraryMode.Glyphs => _glyphLibrary.HandleEscape(),

                LibraryMode.Colors => _colorLibrary.HandleEscape(),

                _ => false
            };
        }

        private void CancelAssignment()
        {
            switch (_activeLibrary)
            {
                case LibraryMode.Fields:
                    MissingFieldAssignmentContext.EndAssignment();
                    break;

                case LibraryMode.Entries:
                    EntryAssignmentContext.EndAssignment();
                    break;

                case LibraryMode.Glyphs:
                    GlyphAssignmentContext.EndAssignment();
                    break;

                case LibraryMode.Colors:
                    ColorAssignmentContext.EndAssignment();
                    break;
            }
        }

        private void PrepareGlyphPicker()
        {
            _glyphLibrary.SetTitle(LoreText.Library.GlyphPickerTitle);

            _glyphLibrary.ClearFilter();
            _glyphLibrary.FocusSearch();
        }

        private void PrepareColorPicker()
        {
            _colorLibrary.SetTitle(LoreText.Library.ColorPickerTitle);

            _colorLibrary.ClearFilter();
            _colorLibrary.FocusSearch();
        }
        #endregion

        #region Actions
        private static void AssignEntry(EntryLibraryItem item)
        {
            EntryAssignmentContext.Assign(item.Entry);
        }

        private void AssignField(FieldLibraryItem item)
        {
            if (item?.Field == null)
                return;

            if (_interactionMode == LibraryInteractionMode.Manage)
            {
                return;
            }

            if (MissingFieldAssignmentContext.IsActive)
            {
                Category repairCategory = MissingFieldAssignmentContext.TargetCategory;

                int fieldIndex = MissingFieldAssignmentContext.TargetFieldIndex;

                bool replaced =
                    _fieldIntegrityRepairService
                        .ReplaceMissingField(
                            repairCategory,
                            fieldIndex,
                            item.Field);

                if (!replaced)
                    return;

                MissingFieldAssignmentContext.EndAssignment();

                Close();

                return;
            }

            if (item.IsInCategory)
                return;

            Category category = SelectionService.CurrentCategory;

            if (category == null)
                return;

            bool added =
                new FieldService()
                    .AddExistingField(
                        category,
                        item.Field);

            if (!added)
                return;

            Close();
        }

        private void AssignTag(
            TagLibraryItem item)
        {
            if (item?.Tag == null)
                return;

            if (_interactionMode ==
                LibraryInteractionMode.Manage)
            {
                return;
            }

            // Tabular Editing selection: added to all selected Entries.
            if (TagAssignmentContext.IsActive)
            {
                if (!_entryService.AddTag(
                        TagAssignmentContext.Entries,
                        item.Tag))
                {
                    return;
                }

                Close();

                return;
            }

            Entry entry =
                SelectionService.CurrentEntry;

            if (entry == null)
                return;

            if (!_entryService.AddTag(
                    entry,
                    item.Tag))
            {
                return;
            }

            Close();
        }

        private void AssignGlyph(GlyphLibraryItem item)
        {
            if (_interactionMode == LibraryInteractionMode.Manage)
            {
                return;
            }

            if (!GlyphAssignmentContext.Assign(item.Glyph))
            {
                return;
            }

            Return();
        }

        private void UseCategoryGlyph(Entry entry)
        {
            if (entry == null)
                return;

            if (!_entryService.ClearGlyphOverride(
                    entry))
            {
                return;
            }

            GlyphAssignmentContext.EndAssignment();

            Close();
        }

        private void AssignColor(ColorLibraryItem item)
        {
            if (_interactionMode == LibraryInteractionMode.Manage)
            {
                return;
            }

            if (!ColorAssignmentContext.Assign(item.Color))
            {
                return;
            }

            Return();
        }

        private void HandleTagEditingCommitted(
            TagDefinition tag)
        {
            if (tag == null || tag != _tagCloseAfterEdit)
            {
                return;
            }

            _tagCloseAfterEdit = null;

            Close();
        }
        #endregion

        #region Events
        private void Subscribe()
        {
            LibraryService.OpenEntryLibrary += ShowEntries;
            LibraryService.OpenFieldLibrary += ShowFields;
            LibraryService.OpenTagLibrary += ShowTags;
            LibraryService.OpenGlyphLibrary += ShowGlyphs;
            LibraryService.OpenColorLibrary += ShowColors;

            LibraryService.OpenNestedGlyphLibrary += ShowNestedGlyphs;
            LibraryService.OpenNestedColorLibrary += ShowNestedColors;

            LibraryService.OpenGlyphLibraryWithSelection += ShowGlyphs;
            LibraryService.OpenColorLibraryWithSelection += ShowColors;
            LibraryService.OpenNestedGlyphLibraryWithSelection += ShowNestedGlyphs;
            LibraryService.OpenNestedColorLibraryWithSelection += ShowNestedColors;

            LibraryService.OpenEntryGlyphLibrary += ShowEntryGlyphs;

            LibraryService.OpenUnusedFieldManager += ManageUnusedFields;
            LibraryService.RevealField += RevealField;
            LibraryService.OpenUnusedTagManager += ManageUnusedTags;
            LibraryService.OpenUnusedGlyphManager += ManageUnusedGlyphs;
            LibraryService.OpenUnusedColorManager += ManageUnusedColors;

            LibraryService.CloseLibrary += Close;
        }

        private void Unsubscribe()
        {
            LibraryService.OpenEntryLibrary -= ShowEntries;
            LibraryService.OpenFieldLibrary -= ShowFields;
            LibraryService.OpenTagLibrary -= ShowTags;
            LibraryService.OpenGlyphLibrary -= ShowGlyphs;
            LibraryService.OpenColorLibrary -= ShowColors;

            LibraryService.OpenNestedGlyphLibrary -= ShowNestedGlyphs;
            LibraryService.OpenNestedColorLibrary -= ShowNestedColors;

            LibraryService.OpenGlyphLibraryWithSelection -= ShowGlyphs;
            LibraryService.OpenColorLibraryWithSelection -= ShowColors;
            LibraryService.OpenNestedGlyphLibraryWithSelection -= ShowNestedGlyphs;
            LibraryService.OpenNestedColorLibraryWithSelection -= ShowNestedColors;

            LibraryService.OpenEntryGlyphLibrary -= ShowEntryGlyphs;

            LibraryService.OpenUnusedTagManager -= ManageUnusedTags;
            LibraryService.OpenUnusedFieldManager -= ManageUnusedFields;
            LibraryService.RevealField -= RevealField;
            LibraryService.OpenUnusedGlyphManager -= ManageUnusedGlyphs;
            LibraryService.OpenUnusedColorManager -= ManageUnusedColors;

            LibraryService.CloseLibrary -= Close;
        }
        #endregion
    }
}