# Changelog

All notable changes to this package are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.5] - 2026-11-05

### Fixed

- Lore no longer affects text input in other editor windows: undoing a Lore step from another window (e.g. the
  Inspector) no longer takes the keyboard focus from the field being edited there, and ending a Lore edit while
  another window has the keyboard no longer ends text editing in that window.
- Lore edits repaint only the views that show Lore data (Lore windows, Inspectors, Project Settings) instead of every
  editor view.

## [0.3.4] - 2026-10-11

### Added

- Oz Demo sample: a playable scene, **Oz Demo** (`Scenes/Oz Demo.unity`), that shows Lore at runtime. Dorothy travels
  the map of Oz, picks up Items, talks to Characters and completes the Quests of the book. The whole game state lives
  in Lore Fields (`Set…` through `LoreState`), so Save / Load is one `LoreState.SaveToJson()` / `LoadFromJson()`; values
  changed at runtime are marked in the view. Drawn with IMGUI, so it needs no UI packages; works with the built-in
  render pipeline and URP.
- Oz Demo sample: generated Field constants (`OzFields`) used by the demo scripts.

### Changed

- Oz Demo data: new Fields `Location` (Characters, shared with Items and Quests: the player's position) and
  `Completed` (Quests). Values describe the start of the game: the Wicked Witch of the West is alive, and the Silver
  Shoes, the Bucket of Water and the Wizard's gifts are not owned by the party yet.

## [0.3.3] - 2026-10-11

### Changed

- Runtime API sample: one script with a region per feature (Project & Categories, Entries & field values, field
  keys, descriptions, tags, runtime changes incl. save/load), each part switchable in the Inspector. Covers all field
  types and `LoreState`; the defaults (Scarecrow, Health, Witch) match the Oz Demo.

## [0.3.2] - 2026-10-11

### Added

- Runtime changes of field values: `entry.SetText` / `SetBool` / `SetInt` / `SetFloat` / `SetVector2` /
  `SetVector3` / `SetColor` / `SetEnum` (by option label) / `SetEntry` (and `TrySet…`) change a value while the game
  runs, without touching the Lore assets. `SetEntry` only accepts Entries of Categories the Field allows. The getters (`GetInt`, …) return
  the changed value, otherwise the value from Lore; `GetOriginal…` always returns the value from Lore,
  `IsOriginal` tells whether a value was changed, `ResetToOriginal` undoes a change. `LoreState` holds all
  changes: `HasChanges`, `ResetAllToOriginal`, `Changed` event. Changes are cleared at the start and end of Play
  mode.
- Save games for runtime changes: `LoreState.SaveToJson()` returns all changes as JSON (keyed by Entry and Field
  IDs, so renames are safe), `LoadFromJson(json)` replaces the current changes with them. Changes of Entries or
  Fields that no longer exist are kept but not used; invalid JSON is rejected (returns `false`) and keeps the current
  changes.

### Changed

- The Library update dialog now states that glyphs and colors already assigned to Categories, Entries, Fields and
  tags are never affected by the choice.

## [0.3.1] - 2026-10-11

### Added

- Library update after a Lore update: when a new version ships changed default Glyph or Color Libraries, opening the
  Lore window asks once how to proceed with the existing Libraries: "Update all libraries" (regenerate), "Append
  only new" (adds missing defaults at their default position, keeps everything else) or "Skip library updates"
  (no changes). Never asks while Unity compiles, imports or is in Play mode; new Libraries are created up to date.

## [0.3.0] - 2026-10-10

### Added

- Field type Color: a color with alpha, black when empty (never counts as a missing required value). Edited with
  Unity's color picker on the Entry page, in Tabular Editing and bulk editing; CSV as `#RRGGBBAA`; sorts by hue.
  Runtime API: `GetColor` / `TryGetColor`.
- Field type Vector: a Vector2 or Vector3, chosen per Field in the Field card ("Dimension"; switching keeps the
  values, a 2D Field keeps the stored z). Shown as "Vector2" / "Vector3"; never counts as a missing required value.
  Edited as X/Y/Z number fields on the Entry page, in Tabular Editing and bulk editing (a change sets only that
  component for all selected Entries); CSV as `x;y` / `x;y;z`; sorts by x, then y, then z. Runtime API:
  `GetVector2` / `GetVector3` (both work for either dimension) and `Field.VectorDimension`.
- Field type Enums: several options of the Field (same option editor as Enum), kept in option order. Edited with a
  dropdown that ticks options; bulk editing adds an option to all selected Entries or removes it from all; CSV as
  option names separated by `|`; searchable; an empty selection counts as a missing required value. Runtime API:
  `GetEnums` / `TryGetEnums` / `HasEnum`.
- Field type Folder: a project folder (other assets are not accepted). Stores the folder and its path, which follows
  moved/renamed folders; a deleted folder is a missing reference (integrity). In Tabular Editing, bulk editing,
  CSV (folder path), search (folder name) and sorting (path) like the other asset types. Runtime API:
  `GetFolderPath` / `TryGetFolderPath`.
- Field type Object: an asset of any other Unity type, chosen per Field like the ScriptableObject type (the picker
  opens right away; shown as e.g. "Texture2D (OBJ)"). Offers Unity's own types (Texture2D, Material,
  AnimationClip, abstract bases like Texture, components like Rigidbody, which pick prefabs) and the project's
  classes; types with a Field type of their own (GameObject, Sprite, AudioClip, the ScriptableObject classes) are left
  out. A missing type and a deleted asset are integrity issues, like for ScriptableObject Fields. In Tabular Editing,
  bulk editing, CSV (asset path), search and sorting like the other asset types. Runtime API: `GetObject<T>` /
  `TryGetObject<T>`.
  The type picker groups the types by function (Animation, Physics, Rendering, Textures, UI, ...), with the types
  already used by Object Fields and the project's own types on top; the search covers all groups.
