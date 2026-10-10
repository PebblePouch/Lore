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
        private readonly List<LoreProject> _projects = new();

        private bool _fieldTimestampsEnsured;
        #endregion

        #region Properties
        public LoreProject Project { get; private set; }

        // Every Project asset found; more than one opens the Multiple Projects window (Lore uses the first).
        public IReadOnlyList<LoreProject> Projects => _projects;

        public IReadOnlyList<Category> Categories => _categories;
        public IEnumerable<Entry> Entries => _entries;
        public IReadOnlyList<CategoryEntryGroup> Groups => _groups;
        #endregion

        #region Constructor
        private LoreRepository()
        {
            Rebuild();

            // Without a Project nothing is created (the window shows the Welcome page until the user starts one).
            if (Project != null)
                FolderService.EnsureStructure();

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

            SyncAssetPaths();

            LoreDataVersion.Increment();
        }

        // Scene and Folder values also store the asset's path (games cannot read SceneAssets or folder assets): fills
        // it for older values and follows moved/renamed scenes and folders. Derived data, so no Modified timestamp
        // change (SetDirty only).
        private void SyncAssetPaths()
        {
            foreach (Entry entry in _entries)
            {
                bool changed = false;

                foreach (FieldValue fieldValue in entry.Fields)
                {
                    if (fieldValue == null)
                        continue;

                    switch (fieldValue.Type)
                    {
                        case FieldType.Scene:
                            changed |=
                                fieldValue.SetScenePath(
                                    GetAssetPath(fieldValue.SceneValue));
                            break;

                        case FieldType.Folder:
                            changed |=
                                fieldValue.SetFolderPath(
                                    GetAssetPath(fieldValue.FolderValue));
                            break;
                    }
                }

                if (changed)
                    EditorUtility.SetDirty(entry);
            }
        }

        // Empty for no or deleted assets.
        private static string GetAssetPath(UnityEngine.Object asset) =>
            asset != null
                ? AssetDatabase.GetAssetPath(asset)
                : string.Empty;

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
            _projects.Clear();

            string[] assetGuids =
                AssetDatabase.FindAssets(
                    "t:LoreProject");

            foreach (string assetGuid in
                     assetGuids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        assetGuid);

                var project = AssetDatabase.LoadAssetAtPath<LoreProject>(path);

                if (project != null)
                    _projects.Add(project);
            }

            return _projects.Count > 0
                ? _projects[0]
                : null;
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