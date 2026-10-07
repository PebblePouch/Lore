using System;

namespace PebblePouch.Lore.Editor
{
    public static class LibraryFactory
    {
        #region Public API
        public static ItemBrowser<EntryLibraryItem> CreateEntryLibrary(Action<EntryLibraryItem> itemClicked)
        {
            var source = new EntryLibrarySource();

            var state = new BrowserState();

            var model = new LibraryModel<EntryLibraryItem>(source);

            var body = new EntryCardBody();

            var renderer =
                new LibraryCardRenderer<EntryLibraryItem>(
                    body.Draw);

            var browser =
                new ItemBrowser<EntryLibraryItem>(
                    LoreText.Library.EntryPickerTitle,
                    LoreText.Library.EntrySearch,
                    LoreText.Library.EntryEmpty,
                    LoreText.Library.EntryNoMatches,
                    state,
                    model,
                    renderer);

            browser.ItemClicked = itemClicked;

            return browser;
        }

        public static ItemBrowser<FieldLibraryItem> CreateFieldLibrary(
            Action repaint,
            Action<FieldLibraryItem> itemClicked,
            Func<bool> pickingEnabled,
            Func<bool> createStandalone)
        {
            var source = new FieldLibrarySource();

            var state = new BrowserState();

            var model = new LibraryModel<FieldLibraryItem>(source);

            FieldCardBody body = null;

            ItemBrowser<FieldLibraryItem> browser = null;

            void RevealAndEdit(Field field)
            {
                if (field == null)
                    return;

                browser.ClearSearch();
                browser.Refresh();

                browser.Reveal(item => item.Field == field);

                body.BeginEditing(field);

                repaint?.Invoke();
            }

            void CreateField()
            {
                var service = new FieldService();

                if (MissingFieldAssignmentContext.IsActive)
                {
                    Category repairCategory = MissingFieldAssignmentContext.TargetCategory;

                    int repairIndex = MissingFieldAssignmentContext.TargetFieldIndex;

                    if (repairCategory == null 
                        || repairIndex < 0)
                    {
                        MissingFieldAssignmentContext.EndAssignment();

                        return;
                    }

                    Field field = service.CreateField();

                    if (field == null)
                        return;

                    var repairService = new FieldIntegrityRepairService();

                    bool replaced =
                        repairService.ReplaceMissingField(
                            repairCategory,
                            repairIndex,
                            field);

                    if (!replaced)
                    {
                        service.DeleteField(field);

                        return;
                    }

                    MissingFieldAssignmentContext.EndAssignment();

                    RevealAndEdit(field);

                    return;
                }

                if (createStandalone?.Invoke() ?? false)
                {
                    Field field = service.CreateField();

                    if (field == null)
                        return;

                    LoreRefreshService.Refresh();

                    RevealAndEdit(field);

                    return;
                }

                Category category = SelectionService.CurrentCategory;

                if (category == null)
                    return;

                Field createdField = service.CreateField(category);

                if (createdField == null)
                    return;

                RevealAndEdit(createdField);
            }

            body = new FieldCardBody();

            body.FieldDuplicated = RevealAndEdit;

            body.FieldEditingEnded =
                field =>
                {
                    if (field == null)
                        return;

                    browser.Reveal(item => item.Field == field);

                    browser.Highlight(field.Id);

                    repaint?.Invoke();
                };

            body.FieldFeedbackShown =
                field =>
                {
                    if (field == null)
                        return;

                    browser.Reveal(item => item.Field == field);

                    repaint?.Invoke();
                };

            var renderer =
                new LibraryCardRenderer<FieldLibraryItem>(
                    body.Draw,
                    item => item.IsInCategory,
                    item => item.EditingId,
                    body.GetAdditionalHeight,
                    body.DrawAdditionalContent);

            browser =
                new ItemBrowser<FieldLibraryItem>(
                    LoreText.Library.FieldPickerTitle,
                    LoreText.Library.FieldSearch,
                    LoreText.Library.FieldEmpty,
                    LoreText.Library.FieldNoMatches,
                    state,
                    model,
                    renderer,
                    CreateField,
                    item =>
                        (pickingEnabled?.Invoke() ?? true) &&
                        !item.IsInCategory,
                    item =>
                        body.BeginEditing(
                            item.Field),
                    (item, completed) =>
                        body.Delete(
                            item.Field,
                            completed));

            body.FieldDeleteRequested =
                item =>
                    browser.RequestDelete(
                        item);

            browser.ItemClicked =
                itemClicked;

            return browser;
        }

