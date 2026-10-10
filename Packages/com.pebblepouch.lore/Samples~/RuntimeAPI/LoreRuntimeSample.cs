using System.Text;

using UnityEngine;

using PebblePouch.Lore;

// Sample for the Lore Runtime API (see the package documentation, Documentation~/RuntimeAPI.md).
// Imported into the project (outside Lore's assemblies) on purpose: it only sees what games see.
// Usage: add to a GameObject, assign the Project asset (and optionally an Entry), press Play.
public class LoreRuntimeSample : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private LoreProject lore;

    [Tooltip("Entry to print in detail. Empty: the first Entry of the Project.")]
    [SerializeField]
    private Entry entry;

    [Tooltip("Optional: a Field name to read from the Entry by name (case-insensitive).")]
    [SerializeField]
    private string fieldName;

    [Tooltip("Optional: a Tag title to list all Entries with that Tag.")]
    [SerializeField]
    private string tagTitle;

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

        if (entry == null && lore.Entries.Count > 0)
            entry = lore.Entries[0];

        if (!string.IsNullOrEmpty(fieldName))
            _field = lore.FindField(fieldName);
    }

    private void Start()
    {
        LogProject();

        if (entry != null)
            LogEntry(entry);

        if (entry != null && _field != null)
            LogField(entry, _field);

        if (!string.IsNullOrEmpty(tagTitle))
            LogTag(tagTitle);
    }
    #endregion

    #region Sample
    // Project overview: title, description, Categories with their Entry counts.
    private void LogProject()
    {
        var text = new StringBuilder();

        text.AppendLine($"<b>{lore.Title}</b>: {lore.Categories.Count} Categories, {lore.Entries.Count} Entries");
        text.AppendLine(lore.GetPlainText(lore.Description));

        foreach (Category category in lore.Categories)
            text.AppendLine($"  {category.Glyph} {category.Title}: {lore.GetEntries(category).Count}");

        Debug.Log(text.ToString(), lore);
    }

    // One Entry: description as plain text, tags, and every field value with the matching typed getter.
    private void LogEntry(Entry target)
    {
        var text = new StringBuilder();

        text.AppendLine($"<b>{target.Glyph} {target.Title}</b> ({(target.Category != null ? target.Category.Title : "-")}) {target.Subtitle}");
        text.AppendLine(lore.GetPlainText(target.Description));

        foreach (TagDefinition tag in lore.GetTags(target))
            text.AppendLine($"  #{tag.Title}");

        foreach (FieldValue value in target.Fields)
            text.AppendLine($"  {value.Field.Title}: {Describe(value)}");

        Debug.Log(text.ToString(), target);
    }

    // Both lookups (reference and name) find the same value; TryGet… never warns.
    private void LogField(Entry target, Field field)
    {
        FieldValue byReference = target.GetValue(field);
        FieldValue byName = target.GetValue(field.Title);

        Debug.Log(
            $"Field '{field.Title}' ({field.Type}) on '{target.Title}': {Describe(byReference)} " +
            $"(same value by name: {ReferenceEquals(byReference, byName)})",
            field);

        if (target.TryGetValue(field, out FieldValue value) &&
            value.TryGetInt(out int number))
        {
            Debug.Log($"'{field.Title}' is an Integer field: {number}");
        }
    }

    private void LogTag(string title)
    {
        var text = new StringBuilder();

        text.AppendLine($"Entries with tag '{title}':");

        foreach (Entry tagged in lore.GetEntriesWithTag(title))
            text.AppendLine($"  {tagged.Glyph} {tagged.Title}");

        Debug.Log(text.ToString());
    }

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
            FieldType.Object => value.TryGetObject(out UnityEngine.Object obj) ? $"{obj.name} ({obj.GetType().Name})" : "(none)",
            FieldType.Enum => value.TryGetEnum(out string option) ? $"{option} (#{value.GetEnumIndex()})" : "(none)",
            FieldType.Enums => value.TryGetEnums(out var options) ? string.Join(", ", options) : "(none)",
            FieldType.Entries => value.TryGetEntries(out var listed) ? string.Join(", ", System.Linq.Enumerable.Select(listed, linked => linked.Title)) : "(none)",
            FieldType.Audio => value.TryGetAudioClip(out AudioClip clip) ? clip.name : "(none)",
            FieldType.Color => "#" + ColorUtility.ToHtmlStringRGBA(value.GetColor()),
            FieldType.Vector => value.Field.VectorDimension == 2 ? value.GetVector2().ToString() : value.GetVector3().ToString(),
            _ => $"({value.Type})"
        };
    }
    #endregion
}
