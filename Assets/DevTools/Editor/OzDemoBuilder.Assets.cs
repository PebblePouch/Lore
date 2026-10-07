using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace PebblePouch.Lore.Editor.Dev
{
    // Entries, their values and the Unity assets the values point to (generated, so the sample needs no art):
    // Portraits/Icons = colored discs, Prefabs = primitives, Scenes = empty scenes, Dialogues = OzDialogue assets.
    internal static partial class OzDemoBuilder
    {
        #region Fields
        private const string ArtFolder = "Art";
        private const string PrefabsFolder = "Prefabs";
        private const string DialoguesFolder = "Dialogues";
        private const string LocationScenesFolder = "Scenes/Locations";

        private const int SpriteSize = 128;
        private const float SpriteBorder = 6f;

        // A scene file needs nothing but the YAML header; Unity fills in the default settings when it is opened.
        private const string EmptySceneYaml = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n";

        private static readonly Dictionary<string, Sprite> Sprites = new();
        private static readonly Dictionary<string, GameObject> Prefabs = new();
        private static readonly Dictionary<string, Object> Scenes = new();
        private static readonly Dictionary<string, ScriptableObject> Dialogues = new();

        private static readonly Dictionary<string, Color> KindColors = new()
        {
            ["Human"] = new Color(0.85f, 0.65f, 0.50f),
            ["Witch"] = new Color(0.50f, 0.25f, 0.70f),
            ["Animal"] = new Color(0.65f, 0.45f, 0.25f),
            ["Enchanted"] = new Color(0.35f, 0.65f, 0.35f),
            ["Creature"] = new Color(0.45f, 0.45f, 0.55f),
        };

        private static readonly Dictionary<string, Color> RarityColors = new()
        {
            ["Common"] = new Color(0.60f, 0.60f, 0.60f),
            ["Rare"] = new Color(0.25f, 0.45f, 0.85f),
            ["Magical"] = new Color(0.60f, 0.30f, 0.80f),
            ["Unique"] = new Color(0.90f, 0.70f, 0.15f),
        };

        // Lines from the book (public domain).
        private static readonly Dictionary<string, (string Speaker, string Text)[]> DialogueLines = new()
        {
            ["No Place Like Home"] = new[]
            {
                ("Scarecrow",
                    "I cannot understand why you should wish to leave this beautiful country and go back to the " +
                    "dry, gray place you call Kansas."),
                ("Dorothy", "That is because you have no brains."),
                ("Dorothy",
                    "No matter how dreary and gray our homes are, we people of flesh and blood would rather live " +
                    "there than in any other country, be it ever so beautiful. There is no place like home."),
            },
            ["Brains or a Heart"] = new[]
            {
                ("Scarecrow",
                    "I shall ask for brains instead of a heart; for a fool would not know what to do with a heart if " +
                    "he had one."),
                ("Tin Woodman",
                    "I shall take the heart; for brains do not make one happy, and happiness is the best thing in " +
                    "the world."),
            },
            ["The Great and Terrible"] = new[]
            {
                ("Oz", "I am Oz, the Great and Terrible. Who are you, and why do you seek me?"),
                ("Dorothy", "I am Dorothy, the Small and Meek. I have come to you for help."),
            },
            ["Melting"] = new[]
            {
                ("Wicked Witch of the West", "See what you have done! In a minute I shall melt away."),
                ("Dorothy", "I'm very sorry, indeed."),
            },
        };
        #endregion

        #region Entries
        // Assets with title, subtitle, glyph and Tags; values follow once every Entry exists (references, links).
        private static void CreateEntries()
        {
            var service = new EntryService();
            var sortOrders = new Dictionary<string, int>();

            foreach (EntryData data in Content)
            {
                sortOrders.TryGetValue(data.Category, out int sortOrder);
                sortOrders[data.Category] = sortOrder + 1;

                Entry entry = service.CreateEntryAsset(Lookup(Categories, data.Category, "Category"), data.Title, sortOrder);

                if (entry == null || entry.name != data.Title)
                    throw new InvalidOperationException($"Could not create Entry '{data.Title}'.");

                entry.SetSubtitle(data.Subtitle);

                if (!string.IsNullOrEmpty(data.Glyph))
                    entry.SetGlyph(Glyph(data.Glyph));

                foreach (string tag in data.Tags)
                    entry.AddTag(Lookup(Tags, tag, "Tag").Id);

                EditorUtility.SetDirty(entry);

                Entries[data.Title] = entry;
            }

            // Description links resolve titles through the repository.
            LoreRefreshService.Refresh();
        }

        private static void FillEntries()
        {
            foreach (EntryData data in Content)
            {
                Entry entry = Entries[data.Title];

                entry.SetDescription(DescriptionLinkFormat.ToStored(data.Description, null));

                foreach (KeyValuePair<string, object> value in data.Values)
                    SetValue(entry, value.Key, value.Value);

                if (data.Category == "Characters")
                {
                    GetFieldValue(entry, "Portrait").SetSprite(Lookup(Sprites, data.Title, "Sprite"));
                    GetFieldValue(entry, "Prefab").SetGameObject(Lookup(Prefabs, data.Title, "Prefab"));
                }
                else if (data.Category == "Items")
                {
                    GetFieldValue(entry, "Icon").SetSprite(Lookup(Sprites, data.Title, "Sprite"));
                    GetFieldValue(entry, "Prefab").SetGameObject(Lookup(Prefabs, data.Title, "Prefab"));
                }

                LoreChangeTracker.MarkModified(entry);
            }

            LoreRefreshService.Refresh();
        }

        private static void LinkProjectDescription()
        {
            LoreProject project = LoreRepository.Instance.Project;

            project.SetDescription(
                DescriptionLinkFormat.ToStored(
                    project.Description +
                    "\n\nStart with [[Dorothy Gale]], or follow the story from its first Quest, [[The Council with " +
                    "the Munchkins]].",
                    null));

            LoreChangeTracker.MarkModified(project);
        }

        private static void SetValue(Entry entry, string fieldName, object value)
        {
            FieldValue fieldValue = GetFieldValue(entry, fieldName);
            Field field = fieldValue.Field;

            switch (field.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                    fieldValue.SetText((string)value);
                    break;

                case FieldType.Enum:
                    EnumOption option =
                        field.EnumOptions.FirstOrDefault(candidate => candidate.Label == (string)value) ??
                        throw new InvalidOperationException($"Field '{field.Title}' has no option '{value}'.");

                    fieldValue.SetEnum(option.Id);
                    break;

                case FieldType.Entry:
                    fieldValue.SetEntry(Lookup(Entries, (string)value, "Entry"));
                    break;

                case FieldType.Entries:
                    fieldValue.SetEntries(((string[])value).Select(title => Lookup(Entries, title, "Entry")));
                    break;

                case FieldType.Boolean:
                    fieldValue.SetBoolean((bool)value);
                    break;

                case FieldType.Integer:
                    fieldValue.SetInteger((int)value);
                    break;

                case FieldType.Float:
                    fieldValue.SetFloat((float)value);
                    break;

                case FieldType.Scene:
                    Object scene = Lookup(Scenes, (string)value, "Scene");

                    fieldValue.SetScene(scene, AssetDatabase.GetAssetPath(scene));
                    break;

                case FieldType.ScriptableObject:
                    fieldValue.SetObject(Lookup(Dialogues, (string)value, "Dialogue"));
                    break;

                default:
                    throw new InvalidOperationException($"No value conversion for Field '{field.Title}' ({field.Type}).");
            }
        }

        private static FieldValue GetFieldValue(Entry entry, string fieldName)
        {
            Field field = Lookup(Fields, fieldName, "Field");

            return entry.Fields.FirstOrDefault(value => value.Field == field) ??
                   throw new InvalidOperationException($"Entry '{entry.Title}' has no Field '{fieldName}'.");
        }

        private static T Lookup<T>(Dictionary<string, T> items, string key, string kind)
        {
            if (key != null && items.TryGetValue(key, out T item))
                return item;

            throw new InvalidOperationException($"{kind} '{key}' not found.");
        }
        #endregion

        #region Unity Assets
        private static void CreateAssets(string sampleRoot)
        {
            Sprites.Clear();
            Prefabs.Clear();
            Scenes.Clear();
            Dialogues.Clear();

            foreach (EntryData data in Content)
            {
                if (data.Category == "Characters")
                {
                    Sprites[data.Title] =
                        CreateSprite(
                            $"{sampleRoot}/{ArtFolder}/Portraits/{data.Title}.png",
                            Lookup(KindColors, (string)data.Values["Kind"], "Kind color"));

                    // Capsule height 2 = Height Field in meters.
                    Prefabs[data.Title] =
                        CreatePrefab(
                            $"{sampleRoot}/{PrefabsFolder}/Characters/{data.Title}.prefab",
                            PrimitiveType.Capsule,
                            new Vector3(0.5f, (float)data.Values["Height"] / 2f, 0.5f));
                }
                else if (data.Category == "Items")
                {
                    Sprites[data.Title] =
                        CreateSprite(
                            $"{sampleRoot}/{ArtFolder}/Icons/{data.Title}.png",
                            Lookup(RarityColors, (string)data.Values["Rarity"], "Rarity color"));

                    Prefabs[data.Title] =
                        CreatePrefab(
                            $"{sampleRoot}/{PrefabsFolder}/Items/{data.Title}.prefab",
                            PrimitiveType.Cube,
                            Vector3.one * 0.3f);
                }

                if (data.Values.TryGetValue("Scene", out object sceneName) &&
                    !Scenes.ContainsKey((string)sceneName))
                {
                    Scenes[(string)sceneName] =
                        CreateScene(
                            $"{sampleRoot}/{LocationScenesFolder}/{sceneName}.unity");
                }
            }

            foreach (KeyValuePair<string, (string Speaker, string Text)[]> dialogue in DialogueLines)
            {
                Dialogues[dialogue.Key] =
                    CreateDialogue(
                        $"{sampleRoot}/{DialoguesFolder}/{dialogue.Key}.asset",
                        dialogue.Value);
            }
        }

        private static void DeleteGeneratedAssets(string sampleRoot)
        {
            foreach (string folder in new[] { ArtFolder, PrefabsFolder, DialoguesFolder, LocationScenesFolder })
            {
                string path = $"{sampleRoot}/{folder}";

                if (AssetDatabase.IsValidFolder(path))
                    AssetDatabase.DeleteAsset(path);
            }
        }

        // A filled disc with a darker border on transparent ground.
        private static Sprite CreateSprite(string path, Color color)
        {
            EnsureParentFolder(path);

            var texture = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false);

            Color border = Color.Lerp(color, Color.black, 0.4f);
            float center = (SpriteSize - 1) / 2f;
            float radius = SpriteSize / 2f - 1f;

            for (int y = 0; y < SpriteSize; y++)
            {
                for (int x = 0; x < SpriteSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                    texture.SetPixel(
                        x,
                        y,
                        distance > radius
                            ? Color.clear
                            : distance > radius - SpriteBorder
                                ? border
                                : color);
                }
            }

            File.WriteAllBytes(path, texture.EncodeToPNG());

            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path) ??
                   throw new InvalidOperationException($"Could not create sprite {path}.");
        }

        private static GameObject CreatePrefab(string path, PrimitiveType primitive, Vector3 scale)
        {
            EnsureParentFolder(path);

            GameObject instance = GameObject.CreatePrimitive(primitive);

            instance.name = Path.GetFileNameWithoutExtension(path);
            instance.transform.localScale = scale;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);

            Object.DestroyImmediate(instance);

            return prefab != null
                ? prefab
                : throw new InvalidOperationException($"Could not create prefab {path}.");
        }

        private static Object CreateScene(string path)
        {
            EnsureParentFolder(path);

            File.WriteAllText(path, EmptySceneYaml);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            return AssetDatabase.LoadAssetAtPath<SceneAsset>(path) ??
                   throw new InvalidOperationException($"Could not create scene {path}.");
        }

        // OzDialogue lives in the sample assembly (not referenced here): created by type name, filled through
        // its serialized fields.
        private static ScriptableObject CreateDialogue(string path, (string Speaker, string Text)[] lines)
        {
            EnsureParentFolder(path);

            ScriptableObject dialogue = ScriptableObject.CreateInstance(Type.GetType(DialogueTypeName, true));

            AssetDatabase.CreateAsset(dialogue, path);

            var serialized = new SerializedObject(dialogue);

            SerializedProperty list = serialized.FindProperty("_lines");

            list.arraySize = lines.Length;

            for (int i = 0; i < lines.Length; i++)
            {
                SerializedProperty line = list.GetArrayElementAtIndex(i);

                line.FindPropertyRelative("_speaker").stringValue = lines[i].Speaker;
                line.FindPropertyRelative("_text").stringValue = lines[i].Text;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssetIfDirty(dialogue);

            return dialogue;
        }

        private static void EnsureParentFolder(string path)
        {
            FolderService.EnsureFolderPath((Path.GetDirectoryName(path) ?? string.Empty).Replace('\\', '/'));
        }
        #endregion
    }
}