- Field and Tag Library menus: "References" has a glyph (🔗), like the other menu items.
- Manual (Welcome page, `Documentation~/Manual.md`): replaces the placeholder with the full user manual, starting
  with ten First Steps for a new Project, then every part of Lore from Categories to the Runtime API and the
  keyboard shortcuts.
- Project glyph: the Project page header shows a glyph before the title (📜 by default), changed like Category and
  Entry glyphs; glyph and title line up with the Category and Entry headers. Runtime API: `LoreProject.Glyph`.

### Changed

- The Field type menu is grouped (Basic, Choice, Unity) with separators between the groups.
- Changing a Field between Enum and Enums or between Entry and Entries keeps the values: a single value becomes a
  list, a list with one value becomes the single value. Only lists with several values are cleared (the confirmation
  says how many Entries). Switching Enum ↔ Enums keeps the options.
- Oz Demo: shows the new Field types and features. Characters have Weaknesses (Enums), Places a region Color, a
  Banner (Object, Texture2D) and a Map Position (Vector2), Items a Pickup Sound (Audio, a chime per Rarity). The
  Project has a glyph (🌪️, the cyclone), and some descriptions use bold and italic.

## [0.2.1] - 2026-10-10

### Added

- Markdown emphasis in descriptions: `**bold**` / `__bold__`, `*italic*` / `_italic_`. View mode shows the
  formatting (also on links), edit mode keeps the markers; search ignores them.
- Runtime API: `LoreProject.GetRichText` (bold/italic as `<b>`/`<i>` tags) and `DescriptionPart.IsBold` /
  `IsItalic`. `GetPlainText`, `FormatDescription` and `ParseDescription` remove the markers.
- Field type Audio: an AudioClip (.wav, .mp3, .ogg, …) with a play/stop button for a preview in the editor; in
  Tabular Editing, bulk editing, CSV, search and integrity checks like the other asset types. Runtime API:
  `GetAudioClip` / `TryGetAudioClip`. The package now depends on the built-in Audio module.
- Default Glyph Library: 384 more glyphs (1047 in total), e.g. bathroom, clothes and shoes, activities, weather,
  tools, medicine, music, sports, office, zodiac and signs. Existing Glyph Libraries keep their glyphs; select the
  Glyph Library asset and use "Generate Default Glyph Library" to get the new list (it replaces the whole list).

### Fixed

- Text editing: End (and Shift+End) moves the caret to the end of the current row instead of the start of the
  next one (Windows).

## [0.2.0] - 2026-10-08

