using System.Linq;
using System.Text;

using UnityEngine;

using PebblePouch.Lore;

// Sample for the Lore Runtime API (see the package documentation, Documentation~/RuntimeAPI.md).
// Imported into the project (outside Lore's assemblies) on purpose: it only sees what games see.
// Usage: add to a GameObject, assign the Project asset, press Play. Each feature is one region below and logs one
// console message; switch the parts on or off in the Inspector.
// The defaults (Scarecrow, Health, Witch) match the Oz Demo sample; with your own Project, pick your own.
public class LoreRuntimeSample : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private LoreProject lore;

    [Header("Examples")]
    [Tooltip("Entry used by the examples. Empty: the Entry titled below, else the first Entry of the Project.")]
    [SerializeField]
    private Entry entry;

    [Tooltip("Title of the Entry to use when no Entry is assigned (case-insensitive).")]
    [SerializeField]
    private string entryTitle = "Scarecrow";

    [Tooltip("A Field name to read and change on the Entry (case-insensitive).")]
    [SerializeField]
    private string fieldName = "Health";

    [Tooltip("A Tag title to list all Entries with that Tag (case-insensitive).")]
    [SerializeField]
    private string tagTitle = "Witch";

    [Header("Parts")]
    [SerializeField]
    private bool logProject = true;

    [SerializeField]
    private bool logEntry = true;

    [SerializeField]
    private bool logFieldKeys = true;

    [SerializeField]
    private bool logDescription = true;

    [SerializeField]
    private bool logTags = true;

    [SerializeField]
    private bool logRuntimeChanges = true;

    // Resolved once from fieldName: lookups by reference are rename-safe and skip the name comparison.
    private Field _field;
    #endregion

    #region Unity
    private void Awake()
    {
        if (lore == null)
        {
            Debug.LogError("LoreRuntimeSample: assign the Lore Project asset.", this);
            enabled = false;

            return;
        }

        if (entry == null && !string.IsNullOrEmpty(entryTitle))
            entry = lore.FindEntryByTitle(entryTitle);

        if (entry == null && lore.Entries.Count > 0)
            entry = lore.Entries[0];

        if (!string.IsNullOrEmpty(fieldName))
            _field = lore.FindField(fieldName);
    }

    private void Start()
    {
        if (logProject)
            LogProject();

        if (logEntry && entry != null)
            LogEntry(entry);

        if (logFieldKeys && entry != null && _field != null)
            LogFieldKeys(entry, _field);

        if (logDescription && entry != null)
            LogDescription(entry);

        if (logTags && !string.IsNullOrEmpty(tagTitle))
            LogTags(tagTitle);

        if (logRuntimeChanges && entry != null && _field != null)
            LogRuntimeChanges(entry, _field);
    }
    #endregion

    #region Project & Categories
    // Project overview: title, description, Categories with their Entry counts.
    private void LogProject()
    {
        var text = new StringBuilder();

        text.AppendLine($"<b>{lore.Glyph} {lore.Title}</b>: {lore.Categories.Count} Categories, {lore.Entries.Count} Entries");
        text.AppendLine(lore.GetPlainText(lore.Description));

        foreach (Category category in lore.Categories)
            text.AppendLine($"  {category.Glyph} {category.Title}: {lore.GetEntries(category).Count} Entries, {category.Fields.Count} Fields");

        Debug.Log(text.ToString(), lore);
    }
    #endregion

    #region Entries & field values
    // One Entry: found again by ID (how save games store references), then every field value with its typed getter.
    private void LogEntry(Entry target)
    {
        var text = new StringBuilder();
        string category = target.Category != null ? target.Category.Title : "-";

        text.AppendLine($"<b>{target.Glyph} {target.Title}</b> ({category}) {target.Subtitle}");
        text.AppendLine($"  ID {target.Id}, found by ID: {lore.FindEntry(target.Id) == target}");

        foreach (FieldValue value in target.Fields)
        {
            string required = target.Category != null && target.Category.IsRequired(value.Field) ? " (required)" : "";

            text.AppendLine($"  {value.Field.Glyph} {value.Field.Title}{required}: {Describe(value)}");
        }

        Debug.Log(text.ToString(), target);
    }

    // The typed getter for every field type. TryGet… returns false for empty references, so "(none)" needs no null check.
    private static string Describe(FieldValue value)
    {
        if (value == null)
            return "(missing)";

        return value.Type switch
        {
            FieldType.Text => value.GetText(),
            FieldType.Url => value.GetUrl(),
            FieldType.Boolean => value.GetBool().ToString(),
            FieldType.Integer => value.GetInt().ToString(),
            FieldType.Float => value.GetFloat().ToString("0.##"),
            FieldType.Entry => value.TryGetEntry(out Entry linked) ? linked.Title : "(none)",
            FieldType.GameObject => value.TryGetGameObject(out GameObject prefab) ? prefab.name : "(none)",
            FieldType.Sprite => value.TryGetSprite(out Sprite sprite) ? sprite.name : "(none)",
            FieldType.Scene => value.TryGetSceneName(out string scene) ? $"{scene} ({value.GetScenePath()})" : "(none)",
            FieldType.Folder => value.TryGetFolderPath(out string folder) ? folder : "(none)",
            FieldType.ScriptableObject => value.TryGetScriptableObject(out ScriptableObject asset) ? $"{asset.name} ({asset.GetType().Name})" : "(none)",
            FieldType.Object => value.TryGetObject(out Object obj) ? $"{obj.name} ({obj.GetType().Name})" : "(none)",
            FieldType.Enum => value.TryGetEnum(out string option) ? $"{option} (#{value.GetEnumIndex()})" : "(none)",
            FieldType.Enums => value.TryGetEnums(out var options) ? string.Join(", ", options) : "(none)",
            FieldType.Entries => value.TryGetEntries(out var listed) ? string.Join(", ", listed.Select(linked => linked.Title)) : "(none)",
            FieldType.Audio => value.TryGetAudioClip(out AudioClip clip) ? $"{clip.name} ({clip.length:0.##} s)" : "(none)",
            FieldType.Color => "#" + ColorUtility.ToHtmlStringRGBA(value.GetColor()),
            FieldType.Vector => value.Field.VectorDimension == 2 ? value.GetVector2().ToString() : value.GetVector3().ToString(),
            _ => $"({value.Type})"
        };
    }
    #endregion

    #region Field keys
    // The same value by reference, by name and by ID. Generated Field constants (Tools > Lore > Generate Field
    // Constants) are keys by ID: entry.GetInt(LoreFields.Health). TryGet… never warns.
    private void LogFieldKeys(Entry target, Field field)
    {
        FieldValue byReference = target.GetValue(field);
        FieldValue byName = target.GetValue(field.Title);
        FieldValue byId = target.GetValue(FieldKey.FromId(field.Id, field.Title));

        var text = new StringBuilder();

        text.AppendLine($"Field '{field.Title}' ({field.Type}) on '{target.Title}': {Describe(byReference)}");
        text.AppendLine($"  same value by name: {byName == byReference}, by ID: {byId == byReference}");
        text.AppendLine($"  has field 'Wish': {target.HasField("Wish")}");

        if (target.TryGetValue(field, out FieldValue value) &&
            value.TryGetInt(out int number))
        {
            text.AppendLine($"  '{field.Title}' is an Integer field: {number}, shortcut: {target.GetInt(field)}");
        }

        Debug.Log(text.ToString(), field);
    }
    #endregion

    #region Descriptions
    // Descriptions store links in an internal form: always read them through the Project.
    private void LogDescription(Entry target)
    {
        var text = new StringBuilder();

        text.AppendLine($"Description of '{target.Title}'");
        text.AppendLine($"  plain: {lore.GetPlainText(target.Description)}");
        text.AppendLine($"  rich: {lore.GetRichText(target.Description, part => $"<color=#7FB2FF>{part.Text}</color>")}");

        foreach (DescriptionPart part in lore.ParseDescription(target.Description))
        {
            if (part.IsLink)
                text.AppendLine($"  link: '{part.Text}' -> {(part.IsBrokenLink ? "(broken)" : part.Entry.Title)}");
        }

        Debug.Log(text.ToString(), target);
    }
    #endregion

    #region Tags
    private void LogTags(string title)
    {
        var text = new StringBuilder();

        text.AppendLine($"Entries with tag '{title}':");

        foreach (Entry tagged in lore.GetEntriesWithTag(title))
        {
            string tags = string.Join(", ", lore.GetTags(tagged).Select(tag => tag.Title));

            text.AppendLine($"  {tagged.Glyph} {tagged.Title} ({tags})");
        }

        if (entry != null)
            text.AppendLine($"'{entry.Title}' has tag '{title}': {lore.HasTag(entry, title)}");

        Debug.Log(text.ToString());
    }
    #endregion

    #region Runtime changes
    // Changes live in LoreState (memory only), the assets never change. Getters return the changed value,
    // GetOriginal… the value from Lore. All changes are cleared when Play mode starts and ends.
    private void LogRuntimeChanges(Entry target, Field field)
    {
        var text = new StringBuilder();

        text.AppendLine($"Runtime changes of '{field.Title}' on '{target.Title}'");

        if (!Change(target, field, text))
        {
            text.AppendLine($"  {field.Type} fields are read-only at runtime.");
            Debug.Log(text.ToString(), target);

            return;
        }

        text.AppendLine($"  original: {target.IsOriginal(field)}, any changes: {LoreState.HasChanges}");

        // Save games: the JSON uses IDs, so renaming in Lore never breaks it. Where it is stored is up to the game.
        string json = LoreState.SaveToJson();

        text.AppendLine($"  saved: {json}");

        target.ResetToOriginal(field);
        text.AppendLine($"  after reset: {Read(target, field)} (original: {target.IsOriginal(field)})");

        LoreState.LoadFromJson(json);
        text.AppendLine($"  after load: {Read(target, field)} (original: {target.IsOriginal(field)})");

        LoreState.ResetAllToOriginal();
        text.AppendLine($"  after reset all: {Read(target, field)}, any changes: {LoreState.HasChanges}");

        Debug.Log(text.ToString(), target);
    }

    // One Set… per writable type; returns false for read-only types.
    private static bool Change(Entry target, Field field, StringBuilder text)
    {
        switch (field.Type)
        {
            case FieldType.Text:
                target.SetText(field, target.GetText(field) + " (changed)");
                text.AppendLine($"  SetText: {target.GetText(field)} (original: {target.GetOriginalText(field)})");
                return true;
            case FieldType.Boolean:
                target.SetBool(field, !target.GetBool(field));
                text.AppendLine($"  SetBool: {target.GetBool(field)} (original: {target.GetOriginalBool(field)})");
                return true;
            case FieldType.Integer:
                target.SetInt(field, target.GetInt(field) - 1);
                text.AppendLine($"  SetInt: {target.GetInt(field)} (original: {target.GetOriginalInt(field)})");
                return true;
            case FieldType.Float:
                target.SetFloat(field, target.GetFloat(field) * 2f);
                text.AppendLine($"  SetFloat: {target.GetFloat(field):0.##} (original: {target.GetOriginalFloat(field):0.##})");
                return true;
            case FieldType.Vector:
                target.SetVector3(field, target.GetVector3(field) + Vector3.one);
                text.AppendLine($"  SetVector3: {target.GetVector3(field)} (original: {target.GetOriginalVector3(field)})");
                return true;
            case FieldType.Color:
                target.SetColor(field, Color.Lerp(target.GetColor(field), Color.gray, 0.5f));
                text.AppendLine($"  SetColor: #{ColorUtility.ToHtmlStringRGBA(target.GetColor(field))} (original: #{ColorUtility.ToHtmlStringRGBA(target.GetOriginalColor(field))})");
                return true;
            case FieldType.Enum:
                // Next option by label; the last option wraps to the first.
                var labels = field.EnumOptions.Select(option => option.Label).ToList();

                if (labels.Count == 0)
                    return false;

                target.SetEnum(field, labels[(target.GetEnumIndex(field) + 1) % labels.Count]);
                text.AppendLine($"  SetEnum: {target.GetEnum(field)} (original: {target.GetOriginalEnum(field)})");
                return true;
            case FieldType.Entry:
                // Entry fields only accept Entries of allowed Categories; null clears the value.
                target.SetEntry(field, null);
                text.AppendLine($"  SetEntry: {Read(target, field)} (original: {target.GetOriginalEntry(field)?.Title ?? "(none)"})");
                return true;
            default:
                return false;
        }
    }

    // The current value (changed or from Lore) through the typed Entry shortcuts, which see runtime changes.
    private static string Read(Entry target, Field field)
    {
        return field.Type switch
        {
            FieldType.Text => target.GetText(field),
            FieldType.Boolean => target.GetBool(field).ToString(),
            FieldType.Integer => target.GetInt(field).ToString(),
            FieldType.Float => target.GetFloat(field).ToString("0.##"),
            FieldType.Vector => target.GetVector3(field).ToString(),
            FieldType.Color => "#" + ColorUtility.ToHtmlStringRGBA(target.GetColor(field)),
            FieldType.Enum => target.GetEnum(field),
            FieldType.Entry => target.GetEntry(field)?.Title ?? "(none)",
            _ => $"({field.Type})"
        };
    }
    #endregion
}
