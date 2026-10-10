# Changelog

All notable changes to this package are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