### Added

- Welcome page: without a Lore Project, the Lore window shows "Welcome to Lore" instead of Home, with Getting
  Started, the manual, links and credits. Start your Lore (pick a folder inside Assets), import an exported Project
  (Import Unity Package) or create the example Project. Lore no longer creates folders or Libraries on its own
  before a Project exists.
- Example Project "Oz Demo" (package sample): "The Wonderful Wizard of Oz" as the lore of a small game. Import it
  from the Welcome page or Package Manager > Lore > Samples.
- Multiple Projects window: opens when the Unity project contains more than one Lore Project, lists them (title,
  folder, Category and Entry counts) and exports + removes the selected one until one is left.

### Changed

- Libraries (Tags, Glyphs, Colors) next to the Project win over Libraries elsewhere in Assets.
- Project Insights no longer recalculate on every asset import in the Unity project, only on Lore-relevant changes
  (including deleted or moved assets that field values reference).
- Broken description links that can be repaired are now yellow (still struck through) instead of link blue, so
  they stand apart from working links.
- Category > Fields: a Field whose type is missing gets a red type cell, like other integrity marks.

### Fixed

- Confirmation dialogs grow with their message, so the last line is no longer cut off.
- Closing a Library through History, Home or a click elsewhere now also ends a running Entry, Glyph or Color pick,
  so its target no longer stays highlighted.
- Delete confirmations (Entry and Delete Selected) now say how many Field references and description links will
  break, instead of wrongly saying that references are cleared (they stay as broken references).

## [0.1.1] - 2026-10-07

### Changed

- License: free for noncommercial use under the PolyForm Noncommercial License 1.0.0; commercial use needs the
  Lore Commercial License (`LICENSE-COMMERCIAL.md`), included with the Supporter tier on Patreon.

## [0.1.0] - 2026-10-07

### Added

- Lore editor window (Tools > Lore > Open Lore) with a Project page, Categories, Entries and Fields, a Content
  browser with full-text search (titles, descriptions, field values) and Tag filters, and Quick Search (Ctrl+F /
  Cmd+F, also fuzzy: "wlf" finds "Wolf").
- Field types: Text, URL, Boolean, Integer, Float, Enum (options defined on the Field), Entry, Entries (several
  Entry references), GameObject, Sprite, Scene and ScriptableObject. Entry and Entries Fields can be limited to some
  Categories.
- Required Fields per Category (✱): Entries with empty required values are marked and counted in Health.
- Libraries for Fields, Tags, Colors, Glyphs and Entries, with default Color and Glyph Libraries.
- Description links to Entries, integrity checks (broken Fields, links and references, marked red) and content
  health (empty required values, empty Entries lists, marked yellow), plus Insights.
- Tabular Editing on the Category page: all Entries as rows, Fields as columns, editable in place, with search and
  sorting. Bulk editing: select rows to set a value, add or remove Tags or delete Entries for all of them at once.
- CSV export and import per Category, with a preview of all changes and warnings before anything is applied
  (see `Documentation~/CSV.md`).
- Tools > Lore > Export Project… / Import Project…: move the Lore data to another Unity project as a
  `.unitypackage`, optionally with the game assets it references.
- Undo/redo (Ctrl+Z / Ctrl+Y) for Lore edits, shown as "Lore: …" in the Edit menu. Creating, duplicating, deleting
  and renaming assets are not undoable, like in Unity's Project window.
- Entry, Category and Field fields in your own scripts show their Lore value with a searchable picker, drag & drop
  and double-click to open in Lore; `[LoreFilter(...)]` limits them by Category or Field type.
- Generated Field constants (Tools > Lore > Generate Field Constants): one `FieldKey` per Field
  (`entry.GetInt(LoreFields.Health)`), keyed by Field ID so renamed Fields keep working. Settings in
  Project Settings > Lore, shared by the team through `ProjectSettings/LoreSettings.asset`.
- Read-only Runtime API (namespace `PebblePouch.Lore`) with documentation (`Documentation~/RuntimeAPI.md`, including
  memory and loading) and the Runtime API sample.
- Lore only reacts to changes of its own assets, saves only its own assets and caches its analyses, so it stays out
  of the way in large projects.
