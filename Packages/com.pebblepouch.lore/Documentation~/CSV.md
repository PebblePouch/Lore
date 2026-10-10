# CSV Export & Import

Edit the Entries of a Category outside Unity, for example to balance values in a spreadsheet, let writers work
on texts, or create many Entries at once. Both commands are in the Category page's menu (**⋮**).

CSV covers **content** (Entries and their values), not **structure**: the Category and its Fields must exist in
Lore. Your Lore data itself is the asset folder (`Assets/Lore` and the Libraries): back it up with version control.

## Export CSV…

Writes one row per Entry (browser order) and these columns:

| Column | Content |
|---|---|
| `Id` | The Entry's ID. Keep it: the import uses it to find the Entry, also after a rename. |
| `Title` | Entry name |
| `Subtitle` | |
| `Glyph` | The Entry's own glyph; empty = the Category's glyph |
| `Tags` | Tag titles, separated by `\|` |
| `Description` | As in the editor; links as `[[Title]]` or `[[Category/Title]]` |
| one column per Field | Header = Field name, in the Category's order |

The file is a comma-separated, UTF-8 CSV with English number format, which Excel, Google Sheets and LibreOffice
read directly. In Excel with other regional settings, save as **CSV UTF-8 (comma delimited)**, not "CSV" with
semicolons.

### Field values

| Field type | Cell |
|---|---|
| Text, URL | The text |
| Boolean | `TRUE` / `FALSE` (import also reads `1`/`0`, `yes`/`no`) |
| Integer, Float | `.` as decimal point: `3.5` |
| Enum | Option name |
| Entry | Entry title; `Category/Title` if the title exists in several Categories the Field allows |
| Entries | Titles separated by `\|`: `Wolf\|Bear` |
| GameObject, Sprite, Scene, ScriptableObject, Audio | Asset path: `Assets/Prefabs/Wolf.prefab`; one of several Sprites (or ScriptableObjects) in a file: `Assets/Art/Sheet.png#Wolf_0` |

Broken references (deleted Entries, missing assets, deleted Enum options) are written as empty cells; the console
tells how many.

## Import CSV…

Reads a file in the same format and shows a **preview** first: Entries updated (and values changed), created,
renamed, unchanged, and Entries of the Category that are not in the file. Nothing changes until **Apply**.

- **Finding Entries:** by `Id`, else by `Title` (within the Category, case-insensitive). Rows that match no Entry
  create **new Entries** (they need a Title).
- **Renaming:** a row whose `Id` matches but whose `Title` differs renames the Entry.
- **Not in the file:** Entries without a row and columns that are missing stay as they are. To update only some
  values, delete the other columns.
- **Empty cells clear** the value (empty text, `0`, `FALSE`, no Entry). Broken references are the exception: they
  stay, so exporting and importing a file never removes them.
- **Problems** are skipped and listed in the preview with their row: invalid numbers, unknown Enum options, Entries,
  Tags or assets, ambiguous titles, name conflicts. The rest of the row is imported.
- Tags must exist in the Tag Library; columns without a matching Field are ignored.
- References between new Entries of the same file work (Entry fields and description links).

**Undo:** value changes are one undo step (*Edit > Undo Lore: Import CSV*). New and renamed Entries are file
operations and cannot be undone.