        public static ItemBrowser<TagLibraryItem> CreateTagLibrary(
            Action repaint,
            Action<TagLibraryItem> itemClicked,
            Action<TagDefinition> editingCommitted = null,
            Func<bool> pickingEnabled = null)
        {
            var service = new TagLibraryService();

            var source =
                new TagLibrarySource(
                    service.GetCollection);

            var state = new BrowserState();

            var model = new LibraryModel<TagLibraryItem>(source);

            var body = new TagCardBody();

            ItemBrowser<TagLibraryItem> browser = null;

            void RevealAndEdit(TagDefinition tag)
            {
                if (tag == null)
                    return;

                browser.ClearSearch();
                browser.Refresh();

                browser.Reveal(item => item.Tag == tag);

                body.BeginEditing(tag);

                repaint?.Invoke();
            }

            void CreateTag()
            {
                TagDefinition tag =
                    service.CreateTag(
                        LoreText.Defaults.NewTag);

                if (tag == null)
                    return;

                RevealAndEdit(tag);
            }

            body.TagEditingEnded =
                tag =>
                {
                    if (tag == null)
                        return;

                    browser.Reveal(
                        item => item.Tag == tag);

                    browser.Highlight(
                        tag.Id);

                    repaint?.Invoke();
                };

            body.TagFeedbackShown =
                tag =>
                {
                    if (tag == null)
                        return;

                    browser.Reveal(
                        item =>
                            item.Tag == tag);

                    repaint?.Invoke();
                };

            body.TagEditingCommitted =
                tag =>
                {
                    editingCommitted?.Invoke(tag);
                };

            var renderer =
                new LibraryCardRenderer<TagLibraryItem>(
                    body.Draw,
                    item => item.IsInEntry,
                    item => item.EditingId,
                    body.GetAdditionalHeight,
                    body.DrawAdditionalContent);

            browser =
                new ItemBrowser<TagLibraryItem>(
                    LoreText.Library.TagPickerTitle,
                    LoreText.Library.TagSearch,
                    LoreText.Library.TagEmpty,
                    LoreText.Library.TagNoMatches,
                    state,
                    model,
                    renderer,
                    CreateTag,
                    item =>
                        (pickingEnabled?.Invoke() ?? true) &&
                        !item.IsInEntry,
                    item =>
                        body.BeginEditing(
                            item.Tag),
                    (item, completed) =>
                        body.Delete(
                            item.Tag,
                            completed));

            body.TagDeleteRequested =
                item =>
                    browser.RequestDelete(
                        item);

            browser.ItemClicked =
                itemClicked;

            return browser;
        }

        public static ItemBrowser<GlyphLibraryItem> CreateGlyphLibrary(
            Action<GlyphLibraryItem> itemClicked,
            Func<Entry> getEntryContext,
            Action<Entry> useCategoryGlyph,
            Func<bool> pickingEnabled = null)
        {
            var service = new GlyphLibraryService();

            var source =
                new GlyphLibrarySource(
                    service.GetCollection);

            var state = new BrowserState();

            var model =
                new LibraryModel<GlyphLibraryItem>(
                    source);

            var body = new GlyphCardBody();

            var renderer =
                new LibraryCardRenderer<GlyphLibraryItem>(
                    body.Draw);

            void DrawTools()
            {
                Entry entry = getEntryContext?.Invoke();

                if (entry == null)
                    return;

                string categoryGlyph =
                    entry.Category != null
                        ? entry.Category.Glyph
                        : "?";

                LibraryToolCard.Draw(
                    string.IsNullOrWhiteSpace(
                        categoryGlyph)
                        ? "?"
                        : categoryGlyph,
                    LoreText.Library.UseCategoryGlyph,
                    () => useCategoryGlyph?.Invoke(
                        entry),
                    !entry.HasGlyphOverride);
            }

            float GetToolsHeight()
            {
                return getEntryContext?.Invoke() != null
                    ? LibraryStyle.CardHeight
                    + LibraryStyle.CardSpacing
                    : 0f;
            }

            var browser =
                new ItemBrowser<GlyphLibraryItem>(
                    LoreText.Library.GlyphPickerTitle,
                    LoreText.Library.GlyphSearch,
                    LoreText.Library.GlyphEmpty,
                    LoreText.Library.GlyphNoMatches,
                    state,
                    model,
                    renderer,
                    canPick: item => pickingEnabled?.Invoke() ?? true,
                    drawTools: DrawTools,
                    getToolsHeight: GetToolsHeight);

            browser.ItemClicked =
                itemClicked;

            return browser;
        }

        public static ItemBrowser<ColorLibraryItem> CreateColorLibrary(
            Action<ColorLibraryItem> itemClicked,
            Func<bool> pickingEnabled = null)
        {
            var service = new ColorLibraryService();

            var source =
                new ColorLibrarySource(
                    service.GetCollection);

            var state = new BrowserState();

            var model = new LibraryModel<ColorLibraryItem>(source);

            var body = new ColorCardBody();

            var renderer =
                new LibraryCardRenderer<ColorLibraryItem>(
                    body.Draw);

            var browser =
                new ItemBrowser<ColorLibraryItem>(
                    LoreText.Library.ColorPickerTitle,
                    LoreText.Library.ColorSearch,
                    LoreText.Library.ColorEmpty,
                    LoreText.Library.ColorNoMatches,
                    state,
                    model,
                    renderer,
                    canPick: item => pickingEnabled?.Invoke() ?? true);

            browser.ItemClicked = itemClicked;

            return browser;
        }
        #endregion
    }
}