# Lore Manual

Lore keeps the world of your game in one place, right inside the Unity Editor: Categories, Entries, Fields, Tags and the links between them. Your game reads it at runtime through a small API.

## First Steps

A new Lore in ten minutes:

1. **Start your Lore.** On this page, click **🌱 Start your Lore** and pick a folder inside Assets. Lore creates a "Lore" folder there with your Project and its Libraries.
2. **Name your Project.** Click the title on the Project page and type the name of your world. Click the 📜 glyph to pick another one, and click the description to say what the world is about.
3. **Create a Category.** Click **✨** next to "Contents" in the browser on the left (or press Insert there). Rename it, e.g. "Characters", and give it a glyph and a color.
4. **Add Fields.** On the Category page, open the Fields section and click **✨** to create a new Field, e.g. "Health" (Integer) or "Home" (Entry). Pick the type on the Field's card and confirm with 💾. Use **➕** instead to add Fields that already exist in the Field Library.
5. **Create an Entry.** Open the Category's **⋮** menu and choose **➕ Add** (or press Insert on the Category page). Name it, e.g. "Dorothy", and add a subtitle and a glyph.
6. **Fill in the values.** The Entry page lists every Field of its Category. Type, pick or drag values in, and write notes about the Entry.
7. **Link Entries.** In the notes, type two opening square brackets and pick another Entry. The link follows the Entry when you rename it.
8. **Tag it.** In the Entry's Tags section, click **➕** to add Tags from the Tag Library or **✨** to create a new one, e.g. "Companion".
9. **Look at the Project page.** Insights count your content, Health shows what is unfinished, Integrity shows what is broken.
10. **Use it in your game.** See "Using Lore in Your Game" at the end of this manual.

Want to see a finished Lore first? **📖 Create Example Project** imports "The Wonderful Wizard of Oz" as the lore of a small game. Delete its sample folder (Assets/Samples/Lore/…) before you start your own Lore: a Unity project holds one Lore Project.

## The Lore Window

Open it with **Tools > Lore > Open Lore**.

- **Contents** (left): your Categories with their Entries. The search box finds titles, notes and field values; the Tag filter shows only Entries with the chosen Tags.
- **Page** (right): the selected Project, Category or Entry. Every page has a header (glyph, title, subtitle), sections you can fold, and a ⋮ menu with more actions.
- **Toolbar:** Home (the Project page), Back and Forward through your history, and Quick Search.
- **Quick Search** (Ctrl+F / Cmd+F) jumps to any Entry, Category or Field by name, also fuzzy: "wlf" finds "Wolf".

## Project

The Project is your world: one per Unity project. Its page holds the title, glyph, a description, Links (web pages such as a wiki or a design document) and the Insights. All Lore data lives in the folder of the Project asset (Lore.asset). You can move that folder anywhere inside Assets and rename the asset; Lore finds its data through it.

## Categories

Categories group your Entries, e.g. Characters, Places, Items or Quests. Each Category has a glyph, a color, a description and a list of Fields.

- **Fields section:** the Fields every Entry of this Category has. Drag them to change the order. Click ✱ to make a Field **required**: Entries that leave it empty count as unfinished (Health).
- **Tabular Editing:** all Entries of the Category as a table (see below).
- **⋮ menu:** add an Entry, export or import CSV, delete the Category (only when it has no Entries).

## Fields

A Field is defined once in the **Field Library** and can be used by several Categories, e.g. "Prefab" for Characters and Items. Renaming a Field or changing its glyph changes it everywhere.

Field types:

- **Basic:** Text, URL, Boolean, Integer, Float, Vector (2D or 3D), Color
- **Choice:** Enum (one option), Enums (several options), Entry (one Entry), Entries (several Entries)
- **Unity:** GameObject, Sprite, Audio, Scene, Folder, ScriptableObject (one of your classes), Object (any other asset type, e.g. Texture2D, Material or a component on a prefab)

Notes on the types:

- Enum and Enums Fields define their options on the Field card.
- Entry and Entries Fields can be limited to some Categories, e.g. "Home" only accepts Places.
- ScriptableObject and Object Fields ask for the type when you choose them.
- Changing the type of a Field clears its values. Exceptions: Enum ↔ Enums and Entry ↔ Entries keep what fits (a single value becomes a list, a list with one value becomes a single value).
- Removing a Field from a Category clears its values in that Category; Lore asks first and says how many Entries have values.
- The Field's ⋮ menu shows 🔗 References: the Entries that use it.

## Entries

An Entry is one thing in your world: a character, a place, an item, a quest. Its page shows the header (glyph, title, subtitle), its Field values, its Tags and its notes.

- Click a glyph to pick another one from the Glyph Library, click a color to pick from the Color Library.
- Entry and Entries values are picked from the **Entry Library**.
- Audio values have a ▶️ button to listen in the editor.
- The Entry's ⋮ menu duplicates (🪞) or deletes it. Deleting asks first and says how many references will break.

## Tags

Tags mark Entries across Categories, e.g. "Witch", "Magic" or "Danger". Each Tag has a color. Manage them in the **Tag Library**: rename, recolor, delete, or show the Entries that use a Tag (🔗 References). Filter the Contents by Tags with the Tag filter in the Contents.

