# Lore

Worldbuilding inside the Unity Editor. Lore organizes your game's lore in **Categories**, **Entries** and
**Fields**, links Entries in descriptions, keeps an eye on integrity and content health, and lets your game
read everything at runtime.

## Installation

- **Git URL:** Package Manager > + > *Install package from git URL…* >
  `https://github.com/PebblePouch/Lore.git?path=/Packages/com.pebblepouch.lore#0.2.0`
  (the `#0.2.0` tag pins a version; without it, the package follows the latest state of `main`).
- **From disk:** Package Manager > + > *Install package from disk…* > select this folder's `package.json`.

Tested with Unity 6000.2 and 6000.6. Other Unity 6 versions are supported on a best-effort basis.

Before updating Lore or Unity, commit your project to version control or back up your Lore data (the data folder,
`Assets/Lore` by default, and your Libraries).

## Getting started

Open **Tools > Lore > Open Lore**. Without a Lore Project, the window shows the **Welcome** page with a short manual
and three ways to start:

- **Create:** pick a folder inside `Assets`; Lore creates a `Lore` folder there with your Project asset (`Lore.asset`),
  Categories, Entries, Fields and Libraries.
- **Continue:** import a Project exported from another Unity project (see below).
- **Demo:** import the example Project (see below).

You can move the data folder anywhere under `Assets` and rename the Project asset; Lore finds the data through it.
A Unity project holds one Lore Project; if it contains more, Lore helps you export and remove the extra ones.

## Example Project

The **Oz Demo** sample turns "The Wonderful Wizard of Oz" into the lore of a small game: Categories, Entries, Tags,
description links and field values that point to sprites, prefabs, scenes and ScriptableObjects. Import it from the
Welcome page or Package Manager > Lore > Samples, into a Unity project without a Lore Project of its own. Delete the
imported sample folder (`Assets/Samples/Lore/…`) before you start your own Lore.

## Tabular Editing & bulk editing

A Category page's **Tabular Editing** section shows its Entries as rows and its Fields as columns, editable in place.
Tick rows (Shift+click: range) to edit them together: the green selection row under the header applies a value to
all selected Entries ("—" marks differing values), and the buttons next to the search add or remove Tags or delete
the selected Entries. Rows hidden by the search are not changed.

## CSV export & import

A Category's menu (**⋮**) exports its Entries as a CSV file for spreadsheets and imports such a file back, with a
preview before anything changes. See `Documentation~/CSV.md`.

## Moving Lore data to another project

**Tools > Lore > Export Project…** packs the Lore data (data folder, Libraries, generated Field constants) into a
`.unitypackage`; **Tools > Lore > Import Project…** (or a double-click on the file) imports it into another Unity
project with Lore installed, ideally one without Lore data yet. Field values that point to game assets (prefabs,
sprites, scenes, ScriptableObjects) only work if those assets exist there too; the export can include them.
Project Settings > Lore are not part of the package.

## Runtime API

Game code reads Lore data through a small read-only API (namespace `PebblePouch.Lore`). See
`Documentation~/RuntimeAPI.md` and the **Runtime API** sample (Package Manager > Lore > Samples).

## License

Free for noncommercial use under the PolyForm Noncommercial License 1.0.0 (`LICENSE.md`). Commercial use needs the
Lore Commercial License (`LICENSE-COMMERCIAL.md`), included with the **Supporter** tier on Patreon
(https://www.patreon.com/c/pebblepouch). Details and examples: [repository README](https://github.com/PebblePouch/Lore#license).
Contact: pebblepouchproductions@gmail.com
