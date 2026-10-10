using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Tabular Editing, bulk editing: the cells of the selection row (under the header while rows are selected).
    // Each cell shows the value the selected Entries share, or "—" when they differ. A change applies to all of
    // them as one undo step: text and numbers on Enter or when the input loses focus, other controls right away.
    // The clear button clears the value in all of them. Entries without a value for the Field (integrity issue)
    // are left out.
    // Entry: click picks in the Entry Library for all. Entries: "+" first (adds through the Entry Library), then one
    // chip per Entry listed in any of them, with the count ("🐺 Wolf (2/3)", most common first): ✕ removes it from
    // all, a click adds it to the rest (full chips open the Entry). Chips wrap up to SelectionChipMaxLines (the row
    // grows); when they do not fit, they show the glyph only, and the rest becomes a "+N" chip.
    public sealed class TableSelectionRow
    {
        #region Types
        private readonly struct SharedValue
        {
            // Value of the first selected Entry with this Field (null: none has it); the shared value unless Mixed.
            public readonly FieldValue Value;
            public readonly bool Mixed;

            public SharedValue(
                FieldValue value,
                bool mixed)
            {
                Value = value;
                Mixed = mixed;
            }
        }

        // Entries cell: one chip per listed Entry (Entry null: deleted Entries) and how many targets list it.
        private sealed class ListedEntry
        {
            public Entry Entry;
            public int Count;

            // "🐺 Wolf (2/3)", compact: "🐺 (2/3)"; widths include padding and ✕.
            public string Text;
            public float Width;
            public string CompactText;
            public float CompactWidth;
        }

        private sealed class ListedEntries
        {
            public List<ListedEntry> Items = new();

            // Selected Entries with a value for the Field.
            public int Total;
        }

        private readonly struct Chip
        {
            public readonly Rect Rect;
            public readonly int Index;

            public Chip(
                Rect rect,
                int index)
            {
                Rect = rect;
                Index = index;
            }
        }

        private const int AddChipIndex = -1;
        private const int OverflowChipIndex = -2;
        #endregion

        #region Fields
        // Control names of the text inputs: prefix + Field id (Tabular Editing detects edits by it).
        public const string ControlNamePrefix = "TableSelection_";

        private readonly EntryFieldValueCard _entryCard = new();

        // Performance: shared values and listed Entries per Field, recalculated when the selected Entries or the
        // data change.
        private readonly Dictionary<Field, SharedValue> _shared = new();
        private readonly Dictionary<Field, ListedEntries> _listed = new();
        private IReadOnlyList<Entry> _cacheEntries;
        private int _cacheVersion = -1;
        #endregion

        #region Public API
        // Row height: one line, or more for wrapping Entries chips (all Entries Fields, not only the visible ones,
        // so the row does not change height while scrolling sideways). cellWidth: width of a cell's content rect.
        public float CalcHeight(
            IReadOnlyList<Entry> entries,
            IReadOnlyList<Field> fields,
            float cellWidth)
        {
            float height = TableStyle.RowHeight;

            foreach (Field field in fields)
            {
                if (field.Type != FieldType.Entries)
                    continue;

                ListedEntries listed = GetListed(entries, field);

                if (listed.Total == 0)
                    continue;

                LayoutChips(
                    listed,
                    new Rect(0f, 0f, GetValueWidth(cellWidth), 0f),
                    out float chipsHeight,
                    out _);

                // + 1: the grid line under the content rect.
                height = Mathf.Max(height, chipsHeight + 1f);
            }

            return height;
        }

        // rect: the cell's content rect (inside the grid lines).
        public void DrawCell(
            Rect rect,
            IReadOnlyList<Entry> entries,
            Field field)
        {
            SharedValue shared = GetShared(entries, field);

            if (shared.Value == null)
                return;

            Rect valueRect = new(rect.x, rect.y, GetValueWidth(rect.width), rect.height);

            Rect buttonRect =
                GetCenteredControlRect(
                    new Rect(
                        valueRect.xMax + TableStyle.ControlPadding,
                        rect.y,
                        InspectorStyle.FieldButtonColumnWidth,
                        GetFirstLine(rect).height));

            EditorGUI.showMixedValue = shared.Mixed;

            try
            {
                DrawValue(valueRect, entries, field, shared);
            }
            finally
            {
                EditorGUI.showMixedValue = false;
            }

            ButtonDrawer.Clear(
                buttonRect,
                () => Apply(entries, field, value => value.Clear(), LoreText.Undo.ClearFieldValue),
                ButtonDrawer.DisplayMode.Glyph,
                InspectorStyle.FieldButton);
        }
        #endregion

        #region Draw
        private void DrawValue(
            Rect rect,
            IReadOnlyList<Entry> entries,
            Field field,
            SharedValue shared)
        {
            FieldValue value = shared.Value;

            // Single-line controls sit in the first line (the row grows for Entries chips).
            Rect firstLine = GetFirstLine(rect);
            Rect line = GetCenteredControlRect(firstLine);

            switch (field.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                {
                    string controlName = BeginTextInput(field);

                    EditorGUI.BeginChangeCheck();

                    string text =
                        EditorGUI.DelayedTextField(
                            line,
                            value.StringValue ?? string.Empty,
                            InspectorStyle.FieldValueText);

                    if (EditorGUI.EndChangeCheck())
                        Apply(entries, field, target => target.SetText(text), LoreText.Undo.EditFieldValue);

                    EndTextInput(controlName, line);
                    break;
                }

                case FieldType.Integer:
                {
                    string controlName = BeginTextInput(field);

                    EditorGUI.BeginChangeCheck();

                    int number =
                        EditorGUI.DelayedIntField(
                            line,
                            value.IntValue,
                            InspectorStyle.FieldValueNumber);

                    if (EditorGUI.EndChangeCheck())
                        Apply(entries, field, target => target.SetInteger(number), LoreText.Undo.EditFieldValue);

                    EndTextInput(controlName, line);
                    break;
                }

                case FieldType.Float:
                {
                    string controlName = BeginTextInput(field);

                    EditorGUI.BeginChangeCheck();

                    float number =
                        EditorGUI.DelayedFloatField(
                            line,
                            value.FloatValue,
                            InspectorStyle.FieldValueNumber);

                    if (EditorGUI.EndChangeCheck())
                        Apply(entries, field, target => target.SetFloat(number), LoreText.Undo.EditFieldValue);

                    EndTextInput(controlName, line);
                    break;
                }

                case FieldType.Boolean:
                {
                    // Default toggle style: it has the mixed ("—") look.
                    EditorGUI.BeginChangeCheck();

                    bool flag = EditorGUI.Toggle(line, value.BoolValue);

                    if (EditorGUI.EndChangeCheck())
                        Apply(entries, field, target => target.SetBoolean(flag), LoreText.Undo.EditFieldValue);

                    break;
                }

                case FieldType.Enum:
                    DrawEnum(line, entries, field, value);
                    break;

                case FieldType.GameObject:
                {
                    EditorGUI.BeginChangeCheck();

                    GameObject gameObject =
                        (GameObject)EditorGUI.ObjectField(
                            line,
                            GUIContent.none,
                            value.GameObjectValue,
                            typeof(GameObject),
                            true);

                    if (EditorGUI.EndChangeCheck())
                        Apply(entries, field, target => target.SetGameObject(gameObject), LoreText.Undo.EditFieldValue);

                    break;
                }

                case FieldType.Sprite:
                {
                    EditorGUI.BeginChangeCheck();

                    Sprite sprite =
                        (Sprite)EditorGUI.ObjectField(
                            line,
                            GUIContent.none,
                            value.SpriteValue,
                            typeof(Sprite),
                            true);

                    if (EditorGUI.EndChangeCheck())
                        Apply(entries, field, target => target.SetSprite(sprite), LoreText.Undo.EditFieldValue);

                    break;
                }

                case FieldType.Audio:
                {
                    EditorGUI.BeginChangeCheck();

                    AudioClip clip =
                        (AudioClip)EditorGUI.ObjectField(
                            line,
                            GUIContent.none,
                            value.AudioClipValue,
                            typeof(AudioClip),
                            false);

                    if (EditorGUI.EndChangeCheck())
                        Apply(entries, field, target => target.SetAudioClip(clip), LoreText.Undo.EditFieldValue);

                    break;
                }

                case FieldType.ScriptableObject:
                {
                    // Missing type: nothing to pick (the cells of the rows show the fix hint).
                    if (field.ObjectType == null)
                        break;

                    EditorGUI.BeginChangeCheck();

                    UnityEngine.Object asset =
                        EditorGUI.ObjectField(
                            line,
                            GUIContent.none,
                            value.ObjectValue,
                            field.ObjectType,
                            false);

                    if (EditorGUI.EndChangeCheck())
                        Apply(entries, field, target => target.SetObject(asset), LoreText.Undo.EditFieldValue);

                    break;
                }

                case FieldType.Scene:
                {
                    EditorGUI.BeginChangeCheck();

                    SceneAsset scene =
                        (SceneAsset)EditorGUI.ObjectField(
                            line,
                            GUIContent.none,
                            value.SceneValue as SceneAsset,
                            typeof(SceneAsset),
                            false);

                    if (EditorGUI.EndChangeCheck())
                    {
                        string path = scene != null ? AssetDatabase.GetAssetPath(scene) : string.Empty;

                        Apply(entries, field, target => target.SetScene(scene, path), LoreText.Undo.EditFieldValue);
                    }

                    break;
                }

                case FieldType.Entry:
                    DrawEntry(firstLine, entries, field, shared);
                    break;

                case FieldType.Entries:
                    DrawEntries(rect, entries, field);
                    break;
            }
        }

        // Same popup as the Enum cells ("not set" first).
        private static void DrawEnum(
            Rect rect,
            IReadOnlyList<Entry> entries,
            Field field,
            FieldValue value)
        {
            var labels = new GUIContent[field.EnumOptions.Count + 1];

            labels[0] = new GUIContent(LoreText.Inspector.EnumNotSet);

            for (int i = 0; i < field.EnumOptions.Count; i++)
                labels[i + 1] = new GUIContent(FieldValueEnumElement.GetMenuLabel(field.EnumOptions[i]));

            EditorGUI.BeginChangeCheck();

            int index =
                EditorGUI.Popup(
                    rect,
                    field.GetEnumOptionIndex(value.EnumValue) + 1,
                    labels,
                    InspectorStyle.FieldValueEnum);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.Label(
                    new Rect(
                        rect.xMax - InspectorStyle.FieldValueEnumArrowWidth,
                        rect.y,
                        InspectorStyle.FieldValueEnumArrowWidth,
                        rect.height),
                    InspectorStyle.FieldValueEnumArrow,
                    InspectorStyle.FieldValueEnumArrowContent);
            }

            if (EditorGUI.EndChangeCheck())
            {
                string optionId =
                    index > 0
                        ? field.EnumOptions[index - 1]?.Id
                        : string.Empty;

                Apply(entries, field, target => target.SetEnum(optionId), LoreText.Undo.EditFieldValue);
            }
        }

        // Shared Entry card (or "—"); click: pick in the Entry Library for all selected Entries.
        private void DrawEntry(
            Rect rect,
            IReadOnlyList<Entry> entries,
            Field field,
            SharedValue shared)
        {
            if (EntryAssignmentContext.IsBulkTarget(field) &&
                Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, InspectorStyle.FieldSelectBackground);
            }

            if (shared.Mixed)
                GUI.Label(rect, LoreText.Inspector.TableMixedValue, TableStyle.CellTextMuted);
            else
                _entryCard.DrawCompact(rect, shared.Value.EntryValue);

            GUI.Label(rect, new GUIContent(string.Empty, LoreText.Inspector.TableSelectionPickTooltip), GUIStyle.none);

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown ||
                Event.current.button != 0 ||
                !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            BeginPicking(entries, field);

            Event.current.Use();
        }

        private void DrawEntries(
            Rect rect,
            IReadOnlyList<Entry> entries,
            Field field)
        {
            ListedEntries listed = GetListed(entries, field);

            if (EntryAssignmentContext.IsBulkTarget(field) &&
                Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, InspectorStyle.FieldSelectBackground);
            }

            List<Chip> chips = LayoutChips(listed, rect, out _, out bool compact);

            int shown = chips.Count(chip => chip.Index >= 0);

            // After drawing: the lists must not change while their chips are drawn.
            Action action = null;

            foreach (Chip chip in chips)
            {
                switch (chip.Index)
                {
                    case AddChipIndex:
                        DrawAddChip(chip.Rect, rect, entries, field);
                        break;

                    case OverflowChipIndex:
                        DrawOverflowChip(chip.Rect, listed, shown);
                        break;

                    default:
                        action ??= DrawEntryChip(chip.Rect, listed.Items[chip.Index], listed.Total, compact, entries, field);
                        break;
                }
            }

            action?.Invoke();
        }

        // Returns the action of a click (✕: remove from all; text: add to the rest, or open the Entry), else null.
        // Compact chips show the full text in the tooltip.
        private static Action DrawEntryChip(
            Rect rect,
            ListedEntry item,
            int total,
            bool compact,
            IReadOnlyList<Entry> entries,
            Field field)
        {
            Entry entry = item.Entry;
            bool missing = entry == null;
            bool everywhere = item.Count >= total;

            Rect removeRect =
                new(
                    rect.xMax - InspectorStyle.EntriesChipRemoveWidth,
                    rect.y,
                    InspectorStyle.EntriesChipRemoveWidth,
                    rect.height);

            Rect textRect =
                new(
                    rect.x + InspectorStyle.EntriesChipHorizontalPadding,
                    rect.y,
                    removeRect.x - rect.x - InspectorStyle.EntriesChipHorizontalPadding,
                    rect.height);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(
                    rect,
                    missing
                        ? InspectorStyle.EntriesChipMissingColor
                        : InspectorStyle.EntriesChipColor);
            }

            string hint =
                missing
                    ? null
                    : everywhere
                        ? LoreText.Inspector.TableOpenEntryTooltip
                        : LoreText.Inspector.TableSelectionAddToRestTooltip;

            GUI.Label(
                textRect,
                new GUIContent(
                    compact ? item.CompactText : item.Text,
                    compact
                        ? hint != null ? item.Text + "\n" + hint : item.Text
                        : hint),
                missing
                    ? InspectorStyle.EntriesChipMissingText
                    : InspectorStyle.EntriesChipText);

            if (GUI.Button(removeRect, InspectorStyle.EntriesRemoveGlyph, InspectorStyle.EntriesChipButton))
                return () => RemoveFromAll(entries, field, entry);

            if (missing)
                return null;

            EditorGUIUtility.AddCursorRect(textRect, MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown ||
                Event.current.button != 0 ||
                !textRect.Contains(Event.current.mousePosition))
            {
                return null;
            }

            Event.current.Use();

            return everywhere
                ? () => LoreNavigation.NavigateToEntry(entry)
                : () => AddToAll(entries, field, entry);
        }

        // "+N": the chips that do not fit (listed in the tooltip).
        private static void DrawOverflowChip(
            Rect rect,
            ListedEntries listed,
            int shown)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, InspectorStyle.EntriesChipColor);

            int hidden = listed.Items.Count - shown;

            GUI.Label(
                new Rect(
                    rect.x + InspectorStyle.EntriesChipHorizontalPadding,
                    rect.y,
                    Mathf.Max(0f, rect.width - InspectorStyle.EntriesChipHorizontalPadding * 2f),
                    rect.height),
                new GUIContent(
                    string.Format(LoreText.Inspector.TableSelectionOverflow, hidden),
                    string.Join("\n", listed.Items.Skip(shown).Select(item => item.Text))),
                InspectorStyle.EntriesChipText);
        }

        // Same button as the clear button (style, size) and on its line: centered in the cell's first line.
        private static void DrawAddChip(
            Rect rect,
            Rect cellRect,
            IReadOnlyList<Entry> entries,
            Field field)
        {
            Rect line = GetCenteredControlRect(GetFirstLine(cellRect));

            rect.y = line.y;
            rect.height = line.height;

            ButtonDrawer.Add(
                rect,
                () => BeginPicking(entries, field),
                ButtonDrawer.DisplayMode.Glyph,
                InspectorStyle.FieldButton);
        }

        private static string BeginTextInput(Field field)
        {
            string controlName = ControlNamePrefix + field.Id;

            GUI.SetNextControlName(controlName);

            return controlName;
        }

        // While focused, the input reports its rect (see FieldValueElement.Draw).
        private static void EndTextInput(
            string controlName,
            Rect rect)
        {
            LoreEditService.ReportEditor(
                controlName,
                rect,
                () => GUI.GetNameOfFocusedControl() == controlName);
        }
        #endregion

        #region Chips layout
        // Chip positions in rect (wrapping), the add chip first (it keeps its place). At most SelectionChipMaxLines
        // lines, as in the Entries cells of the rows: glyph and title when they fit, else compact (glyph only), and
        // chips that still do not fit become the "+N" chip. height = total height used.
        private static List<Chip> LayoutChips(
            ListedEntries listed,
            Rect rect,
            out float height,
            out bool compact)
        {
            int shown = listed.Items.Count;

            compact = false;

            List<Chip> chips = TryLayoutChips(listed, rect, shown, compact, out height, out int lines);

            if (lines <= TableStyle.SelectionChipMaxLines)
                return chips;

            compact = true;

            chips = TryLayoutChips(listed, rect, shown, compact, out height, out lines);

            while (lines > TableStyle.SelectionChipMaxLines &&
                   shown > 0)
            {
                shown--;

                chips = TryLayoutChips(listed, rect, shown, compact, out height, out lines);
            }

            return chips;
        }

        private static List<Chip> TryLayoutChips(
            ListedEntries listed,
            Rect rect,
            int shown,
            bool compact,
            out float height,
            out int lines)
        {
            var chips = new List<Chip>();

            float x = rect.x;
            float y = rect.y + InspectorStyle.EntriesVerticalPadding;
            int lineCount = 1;

            void Place(float width, int index)
            {
                width = Mathf.Min(width, rect.width);

                if (x > rect.x &&
                    x + width > rect.xMax)
                {
                    x = rect.x;
                    y += InspectorStyle.EntriesChipHeight + InspectorStyle.EntriesLineSpacing;
                    lineCount++;
                }

                chips.Add(
                    new Chip(
                        new Rect(x, y, width, InspectorStyle.EntriesChipHeight),
                        index));

                x += width + InspectorStyle.EntriesChipSpacing;
            }

            Place(InspectorStyle.EntriesAddWidth, AddChipIndex);

            for (int i = 0; i < shown; i++)
                Place(compact ? listed.Items[i].CompactWidth : listed.Items[i].Width, i);

            if (shown < listed.Items.Count)
            {
                string overflow =
                    string.Format(
                        LoreText.Inspector.TableSelectionOverflow,
                        listed.Items.Count - shown);

                Place(
                    InspectorStyle.EntriesChipHorizontalPadding * 2f
                    + InspectorStyle.EntriesChipText.CalcSize(new GUIContent(overflow)).x,
                    OverflowChipIndex);
            }

            height = y + InspectorStyle.EntriesChipHeight + InspectorStyle.EntriesVerticalPadding - rect.y;
            lines = lineCount;

            // A single line is centered in the normal row height.
            float rowHeight = TableStyle.RowHeight;

            if (height < rowHeight)
            {
                float offset = (rowHeight - height) * 0.5f;

                for (int i = 0; i < chips.Count; i++)
                {
                    Rect chipRect = chips[i].Rect;
                    chipRect.y += offset;

                    chips[i] = new Chip(chipRect, chips[i].Index);
                }
            }

            return chips;
        }
        #endregion

        #region Apply
        private static void BeginPicking(
            IReadOnlyList<Entry> entries,
            Field field)
        {
            EntryAssignmentContext.BeginBulkAssignment(entries, field);

            LibraryService
                .OpenEntryLibrary?
                .Invoke();
        }

        private static void AddToAll(
            IReadOnlyList<Entry> entries,
            Field field,
            Entry entry)
        {
            Apply(
                entries,
                field,
                value => value.AddListEntry(entry),
                LoreText.Undo.EditFieldValue,
                value => !value.EntriesValue.Contains(entry));
        }

        // entry null: removes the deleted Entries.
        private static void RemoveFromAll(
            IReadOnlyList<Entry> entries,
            Field field,
            Entry entry)
        {
            if (entry == null)
            {
                Apply(
                    entries,
                    field,
                    value => value.RemoveMissingListEntries(),
                    LoreText.Undo.EditFieldValue,
                    value => value.EntriesValue.Any(listed => listed == null));

                return;
            }

            Apply(
                entries,
                field,
                value => value.RemoveListEntry(entry),
                LoreText.Undo.EditFieldValue,
                value => value.EntriesValue.Contains(entry));
        }

        // affects: limits the change to Entries whose value needs it (others are not recorded or marked modified).
        private static void Apply(
            IReadOnlyList<Entry> entries,
            Field field,
            Action<FieldValue> change,
            string undoName,
            Func<FieldValue, bool> affects = null)
        {
            List<Entry> targets =
                entries
                    .Where(entry =>
                        entry != null &&
                        entry.TryGetValue(field, out FieldValue value) &&
                        (affects == null || affects(value)))
                    .ToList();

            if (targets.Count == 0)
                return;

            LoreUndo.Record(targets, undoName);

            foreach (Entry entry in targets)
            {
                entry.TryGetValue(field, out FieldValue value);

                change(value);

                LoreChangeTracker.MarkModified(entry);
            }

            if (FieldValueUtility.IsEntryReference(field.Type))
                LoreRelationshipIndex.Instance.Refresh();

            LoreRepaintService.Repaint();
        }
        #endregion

        #region Shared values
        private void ResetCacheIfChanged(IReadOnlyList<Entry> entries)
        {
            if (entries == _cacheEntries &&
                _cacheVersion == LoreDataVersion.Current)
            {
                return;
            }

            _shared.Clear();
            _listed.Clear();

            _cacheEntries = entries;
            _cacheVersion = LoreDataVersion.Current;
        }

        private SharedValue GetShared(
            IReadOnlyList<Entry> entries,
            Field field)
        {
            ResetCacheIfChanged(entries);

            if (!_shared.TryGetValue(field, out SharedValue shared))
            {
                shared = CalculateShared(entries, field);

                _shared[field] = shared;
            }

            return shared;
        }

        private ListedEntries GetListed(
            IReadOnlyList<Entry> entries,
            Field field)
        {
            ResetCacheIfChanged(entries);

            if (!_listed.TryGetValue(field, out ListedEntries listed))
            {
                listed = CalculateListed(entries, field);

                _listed[field] = listed;
            }

            return listed;
        }

        private static SharedValue CalculateShared(
            IReadOnlyList<Entry> entries,
            Field field)
        {
            FieldValue first = null;

            foreach (Entry entry in entries)
            {
                if (entry == null ||
                    !entry.TryGetValue(field, out FieldValue value))
                {
                    continue;
                }

                if (first == null)
                    first = value;
                else if (!HasSameValue(first, value, field))
                    return new SharedValue(first, true);
            }

            return new SharedValue(first, false);
        }

        // All Entries listed by the targets (deleted ones as one chip), most common first, then in listing order.
        private static ListedEntries CalculateListed(
            IReadOnlyList<Entry> entries,
            Field field)
        {
            var listed = new ListedEntries();
            var items = new Dictionary<Entry, ListedEntry>();
            var seen = new HashSet<Entry>();

            ListedEntry missing = null;

            foreach (Entry entry in entries)
            {
                if (entry == null ||
                    !entry.TryGetValue(field, out FieldValue value))
                {
                    continue;
                }

                listed.Total++;

                seen.Clear();

                bool hasMissing = false;

                foreach (Entry listedEntry in value.EntriesValue)
                {
                    if (listedEntry == null)
                    {
                        hasMissing = true;
                        continue;
                    }

                    if (!seen.Add(listedEntry))
                        continue;

                    if (!items.TryGetValue(listedEntry, out ListedEntry item))
                    {
                        item = new ListedEntry { Entry = listedEntry };

                        items.Add(listedEntry, item);
                        listed.Items.Add(item);
                    }

                    item.Count++;
                }

                if (hasMissing)
                {
                    if (missing == null)
                    {
                        missing = new ListedEntry();

                        listed.Items.Add(missing);
                    }

                    missing.Count++;
                }
            }

            // Stable: equal counts keep the listing order.
            listed.Items = listed.Items.OrderByDescending(item => item.Count).ToList();

            foreach (ListedEntry item in listed.Items)
            {
                string missingText = string.Format(LoreText.Drawers.Missing, LoreText.Drawers.EntryKind);

                string chip =
                    item.Entry != null
                        ? string.Format(LoreText.Drawers.GlyphTitle, item.Entry.Glyph, item.Entry.Title)
                        : missingText;

                string compactChip =
                    item.Entry != null
                        ? item.Entry.Glyph
                        : missingText;

                item.Text = string.Format(LoreText.Inspector.TableSelectionChip, chip, item.Count, listed.Total);
                item.CompactText = string.Format(LoreText.Inspector.TableSelectionChip, compactChip, item.Count, listed.Total);

                GUIStyle style =
                    item.Entry != null
                        ? InspectorStyle.EntriesChipText
                        : InspectorStyle.EntriesChipMissingText;

                item.Width = GetChipWidth(item.Text, style);
                item.CompactWidth = GetChipWidth(item.CompactText, style);
            }

            return listed;
        }

        private static bool HasSameValue(
            FieldValue a,
            FieldValue b,
            Field field)
        {
            switch (field.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                    return (a.StringValue ?? string.Empty) == (b.StringValue ?? string.Empty);

                case FieldType.Integer:
                    return a.IntValue == b.IntValue;

                case FieldType.Float:
                    return a.FloatValue == b.FloatValue;

                case FieldType.Boolean:
                    return a.BoolValue == b.BoolValue;

                // A removed option counts as not set (as in the popup).
                case FieldType.Enum:
                    return field.GetEnumOptionIndex(a.EnumValue) == field.GetEnumOptionIndex(b.EnumValue);

                case FieldType.Entry:
                    return a.EntryValue == b.EntryValue;

                case FieldType.Entries:
                    return a.EntriesValue.SequenceEqual(b.EntriesValue);

                case FieldType.GameObject:
                    return a.GameObjectValue == b.GameObjectValue;

                case FieldType.Sprite:
                    return a.SpriteValue == b.SpriteValue;

                case FieldType.ScriptableObject:
                case FieldType.Audio:
                    return a.ObjectValue == b.ObjectValue;

                case FieldType.Scene:
                    return (a.ScenePath ?? string.Empty) == (b.ScenePath ?? string.Empty);

                default:
                    return true;
            }
        }
        #endregion

        #region Helpers
        private static float GetChipWidth(
            string text,
            GUIStyle style) =>
            InspectorStyle.EntriesChipHorizontalPadding
            + style.CalcSize(new GUIContent(text)).x
            + InspectorStyle.EntriesChipRemoveWidth;

        private static float GetValueWidth(float cellWidth) =>
            Mathf.Max(0f, cellWidth - InspectorStyle.FieldButtonColumnWidth - TableStyle.ControlPadding);

        private static Rect GetFirstLine(Rect rect) =>
            new(rect.x, rect.y, rect.width, Mathf.Min(rect.height, TableStyle.RowHeight - 1f));

        private static Rect GetCenteredControlRect(Rect rect)
        {
            float height = Mathf.Min(EditorGUIUtility.singleLineHeight, rect.height);

            return new Rect(rect.x, rect.center.y - height * 0.5f, rect.width, height);
        }
        #endregion
    }
}
