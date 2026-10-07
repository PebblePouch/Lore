# Changelog

All notable changes to this package are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
