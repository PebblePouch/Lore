# Lore Runtime API

Lore data lives in ScriptableObjects (Project, Categories, Entries, Fields). The Runtime API lets game code
**read** that data: find Entries, read field values with the right type, turn descriptions into text and
work with tags. Lore content is edited in the Lore window; games can change some field values at runtime
(see [4. Changing values at runtime](#4-changing-values-at-runtime)) without touching the assets.

Namespace: `PebblePouch.Lore` · Assembly: `PebblePouch.Lore` (referenced automatically by `Assembly-CSharp`;
assemblies with their own `.asmdef` add `PebblePouch.Lore` to their references).

A complete example is the **Runtime API** sample: Package Manager > Lore > Samples > Import. The **Oz Demo** sample
uses the API in a small playable game (scene `Scenes/Oz Demo.unity`, rules in `Scripts/OzJourney.cs`): finding
Entries, typed values, generated Field constants, descriptions, runtime changes and save games.

---

## 1. Getting started

Reference the Project asset (`Assets/Lore/Lore.asset` by default) from your script:

```csharp
using PebblePouch.Lore;
using UnityEngine;

public class Bestiary : MonoBehaviour
{
    [SerializeField] private LoreProject lore;

    private void Start()
    {
        Entry wolf = lore.FindEntryByTitle("Wolf");

        Debug.Log($"{wolf.Title}: {wolf.GetInt("Health")} HP");
        Debug.Log(lore.GetPlainText(wolf.Description));
    }
}
```

The Project is the entry point for everything else. Whatever references the Project includes **all Lore data**
in a build, including assets referenced by field values (Sprites, GameObjects, …). It also **loads** all of it
when the referencing scene loads; see [10. Memory & loading](#10-memory--loading) before using Lore with many heavy
assets.

Single Entries can also be referenced directly (`[SerializeField] private Entry wolf;`); lookups such as tags
and descriptions still need the Project.

### Lore fields in the Inspector

`Entry`, `Category` and `Field` fields in your scripts (also lists and arrays) show their Lore value:
`🐺 Wolf  (Characters)`, `📁 Characters`, `❤ Health  (Integer)`. The picker button opens a searchable list (Entries
grouped by Category); drag & drop works as usual. Click pings the asset, double-click opens it in Lore.

Limit what the picker and drag & drop accept with `[LoreFilter]`:

```csharp
[LoreFilter(Category = "Characters")]                                public Entry hero;
[LoreFilter(Category = "Items|Weapons")]                             public List<Entry> loot;   // "|" = or
[LoreFilter(FieldType = FieldType.Integer)]                          public Field anyNumber;
[LoreFilter(Category = "Characters", FieldType = FieldType.Integer)] public Field healthField; // and
```

| Criterion | Entry fields | Category fields | Field fields |
|---|---|---|---|
| `Category` | Entries of these Categories | these Categories | Fields used by these Categories |
| `FieldType` | – | – | Fields of this type |

Category titles are case-insensitive. An unknown title or a criterion that does not apply shows a hint below the
field. `[LoreFilter]` only affects the Editor.

---

## 2. Finding data

| Member | Returns |
|---|---|
| `lore.Categories` | All Categories, in Lore's order |
| `lore.Entries` | All Entries, grouped by Category, in Lore's order |
| `lore.GetEntries(category)` | Entries of one Category (new list per call) |
| `lore.FindEntry(id)` / `lore.FindCategory(id)` | By ID (`entry.Id`), e.g. from save games |
| `lore.FindEntryByTitle(title, category = null)` | First Entry with that title (case-insensitive) |
| `lore.FindCategoryByTitle(title)` | Category with that title (case-insensitive) |
| `lore.FindField(name)` | Field used by any Category (case-insensitive) |

Entry titles are only unique within a Category; pass the Category to `FindEntryByTitle` when titles repeat.
IDs never change, also not when an Entry is renamed or moved; use them to store references (save games).

Basic properties:

| Type | Properties |
|---|---|
| `LoreProject` | `Title`, `Glyph` (📜 until one is chosen), `Description`, `Links` (`Label`, `Url`), `TagLibrary` |
| `Category` | `Id`, `Title`, `Glyph`, `Color`, `Description`, `Fields`, `RequiredFields` / `IsRequired(field)` |
| `Entry` | `Id`, `Title`, `Glyph` (falls back to the Category glyph), `Subtitle`, `Description`, `Category`, `Fields`, `TagIds` |
| `Field` | `Id`, `Title`, `Type`, `Glyph`, `ObjectType` (ScriptableObject and Object fields), `EnumOptions` (Enum fields: `Id`, `Label`), `AllowedCategories` (Entry / Entries fields, empty = all) |
| all | `Created`, `Modified` (UTC) |

---

## 3. Reading field values

### Field keys: reference, ID or name

Every lookup takes a `FieldKey`, which is a **Field reference**, a **Field ID** or a **Field name**:

```csharp
[SerializeField] private Field healthField;     // drag the Field asset from Assets/Lore/Fields

entry.GetInt(healthField);   // by reference: rename-safe, no typos possible
entry.GetInt("Health");      // by name: quick, case-insensitive
```

Names break silently when a Field is renamed in Lore. A good middle ground: resolve names once and keep the
reference, so a missing name shows up right at startup:

```csharp
private Field _health;

private void Awake() => _health = lore.FindField("Health");   // null + easy to spot if renamed
```

Keys by ID match the Field's `Id` and survive renames. They are mainly meant for generated Field constants;
the name is only used in warnings:

```csharp
FieldKey health = FieldKey.FromId("8f3c…", "Health");
entry.GetInt(health);
```

### Generated Field constants

**Tools > Lore > Generate Field Constants** (or **Generate** in Project Settings > Lore) writes a class with one
key per Field asset, so code gets autocompletion and no typos:

```csharp
int hp = entry.GetInt(LoreFields.Health);
```

- Keys use Field IDs: renamed Fields keep working until you generate again. Then the constant gets the new name,
  and the compiler shows every place that still uses the old one.
- Constant names are the Field titles in PascalCase (`Max HP` → `MaxHP`); titles that end up with the same name
  get a number (`MaxHP2`).
- Settings (Project Settings > Lore): output file (default: `Generated/LoreFields.cs` next to the Lore Project),
  namespace and class name. If your game code lives in an assembly definition, pick a folder of that assembly (or
  of one it references).
- Generation never runs on its own; the file only changes when the Fields do. Once generated, the Project page shows
  a notice with **Generate now** while the constants are out of date.
- Commit `ProjectSettings/LoreSettings.asset` together with the generated file. It holds these settings and the
  path of the last generation, so everyone on the team writes to the same file, and the notice knows the constants
  exist.

### Two layers

```csharp
// 1) Find the value of a Field on an Entry
FieldValue value = entry.GetValue("Health");              // null (+ warning) if the Entry has no such Field
bool found = entry.TryGetValue("Health", out FieldValue v);
bool has = entry.HasField("Health");

// 2) Read it with the getter for its type
int hp = value.GetInt();
bool ok = value.TryGetInt(out int hp2);

// Shortcut for both layers
int hp3 = entry.GetInt("Health");
```

### Getters per field type

| Field type | FieldValue | Entry shortcut | Value when empty |
|---|---|---|---|
| Text | `GetText()` / `TryGetText(out)` | `GetText(key)` | `""` |
| URL | `GetUrl()` / `TryGetUrl(out)` | `GetUrl(key)` | `""` |
| Boolean | `GetBool()` / `TryGetBool(out)` | `GetBool(key)` | `false` |
| Integer | `GetInt()` / `TryGetInt(out)` | `GetInt(key)` | `0` |
| Float | `GetFloat()` / `TryGetFloat(out)` | `GetFloat(key)` | `0` |
| Entry | `GetEntry()` / `TryGetEntry(out)` | `GetEntry(key)` | `null` |
| GameObject | `GetGameObject()` / `TryGetGameObject(out)` | `GetGameObject(key)` | `null` |
| Sprite | `GetSprite()` / `TryGetSprite(out)` | `GetSprite(key)` | `null` |
| Scene | `GetScenePath()`, `GetSceneName()` / `TryGet…(out)` | `GetScenePath(key)`, `GetSceneName(key)` | `""` |
| Folder | `GetFolderPath()` / `TryGetFolderPath(out)` | `GetFolderPath(key)` | `""` |
| ScriptableObject | `GetScriptableObject<T>()` / `TryGetScriptableObject<T>(out)` | `GetScriptableObject<T>(key)` | `null` |
| Object | `GetObject<T>()` / `TryGetObject<T>(out)` | `GetObject<T>(key)` | `null` |
| Enum | `GetEnum()`, `GetEnumIndex()` / `TryGetEnum(out)`, `TryGetEnumIndex(out)` | `GetEnum(key)`, `GetEnumIndex(key)` | `""` / `-1` |
| Enums | `GetEnums()`, `HasEnum(label)` / `TryGetEnums(out)` | `GetEnums(key)`, `HasEnum(key, label)` | empty list / `false` |
| Entries | `GetEntries()` / `TryGetEntries(out)` | `GetEntries(key)` | empty list |
| Audio | `GetAudioClip()` / `TryGetAudioClip(out)` | `GetAudioClip(key)` | `null` |
| Color | `GetColor()` / `TryGetColor(out)` | `GetColor(key)` | black |
| Vector | `GetVector2()`, `GetVector3()` / `TryGetVector2(out)`, `TryGetVector3(out)` | `GetVector2(key)`, `GetVector3(key)` | zero |

`value.Field` and `value.Type` tell which Field and type a value has (useful when iterating `entry.Fields`).
For ScriptableObject and Object fields, `field.ObjectType` is the type chosen in Lore (`null` if that type no longer
exists).
Vector fields work with both getters: `field.VectorDimension` is 2 or 3; `GetVector2` drops z, `GetVector3` of a 2D
field returns z = 0.

### Rules

- **Types are strict.** `GetInt` only reads Integer fields, `GetText` only Text fields (not URLs).
- **`Get…` never throws.** On a missing Field or wrong type it returns the default value and logs a warning.
- **`TryGet…` never warns.** It returns `false` on a wrong type; for references (Entry, GameObject, Sprite, Audio,
  ScriptableObject, Object) and scenes and folders also when the value is empty, so the out value is safe to use:

  ```csharp
  if (entry.TryGetValue("Portrait", out FieldValue value) &&
      value.TryGetSprite(out Sprite portrait))
  {
      image.sprite = portrait;
  }
  ```

- **Scenes** are returned as path (`Assets/Scenes/Forest.unity`) or name (`Forest`); both work with
  `SceneManager.LoadScene` as long as the scene is in the build's scene list.
- **Folders** are returned as path (`Assets/Art/Wolves`). Folder assets do not exist in builds: use the path, e.g.
  for your own asset lookups or Addressables labels/keys.
- **ScriptableObjects** are read with your own type:

  ```csharp
  DialogDefinition dialog = entry.GetScriptableObject<DialogDefinition>("Dialog");
  ```

  `T` can be the type chosen in Lore, a base class or a subclass. A value that is not a `T` returns `null`
  (`Get…` with a warning, `TryGet…` returns `false`). Without `T`, `GetScriptableObject()` returns the
  value as `ScriptableObject`.
- **Object fields** hold an asset of any other Unity type chosen in Lore (Texture2D, Material, AnimationClip, ...) and
  are read the same way, with the same rules for `T`:

  ```csharp
  Material skin = entry.GetObject<Material>("Skin");
  Rigidbody body = entry.GetObject<Rigidbody>("Physics Template");
  ```

  For a component type (e.g. `Rigidbody`) the value is that component on a prefab. Without `T`, `GetObject()`
  returns the value as `UnityEngine.Object`.
- **Enum fields** return the label of the selected option (`"Rare"`); `GetEnumIndex` returns its position in the
  Field's option list (`0` = first option), e.g. for ranks or sorting. An Enum value that is empty or points to a removed
  option counts as not set.
- **Entries fields** return the referenced Entries in their order, without deleted or missing ones (a new list per
  call): `foreach (Entry level in gretel.GetEntries("Appears in")) { … }`. `TryGetEntries` returns `false` for an
  empty list.

---

## 4. Changing values at runtime

Games can change field values while they run, e.g. health that drops, an item that changes its owner or a quest
that becomes active. Changes are kept in memory by `LoreState`; the Lore assets never change.

```csharp
Entry scarecrow = lore.FindEntryByTitle("Scarecrow");

scarecrow.SetInt("Health", 3);
scarecrow.GetInt("Health");           // 3: getters return the changed value, otherwise the value from Lore
scarecrow.GetOriginalInt("Health");   // the value from Lore, also while changed
scarecrow.IsOriginal("Health");       // false: the value was changed
scarecrow.ResetToOriginal("Health");  // back to the value from Lore
```

Just use `Set…` and `Get…`: a `Get…` always returns a value, changed or not. `GetOriginal…` is only needed when you
explicitly want the value from Lore.

### Writable field types

| Field type | Set | Original |
|---|---|---|
| Text | `SetText(key, text)` | `GetOriginalText(key)` |
| Boolean | `SetBool(key, value)` | `GetOriginalBool(key)` |
| Integer | `SetInt(key, value)` | `GetOriginalInt(key)` |
| Float | `SetFloat(key, value)` | `GetOriginalFloat(key)` |
| Vector | `SetVector2(key, value)`, `SetVector3(key, value)` | `GetOriginalVector2(key)`, `GetOriginalVector3(key)` |
| Color | `SetColor(key, color)` | `GetOriginalColor(key)` |
| Enum | `SetEnum(key, label)` | `GetOriginalEnum(key)`, `GetOriginalEnumIndex(key)` |
| Entry | `SetEntry(key, entry)` | `GetOriginalEntry(key)` |

Every `Set…` has a `TrySet…` that returns `false` instead of warning. Other field types (assets, scenes, folders,
URLs, Enums, Entries) are read-only.

```csharp
silverShoes.SetEntry("Owner", dorothy);           // Dorothy takes the Silver Shoes
winkieCountry.SetEntry("Ruler", tinWoodman);
emeraldCity.SetColor("Color", Color.gray);        // the green glasses come off
scarecrow.SetText("Wish", "Granted");
goldenCap.SetEnum("Rarity", "Unique");
```

### Rules

- **Same keys and types as reading.** `SetInt` only changes Integer fields. A missing Field or wrong type changes
  nothing and logs a warning (`TrySet…`: returns `false`, no warning).
- **Changes belong to the Entry and Field**, not to the key: a change made by name is read by reference or ID too.
- **Setting the original value removes the change**, so `IsOriginal` is `true` again.
- **Enum fields** take the option label (case-insensitive); an unknown label is rejected, `null` or `""` sets no
  option. A changed option that is later removed from the Field counts as not set.
- **Entry fields** only accept Entries of Categories the Field allows (`AllowedCategories`); `null` sets no Entry.
- **Vector fields**: `SetVector2` keeps the current z of a 3D field; 2D fields always keep z = 0.
- **Only the typed `Get…` shortcuts see changes.** `entry.GetValue(key)` / `TryGetValue` and the `FieldValue`
  getters always return the value from Lore.
- **Play mode:** all changes are cleared when Play mode starts and ends, so the Lore window always shows the values
  from Lore.

### LoreState

`LoreState` holds the changes of all Entries:

| Member | Purpose |
|---|---|
| `LoreState.HasChanges` | `true` while any value is changed |
| `LoreState.ResetAllToOriginal()` | Removes all changes (e.g. "New Game") |
| `LoreState.Changed` | Event `(Entry entry, Field field)` after a value changed; both `null` when many values changed at once (reset all, load) |
| `LoreState.SaveToJson(prettyPrint = false)` | All changes as JSON |
| `LoreState.LoadFromJson(json)` | Replaces all changes with the saved ones |

```csharp
LoreState.Changed += (entry, field) => RefreshHud();   // entry/field null: refresh everything
```

### Save games

`SaveToJson` returns a string; where it goes is up to the game (file, PlayerPrefs, your own save system):

```csharp
string path = Path.Combine(Application.persistentDataPath, "lore.json");

File.WriteAllText(path, LoreState.SaveToJson());           // save

if (File.Exists(path))
    LoreState.LoadFromJson(File.ReadAllText(path));        // load
```

- Saves use Entry, Field and option IDs, so renaming in Lore never breaks them. Numbers are written the same way
  on every system (independent of the language settings).
- Changes of Entries or Fields that no longer exist (deleted after an update) are kept but not used, so saving
  again loses nothing.
- `LoadFromJson` returns `false` for invalid JSON and keeps the current changes. `null` or `""` loads "no changes".
- **Entry values** are found by ID among the loaded Entries. Reference the Lore Project in the scene that loads
  the save, so all Entries are loaded; otherwise `GetEntry` may return `null` (with a warning).

---

## 5. Descriptions

Descriptions (Project, Category, Entry) are stored with links in an internal form (`[[id|Title]]`). Don't show
`Description` directly; use the Project to turn it into text:

```csharp
lore.GetPlainText(entry.Description);
// "The Wolf hunts at night."  (links show the current Entry title)

lore.FormatDescription(entry.Description, part => $"<u>{part.Text}</u>");
// Plain text as is, links formatted by you, e.g. clickable TextMeshPro links:
lore.FormatDescription(entry.Description, part => $"<link={part.Entry?.Id}><u>{part.Text}</u></link>");

lore.GetRichText(entry.Description);
// "The <b>Wolf</b> hunts at <i>night</i>."  (bold/italic as rich text tags for TextMeshPro and uGUI)
lore.GetRichText(entry.Description, part => $"<link={part.Entry?.Id}><u>{part.Text}</u></link>");

foreach (DescriptionPart part in lore.ParseDescription(entry.Description))
{
    // part.Text, part.IsLink, part.Entry (linked Entry), part.IsBrokenLink, part.IsBold, part.IsItalic
}
```

Links follow renames (they resolve by ID). A link to a deleted Entry is a *broken link*: it keeps its last
known title as `Text` and has no `Entry`. Glyphs are not part of the text.

Descriptions may use Markdown emphasis: `**bold**` / `__bold__`, `*italic*` / `_italic_`, `***both***`.
`GetPlainText`, `FormatDescription` and `ParseDescription` remove the markers (`ParseDescription` reports them as
`IsBold` / `IsItalic`); only `GetRichText` turns them into tags. Emphasis does not cross line breaks, `_` inside a
word (`snake_case`) is plain text, and `\*` / `\_` stand for the marker itself.

---

## 6. Tags

```csharp
lore.GetTags(entry);              // List<TagDefinition> (Title, Color, Id), in the Entry's order
lore.HasTag(entry, "Boss");       // by title, case-insensitive
lore.GetEntriesWithTag("Boss");   // all Entries with that tag, in Lore's order
lore.TagLibrary.FindByTitle("Boss");
```

An unknown tag title logs a warning (`HasTag` returns `false`, `GetEntriesWithTag` an empty list).

---

## 7. Warnings

Missing Fields, wrong field types and unknown tags log a warning prefixed with `[Lore]`:

- only in the Editor and in development builds (release builds stay silent),
- each message only **once per Play session**, so lookups in `Update` don't flood the console,
- clicking the message highlights the related asset (Entry, Field or Tag library).

---

## 8. Data maintained by the Editor

Some runtime data is derived and kept up to date automatically by the Lore window; it never needs manual work:

- **Project index**: the lists behind `Categories` / `Entries` / `TagLibrary`, updated whenever Lore content
  changes.
- **Scene paths**: Scene values also store the scene path (scene assets don't exist in builds), updated when a
  scene is moved or renamed.
- **Data format**: `project.FormatVersion` is the format of the Lore data (`LoreProject.CurrentFormatVersion` for
  the installed Lore). Data from older Lore versions is upgraded automatically when the editor loads the Project.
- **Before every build**, Lore refreshes all of this and saves the assets, so a build never contains stale data.

These updates don't change any Created/Modified timestamps.

---

## 9. Performance notes

- `FindEntry` / `FindCategory` (by ID) use a dictionary built on first use: fast.
- Title and name lookups (`FindEntryByTitle`, `FindField`, field values by name) compare strings over the
  lists: fine for occasional use; resolve once and keep the result when calling every frame.
- `GetEntries`, `GetTags`, `GetEntriesWithTag`, `ParseDescription` return **new lists** per call: cache the
  result if you need it repeatedly.

---

## 10. Memory & loading

Lore field values are **direct references**. In Unity, loading an object also loads everything it references.
So when a scene loads that references the Project, Unity loads:

- every Entry (through the Project index), with its Category and Fields: mostly text, small,
- every asset referenced by a field value: **Sprites** (their textures), **GameObjects** (prefabs with meshes,
  materials, textures, audio), **AudioClips**, **ScriptableObjects** and **Objects** (and whatever they reference).

All of it stays in memory while something references the Project.

**Who needs to care:** projects with many Entries **and** heavy field values. Text, numbers, booleans, URLs,
scenes (stored as paths) and Entry references cost little; even a thousand text-only Entries take only a few MB.
200 Entries with a portrait Sprite and a prefab each can easily take 100 MB or more, loaded at scene start even if
the game only shows three of them.

### Recommendations

- **Reference the Project only where you need lookups** (`FindEntry`, `GetEntries`, tags, descriptions with
  links), e.g. in a menu or codex scene, not in every gameplay scene.
- **Reference single Entries directly** where a script needs only one or a few
  (`[SerializeField] private Entry wolf;`). That loads only those Entries and their assets.
- **Keep very heavy assets out of Lore** when they are only needed in a few places, e.g. store the prefab in your
  own spawning system and use the Entry for its data (name, stats, description).
- **Prefer light field types where they are enough**: a Text field (e.g. an asset key for your own loading code)
  or a Scene field (stored as a path) costs nothing to load, a prefab reference does.
- **Measure** with Unity's Memory Profiler: open a scene that references the Project and look at what is loaded.

### Checking the footprint

1. Install the **Memory Profiler** package (Window > Package Management > Package Manager).
2. Enter Play Mode (or run a development build) in a scene that references the Project.
3. Window > Analysis > Memory Profiler > *Capture*, then look at Textures, Meshes and ScriptableObjects: assets
   referenced by Lore Entries show up with their sizes.

Loading heavy assets on demand (e.g. through Addressables) is not part of Lore yet.