## Libraries

Libraries are shared lists that open on the right side of the window, next to the page:

- **Field Library:** all Fields of the Project.
- **Tag Library:** all Tags.
- **Entry Library:** all Entries, to pick values for Entry and Entries Fields.
- **Glyph Library:** emoji to use as glyphs, searchable by name and keyword.
- **Color Library:** a palette for Categories and Tags.

Libraries are assets in the "Libraries" folder next to the Project. Glyphs are emoji of your system font, so they can look a little different on other systems.

## Notes, Descriptions and Links

Projects and Categories have a description, Entries have notes. Click the text to edit it; click outside or press Escape to finish.

- **Links:** type two opening square brackets to link an Entry. In view mode, a click opens the linked Entry. Links stay intact when you rename the Entry.
- **Emphasis:** two asterisks or two underscores around a text make it bold, one asterisk or one underscore makes it italic. The view shows the formatting; while you edit, the markers stay visible. Search ignores them.
- **Broken links** (the linked Entry was deleted) are struck through. Yellow ones can be repaired: click them to pick the right Entry or to remove the link and keep the text.

## Health and Integrity

Lore keeps an eye on your content and marks problems with two colors:

- **Yellow = Health:** unfinished content, e.g. an empty required value, an Entry without Tags or notes, or an empty Entries list.
- **Red = Integrity:** broken data, e.g. a Field whose type no longer exists, a value that points to a deleted Entry or asset, or a broken link.

Deleted Entries stay as broken references until you fix them, so nothing is lost silently. The Project page sums up all integrity issues and the Health card of the Insights counts unfinished content; click a group or a count to show the affected Entries in the Contents.

## Tabular Editing and Bulk Editing

The **Tabular Editing** section of a Category page shows its Entries as rows and its Fields as columns. Edit values right in the cells, search the rows, and click a column header to sort.

To edit several Entries at once, tick their checkboxes (Shift+click selects a range). The green selection row under the header applies a value to all selected Entries; "—" marks values that differ. The buttons next to the search add or remove Tags or delete the selected Entries. Rows hidden by the search are not changed.

## Undo and Saving

Lore saves its own assets automatically after every change. Edits can be undone with Ctrl+Z and redone with Ctrl+Y (Cmd+Z / Cmd+Shift+Z on macOS); they show up as "Lore: …" in Unity's Edit menu. Creating, duplicating, deleting and renaming assets cannot be undone, like in Unity's Project window.

Lore data is made of normal Unity assets, so it works with version control. Commit your project or back up the Lore folder before you update Lore or Unity.

## CSV Export and Import

A Category's ⋮ menu exports its Entries as a CSV file for spreadsheets (📤) and imports such a file back (📥). The import shows a preview of every change and warning before anything is applied. Details: Documentation~/CSV.md in the package.

## Moving Lore to Another Project

**Tools > Lore > Export Project…** packs your Lore data into a .unitypackage, optionally with the game assets your values point to. **Tools > Lore > Import Project…** (or 📦 Import Unity Package on this page) brings it into another Unity project with Lore installed. Values that point to game assets only work if those assets exist there too.

## Using Lore in Your Game

Your game reads Lore through an API in the namespace PebblePouch.Lore. Give a MonoBehaviour a field of type LoreProject, assign your Project asset, then look up Categories and Entries and read their values, e.g. entry.GetInt("Health").

- **Runtime changes:** the game can change Text, Boolean, Integer, Float, Vector, Color, Enum and Entry values while it runs, e.g. entry.SetInt("Health", 3), and save them with LoreState.SaveToJson. The Lore assets never change; changes are cleared when Play mode starts and ends.
- **Field constants:** Tools > Lore > Generate Field Constants creates a class with one key per Field, so your code uses LoreFields.Health instead of a name in quotes and keeps working when you rename Fields. Settings are in Project Settings > Lore.
- **Inspector fields:** Entry, Category and Field fields in your own scripts get a searchable picker, drag & drop, and double-click to open in Lore. The LoreFilter attribute limits them to a Category or a Field type.
- **Descriptions:** read them as plain text or as rich text with bold, italic and links.

The full reference is Documentation~/RuntimeAPI.md in the package; the **Runtime API** sample (Package Manager > Lore > Samples) shows it in a script.

## Keyboard Shortcuts

- **Ctrl+F / Cmd+F:** Quick Search
- **Alt+Left / Backspace, Alt+Right:** Back, Forward
- **Arrow keys:** move through the Contents; Left jumps from the page back to the Contents
- **Insert:** new Category (in the Contents), new Entry (on a Category page), duplicate (on an Entry page)
- **F2:** rename the selected Category or Entry
- **Delete:** delete the selected Category or Entry (asks first)
- **+:** open the matching Library
- **Up, Down, Page Up, Page Down, Home, End:** scroll the page
- **Escape:** finish editing, clear the search, close a Library

## Good to Know

- One Lore Project per Unity project. If there are more, Lore opens a window to export and remove the extra ones.
- Lore only reacts to its own assets, so it stays fast in large projects.
- Lore is free for noncommercial use; commercial use needs the Lore Commercial License (Supporter tier on Patreon). See the Links below.
