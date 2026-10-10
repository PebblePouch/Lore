using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldCardBody
    {
        #region Fields
        private readonly FieldService _fieldService = new();
        private readonly FieldTypeChangeAnalyzer _typeChangeAnalyzer = new();
        private readonly FieldDeleteAnalyzer _deleteAnalyzer = new();
        private readonly RelationshipQueryService _relationshipQueries = new(LoreRepository.Instance);

        private Field _pendingTypeChangeConfirmation;
        private Field _approvedTypeChange;

        private readonly Dictionary<Field, LibraryInlineTextField> _titleEditors = new();
        private readonly Dictionary<Field, LibraryInlineEnumSelector> _typeEditors = new();
        private readonly Dictionary<Field, FieldCardEditState> _editStates = new();

        private readonly EnumOptionsEditor _enumOptionsEditor = new();
        #endregion

        #region Callbacks
        public Action<Field> FieldDuplicated;
        public Action<Field> FieldEditingEnded;
        public Action<Field> FieldFeedbackShown;

        public Action<FieldLibraryItem> FieldDeleteRequested;
        #endregion

        #region Public API
        public void Draw(
            Rect rect,
            FieldLibraryItem item)
        {
            if (LibraryEditRequestService.ConsumeField(item.Field))
            {
                BeginEditing(item.Field);
            }

            bool editing = LibraryEditingService.IsEditing(item.EditingId);

            LibraryCardLayout layout =
                new(
                    rect,
                    false,
                    true,
                    LibraryStyle.FieldCardMetadataRatio);

            if (editing)
            {
                DrawEditable(
                    layout,
                    item);
            }
            else if (item.IsInCategory)
            {
                DrawInactiveViewer(
                    layout,
                    item);
            }
            else
            {
                DrawViewer(
                    layout,
                    item);
            }

            DrawMenu(
                layout.Menu,
                item);
        }

        public void ShowReferences(Field field)
        {
            if (field == null)
                return;

            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    entry != null &&
                    entry.Fields.Any(
                        fieldValue =>
                            fieldValue?.Field ==
                            field),
                string.Format(LoreText.Library.ReferencesFilter, field.name),
                field);

            LibraryService.CloseLibrary?.Invoke();

            LoreRepaintService.Repaint();
        }

        public void Delete(
            Field field,
            Action<bool> completed = null)
        {
            if (field == null)
            {
                completed?.Invoke(
                    false);

                return;
            }

            if (LibraryEditingService.IsEditing(
                    $"Field:{field.Id}"))
            {
                if (!LibraryEditingService.CommitEditing())
                {
                    completed?.Invoke(
                        false);

                    return;
                }
            }

            FieldDeleteReport report =
                _deleteAnalyzer.Analyze(
                    field);

            LoreDialog.ConfirmDestructive(
                string.Format(LoreText.Library.FieldDialogTitle, field.name),
                GetDeleteWarning(
                    report),
                LoreText.Common.DeleteButton,
                () =>
                {
                    bool deleted =
                        _fieldService.DeleteField(
                            field);

                    if (deleted)
                    {
                        LoreRefreshService.Refresh();
                    }

                    completed?.Invoke(
                        deleted);
                },
                cancelled:
                    () =>
                        completed?.Invoke(
                            false));
        }

        public void BeginEditing(Field field)
        {
            if (field == null)
                return;

            string editingId = $"Field:{field.Id}";

            var state =
                new FieldCardEditState(
                    field);

            _editStates[field] = state;

            LibraryInlineTextField titleEditor =
                GetTitleEditor(
                    field);

            titleEditor.BeginEditing(
                state.Title);

            bool started =
                LibraryEditingService
                    .BeginEditing<FieldLibraryItem>(
                        editingId,
                        () => CommitEditing(
                            field),
                        () => CancelEditing(
                            field));

            if (started)
                return;

            titleEditor.EndEditing();

            _editStates.Remove(
                field);
        }

        public float GetAdditionalHeight(FieldLibraryItem item)
        {
            if (item?.Field == null)
                return 0f;

            if (!_editStates.TryGetValue(
                    item.Field,
                    out FieldCardEditState state))
            {
                return 0f;
            }

            // Type settings (edit mode: Enum options, allowed Categories or Vector dimension), then the feedback line.
            return EnumOptionsEditor.GetHeight(state)
                   + AllowedCategoriesEditor.GetHeight(state)
                   + VectorDimensionEditor.GetHeight(state)
                   + (string.IsNullOrWhiteSpace(state.Feedback)
                       ? 0f
                       : LibraryStyle.FeedbackHeight);
        }

        public void DrawAdditionalContent(
            Rect rect,
            FieldLibraryItem item)
        {
            if (item?.Field == null)
                return;

            if (!_editStates.TryGetValue(
                    item.Field,
                    out FieldCardEditState state))
            {
                return;
            }

            float optionsHeight =
                EnumOptionsEditor.GetHeight(
                    state);

            if (optionsHeight > 0f)
            {
                _enumOptionsEditor.Draw(
                    new Rect(rect.x, rect.y, rect.width, optionsHeight),
                    state);

                rect.yMin += optionsHeight;
            }

            float categoriesHeight =
                AllowedCategoriesEditor.GetHeight(
                    state);

            if (categoriesHeight > 0f)
            {
                AllowedCategoriesEditor.Draw(
                    new Rect(rect.x, rect.y, rect.width, categoriesHeight),
                    state);

                rect.yMin += categoriesHeight;
            }

            float dimensionHeight =
                VectorDimensionEditor.GetHeight(
                    state);

            if (dimensionHeight > 0f)
            {
                VectorDimensionEditor.Draw(
                    new Rect(rect.x, rect.y, rect.width, dimensionHeight),
                    state);

                rect.yMin += dimensionHeight;
            }

            if (string.IsNullOrWhiteSpace(
                    state.Feedback))
            {
                return;
            }

            Rect feedbackRect =
                new(
                    rect.x
                        + LibraryStyle.FeedbackHorizontalPadding,
                    rect.y
                        + LibraryStyle.FeedbackVerticalPadding,
                    Mathf.Max(
                        0f,
                        rect.width
                            - LibraryStyle.FeedbackHorizontalPadding * 2f),
                    Mathf.Max(
                        0f,
                        rect.height
                            - LibraryStyle.FeedbackVerticalPadding * 2f));

            GUI.Label(
                feedbackRect,
                state.Feedback,
                LibraryStyle.Feedback);
        }
        #endregion

        #region Draw
        private static void DrawViewer(
            LibraryCardLayout layout,
            FieldLibraryItem item)
        {
            GUI.Label(
                layout.Visual,
                item.Glyph,
                LibraryStyle.Glyph);

            GUI.Label(
                layout.Title,
                item.Title,
                LibraryStyle.Title);

            // Missing ScriptableObject / Object type: red, the tooltip explains the fix.
            bool missingType =
                FieldTypeIntegrityAnalyzer.HasMissingType(
                    item.Field);

            GUI.Label(
                layout.Metadata,
                new GUIContent(
                    item.Type,
                    !missingType
                        ? null
                        : string.IsNullOrEmpty(item.Field.ObjectTypeName)
                            ? LoreText.Validation.ObjectTypeRequired
                            : string.Format(
                                LoreText.Library.ObjectTypeMissingTooltip,
                                ScriptableObjectTypeService.GetShortName(item.Field.ObjectTypeName))),
                missingType
                    ? LibraryStyle.MetadataBroken
                    : LibraryStyle.Metadata);
        }

        private static void DrawInactiveViewer(
            LibraryCardLayout layout,
            FieldLibraryItem item)
        {
            Color previousColor =
                GUI.color;

            GUI.color =
                new Color(
                    previousColor.r,
                    previousColor.g,
                    previousColor.b,
                    previousColor.a * LibraryStyle.InactiveAlphaMultiplier);

            DrawViewer(
                layout,
                item);

            GUI.color =
                previousColor;
        }

        private void DrawEditable(
            LibraryCardLayout layout,
            FieldLibraryItem item)
        {
            if (!_editStates.TryGetValue(
                    item.Field,
                    out FieldCardEditState state))
            {
                return;
            }

            DrawGlyph(
                layout.Visual,
                item,
                state);

            DrawTitle(
                layout.Title,
                item,
                state);

            DrawType(
                layout.Metadata,
                item,
                state);
        }

        private void DrawGlyph(
            Rect rect,
            FieldLibraryItem item,
            FieldCardEditState state)
        {
            GUI.Label(
                rect,
                state.Glyph,
                LibraryStyle.Glyph);

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown ||
                !rect.Contains(
                    Event.current.mousePosition))
            {
                return;
            }

            GlyphAssignmentContext.BeginAssignment(
                glyph =>
                {
                    state.Glyph = glyph;
                });

            LibraryService.OpenNestedGlyphLibraryWithSelection?.Invoke(state.Glyph);

            Event.current.Use();
        }

        private void DrawTitle(
            Rect rect,
            FieldLibraryItem item,
            FieldCardEditState state)
        {
            LibraryInlineTextFieldResult result =
                GetTitleEditor(item.Field).Draw(
                    item.EditingId,
                    rect,
                    state.Title,
                    LoreText.Library.FieldTitlePlaceholder,
                    LibraryStyle.TitleField);

            if (state.Title != result.Value)
            {
                state.Feedback = string.Empty;
            }

            state.Title = result.Value;
        }

        // Field type selector. ScriptableObject / Object Fields show "DialogDefinition (SO)" / "Texture2D (OBJ)",
        // right-aligned like the viewer: the suffix is the type selector, the type name opens the searchable type
        // dropdown.
        private void DrawType(
            Rect rect,
            FieldLibraryItem item,
            FieldCardEditState state)
        {
            if (!ScriptableObjectTypeService.HasObjectType(state.Type))
            {
                GetTypeEditor(item.Field).Draw(
                    rect,
                    state.Type,
                    LibraryStyle.Metadata,
                    type => SelectType(state, type),
                    LoreText.FieldTypes.Get,
                    groups: FieldTypeMenu.Groups);

                return;
            }

            string suffix =
                ScriptableObjectTypeService.GetObjectTypeSuffix(
                    state.Type);

            string objectTypeText =
                GetObjectTypeText(
                    state);

            float suffixWidth =
                LibraryStyle.Metadata.CalcSize(new GUIContent(suffix)).x;

            // Gap between type name and suffix, measured in context (a lone space can measure ~0).
            float gapWidth =
                Mathf.Max(
                    0f,
                    LibraryStyle.Metadata.CalcSize(
                            new GUIContent(
                                string.Format(
                                    LoreText.FieldTypes.WithObjectType,
                                    objectTypeText,
                                    suffix)))
                        .x
                    - LibraryStyle.Metadata.CalcSize(new GUIContent(objectTypeText)).x
                    - suffixWidth);

            Rect suffixRect =
                new(
                    rect.xMax - suffixWidth,
                    rect.y,
                    Mathf.Min(
                        suffixWidth,
                        rect.width),
                    rect.height);

            GetTypeEditor(item.Field).Draw(
                suffixRect,
                state.Type,
                LibraryStyle.Metadata,
                type => SelectType(state, type),
                LoreText.FieldTypes.Get,
                suffix,
                FieldTypeMenu.Groups);

            DrawObjectType(
                new Rect(
                    rect.x,
                    rect.y,
                    Mathf.Max(
                        0f,
                        suffixRect.x - gapWidth - rect.x),
                    rect.height),
                objectTypeText,
                state);
        }

        private static void SelectType(
            FieldCardEditState state,
            FieldType type)
        {
            // Choosing ScriptableObject or Object opens the type dropdown right away (on the card's next draw). A type
            // that does not fit the new Field type (ScriptableObject class for an Object Field) is dropped.
            if (ScriptableObjectTypeService.HasObjectType(type) &&
                state.Type != type)
            {
                if (!ScriptableObjectTypeService.IsCandidate(
                        type,
                        state.ObjectTypeName))
                {
                    state.ObjectTypeName = string.Empty;
                    state.ObjectScriptGuid = string.Empty;
                }

                state.ObjectTypePickerRequested = true;
            }

            state.Type = type;
            state.Feedback = string.Empty;

            LoreRepaintService.Repaint();
        }

        // The chosen type name, right-aligned in rect; clicking it (or a pending request) opens the type dropdown.
        private static void DrawObjectType(
            Rect rect,
            string objectTypeText,
            FieldCardEditState state)
        {
            GUIStyle style =
                string.IsNullOrEmpty(state.ObjectTypeName)
                    ? LibraryStyle.MetadataPlaceholder
                    : LibraryStyle.Metadata;

            float width =
                Mathf.Min(
                    style.CalcSize(new GUIContent(objectTypeText)).x,
                    rect.width);

            Rect textRect =
                new(
                    rect.xMax - width,
                    rect.y,
                    width,
                    rect.height);

            GUI.Label(
                textRect,
                objectTypeText,
                style);

            EditorGUIUtility.AddCursorRect(
                textRect,
                MouseCursor.Link);

            bool clicked =
                Event.current.type == EventType.MouseDown &&
                textRect.Contains(Event.current.mousePosition);

            // Requested picker: on Repaint, where the card rect is real (not a Layout placeholder).
            bool requested =
                state.ObjectTypePickerRequested &&
                Event.current.type == EventType.Repaint;

            if (!clicked &&
                !requested)
            {
                return;
            }

            state.ObjectTypePickerRequested = false;

            new ScriptableObjectTypeDropdown(
                    state.Type,
                    type =>
                    {
                        state.ObjectTypeName = Field.GetTypeName(type);
                        state.ObjectScriptGuid = ScriptableObjectTypeService.FindScriptGuid(type);
                        state.Feedback = string.Empty;

                        LoreRepaintService.Repaint();
                    })
                .Show(textRect);

            if (clicked)
                Event.current.Use();
        }

        private static string GetObjectTypeText(FieldCardEditState state)
        {
            return string.IsNullOrEmpty(state.ObjectTypeName)
                ? LoreText.Library.ObjectTypePlaceholder
                : ScriptableObjectTypeService.GetShortName(state.ObjectTypeName);
        }

        private void DrawMenu(
            Rect rect,
            FieldLibraryItem item)
        {
            Rect menuRect = new(
                rect.x,
                rect.y + LibraryStyle.CardMenuVerticalInset,
                rect.width,
                rect.height - LibraryStyle.CardMenuVerticalInset * 2f);

            bool editing =
                LibraryEditingService.IsEditing(
                    item.EditingId);

            var menu =
                new FieldMenuElement();

            menu.SetField(
                item.Field,
                editing,
                () => BeginEditing(
                    item),
                () => SaveEditing(
                    item),
                () => ShowReferences(
                    item.Field),
                () =>
                {
                    if (FieldDeleteRequested != null)
                    {
                        FieldDeleteRequested.Invoke(
                            item);

                        return;
                    }

                    Delete(
                        item.Field);
                },
                field => FieldDuplicated?.Invoke(
                    field),
                LoreRefreshService.Refresh);

            menu.Draw(
                menuRect);
        }
        #endregion

        #region Commands
        private void BeginEditing(FieldLibraryItem item)
        {
            BeginEditing(item.Field);
        }

        private bool SaveEditing(FieldLibraryItem item)
        {
            if (!LibraryEditingService.IsEditing(item.EditingId))
            {
                return true;
            }

            return LibraryEditingService.CommitEditing();
        }

        private bool CommitEditing(Field field)
        {
            if (!_editStates.TryGetValue(
                    field,
                    out FieldCardEditState state))
            {
                return true;
            }

            LoreOperationResult validation =
                _fieldService.ValidateUpdate(
                    field,
                    state.Title);

            if (!validation.Success)
            {
                state.Feedback = validation.Message;

                FieldFeedbackShown?.Invoke(field);

                LoreRepaintService.Repaint();

                return false;
            }

            // A ScriptableObject / Object Field needs its type (also blocks saving after the type was lost).
            if (ScriptableObjectTypeService.HasObjectType(state.Type) &&
                string.IsNullOrEmpty(state.ObjectTypeName))
            {
                state.Feedback = LoreText.Validation.ObjectTypeRequired;

                FieldFeedbackShown?.Invoke(field);

                LoreRepaintService.Repaint();

                return false;
            }

            string optionsError =
                ValidateEnumOptions(
                    state);

            if (optionsError != null)
            {
                state.Feedback = optionsError;

                FieldFeedbackShown?.Invoke(field);

                LoreRepaintService.Repaint();

                return false;
            }

            state.Feedback = string.Empty;

            FieldTypeChangeReport typeChange =
                _typeChangeAnalyzer.Analyze(
                    field,
                    state.Type,
                    state.ObjectTypeName);

            bool typeChangeApproved = _approvedTypeChange == field;

            if (typeChangeApproved)
            {
                _approvedTypeChange = null;
            }

            if (typeChange.RequiresConfirmation && !typeChangeApproved)
            {
                RequestTypeChangeConfirmation(field, typeChange);

                return false;
            }

            // Removed options that are in use clear values (only when the type keeps options, e.g. Enum → Enums; a
            // confirmed type change that clears values skips this).
            if (FieldValueUtility.HasEnumOptions(state.Type) &&
                FieldValueUtility.HasEnumOptions(field.Type) &&
                !typeChangeApproved)
            {
                (IReadOnlyList<string> removedLabels, int affectedEntryCount) =
                    _fieldService.GetEnumOptionRemoval(
                        field,
                        state.EnumOptions);

                if (affectedEntryCount > 0)
                {
                    RequestConfirmation(
                        field,
                        string.Format(LoreText.Library.FieldTypeDialogTitle, field.name),
                        GetOptionRemovalWarning(removedLabels, affectedEntryCount));

                    return false;
                }
            }

            bool updated =
                _fieldService.UpdateField(
                    field,
                    state.Title,
                    state.Glyph,
                    state.Type,
                    state.ObjectTypeName,
                    state.ObjectScriptGuid,
                    state.EnumOptions,
                    state.AllowedCategories,
                    state.VectorDimension);

            if (!updated)
                return false;

            _pendingTypeChangeConfirmation = null;
            _approvedTypeChange = null;

            GetTitleEditor(field).EndEditing();

            _editStates.Remove(field);

            FieldEditingEnded?.Invoke(field);

            return true;
        }

        private void RequestTypeChangeConfirmation(
            Field field,
            FieldTypeChangeReport report)
        {
            RequestConfirmation(
                field,
                string.Format(LoreText.Library.FieldTypeDialogTitle, field.name),
                GetTypeChangeWarning(report));
        }

        // Confirms a change that clears set values (type change, removed Enum options); on confirm, the edit is
        // committed again with the change approved.
        private void RequestConfirmation(
            Field field,
            string title,
            string message)
        {
            if (field == null)
                return;

            if (_pendingTypeChangeConfirmation == field)
            {
                return;
            }

            _pendingTypeChangeConfirmation = field;

            LoreDialog.ConfirmDestructive(
                title,
                message,
                LoreText.Common.ChangeButton,
                () =>
                {
                    if (_pendingTypeChangeConfirmation != field)
                    {
                        return;
                    }

                    _pendingTypeChangeConfirmation = null;
                    _approvedTypeChange = field;

                    LibraryEditingService.CommitEditing();

                    LoreRepaintService.Repaint();
                },
                cancelled:
                    () =>
                    {
                        if (_pendingTypeChangeConfirmation == field)
                        {
                            _pendingTypeChangeConfirmation = null;
                        }

                        LoreRepaintService.Repaint();
                    });
        }

        private void CancelEditing(Field field)
        {
            if (_pendingTypeChangeConfirmation == field)
            {
                _pendingTypeChangeConfirmation = null;
            }

            if (_approvedTypeChange == field)
            {
                _approvedTypeChange = null;
            }

            GetTitleEditor(field).EndEditing();

            _editStates.Remove(field);

            FieldEditingEnded?.Invoke(field);
        }

        private void CancelEditing(FieldLibraryItem item)
        {
            if (!LibraryEditingService.IsEditing(
                    item.EditingId))
            {
                return;
            }

            LibraryEditingService.CancelEditing();
        }
        #endregion

        #region Helpers
        // Enum / Enums Fields need at least one option; names must be set and unique (case-insensitive).
        private static string ValidateEnumOptions(FieldCardEditState state)
        {
            if (!FieldValueUtility.HasEnumOptions(state.Type))
                return null;

            if (state.EnumOptions.Count == 0)
                return LoreText.Validation.EnumOptionsRequired;

            var labels =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (EnumOptionDraft option in state.EnumOptions)
            {
                string label = option.Label.Trim();

                if (label.Length == 0)
                    return LoreText.Validation.EnumOptionNameRequired;

                if (!labels.Add(label))
                    return string.Format(LoreText.Validation.EnumOptionExists, label);
            }

            return null;
        }

        private static string GetOptionRemovalWarning(
            IReadOnlyList<string> removedLabels,
            int affectedEntryCount)
        {
            return LoreText.Common.Paragraphs(
                string.Format(
                    LoreText.Library.EnumOptionRemovalWarning.For(removedLabels.Count),
                    LoreText.Common.JoinList(
                        removedLabels
                            .Select(label => $"<b>{label}</b>")
                            .ToList()),
                    LoreText.Common.EntryCount.Format(affectedEntryCount)),
                LoreText.Common.CannotBeUndone);
        }

        private static string GetTypeChangeWarning(FieldTypeChangeReport report)
        {
            return LoreText.Common.Paragraphs(
                string.Format(
                    report.KeepsValues
                        ? LoreText.Library.FieldTypeChangeToSingleWarning
                        : LoreText.Library.FieldTypeChangeWarning,
                    report.CurrentLabel,
                    report.NewLabel,
                    LoreText.Common.EntryCount.Format(report.AffectedEntryCount)),
                LoreText.Common.CannotBeUndone);
        }

        private static string GetDeleteWarning(
            FieldDeleteReport report)
        {
            if (!report.IsInUse)
            {
                return LoreText.Common.Paragraphs(
                    LoreText.Library.FieldDeleteUnusedWarning,
                    LoreText.Common.CannotBeUndone);
            }

            string categories =
                LoreText.Common.CategoryCount.Format(
                    report.AffectedCategoryCount);

            string message =
                report.HasDataLoss
                    ? string.Format(
                        LoreText.Library.FieldDeleteWithValuesWarning,
                        categories,
                        LoreText.Common.EntryCount.Format(
                            report.PopulatedEntryCount))
                    : string.Format(
                        LoreText.Library.FieldDeleteWarning,
                        categories);

            return LoreText.Common.Paragraphs(
                message,
                LoreText.Common.CannotBeUndone);
        }

        private LibraryInlineTextField GetTitleEditor(Field field)
        {
            if (!_titleEditors.TryGetValue(
                    field,
                    out var editor))
            {
                editor = new LibraryInlineTextField();

                _titleEditors.Add(
                    field,
                    editor);
            }

            return editor;
        }

        private LibraryInlineEnumSelector GetTypeEditor(Field field)
        {
            if (!_typeEditors.TryGetValue(
                    field,
                    out var editor))
            {
                editor = new LibraryInlineEnumSelector();

                _typeEditors.Add(
                    field,
                    editor);
            }

            return editor;
        }
        #endregion
    }
}