using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor.Dev
{
    // DEV ONLY (lives in the dev project, never shipped): builds the Lore data of the "Oz Demo" sample.
    // Compiles into PebblePouch.Lore.Editor through the .asmref next to it, so it can use the internal setters.
    //
    // Workflow:
    //   1. Import the "Oz Demo" sample (Package Manager > Lore > Samples).
    //   2. Remove every other Lore Project and Library from Assets (the builder refuses to run otherwise).
    //   3. Tools > Lore > Dev > Build Oz Demo: (re)creates <imported sample>/Lore and the generated assets.
    //   4. Tools > Lore > Dev > Copy Oz Demo to Samples~: copies the result into the package (with .meta files).
    internal static partial class OzDemoBuilder
    {
        #region Fields
        private const string MenuPath = "Tools/Lore/Dev/Build Oz Demo";
        private const string CopyMenuPath = "Tools/Lore/Dev/Copy Oz Demo to Samples~";
        private const string SamplesFolder = "Samples~/OzDemo";
        private const string DialogTitle = "Build Oz Demo";

        private const string DialogueTypeName =
            "PebblePouch.Lore.Samples.OzDemo.OzDialogue, PebblePouch.Lore.Samples.OzDemo";

        private const string ScriptsFolder = "/Scripts/";
        private const string LoreFolderName = "Lore";
        private const string ProjectAssetName = "Lore.asset";

        private static readonly Dictionary<string, Category> Categories = new();
        private static readonly Dictionary<string, Field> Fields = new();
        private static readonly Dictionary<string, TagDefinition> Tags = new();
        private static readonly Dictionary<string, Entry> Entries = new();

        private static GlyphLibraryCollection _glyphs;
        #endregion

        #region Menu
        [MenuItem(MenuPath)]
        private static void Build()
        {
            if (!TryGetSampleRoot(out string sampleRoot))
                return;

            string loreRoot = sampleRoot + "/" + LoreFolderName;

            if (!CheckNoOtherLoreData(loreRoot))
                return;

            if (AssetDatabase.IsValidFolder(loreRoot))
            {
                if (!EditorUtility.DisplayDialog(
                        DialogTitle,
                        $"{loreRoot} already exists. Delete it (and the generated art, prefabs, scenes and " +
                        "dialogues) and build again?",
                        "Rebuild",
                        "Cancel"))
                {
                    return;
                }

                AssetDatabase.DeleteAsset(loreRoot);
            }

            DeleteGeneratedAssets(sampleRoot);

            Categories.Clear();
            Fields.Clear();
            Tags.Clear();
            Entries.Clear();
            _glyphs = null;

            try
            {
                CreateProject(loreRoot);
                CreateLibraries();
                CreateTags();
                CreateSchema();

                CreateEntries();
                CreateAssets(sampleRoot);
                FillEntries();
                LinkProjectDescription();

                Debug.Log($"[Lore Dev] Oz Demo built in {loreRoot}.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                // Built from scratch: nothing to undo.
                LoreUndo.ForgetAll();

                LoreRefreshService.Refresh();
            }
        }

        // Built data (Lore + generated assets, with .meta files = stable GUIDs) → Packages/…/Samples~/OzDemo.
        // Scripts are not copied: Samples~ is their source (edit them there and import the sample again).
        [MenuItem(CopyMenuPath)]
        private static void CopyToSamples()
        {
            if (!TryGetSampleRoot(out string sampleRoot))
                return;

            // Unsaved Lore edits made after the build go into the sample too.
            LoreRefreshService.SaveLoreAssets();

            string target = System.IO.Path.Combine(LorePackage.ResolvedPath, SamplesFolder);

            if (!System.IO.Directory.Exists(target))
            {
                Debug.LogError($"[Lore Dev] {target} not found.");

                return;
            }

            foreach (string folder in new[]
                     {
                         LoreFolderName, ArtFolder, PrefabsFolder, DialoguesFolder, LocationScenesFolder,
                     })
            {
                string source = System.IO.Path.Combine(sampleRoot, folder);
                string destination = System.IO.Path.Combine(target, folder);

                if (System.IO.Directory.Exists(destination))
                    FileUtil.DeleteFileOrDirectory(destination);

                if (System.IO.File.Exists(destination + ".meta"))
                    FileUtil.DeleteFileOrDirectory(destination + ".meta");

                if (!System.IO.Directory.Exists(source))
                    continue;

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destination) ?? target);

                FileUtil.CopyFileOrDirectory(source, destination);

                if (System.IO.File.Exists(source + ".meta"))
                    FileUtil.CopyFileOrDirectory(source + ".meta", destination + ".meta");
            }

            // Parent folder of the location scenes.
            string scenesMeta = System.IO.Path.Combine(sampleRoot, "Scenes.meta");
            string scenesMetaTarget = System.IO.Path.Combine(target, "Scenes.meta");

            if (System.IO.File.Exists(scenesMeta) && !System.IO.File.Exists(scenesMetaTarget))
                FileUtil.CopyFileOrDirectory(scenesMeta, scenesMetaTarget);

            Debug.Log($"[Lore Dev] Oz Demo copied to {target}.");
        }
        #endregion

        #region Checks
        // Imported sample folder = parent of the folder holding the OzDialogue script.
        private static bool TryGetSampleRoot(out string sampleRoot)
        {
            sampleRoot = null;

            Type dialogueType =
                Type.GetType(
                    DialogueTypeName,
                    false);

            string scriptPath =
                dialogueType != null
                    ? AssetDatabase.GUIDToAssetPath(
                        ScriptableObjectTypeService.FindScriptGuid(
                            dialogueType))
                    : string.Empty;

            int scriptsIndex = scriptPath.LastIndexOf(ScriptsFolder, StringComparison.Ordinal);

            if (scriptsIndex < 0)
            {
                EditorUtility.DisplayDialog(
                    DialogTitle,
                    "Import the \"Oz Demo\" sample first (Package Manager > Lore > Samples).",
                    "OK");

                return false;
            }

            sampleRoot = scriptPath.Substring(0, scriptsIndex);

            return true;
        }

        // The demo must not pick up another Project or Library (Lore finds them by type anywhere under Assets).
        private static bool CheckNoOtherLoreData(string loreRoot)
        {
            var others = new List<string>();

            foreach (string typeName in new[]
                     {
                         nameof(LoreProject),
                         nameof(Category),
                         nameof(Field),
                         nameof(Entry),
                         nameof(TagCollection),
                         nameof(GlyphLibraryCollection),
                         nameof(ColorLibraryCollection),
                     })
            {
                others.AddRange(
                    AssetDatabase.FindAssets($"t:{typeName}", new[] { "Assets" })
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Where(path => !path.StartsWith(loreRoot + "/", StringComparison.Ordinal)));
            }

            if (others.Count == 0)
                return true;

            string list = string.Join("\n", others.Distinct().Take(10));

            EditorUtility.DisplayDialog(
                DialogTitle,
                $"Remove the other Lore data first ({others.Count} assets), e.g.:\n\n{list}",
                "OK");

            return false;
        }
        #endregion

        #region Project
        private static void CreateProject(string loreRoot)
        {
            FolderService.EnsureFolderPath(loreRoot);

            var project = ScriptableObject.CreateInstance<LoreProject>();

            project.name = "Lore";

            project.SetTitle("The Wonderful Wizard of Oz");

            project.SetDescription(
                "A demo for Lore: the world of L. Frank Baum's novel \"The Wonderful Wizard of Oz\" (1900, public " +
                "domain), set up as the lore of a small adventure game.\n\n" +
                "Browse the Categories on the left, open the Insights below, and follow the links in the " +
                "descriptions. Some Entries are deliberately unfinished or broken, so Health and Integrity have " +
                "something to show.");

            project.AddLink(
                new ProjectLink(
                    "The book on Project Gutenberg",
                    "https://www.gutenberg.org/ebooks/55"));

            project.AddLink(
                new ProjectLink(
                    "The book on Wikipedia",
                    "https://en.wikipedia.org/wiki/The_Wonderful_Wizard_of_Oz"));

            LoreChangeTracker.MarkCreated(project);

            AssetDatabase.CreateAsset(
                project,
                loreRoot + "/" + ProjectAssetName);

            // The data root follows the Project asset: from now on every service writes into the demo folder.
            FolderService.InvalidateRoot();

            LoreRefreshService.Refresh();

            if (FolderService.Root != loreRoot)
                throw new InvalidOperationException($"Data root is {FolderService.Root}, expected {loreRoot}.");
        }

        private static void CreateLibraries()
        {
            new ColorLibraryService().GetCollection();
            new TagLibraryService().GetCollection();

            _glyphs = new GlyphLibraryService().GetCollection();
        }

        private static void CreateTags()
        {
            CreateTag("Witch", new Color(0.50f, 0.25f, 0.70f));
            CreateTag("Companion", new Color(0.20f, 0.60f, 0.30f));
            CreateTag("Animal", new Color(0.60f, 0.40f, 0.20f));
            CreateTag("Magic", new Color(0.20f, 0.55f, 0.80f));
            CreateTag("Ruler", new Color(0.85f, 0.65f, 0.10f));
            CreateTag("Kansas", new Color(0.55f, 0.55f, 0.55f));
            CreateTag("Munchkin", new Color(0.25f, 0.40f, 0.85f));
            CreateTag("Danger", new Color(0.80f, 0.20f, 0.20f));
            CreateTag("Gift", new Color(0.90f, 0.45f, 0.65f));
            CreateTag("Tool", new Color(0.45f, 0.50f, 0.45f));
        }
        #endregion

        #region Schema
        // Categories first (Entry/Entries Fields restrict to them), then their Fields in display order.
        private static void CreateSchema()
        {
            CreateCategory(
                "Characters",
                "Mage",
                new Color(0.18f, 0.45f, 0.75f),
                "Everyone Dorothy meets on her way through Oz, friend or foe.");

            CreateCategory(
                "Places",
                "Castle",
                new Color(0.15f, 0.55f, 0.30f),
                "Locations of the game, from the gray Kansas prairie to the Emerald City. Every Place belongs to " +
                "one region of Oz, and every region has its own color.");

            CreateCategory(
                "Items",
                "Backpack",
                new Color(0.80f, 0.60f, 0.10f),
                "Things the player can find, earn or be given.");

            CreateCategory(
                "Quests",
                "Scroll",
                new Color(0.70f, 0.25f, 0.20f),
                "The main story, one Quest per part of the book. Quests chain through their Next Quest Field.");

            // Characters
            CreateField("Characters", "Kind", FieldType.Enum, "Silhouette",
                Options("Human", "Witch", "Animal", "Enchanted", "Creature"));
            CreateField("Characters", "Home", FieldType.Entry, "House", Allow("Places"));
            CreateField("Characters", "Wish", FieldType.Text, "Thought Bubble");
            CreateField("Characters", "Allies", FieldType.Entries, "Shake", Allow("Characters"));
            CreateField("Characters", "Alive", FieldType.Boolean, "Heart");
            CreateField("Characters", "Health", FieldType.Integer, "Bandage");
            CreateField("Characters", "Height", FieldType.Float, "Up");
            CreateField("Characters", "Portrait", FieldType.Sprite, "Picture");
            CreateField("Characters", "Prefab", FieldType.GameObject, "Package");
            CreateField("Characters", "Dialogue", FieldType.ScriptableObject, "Speech Bubble", ObjectType(DialogueTypeName));
            CreateField("Characters", "Wiki", FieldType.Url, "Link");

            // Places
            CreateField("Places", "Region", FieldType.Enum, "Compass",
                Options("Kansas", "Munchkin Country", "Emerald City", "Winkie Country", "Quadling Country", "Gillikin Country"));
            CreateField("Places", "Ruler", FieldType.Entry, "Crown", Allow("Characters"));
            CreateField("Places", "Inhabitants", FieldType.Entries, "Group", Allow("Characters"));
            CreateField("Places", "Dangerous", FieldType.Boolean, "Warning");
            CreateField("Places", "Scene", FieldType.Scene, "Clapperboard");

            // Items (Prefab is shared with Characters)
            CreateField("Items", "Rarity", FieldType.Enum, "Gem", Options("Common", "Rare", "Magical", "Unique"));
            CreateField("Items", "Owner", FieldType.Entry, "Receive", Allow("Characters"));
            CreateField("Items", "Location", FieldType.Entry, "Round Map Pin", Allow("Places"));
            CreateField("Items", "Value", FieldType.Integer, "Coin");
            CreateField("Items", "Icon", FieldType.Sprite, "Artist Palette");
            CreateField("Items", "Prefab", FieldType.GameObject, "Package");

            // Quests (Location is shared with Items)
            CreateField("Quests", "Chapter", FieldType.Integer, "Open Book");
            CreateField("Quests", "Quest Giver", FieldType.Entry, "Speaking Head", Allow("Characters"));
            CreateField("Quests", "Party", FieldType.Entries, "Footprints", Allow("Characters"));
            CreateField("Quests", "Location", FieldType.Entry, "Round Map Pin", Allow("Places"));
            CreateField("Quests", "Rewards", FieldType.Entries, "Wrapped Gift", Allow("Items"));
            CreateField("Quests", "Next Quest", FieldType.Entry, "Right", Allow("Quests"));
            CreateField("Quests", "Main Quest", FieldType.Boolean, "Star");

            SetRequired("Characters", "Kind", "Home");
            SetRequired("Places", "Region");
            SetRequired("Items", "Rarity");
            SetRequired("Quests", "Quest Giver");
        }
        #endregion

        #region Helpers
        private static string Glyph(string title)
        {
            GlyphDefinition glyph =
                _glyphs.Glyphs.FirstOrDefault(
                    definition => definition.Title == title);

            if (glyph == null)
                throw new InvalidOperationException($"Glyph '{title}' is not in the Glyph Library.");

            return glyph.Glyph;
        }

        private static void CreateTag(string title, Color color)
        {
            var service = new TagLibraryService();

            TagDefinition tag = service.CreateTag(title);

            service.UpdateColor(tag, color);

            Tags[title] = tag;
        }

        private static void CreateCategory(
            string name,
            string glyphTitle,
            Color color,
            string description)
        {
            var service = new CategoryService();

            Category category = service.CreateCategory(name);

            if (category == null || category.name != name)
                throw new InvalidOperationException($"Could not create Category '{name}'.");

            service.UpdateGlyph(category, Glyph(glyphTitle));
            service.UpdateColor(category, color);
            service.UpdateDescription(category, description);

            Categories[name] = category;
        }

        // A Field name used again adds the existing Field (shared Fields); glyph and configure apply only once.
        private static void CreateField(
            string categoryName,
            string name,
            FieldType type,
            string glyphTitle,
            Action<Field> configure = null)
        {
            var service = new FieldService();

            Category category = Categories[categoryName];

            if (Fields.TryGetValue(name, out Field shared))
            {
                if (shared.Type != type)
                    throw new InvalidOperationException($"Shared Field '{name}' has another type.");

                service.AddExistingField(category, shared);

                return;
            }

            Field field = service.CreateField(category, name, type, Glyph(glyphTitle));

            if (field == null)
                throw new InvalidOperationException($"Could not create Field '{name}'.");

            if (configure != null)
            {
                configure(field);

                LoreChangeTracker.MarkModified(field);
            }

            Fields[name] = field;
        }

        private static Action<Field> Options(params string[] labels)
        {
            return field =>
            {
                foreach (string label in labels)
                    field.AddEnumOption(label);
            };
        }

        private static Action<Field> Allow(params string[] categoryNames)
        {
            return field => field.SetAllowedCategories(categoryNames.Select(name => Categories[name]));
        }

        private static Action<Field> ObjectType(string typeName)
        {
            return field =>
            {
                Type type = Type.GetType(typeName, true);

                field.SetObjectType(
                    Field.GetTypeName(type),
                    ScriptableObjectTypeService.FindScriptGuid(type));
            };
        }

        private static void SetRequired(string categoryName, params string[] fieldNames)
        {
            var service = new CategoryService();

            foreach (string fieldName in fieldNames)
                service.SetFieldRequired(Categories[categoryName], Fields[fieldName], true);
        }
        #endregion
    }
}
