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

        // Inside Scripts/ so the sample assembly (OzDemo.asmdef) compiles the constants with the demo scripts.
        private const string GeneratedFolder = "Scripts/Generated";
        private const string FieldConstantsNamespace = "PebblePouch.Lore.Samples.OzDemo";
        private const string FieldConstantsClassName = "OzFields";

        private const string DemoScenePath = "Scenes/Oz Demo.unity";
        private const string DemoObjectName = "Oz Demo";
        private const string JourneyTypeName = "PebblePouch.Lore.Samples.OzDemo.OzJourney, PebblePouch.Lore.Samples.OzDemo";
        private const string JourneyViewTypeName = "PebblePouch.Lore.Samples.OzDemo.OzJourneyView, PebblePouch.Lore.Samples.OzDemo";
        private const string JourneyLoreProperty = "_lore";

        private static readonly Color DemoBackground = new(0.12f, 0.16f, 0.14f);

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
                SetProjectGlyph();
                CreateTags();
                CreateSchema();

                CreateEntries();
                CreateAssets(sampleRoot);
                FillEntries();
                LinkProjectDescription();
                GenerateFieldConstants(sampleRoot);
                CreateDemoScene(sampleRoot);

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
                         LoreFolderName, ArtFolder, AudioFolder, PrefabsFolder, DialoguesFolder, LocationScenesFolder,
                         GeneratedFolder, DemoScenePath,
                     })
            {
                string source = System.IO.Path.Combine(sampleRoot, folder);
                string destination = System.IO.Path.Combine(target, folder);

                if (System.IO.Directory.Exists(destination) || System.IO.File.Exists(destination))
                    FileUtil.DeleteFileOrDirectory(destination);

                if (System.IO.File.Exists(destination + ".meta"))
                    FileUtil.DeleteFileOrDirectory(destination + ".meta");

                if (!System.IO.Directory.Exists(source) && !System.IO.File.Exists(source))
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
                "descriptions. Some Entries are **deliberately unfinished or broken**, so Health and Integrity have " +
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

        // The cyclone that carries Dorothy's house to Oz.
        private static void SetProjectGlyph()
        {
            LoreProject project = LoreRepository.Instance.Project;

            project.SetGlyph(Glyph("Tornado"));

            LoreChangeTracker.MarkModified(project);
        }

        // OzFields: keys by Field ID for the demo scripts, independent of the project's own constants settings.
        // Not deleted before a rebuild: the demo scripts need it to compile (rebuilt Fields get new IDs, same names).
        private static void GenerateFieldConstants(string sampleRoot)
        {
            string path = $"{sampleRoot}/{GeneratedFolder}/{FieldConstantsClassName}.cs";

            FolderService.EnsureFolderPath($"{sampleRoot}/{GeneratedFolder}");

            System.IO.File.WriteAllText(
                path,
                FieldConstantsService.BuildSource(FieldConstantsNamespace, FieldConstantsClassName),
                new System.Text.UTF8Encoding(false));

            AssetDatabase.ImportAsset(path);
        }

        // The playable scene: a camera and one GameObject with OzJourney + OzJourneyView, referencing the Project.
        // Built from an empty scene (camera added by hand), so it carries no render-pipeline components and opens
        // in any project. Saved over the previous scene, so its GUID stays stable.
        private static void CreateDemoScene(string sampleRoot)
        {
            Type journeyType = Type.GetType(JourneyTypeName, false);
            Type viewType = Type.GetType(JourneyViewTypeName, false);

            if (journeyType == null || viewType == null)
            {
                Debug.LogWarning("[Lore Dev] OzJourney / OzJourneyView not found (copy the scripts first): scene skipped.");

                return;
            }

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                if (string.IsNullOrEmpty(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).path))
                {
                    Debug.LogWarning("[Lore Dev] Save or close the untitled scene first: Oz Demo scene skipped.");

                    return;
                }
            }

            string path = $"{sampleRoot}/{DemoScenePath}";

            EnsureParentFolder(path);

            UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Additive);

            try
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                var camera = cameraObject.AddComponent<Camera>();

                // Hears the pickup sounds of OzJourney's AudioSource.
                cameraObject.AddComponent<AudioListener>();

                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = DemoBackground;

                var demo = new GameObject(DemoObjectName);

                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(demo, scene);

                Component journey = demo.AddComponent(journeyType);

                demo.AddComponent(viewType);

                var serialized = new SerializedObject(journey);

                serialized.FindProperty(JourneyLoreProperty).objectReferenceValue = LoreRepository.Instance.Project;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path))
                    throw new InvalidOperationException($"Could not save scene {path}.");
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void CreateTags()
        {
            CreateTag("Witch", "#6400C8");
            CreateTag("Companion", "#00A300");
            CreateTag("Animal", "#904800");
            CreateTag("Magic", "#B500B5");
            CreateTag("Ruler", "#D6C503");
            CreateTag("Kansas", "#888888");
            CreateTag("Munchkin", "#0000ED");
            CreateTag("Danger", "#DA0000");
            CreateTag("Gift", "#ED5FA6");
            CreateTag("Tool", "#696969");
            CreateTag("Help", "#00B7B7");
        }
        #endregion

        #region Schema
        // Categories first (Entry/Entries Fields restrict to them), then their Fields in display order.
        private static void CreateSchema()
        {
            CreateCategory(
                "Characters",
                "Mage",
                "#0000DA",
                "Everyone Dorothy meets on her way through Oz, friend or foe.");

            CreateCategory(
                "Places",
                "Castle",
                "#009000",
                "Locations of the game, from the gray Kansas prairie to the Emerald City. Every Place belongs to " +
                "one region of Oz, and every region has its own color and banner.");

            CreateCategory(
                "Items",
                "Backpack",
                "#C6B703",
                "Things the player can find, earn or be given.");

            CreateCategory(
                "Quests",
                "Scroll",
                "#B50000",
                "The main story, one Quest per part of the book. Quests chain through their Next Quest Field.");

            // Help for demo users, no Fields (content in OzDemoBuilder.Content, Category "FAQ").
            CreateCategory(
                "FAQ",
                "Question",
                "#00A7A7",
                "Questions you may have while exploring the demo. Not part of Oz, and also an example that Lore " +
                "works for notes about your project, not only for the world of your game.");

            // Characters
            CreateField("Characters", "Kind", FieldType.Enum, "Silhouette",
                Options("Human", "Witch", "Animal", "Enchanted", "Creature"));
            CreateField("Characters", "Home", FieldType.Entry, "House", Allow("Places"));
            CreateField("Characters", "Location", FieldType.Entry, "Round Map Pin", Allow("Places"));
            CreateField("Characters", "Wish", FieldType.Text, "Thought Bubble");
            CreateField("Characters", "Weaknesses", FieldType.Enums, "Shield",
                Options("Water", "Fire", "Rain", "Poppies", "Darkness"));
            CreateField("Characters", "Allies", FieldType.Entries, "Shake", Allow("Characters"));
            CreateField("Characters", "Alive", FieldType.Boolean, "Heart");
            CreateField("Characters", "Health", FieldType.Integer, "Bandage");
            CreateField("Characters", "Height", FieldType.Float, "Up");
            CreateField("Characters", "Portrait", FieldType.Sprite, "Picture");
            CreateField("Characters", "Prefab", FieldType.GameObject, "Package");
            CreateField("Characters", "Dialogue", FieldType.ScriptableObject, "Speech Bubble", ObjectType(Type.GetType(DialogueTypeName, true)));
            CreateField("Characters", "Wiki", FieldType.Url, "Link");

            // Places
            CreateField("Places", "Region", FieldType.Enum, "Compass",
                Options("Kansas", "Munchkin Country", "Emerald City", "Winkie Country", "Quadling Country", "Gillikin Country"));
            CreateField("Places", "Color", FieldType.Color, "Paintbrush");
            CreateField("Places", "Banner", FieldType.Object, "Triangular Flag", ObjectType(typeof(Texture2D)));
            CreateField("Places", "Map Position", FieldType.Vector, "Map", Dimension(2));
            CreateField("Places", "Ruler", FieldType.Entry, "Crown", Allow("Characters"));
            CreateField("Places", "Inhabitants", FieldType.Entries, "Group", Allow("Characters"));
            CreateField("Places", "Dangerous", FieldType.Boolean, "Warning");
            CreateField("Places", "Scene", FieldType.Scene, "Clapperboard");

            // Items (Location and Prefab are shared with Characters)
            CreateField("Items", "Rarity", FieldType.Enum, "Gem", Options("Common", "Rare", "Magical", "Unique"));
            CreateField("Items", "Owner", FieldType.Entry, "Receive", Allow("Characters"));
            CreateField("Items", "Location", FieldType.Entry, "Round Map Pin", Allow("Places"));
            CreateField("Items", "Value", FieldType.Integer, "Coin");
            CreateField("Items", "Icon", FieldType.Sprite, "Artist Palette");
            CreateField("Items", "Pickup Sound", FieldType.Audio, "Bell");
            CreateField("Items", "Prefab", FieldType.GameObject, "Package");

            // Quests (Location is shared with Characters and Items; Completed is changed by the demo scene at runtime)
            CreateField("Quests", "Chapter", FieldType.Integer, "Open Book");
            CreateField("Quests", "Quest Giver", FieldType.Entry, "Speaking Head", Allow("Characters"));
            CreateField("Quests", "Party", FieldType.Entries, "Footprints", Allow("Characters"));
            CreateField("Quests", "Location", FieldType.Entry, "Round Map Pin", Allow("Places"));
            CreateField("Quests", "Rewards", FieldType.Entries, "Wrapped Gift", Allow("Items"));
            CreateField("Quests", "Next Quest", FieldType.Entry, "Right", Allow("Quests"));
            CreateField("Quests", "Main Quest", FieldType.Boolean, "Star");
            CreateField("Quests", "Completed", FieldType.Boolean, "Check Mark");

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

        private static void CreateTag(string title, string colorTitle)
        {
            var service = new TagLibraryService();

            TagDefinition tag = service.CreateTag(title);

            service.UpdateColor(tag, LibraryColor(colorTitle));

            Tags[title] = tag;
        }

        private static void CreateCategory(
            string name,
            string glyphTitle,
            string colorTitle,
            string description)
        {
            var service = new CategoryService();

            Category category = service.CreateCategory(name);

            if (category == null || category.name != name)
                throw new InvalidOperationException($"Could not create Category '{name}'.");

            service.UpdateGlyph(category, Glyph(glyphTitle));
            service.UpdateColor(category, LibraryColor(colorTitle));
            service.UpdateDescription(category, description);

            Categories[name] = category;
        }

        // Categories and Tags take their colors from the Color Library (like picking in the editor), chosen by title
        // (hex code), so the demo's Colors Insight counts them as used. Nearest-color matching picked cyan for the
        // demo's muted blues and greens: the default Library has only saturated hues.
        private static Color LibraryColor(string title)
        {
            ColorDefinition color =
                new ColorLibraryService()
                    .GetCollection()?
                    .Colors
                    .FirstOrDefault(definition => definition.Title == title);

            if (color == null)
                throw new InvalidOperationException($"Color '{title}' is not in the Color Library.");

            return color.Color;
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

        // Script GUID: empty for Unity's own types (no script asset).
        private static Action<Field> ObjectType(Type type)
        {
            return field =>
            {
                field.SetObjectType(
                    Field.GetTypeName(type),
                    ScriptableObjectTypeService.FindScriptGuid(type));
            };
        }

        private static Action<Field> Dimension(int dimension)
        {
            return field => field.SetVectorDimension(dimension);
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
