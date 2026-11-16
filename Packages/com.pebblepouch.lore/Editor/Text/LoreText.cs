namespace PebblePouch.Lore.Editor
{
    // All user-visible Lore text (English). One central file per language.
    //
    // Rules:
    // - Whole sentences with placeholders ({0}, {1}, ...) instead of concatenated fragments,
    //   so translations can reorder words. Count-dependent texts use LorePlural.
    // - static readonly (not const), so a later language switch can turn these into properties
    //   without changing callers. Strings only (no EditorStyles): safe to use anytime.
    // - Not here: Debug.Log messages, technical identifiers (property names, IDs, control names, paths),
    //   and glyph icons (visual, see style classes).
    public static class LoreText
    {
        // Undo step names (Edit menu: "Undo Lore: Edit Field Value"). LoreUndo adds the prefix, which also tells
        // Lore steps apart from other undo steps (see LoreUndo).
        public static class Undo
        {
            public static readonly string Prefix = "Lore: ";

            public static readonly string EditFieldValue = "Edit Field Value";
            public static readonly string ImportCsv = "Import CSV";
            public static readonly string ClearFieldValue = "Clear Field Value";
            public static readonly string EditDescription = "Edit Description";
            public static readonly string EditSubtitle = "Edit Subtitle";
            public static readonly string ChangeGlyph = "Change Glyph";
            public static readonly string UseCategoryGlyph = "Use Category Glyph";
            public static readonly string AddTag = "Add Tag";
            public static readonly string RemoveTag = "Remove Tag";
            public static readonly string ClearTags = "Clear All Tags";
            public static readonly string ReorderEntries = "Reorder Entries";

            public static readonly string EditTitle = "Edit Title";
            public static readonly string ChangeColor = "Change Color";
            public static readonly string ReorderCategories = "Reorder Categories";

            public static readonly string AddField = "Add Field";
            public static readonly string RemoveField = "Remove Field";
            public static readonly string ReorderFields = "Reorder Fields";
            public static readonly string MarkFieldRequired = "Mark Field Required";
            public static readonly string MarkFieldOptional = "Mark Field Optional";
            public static readonly string ChangeFieldType = "Change Field Type";
            public static readonly string EditEnumOptions = "Edit Enum Options";
            public static readonly string EditAllowedCategories = "Edit Allowed Categories";
            public static readonly string EditVectorDimension = "Edit Vector Dimension";
            public static readonly string RemoveBrokenField = "Remove Broken Field";
            public static readonly string ReplaceBrokenField = "Replace Broken Field";

            public static readonly string AddLink = "Add Link";
            public static readonly string EditLink = "Edit Link";
            public static readonly string ReorderLinks = "Reorder Links";

            public static readonly string CreateTag = "Create Tag";
            public static readonly string RenameTag = "Rename Tag";
        }

        // Quick Open (Ctrl+F / Cmd+F): jump to any Entry, Category or Field.
        public static class QuickOpen
        {
            public static readonly string Title = "Quick Search";

            public static readonly string Hint = "Type to find Entries, Categories and Fields.";
            public static readonly string NoMatches = "No matches.";

            public static readonly string CategoryDetail = "Category";

            // "Field · Integer"
            public static readonly string FieldDetail = "Field · {0}";
        }

        // Property drawers for Lore types (Entry, Category, Field) in the user's own Inspectors.
        public static class Drawers
        {
            // {0} kind: "None (Entry)", "Missing (Field)"
            public static readonly string None = "None ({0})";
            public static readonly string Missing = "Missing ({0})";
            public static readonly string MixedValues = "—";

            public static readonly string EntryKind = "Entry";
            public static readonly string CategoryKind = "Category";
            public static readonly string FieldKind = "Field";

            // "🐺 Wolf" / "📁 Characters"
            public static readonly string GlyphTitle = "{0} {1}";

            // "🐺 Wolf  (Characters)", "❤ Health  (Integer)": {0} glyph, {1} title, {2} detail, {3} detail color
            public static readonly string ValueWithDetail = "{0} {1}  <color={3}>({2})</color>";

            // Picker items: "❤ Health  (Integer)"
            public static readonly string ItemWithDetail = "{0} {1}  ({2})";

            public static readonly string ValueTooltip = "Double-click to open in Lore.";

            public static readonly string EntryPickerTitle = "Entries";
            public static readonly string CategoryPickerTitle = "Categories";
            public static readonly string FieldPickerTitle = "Fields";
            public static readonly string PickerNone = "None";
            public static readonly string PickerEmpty = "No matching items.";

            // [LoreFilter] problems below the field.
            public static readonly string UnknownCategory = "Lore: Category '{0}' not found.";

            // "Lore: FieldType has no effect on Entry fields."
            public static readonly string CriterionNotApplicable = "Lore: {0} has no effect on {1} fields.";
            public static readonly string FieldTypeCriterion = "FieldType";
        }

        public static class Common
        {
            // List joining: "A", "A and B", "A, B and C".
            public static readonly string ListSeparator = ", ";
            public static readonly string ListLastSeparator = " and ";

            public static readonly string Uncategorized = "Uncategorized";

            // "Name (3)"
            public static readonly string ItemWithCount = "{0} ({1})";

            // Count shown as statistic card metadata (outside the item color): "(3)"
            public static readonly string Count = "({0})";

            // "Source → Target"
            public static readonly string CategoryRelation = "{0} → {1}";

            #region Counts
            public static readonly LorePlural CategoryCount = new("{0} Category", "{0} Categories");
            public static readonly LorePlural EntryCount = new("{0} Entry", "{0} Entries");
            #endregion

            #region Dialogs
            public static readonly string OkButton = "OK";
            public static readonly string CancelButton = "Cancel";

            public static readonly string DeleteButton = "Delete";
            public static readonly string RemoveButton = "Remove";
            public static readonly string ChangeButton = "Change";

            public static readonly string CannotBeUndone = "<b>This cannot be undone.</b>";

            // Joins paragraphs of a dialog message (e.g. a warning followed by CannotBeUndone).
            public static string Paragraphs(params string[] paragraphs)
            {
                return string.Join("\n\n", paragraphs);
            }

            // "A", "A and B", "A, B and C" (ListSeparator / ListLastSeparator).
            public static string JoinList(System.Collections.Generic.IReadOnlyList<string> items)
            {
                if (items == null || items.Count == 0)
                    return string.Empty;

                if (items.Count == 1)
                    return items[0];

                return string.Join(ListSeparator, System.Linq.Enumerable.Take(items, items.Count - 1))
                       + ListLastSeparator
                       + items[items.Count - 1];
            }
            #endregion
        }

        // Display names of field types (enum names are technical identifiers, e.g. Url → "URL").
        public static class FieldTypes
        {
            public static readonly string Text = "Text";
            public static readonly string Entry = "Entry";
            public static readonly string GameObject = "GameObject";
            public static readonly string Sprite = "Sprite";
            public static readonly string Boolean = "Boolean";
            public static readonly string Integer = "Integer";
            public static readonly string Float = "Float";
            public static readonly string Scene = "Scene";
            public static readonly string Url = "URL";
            public static readonly string ScriptableObject = "ScriptableObject";
            public static readonly string Enum = "Enum";
            public static readonly string Enums = "Enums";
            public static readonly string Entries = "Entries";
            public static readonly string Audio = "Audio";
            public static readonly string Color = "Color";
            public static readonly string Folder = "Folder";
            public static readonly string Object = "Object";
            public static readonly string Vector = "Vector";

            // Vector Fields with their dimension (tables, Field cards): "Vector2", "Vector3"
            public static readonly string Vector2 = "Vector2";
            public static readonly string Vector3 = "Vector3";

            // Vector values: labels of the number fields.
            public static readonly string[] VectorComponents = { "X", "Y", "Z" };

            // ScriptableObject / Object Fields with their type: "DialogDefinition (SO)", "Texture2D (OBJ)"
            // ({0} type, {1} suffix)
            public static readonly string ScriptableObjectSuffix = "(SO)";
            public static readonly string ObjectSuffix = "(OBJ)";
            public static readonly string WithObjectType = "{0} {1}";

            public static string Get(FieldType type)
            {
                return type switch
                {
                    FieldType.Text => Text,
                    FieldType.Entry => Entry,
                    FieldType.GameObject => GameObject,
                    FieldType.Sprite => Sprite,
                    FieldType.Boolean => Boolean,
                    FieldType.Integer => Integer,
                    FieldType.Float => Float,
                    FieldType.Scene => Scene,
                    FieldType.Url => Url,
                    FieldType.ScriptableObject => ScriptableObject,
                    FieldType.Enum => Enum,
                    FieldType.Enums => Enums,
                    FieldType.Entries => Entries,
                    FieldType.Audio => Audio,
                    FieldType.Color => Color,
                    FieldType.Folder => Folder,
                    FieldType.Object => Object,
                    FieldType.Vector => Vector,
                    _ => type.ToString()
                };
            }
        }

        public static class Defaults
        {
            // Names of newly created items (they become asset names).
            public static readonly string NewCategory = "New Category";
            public static readonly string NewEntry = "New Entry";
            public static readonly string NewField = "New Field";
            public static readonly string NewTag = "New Tag";
            public static readonly string NewLink = "New Link";

            // Duplicate: "Wolf Copy"
            public static readonly string CopyName = "{0} Copy";
            // Unique name: "New Field 2"
            public static readonly string NumberedName = "{0} {1}";
        }

        public static class Validation
        {
            #region Names
            public static readonly string EnterName = "Enter a name.";
            public static readonly string NoLeadingOrTrailingSpace = "Cannot begin or end with space.";
            public static readonly string NoTrailingPeriod = "Cannot end with a period.";
            public static readonly string InvalidCharacters = "Do not use \\ / : * ? \" < > |.";
            public static readonly string NoControlCharacters = "Do not use control character.";
            public static readonly string NameTooLong = "The name is too long.";
            public static readonly string ReservedName = "'{0}' is a reserved name.";
            #endregion

            #region Not Found / Not Saved
            public static readonly string CategoryNotFound = "The Category could not be found.";
            public static readonly string EntryNotFound = "The Entry could not be found.";
            public static readonly string FieldNotFound = "The Field could not be found.";
            public static readonly string TagNotFound = "The Tag could not be found.";
            public static readonly string TagNotSaved = "The Tag could not be saved.";
            #endregion

            #region Already Exists
            public static readonly string CategoryExists = "A Category named '{0}' already exists.";
            public static readonly string CategoryFolderExists = "A Category folder named '{0}' already exists.";
            public static readonly string EntryExists = "An Entry named '{0}' already exists.";
            public static readonly string FieldExists = "A Field named '{0}' already exists.";
            public static readonly string TagExists = "A Tag named '{0}' already exists.";
            #endregion

            #region Field Types
            public static readonly string ObjectTypeRequired = "Choose a type.";
            public static readonly string EnumOptionsRequired = "Add at least one option.";
            public static readonly string EnumOptionNameRequired = "Every option needs a name.";
            public static readonly string EnumOptionExists = "The option '{0}' exists twice.";
            #endregion
        }

        public static class Window
        {
            // Window tab title.
            public static readonly string Title = "Lore";

            public static readonly string HomeButton = "Home";
            public static readonly string QuickSearchButton = "Quick Search";
            public static readonly string QuickSearchTooltip = "Quick Search (Ctrl+F)";

            public static readonly string BackTooltip = "Back (Alt+Left / Backspace)";
            public static readonly string ForwardTooltip = "Forward (Alt+Right)";
        }

        // Welcome page: shown instead of Home while the Unity project has no Lore Project.
        public static class Welcome
        {
            public static readonly string Title = "Welcome to Lore";

            public static readonly string GettingStartedHeader = "Getting Started";
            public static readonly string GettingStartedText =
                "Hello and welcome! Lore keeps the world of your game in one place: Categories, Entries, Fields, " +
                "Tags and the links between them, right inside the Unity Editor.\n\n" +
                "Let's set you up. Start a new Lore, continue with a Project you exported before, or explore the " +
                "example Project first. Nothing is created until you choose.";

            public static readonly string CreateHeader = "Create";
            public static readonly string CreateInfo =
                "Pick a folder inside Assets. Lore creates a \"Lore\" folder there with your Project, Categories, " +
                "Fields and Libraries.";

            public static readonly string ContinueHeader = "Continue";
            public static readonly string ContinueInfo =
                "Pick a Project exported with Tools > Lore > Export Project. Unity's import window shows every " +
                "file before it is added.";

            public static readonly string DemoHeader = "Demo";
            public static readonly string DemoInfo =
                "Imports the example Project \"The Wonderful Wizard of Oz\". Remove it again (delete its sample " +
                "folder) before you start your own Lore.";

            public static readonly string ManualHeader = "Manual";
            public static readonly string ManualMissing = "The manual could not be found in the Lore package.";

            public static readonly string LinksHeader = "Links";
            public static readonly string GitHubLink = "Lore on GitHub";
            public static readonly string ChangelogLink = "Changelog";
            public static readonly string LicenseLink = "License";
            public static readonly string PatreonLink = "Support Lore on Patreon";

            public static readonly string CreditsHeader = "Credits";
            public static readonly string CreditsText =
                "Lore is made by Pebble Pouch.\n\n" +
                "Glyphs are emoji of your system font. The example Project is based on \"The Wonderful Wizard of " +
                "Oz\" by L. Frank Baum (1900, public domain).";

            // Start your Lore
            public static readonly string ChooseFolderTitle = "Choose the Folder for Your Lore";
            public static readonly string FolderOutsideAssetsTitle = "Lore: Start Your Lore";
            public static readonly string FolderOutsideAssetsMessage =
                "Please choose a folder inside the Assets folder of this Unity project.";

            // Create Example Project
            public static readonly string DemoFailedTitle = "Lore: Create Example Project";
            public static readonly string DemoNotFoundMessage =
                "The example Project was not found in the Lore package. Please reinstall Lore.";
        }

        // Shown when the Unity project contains more than one Lore Project.
        public static class MultipleProjects
        {
            public static readonly string WindowTitle = "Lore: Multiple Projects";
            public static readonly string Message =
                "This Unity project contains {0} Lore Projects, but Lore works with exactly one. Export the " +
                "Projects you want to keep elsewhere, then remove them until one is left.";

            public static readonly string TitleColumn = "Project";
            public static readonly string PathColumn = "Folder";
            public static readonly string CategoriesColumn = "Categories";
            public static readonly string EntriesColumn = "Entries";

            public static readonly string UntitledProject = "Untitled Project";

            public static readonly string ExportAndRemoveButton = "Export and Remove…";
            public static readonly string ExportAndRemoveTooltip =
                "Exports the selected Project as a .unitypackage, then deletes its folder";

            public static readonly string ConfirmTitle = "Lore: Remove Project";
            public static readonly string ConfirmMessage =
                "The Project \"{0}\" was exported to:\n{1}\n\nDelete its folder now?\n{2}\n\n" +
                "This cannot be undone.";
            public static readonly string RemoveButton = "Delete Folder";

            public static readonly string CannotRemoveMessage =
                "The Project \"{0}\" lies directly in {1}. Lore only removes Project folders; move it into its own " +
                "folder first.";
        }

        // Format version of the Lore data (LoreFormatUpgradeService).
        public static class DataFormat
        {
            // "Lore: The Lore Project at Assets/Lore/Lore.asset was saved by a newer Lore version (data format 3; …"
            public static readonly string NewerFormatWarning =
                "Lore: The Lore Project at {0} was saved by a newer Lore version (data format {1}; this version " +
                "knows up to format {2}). Update Lore before you edit it, or data of the newer version may be lost.";
        }

        // CSV export / import of a Category's Entries.
        public static class Csv
        {
            public static readonly string ExportTitle = "Export Entries as CSV";
            public static readonly string ImportTitle = "Import Entries from CSV";

            #region Preview
            public static readonly string PreviewWindowTitle = "Lore: Import CSV";
            // {0} = Category title
            public static readonly string PreviewHeader = "Import into '{0}'";
            public static readonly string NoChanges = "The file contains no changes.";
            public static readonly string ApplyButton = "Apply";

            public static readonly LorePlural UpdatedEntries = new("{0} Entry updated", "{0} Entries updated");
            public static readonly LorePlural ChangedValues = new("{0} value changed", "{0} values changed");
            public static readonly LorePlural CreatedEntries = new("{0} new Entry", "{0} new Entries");
            public static readonly LorePlural RenamedEntries = new("{0} Entry renamed", "{0} Entries renamed");
            public static readonly LorePlural UnchangedEntries = new("{0} Entry unchanged", "{0} Entries unchanged");
            public static readonly LorePlural UntouchedEntries = new(
                "{0} Entry of the Category is not in the file and stays as it is",
                "{0} Entries of the Category are not in the file and stay as they are");

            public static readonly string UndoNote =
                "Value changes can be undone in one step. New and renamed Entries cannot be undone.";

            public static readonly LorePlural Warnings = new(
                "{0} warning (these rows or cells are skipped):",
                "{0} warnings (these rows or cells are skipped):");
            #endregion

            #region Errors
            // {0} = system message
            public static readonly string ReadFailed = "Could not read the file: {0}";
            public static readonly string NoKnownColumns =
                "The file has no columns this Category knows. Lore reads comma-separated files with a header row, " +
                "like the ones from Export CSV.";
            public static readonly string NoIdOrTitleColumn =
                "The file needs an Id or a Title column to find the Entries.";
            #endregion

            #region Warnings
            // {0} = column name
            public static readonly string UnknownColumn = "Column '{0}' is ignored: the Category has no such Field.";
            public static readonly string DuplicateColumn = "Column '{0}' appears more than once; only the first one is used.";

            // Rows: {0} = row number
            public static readonly string RowOtherCategory =
                "Row {0}: '{1}' belongs to the Category '{2}'. Row skipped.";
            public static readonly string RowDuplicate = "Row {0}: '{1}' is already in row {2}. Row skipped.";
            public static readonly string RowNoTitle = "Row {0}: a new Entry needs a Title. Row skipped.";
            // {1} = title, {2} = reason
            public static readonly string RowNewTitleFailed = "Row {0}: cannot create '{1}': {2} Row skipped.";
            // {1} = old title, {2} = new title, {3} = reason
            public static readonly string RenameFailed = "Row {0}: '{1}' is not renamed to '{2}': {3}";
            public static readonly string UnknownTag =
                "Row {0}: there is no Tag '{1}' (create it in the Tag Library first). The other Tags are imported.";

            // Cells: {0} = row number, {1} = column, {2} = one of the reasons below
            public static readonly string CellSkipped = "Row {0}, {1}: {2} Cell skipped.";

            // Reasons: {0} = cell text, {1} = Field type ("Sprite", "DialogDefinition (SO)")
            public static readonly string InvalidBool = "'{0}' is not TRUE or FALSE.";
            public static readonly string InvalidInteger = "'{0}' is not a whole number.";
            public static readonly string InvalidFloat = "'{0}' is not a number (use . as decimal point).";
            public static readonly string InvalidColor = "'{0}' is not a color (use #RRGGBB or #RRGGBBAA).";
            public static readonly string InvalidVector = "'{0}' is not a vector (use x;y or x;y;z with . as decimal point).";
            public static readonly string UnknownOption = "'{0}' is not an option of this Field.";
            public static readonly string SingleEntryOnly = "this Field holds one Entry, but the cell lists several.";
            public static readonly string EntryNotFound = "there is no Entry '{0}' in the Categories this Field allows.";
            public static readonly string EntryAmbiguous = "'{0}' matches several Entries; write it as Category/Title.";
            public static readonly string AssetNotFound = "there is no asset at '{0}'.";
            public static readonly string AssetWrongType = "'{0}' is not a {1}.";
            public static readonly string AssetAmbiguous =
                "'{0}' contains several assets of this kind; add the name: path#Name.";
            #endregion
        }

        // Tools > Lore > Export / Import Project (.unitypackage).
        public static class ProjectPackage
        {
            public static readonly string ExportDialogTitle = "Lore: Export Project";
            public static readonly string ExportDialogMessage =
                "Exports the Lore data (Project, Categories, Entries, Fields and Libraries) as a .unitypackage " +
                "for another Unity project with Lore installed.\n\n" +
                "Field values can point to assets of your game (prefabs, sprites, scenes, ScriptableObjects). " +
                "Include them only if the other project does not have them: Unity then adds every asset they " +
                "need, including scripts, which can make the package large.";
            public static readonly string LoreDataOnlyButton = "Lore Data Only";
            public static readonly string WithReferencedAssetsButton = "Include Referenced Assets";

            public static readonly string SaveTitle = "Export Lore Project";
            public static readonly string OpenTitle = "Import Lore Project";

            public static readonly string ImportWarningTitle = "Lore: Import Project";
            public static readonly string ImportWarningMessage =
                "This Unity project already has a Lore Project. Importing another one creates a second Project, " +
                "and Lore only uses the first one it finds.\n\n" +
                "Import into a project without Lore data, or deselect the files you do not want in Unity's " +
                "import window.";
            public static readonly string ImportAnywayButton = "Import Anyway";
        }

        // Project Settings > Lore
        public static class Settings
        {
            public static readonly string[] Keywords = { "Field", "Constants", "Code", "Generate", "Namespace" };

            #region Field Constants
            public static readonly string FieldConstantsHeader = "Field Constants";
            public static readonly string FieldConstantsInfo =
                "Generates a C# class with a key for every Field, e.g. entry.GetInt(LoreFields.Health). " +
                "Keys use Field IDs, so renamed Fields keep working until the class is generated again.";

            public static readonly string OutputFile = "Output File";
            public static readonly string OutputFileTooltip =
                "Where the class is written (inside Assets). Pick a folder whose assembly your game code can use. " +
                "Empty = Generated folder next to the Lore Project.";
            public static readonly string BrowseButton = "Browse…";
            public static readonly string BrowseTitle = "Field Constants File";
            public static readonly string BrowseMessage = "Choose where to write the generated Field constants.";

            public static readonly string Namespace = "Namespace";
            public static readonly string NamespaceTooltip = "Namespace of the generated class. Empty = global namespace.";
            public static readonly string GlobalNamespacePlaceholder = "(global namespace)";

            public static readonly string ClassName = "Class Name";
            public static readonly string ClassNameTooltip = "Name of the generated static class.";

            public static readonly string GenerateButton = "Generate";
            #endregion

            #region Generation Results
            // {0} = count, {1} = file path
            public static readonly LorePlural GeneratedConstants = new(
                "Generated {0} Field constant in {1}.",
                "Generated {0} Field constants in {1}.");
            public static readonly string ConstantsUpToDate = "The Field constants in {0} are up to date.";
            public static readonly string ConstantsInvalidSettings =
                "The Field constants were not generated: {0} See Project Settings > Lore.";
            public static readonly string ConstantsWriteFailed = "Could not write the Field constants to {0}: {1}";
            // {0} = list like "MaxHP2 (Max-HP)"
            public static readonly string NumberedConstants =
                "Fields with the same constant name got a number: {0}.";
            #endregion

            #region Issues
            public static readonly string PathOutsideAssets = "The output file must be inside the Assets folder.";
            public static readonly string PathNotScript = "The output file must end with .cs.";
            public static readonly string InvalidNamespace = "'{0}' is not a valid namespace.";
            public static readonly string InvalidClassName = "'{0}' is not a valid class name.";
            #endregion
        }

        public static class Browser
        {
            public static readonly string ContentsTitle = "Contents";

            public static readonly string SearchPlaceholder = "Search Contents...";
            public static readonly string SearchNoContents = "No contents available.";
            public static readonly string SearchNoMatches = "No matching contents.";

            // Category card field list
            public static readonly string NoFields = "- no fields -";
            // Number of further fields not shown: " +3" (leading space separates it from the glyphs)
            public static readonly string MoreCount = " +{0}";
        }

        public static class Dialogs
        {
            #region Titles
            public static readonly string CategoryTitle = "Category: {0}";
            public static readonly string EntryTitle = "Entry: {0}";
            public static readonly string RemoveFieldTitle = "Remove Field: {0}";
            public static readonly string LinkTitle = "Link: {0}";
            // {0} Entry count ("3 Entries")
            public static readonly string DeleteSelectedTitle = "Delete Selected: {0}";
            #endregion

            #region Messages
            public static readonly string CategoryDeleteWarning =
                "This will <b>delete</b> the Category <b>permanently</b>. The Category currently has <b>no Entries</b>.";

            // {0} Entry count ("3 Entries")
            public static readonly string CategoryNotEmpty =
                "This Category contains <b>{0}</b>. Only empty Categories can be deleted.";

            public static readonly LorePlural CategoryNotEmptyAction = new(
                "Delete this Entry before you try again.",
                "Delete all Entries before you try again.");

            public static readonly string LinkRemoveWarning =
                "This will <b>remove</b> the link <b>permanently</b>.";

            public static readonly string EntryDeleteWarning =
                "This will <b>delete</b> the Entry <b>permanently</b>.";

            // Delete Selected (Tabular Editing), followed by the list. {0} = Entry count
            public static readonly LorePlural EntriesDeleteWarning = new(
                "This will <b>delete</b> this Entry <b>permanently</b>.",
                "This will <b>delete</b> these <b>{0} Entries permanently</b>.");

            // Delete Entry / Delete Selected: references that break. {0} = Entry count (only picks the form)
            public static readonly LorePlural DeleteNoReferences = new(
                "Nothing else references this Entry.",
                "Nothing else references these Entries.");

            // {0} = "2 Field references", "1 description link" or both joined by JoinList
            public static readonly string DeleteBreaksReferences =
                "<b>{0}</b> will <b>break</b> and be marked red until fixed.";

            public static readonly LorePlural DeleteFieldReferences = new(
                "{0} Field reference",
                "{0} Field references");

            public static readonly LorePlural DeleteDescriptionLinks = new(
                "{0} description link",
                "{0} description links");

            // One line per Entry ({0} glyph + title); more than the listed Entries: {0} = count not listed.
            public static readonly string DeleteListItem = "• {0}";
            public static readonly string DeleteListMore = "… and {0} more";

            // {0} = number of Entries with values
            public static readonly LorePlural FieldRemoveWarning = new(
                "<b>{0} Entry has set values</b>. Removing this Field will <b>permanently clear these values</b>.",
                "<b>{0} Entries have set values</b>. Removing this Field will <b>permanently clear these values</b>.");
            #endregion
        }

        public static class Inspector
        {
            #region Notices
            // Project page, blue notice. {0} = the "Generate now" link.
            public static readonly string FieldConstantsOutOfDate =
                "<b>Field Constants</b> are out of date. {0}";
            public static readonly string FieldConstantsGenerate = "Generate now";
            #endregion

            #region Header
            // Timestamps: "Created: Oct 3, 2026 (14:05:33)" (.NET custom date format).
            public static readonly string CreatedTimestamp = "Created: {0}";
            public static readonly string ModifiedTimestamp = "Modified: {0}";
            public static readonly string TimestampDateFormat = "MMM d, yyyy (HH:mm:ss)";

            public static readonly string ColorTooltip = "Change Color";
            public static readonly string GlyphTooltip = "Change Glyph";

            public static readonly string CategoryTitleTooltip = "Rename Category";
            public static readonly string EntryTitleTooltip = "Rename Entry";
            public static readonly string ProjectTitleTooltip = "Rename Project";
            public static readonly string SubtitleTooltip = "Change Subtitle";

            public static readonly string EntryTitlePlaceholder = "Entry Title";
            public static readonly string SubtitlePlaceholder = "Subtitle";

            public static readonly string NothingSelected = "Nothing selected.";
            #endregion

            #region Descriptions
            public static readonly string CategoryDescriptionHeader = "Description";
            public static readonly string CategoryDescriptionPlaceholder = "Click to add a description...";
            public static readonly string CategoryDescriptionTooltip = "Edit Description";

            public static readonly string EntryNotesHeader = "Notes";
            public static readonly string EntryNotesPlaceholder = "Click to add notes...";
            public static readonly string EntryNotesTooltip = "Edit Notes";

            public static readonly string ProjectDescriptionHeader = "Description";
            public static readonly string ProjectDescriptionPlaceholder = "Click to add a project description...";
            public static readonly string ProjectDescriptionTooltip = "Edit Description";
            #endregion

            #region Fields and Tags
            // Section headers with count: "Fields (3)"
            public static readonly string FieldsHeader = "Fields ({0})";
            public static readonly string TagsHeader = "Tags ({0})";

            public static readonly string BrowseEntries = "Browse Entries...";

            public static readonly string MissingFieldTitle = "None (Broken Field)";
            public static readonly string MissingFieldFix = "Fix on Category level";

            // Required Fields (per Category).
            public static readonly string FieldRequiredTooltip = "Required: Entries of this Category should fill this Field.";
            public static readonly string FieldOptionalTooltip = "Optional. Click to make it required for Entries of this Category.";

            // ScriptableObject value without a usable type: "Type 'DialogDefinition' not found"
            // Enum value that is not set.
            public static readonly string EnumNotSet = "—";

            // Enums value: selected options joined ("Fire, Ice").
            public static readonly string EnumsSeparator = ", ";

            public static readonly string ObjectTypeMissing = "Type '{0}' not found";
            public static readonly string ObjectTypeNotSet = "No type chosen";
            public static readonly string ObjectTypeFixTooltip = "Choose the type in the Field Library.";

            public static readonly string ReorderFieldsDisabledTooltip = "Fix broken Fields before reordering";
            #endregion

            #region Tabular Editing (Category page)
            public static readonly string TabularEditingHeader = "Tabular Editing";
            public static readonly string TableEntryColumn = "Entry";
            public static readonly string TableNoEntries = "No Entries in this Category yet.";
            public static readonly string TableOpenEntryTooltip = "Open Entry";
            public static readonly string TableEditEntriesTooltip = "Click to edit on the Entry page";
            // Entries cells, if all titles fit: "🍎 Apple 🪲 Beetle"; else glyphs + count: "🍎🪲(2)" ({0} glyphs, {1} count).
            public static readonly string TableEntriesSeparator = " ";
            public static readonly string TableEntriesGlyphs = "{0}({1})";
            public static readonly string TableSortTooltip = "Click to sort (ascending, descending, off)";
            public static readonly string TableSearchPlaceholder = "Search Entries...";
            public static readonly string TableNoMatches = "No Entries match the search.";
            public static readonly string TableSelectTooltip = "Select for bulk editing (Shift+click: range)";
            public static readonly string TableSelectAllTooltip = "Select all / none (visible rows)";
            // Selection row: {0} selected visible rows, {1} selected rows hidden by the search (not changed).
            public static readonly string TableSelectionCount = "{0} selected";
            public static readonly string TableSelectionCountHidden = "{0} selected (excl. {1} hidden)";
            public static readonly string TableSelectionTooltip = "Changes in this row apply to all selected Entries";
            public static readonly string TableMixedValue = "—";
            public static readonly string TableSelectionPickTooltip = "Click to pick the Entry for all selected Entries";
            // Selection row Entries chips: {0} glyph + title (glyph only when space is short), {1} selected Entries
            // listing it, {2} selected Entries.
            public static readonly string TableSelectionChip = "{0} ({1}/{2})";
            public static readonly string TableSelectionAddToRestTooltip = "Click to add to all selected Entries";
            // Chips that do not fit: {0} count (tooltip lists them).
            public static readonly string TableSelectionOverflow = "+{0}";
            // Bulk action buttons: the glyph names the action ("➕ Tag", "✖️ All Tags", "❌ Entries").
            public static readonly string TableToolbarTag = "Tag";
            public static readonly string TableToolbarAllTags = "All Tags";
            public static readonly LorePlural TableToolbarEntries = new("Entry", "Entries");
            // Remove Tag menu: {0} Tag title, {1} selected Entries with the Tag, {2} selected Entries.
            public static readonly string TableSelectionTag = "{0} ({1}/{2})";
            #endregion

            #region Links (Project page)
            // Section header with count: "Links (3)"
            public static readonly string LinksHeader = "Links ({0})";
            public static readonly string NoLinks = "No links yet.";

            // Tooltip on a link label: "Open https://..."
            public static readonly string OpenLinkTooltip = "Open {0}";
            // Tooltip on the dimmed URL, which enters edit mode
            public static readonly string EditLinkTooltip = "Click to edit";

            public static readonly string LinkUrlRequired = "Enter a URL.";

            // Repair menu of broken description links: candidates "🐺 Wolf  (Characters)", then remove link
            public static readonly string LinkCandidate = "{0} {1}  ({2})";
            public static readonly string RemoveLinkKeepText = "Remove link, keep text";
            #endregion
        }

        public static class Library
        {
            #region Browsers (title, search placeholder, empty states, unused filter)
            public static readonly string EntryPickerTitle = "Entry Picker";
            public static readonly string EntrySearch = "Search Entries...";

            // Entry Library while adding to an Entries field: "Add to: Allies"
            public static readonly string EntryAddToListFilter = "Add to: {0}";

            // Entry Library while picking for a Field limited to some Categories: "Categories: Characters and Monsters"
            public static readonly string EntryAllowedCategoriesFilter = "Categories: {0}";
            public static readonly string EntryEmpty = "No entries available.";
            public static readonly string EntryNoMatches = "No matching entries.";

            public static readonly string FieldPickerTitle = "Field Picker";
            public static readonly string FieldManagerTitle = "Field Manager";
            public static readonly string FieldSearch = "Search Fields...";
            public static readonly string FieldEmpty = "No fields available.";
            public static readonly string FieldNoMatches = "No matching fields.";
            public static readonly string FieldUnusedLabel = "Unused Fields";

            public static readonly string TagPickerTitle = "Tag Picker";
            public static readonly string TagManagerTitle = "Tag Manager";
            public static readonly string TagSearch = "Search Tags...";
            public static readonly string TagEmpty = "No tags available.";
            public static readonly string TagNoMatches = "No matching tags.";
            public static readonly string TagUnusedLabel = "Unused Tags";

            public static readonly string GlyphPickerTitle = "Glyph Picker";
            public static readonly string GlyphManagerTitle = "Glyph Manager";
            public static readonly string GlyphSearch = "Search Glyphs...";
            public static readonly string GlyphEmpty = "No glyphs available.";
            public static readonly string GlyphNoMatches = "No matching glyphs.";
            public static readonly string GlyphUnusedLabel = "Unused Glyphs";

            public static readonly string ColorPickerTitle = "Color Picker";
            public static readonly string ColorManagerTitle = "Color Manager";
            public static readonly string ColorSearch = "Search Colors...";
            public static readonly string ColorEmpty = "No colors available.";
            public static readonly string ColorNoMatches = "No matching colors.";
            public static readonly string ColorUnusedLabel = "Unused Colors";
            #endregion

            #region Cards
            public static readonly string FieldTitlePlaceholder = "New Field";
            public static readonly string TagTitlePlaceholder = "New Tag";

            public static readonly string UseCategoryGlyph = "Use Category Glyph";

            // Content browser filter label: "References: Field"
            public static readonly string ReferencesFilter = "References: {0}";

            // ScriptableObject / Object Fields: type selection in the Field card.
            public static readonly string ObjectTypePlaceholder = "Choose type";
            public static readonly string ObjectTypeDropdownTitle = "ScriptableObject Types";
            public static readonly string ObjectTypeEmpty = "No ScriptableObject classes found in your scripts.";
            public static readonly string ObjectTypeDropdownTitleObject = "Object Types";
            public static readonly string ObjectTypeEmptyObject = "No asset types found.";

            // Dropdown item: "DialogDefinition  (MyGame.Dialog, abstract)"
            public static readonly string ObjectTypeDetails = "{0}  ({1})";
            public static readonly string ObjectTypeDetailSeparator = ", ";
            public static readonly string ObjectTypeAbstract = "abstract";

            // Object type dropdown: groups by function (see ObjectTypeGroups).
            public static readonly string ObjectTypeGroupUsed = "Used in Lore";
            public static readonly string ObjectTypeGroupProject = "Project";
            public static readonly string ObjectTypeGroup2D = "2D";
            public static readonly string ObjectTypeGroupAnimation = "Animation";
            public static readonly string ObjectTypeGroupAudioVideo = "Audio & Video";
            public static readonly string ObjectTypeGroupEffects = "Effects";
            public static readonly string ObjectTypeGroupInput = "Input";
            public static readonly string ObjectTypeGroupMaterials = "Materials & Shaders";
            public static readonly string ObjectTypeGroupNavigation = "Navigation";
            public static readonly string ObjectTypeGroupPhysics = "Physics";
            public static readonly string ObjectTypeGroupPhysics2D = "Physics 2D";
            public static readonly string ObjectTypeGroupRendering = "Rendering";
            public static readonly string ObjectTypeGroupTerrain = "Terrain";
            public static readonly string ObjectTypeGroupText = "Text";
            public static readonly string ObjectTypeGroupTextures = "Textures";
            public static readonly string ObjectTypeGroupUI = "UI";
            public static readonly string ObjectTypeGroupGeneral = "General";

            // Field card of a ScriptableObject / Object Field whose type cannot be resolved (type label in red).
            public static readonly string ObjectTypeMissingTooltip = "Type '{0}' not found. Choose the type again.";

            // Entry / Entries Fields: which Categories can be picked.
            public static readonly string AllowedCategoriesLabel = "Categories";
            public static readonly string AllowedCategoriesAll = "All Categories";

            // Vector Fields: dimension selector in the Field card.
            public static readonly string VectorDimensionLabel = "Dimension";
            public static readonly string VectorDimension2 = "2D (x, y)";
            public static readonly string VectorDimension3 = "3D (x, y, z)";

            // Enum Fields: option editor in the Field card.
            public static readonly string EnumOptionsEmpty = "No options yet.";

            // {0} option names ("<b>Rare</b> and <b>Epic</b>"), {1} Entry count ("3 Entries")
            public static readonly LorePlural EnumOptionRemovalWarning = new(
                "Removing the option {0} will clear the set value in <b>{1}</b>.",
                "Removing the options {0} will clear the set value in <b>{1}</b>.");
            #endregion

            #region Dialogs
            public static readonly string FieldDialogTitle = "Field: {0}";
            public static readonly string FieldTypeDialogTitle = "Field Type: {0}";
            public static readonly string TagDialogTitle = "Tag: {0}";

            // {0} current type, {1} new type, {2} Entry count ("3 Entries")
            public static readonly string FieldTypeChangeWarning =
                "Changing field type <b>{0}</b> to <b>{1}</b> will clear the set field value in <b>{2}</b>.";

            // Enums → Enum, Entries → Entry: {0} current type, {1} new type, {2} Entry count ("3 Entries")
            public static readonly string FieldTypeChangeToSingleWarning =
                "Changing field type <b>{0}</b> to <b>{1}</b> keeps single values, but clears the field value in " +
                "<b>{2}</b> with several values.";

            public static readonly string FieldDeleteUnusedWarning =
                "This will <b>delete</b> the Field <b>permanently</b>. The Field is currently <b>not in use</b>.";

            // {0} Category count ("2 Categories")
            public static readonly string FieldDeleteWarning =
                "Deleting this Field will remove it from <b>{0}</b>.";

            // {0} Category count, {1} Entry count with values
            public static readonly string FieldDeleteWithValuesWarning =
                "Deleting this Field will remove it from <b>{0}</b> and clear values in <b>{1}</b>.";

            public static readonly string TagDeleteUnusedWarning =
                "This will <b>delete</b> the Tag <b>permanently</b>. The Tag is currently <b>not in use</b>.";

            // {0} Entry count ("3 Entries")
            public static readonly string TagDeleteWarning =
                "Deleting this Tag will remove it from <b>{0}</b>.";
            #endregion

            #region Default Libraries (inspector buttons and undo names)
            public static readonly string GenerateDefaultGlyphLibrary = "Generate Default Glyph Library";
            public static readonly string GenerateDefaultColorLibrary = "Generate Default Color Library";
            #endregion

            #region Library Update (dialog after a Lore update with new default Libraries)
            public static readonly string GlyphLibraryName = "Glyph Library";
            public static readonly string ColorLibraryName = "Color Library";

            public static readonly string UpdateDialogTitle = "Lore Library Update";

            // {0} = outdated Libraries joined by JoinList ("Glyph Library and Color Library")
            public static readonly string UpdateDialogMessage =
                "A new Lore version was installed with changes to the {0}. Please decide how to proceed with the " +
                "existing Libraries.\n\n" +
                "Update all libraries: replaces them with the shipped version. This might overwrite changes you " +
                "made to the Libraries.\n\n" +
                "Append only new: adds only new Library entries shipped by Lore.\n\n" +
                "Skip library updates: no changes. You might miss out on new Library entries.\n\n" +
                "No matter what choice you make, already assigned glyphs/colors will not be impacted.";

            public static readonly string UpdateAllButton = "Update all libraries";
            public static readonly string AppendOnlyNewButton = "Append only new";
            public static readonly string SkipUpdatesButton = "Skip library updates";

            public static readonly string UpdateLibraries = "Update Libraries";
            #endregion
        }

        public static class Insights
        {
            #region Card Titles
            public static readonly string CategoriesTitle = "Categories";
            public static readonly string EntriesTitle = "Entries";

            public static readonly string TopInboundEntriesTitle = "Top {0} Inbound Entries";
            public static readonly string TopOutboundEntriesTitle = "Top {0} Outbound Entries";
            public static readonly string LatestModifiedEntriesTitle = "Latest {0} Modified";
            public static readonly string LatestCreatedEntriesTitle = "Latest {0} Created";

            public static readonly string FieldsTitle = "Fields";
            public static readonly string TagsTitle = "Tags";
            public static readonly string ColorsTitle = "Colors";
            public static readonly string GlyphsTitle = "Glyphs";

            public static readonly string CategoryReferencesTitle = "Category References";
            public static readonly string EntryFieldCoverageTitle = "Entry Field Utilization";
            public static readonly string ContentHealthTitle = "Health";

            public static readonly string InboundEntriesTitle = "Inbound Entries";
            public static readonly string OutboundEntriesTitle = "Outbound Entries";
            public static readonly string InboundCategoriesTitle = "Inbound Categories";
            public static readonly string OutboundCategoriesTitle = "Outbound Categories";
            #endregion

            #region Counters
            public static readonly LorePlural FieldCount = new("{0} Field", "{0} Fields");
            public static readonly LorePlural TagCount = new("{0} Tag", "{0} Tags");
            public static readonly LorePlural ColorCount = new("{0} Color", "{0} Colors");
            public static readonly LorePlural GlyphCount = new("{0} Glyph", "{0} Glyphs");

            public static readonly string StandaloneCount = "{0} standalone";
            public static readonly string IsolatedCount = "{0} isolated";
            public static readonly string ReferencedCount = "{0} referenced";
            public static readonly string UnreferencedCount = "{0} unreferenced";
            public static readonly string UsedCount = "{0} used";
            public static readonly string UnusedCount = "{0} unused";

            // Content Health card
            public static readonly LorePlural EntriesWithoutDescription = new("{0} Entry without description", "{0} Entries without description");
            public static readonly LorePlural CategoriesWithoutDescription = new("{0} Category without description", "{0} Categories without description");
            public static readonly LorePlural EmptyCategories = new("{0} empty Category", "{0} empty Categories");
            public static readonly LorePlural PlaceholderNames = new("{0} placeholder name", "{0} placeholder names");
            public static readonly LorePlural EntriesWithoutTags = new("{0} Entry without Tags", "{0} Entries without Tags");
            public static readonly LorePlural EntriesMissingRequired = new("{0} Entry missing required values", "{0} Entries missing required values");

            public static readonly LorePlural ReferenceCount = new("{0} reference", "{0} references");
            public static readonly LorePlural TotalReferenceCount = new("{0} total reference", "{0} total references");

            // "(3/5)": populated / total Entries, shown as card metadata after the Field name
            public static readonly string FieldCoverage = "({0}/{1})";
            #endregion

            #region Empty States
            public static readonly string NoEntryRelationships = "No Entry relationships.";
            public static readonly string NoEntries = "No Entries yet.";

            // Latest Modified/Created cards: date shown dimmed after the Entry title (.NET custom date format).
            public static readonly string LatestEntryDateFormat = "MMM d, HH:mm";
            public static readonly string NoInboundCategories = "Not referenced by any Categories.";
            public static readonly string NoOutboundCategories = "Does not reference any Categories.";
            public static readonly string NoInboundEntries = "Not referenced by any Entries.";
            public static readonly string NoOutboundEntries = "Does not reference any Entries.";
            #endregion

            #region Filters (Content browser filter card labels)
            public static readonly string BrokenEntryReferencesFilter = "Broken Entry References";
            public static readonly string MissingUnityReferencesFilter = "Missing Unity References";
            public static readonly string BrokenFieldDefinitionsFilter = "Broken Field Definitions";
            public static readonly string MissingObjectTypesFilter = "Missing Object Types";
            public static readonly string BrokenDescriptionLinksFilter = "Broken Description Links";
            public static readonly string StandaloneCategoriesFilter = "Standalone Categories";
            public static readonly string IsolatedCategoriesFilter = "Isolated Categories";
            public static readonly string UnreferencedEntriesFilter = "Unreferenced Entries";
            public static readonly string IsolatedEntriesFilter = "Isolated Entries";
            public static readonly string EntriesWithoutDescriptionFilter = "Entries without Description";
            public static readonly string CategoriesWithoutDescriptionFilter = "Categories without Description";
            public static readonly string EmptyCategoriesFilter = "Empty Categories";
            public static readonly string PlaceholderNamesFilter = "Placeholder Names";
            public static readonly string EntriesWithoutTagsFilter = "Entries without Tags";
            public static readonly string EntriesMissingRequiredFilter = "Entries missing required values";

            // "Source → Target: Field"
            public static readonly string CategoryRelationFieldFilter = "{0} → {1}: {2}";
            // "Category: Missing Field"
            public static readonly string MissingFieldFilter = "{0}: Missing {1}";
            // "Unreferenced: Category"
            public static readonly string UnreferencedInCategoryFilter = "Unreferenced: {0}";
            // "Isolated: Category"
            public static readonly string IsolatedInCategoryFilter = "Isolated: {0}";
            #endregion

            #region Integrity Callout
            // {0} = list of issue groups (clickable). Verb agrees with the total issue count.
            public static readonly LorePlural Integrity = new(
                "<b>Integrity</b>: Lore contains {0} that needs to get fixed.",
                "<b>Integrity</b>: Lore contains {0} that need to get fixed.");

            // Project page groups (the whole group is one link).
            public static readonly LorePlural BrokenEntryReferences = new("{0} broken Entry reference", "{0} broken Entry references");
            public static readonly LorePlural BrokenFieldDefinitions = new("{0} broken Field definition", "{0} broken Field definitions");
            public static readonly LorePlural MissingUnityReferences = new("{0} missing Unity reference", "{0} missing Unity references");
            public static readonly LorePlural MissingObjectTypes = new("{0} Field with a missing type", "{0} Fields with missing types");

            // All pages: [[links]] in descriptions whose Entry does not exist (Project: filter; own description: scroll up to it).
            public static readonly LorePlural BrokenDescriptionLinks = new("{0} broken description link", "{0} broken description links");

            // Entry/Category groups: {0} = list of clickable field names.
            public static readonly LorePlural BrokenEntryReferencesIn = new("broken Entry reference in {0}", "broken Entry references in {0}");
            public static readonly LorePlural BrokenFieldDefinitionsAt = new("broken Field definition {0}", "broken Field definitions {0}");
            public static readonly LorePlural MissingUnityReferencesIn = new("missing Unity reference in {0}", "missing Unity references in {0}");
            public static readonly LorePlural MissingObjectTypesIn = new("missing type in {0}", "missing types in {0}");

            // Category page: {0} = list of clickable "Field #n".
            public static readonly string BrokenFields = "broken {0}";

            // Link label for a field by position: "Field #2"
            public static readonly string FieldNumber = "Field #{0}";
            #endregion
        }
    }
}
