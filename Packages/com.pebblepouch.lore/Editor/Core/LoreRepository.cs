using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LoreRepository
    {
        #region Singleton
        private static LoreRepository _instance;

        public static LoreRepository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LoreRepository();

                return _instance;
            }
        }

        // False until Lore is used in this session (nothing to keep in sync before that).
        public static bool HasInstance => _instance != null;
        #endregion

        #region Fields
        private readonly Dictionary<string, Entry> _entriesById = new();
        private readonly List<Category> _categories = new();
        private readonly List<Entry> _entries = new();
        private readonly List<CategoryEntryGroup> _groups = new();

        private bool _fieldTimestampsEnsured;
        #endregion

        #region Properties
        public LoreProject Project { get; private set; }

        public IReadOnlyList<Category> Categories => _categories;
        public IEnumerable<Entry> Entries => _entries;
        public IReadOnlyList<CategoryEntryGroup> Groups => _groups;
        #endregion

        #region Constructor
        private LoreRepository()
        {
            FolderService.EnsureStructure();

            Rebuild();

            // No projectChanged subscription: it fires for every import in the game project. Lore asset changes
            // arrive through LoreAssetPostprocessor (LoreRefreshService.RequestExternalRefresh).
        }
        #endregion

        #region Public API
        public void Refresh() => Rebuild();

        public bool TryGetEntry(
            string id,
            out Entry entry)
        {
            entry = null;

            if (string.IsNullOrEmpty(id))
                return false;

            return _entriesById.TryGetValue(
                       id,
                       out entry) &&
                   entry != null;
        }
        #endregion

        #region Helpers
        private void Rebuild()
        {
            // The Project (and with it the data root) may have been moved.
            FolderService.InvalidateRoot();

            Project = FindProject();

            _categories.Clear();
            _categories.AddRange(FindCategories());

            _entries.Clear();
            _entries.AddRange(FindEntries());

            _entriesById.Clear();

            foreach (Entry entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Id))
                    continue;

                if (_entriesById.TryAdd(
                        entry.Id,
                        entry))
                {
                    continue;
                }

                Debug.LogError($"Duplicate Lore ID detected for Entry '{entry.name}'.");
            }

            EnsureTimestamps();

            BuildGroups();

            SyncProjectIndex();

            SyncScenePaths();

            LoreDataVersion.Increment();
        }

        // Scene values also store the scene's path (games cannot read SceneAssets): fills it for older values and
        // follows moved/renamed scenes. Derived data, so no Modified timestamp change (SetDirty only).
        private void SyncScenePaths()
        {
            foreach (Entry entry in _entries)
            {
                bool changed = false;

                foreach (FieldValue fieldValue in entry.Fields)
                {
                    if (fieldValue == null ||
                        fieldValue.Type != FieldType.Scene)
                    {
                        continue;
                    }

                    string path =
                        fieldValue.SceneValue != null
                            ? AssetDatabase.GetAssetPath(fieldValue.SceneValue)
                            : string.Empty;

                    changed |=
                        fieldValue.SetScenePath(
                            path);
                }

                if (changed)
                    EditorUtility.SetDirty(entry);
            }
        }

        // Runtime index on the Project (what games can reach): Categories in order, Entries grouped like the
        // browser. Not a content change, so the Project's Modified timestamp stays (SetDirty, like reordering).
        private void SyncProjectIndex()
        {
            if (Project == null)
                return;

            var entries =
                _groups
                    .SelectMany(group => group.Entries)
                    .ToList();

            // Entries without a (known) Category are not in any group, but still part of the project.
            var grouped =
                new HashSet<Entry>(
                    entries);

            entries.AddRange(
                _entries.Where(
                    entry => !grouped.Contains(entry)));

            bool changed =
                Project.SetIndex(
                    _categories,
                    entries,
                    new TagLibraryService().GetCollection());

            if (changed)
                EditorUtility.SetDirty(Project);
        }

        // No asset without timestamps: loaded assets on every rebuild (cheap), all Field assets (also unused
        // ones, not loaded here) once per domain load; that pass also repairs renamed ScriptableObject Field types.
        private void EnsureTimestamps()
        {
            var assets = new List<LoreAsset>();

            if (Project != null)
                assets.Add(Project);

            assets.AddRange(_categories);
            assets.AddRange(_entries);

            if (!_fieldTimestampsEnsured)
            {
                _fieldTimestampsEnsured = true;

                foreach (string assetGuid in
                         AssetDatabase.FindAssets(
                             "t:Field"))
                {
                    Field field =
                        AssetDatabase.LoadAssetAtPath<Field>(
                            AssetDatabase.GUIDToAssetPath(
                                assetGuid));

                    assets.Add(field);

                    // Once per domain load = after every recompile, where renamed classes show up.
                    ScriptableObjectTypeService.RepairTypeName(field);
                }
            }

            LoreChangeTracker.EnsureTimestamps(assets);
        }

        private LoreProject FindProject()
        {
            string[] assetGuids =
                AssetDatabase.FindAssets(
                    "t:LoreProject");

            if (assetGuids.Length > 1)
                Debug.LogWarning("Multiple Lore Projects found. Using the first one.");

            foreach (string assetGuid in
                     assetGuids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        assetGuid);

                var project = AssetDatabase.LoadAssetAtPath<LoreProject>(path);

                if (project != null)
                    return project;
            }

            return null;
        }

        private List<Category> FindCategories()
        {
            var result = new List<Category>();

            string[] assetGuids =
                AssetDatabase.FindAssets(
                    "t:Category");

            foreach (string assetGuid in
                     assetGuids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        assetGuid);

                var category = AssetDatabase.LoadAssetAtPath<Category>(path);

                if (category != null)
                    result.Add(category);
            }

            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

            return result;
        }

        private List<Entry> FindEntries()
        {
            var result = new List<Entry>();

            string[] assetGuids =
                AssetDatabase.FindAssets(
                    "t:Entry");

            foreach (string assetGuid in
                     assetGuids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        assetGuid);

                var entry = AssetDatabase.LoadAssetAtPath<Entry>(path);

                if (entry != null)
                    result.Add(entry);
            }

            return result;
        }

        private void BuildGroups()
        {
            _groups.Clear();

            foreach (var category in _categories)
            {
                var entries = _entries
                    .Where(a => a.Category == category)
                    .OrderBy(a => a.SortOrder)
                    .ToList();

                _groups.Add(
                    new CategoryEntryGroup(
                        category,
                        entries));
            }
        }
        #endregion
    }
}