using UnityEditor;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class FolderService
    {
        // Owns folder structure and management.
        //
        // User data lives outside the Lore package (packages are read-only and replaced on update). The data root is
        // the folder of the Project asset, so users can move their Lore folder anywhere under Assets; without a
        // Project (new project), new data goes to DefaultRoot.
        //   <Root>/Lore.asset, <Root>/Categories, <Root>/Fields, <Root>/Libraries

        #region Fields
        public const string DefaultRoot = "Assets/Lore";

        public const string ProjectAssetName = "Lore.asset";

        // Resolved data root; reset on every repository rebuild (the Project may have been moved).
        private static string _root;
        #endregion

        #region Properties
        public static string Root
        {
            get
            {
                if (string.IsNullOrEmpty(_root) ||
                    !AssetDatabase.IsValidFolder(_root))
                {
                    _root = FindRoot();
                }

                return _root;
            }
        }

        public static string Categories => Root + "/Categories";
        public static string Fields => Root + "/Fields";
        public static string Libraries => Root + "/Libraries";
        public static string ProjectAsset => Root + "/" + ProjectAssetName;
        #endregion

        #region Public API
        public static void InvalidateRoot() => _root = null;

        public static void EnsureStructure()
        {
            EnsureFolderPath(Root);
            EnsureFolder(Root, "Categories");
            EnsureFolder(Root, "Fields");
        }

        public static string GetCategoriesRoot()
        {
            EnsureStructure();

            return Categories;
        }

        public static string GetCategoryFolder(string categoryName)
        {
            EnsureStructure();

            string path = $"{Categories}/{categoryName}";

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(Categories, categoryName);
            }

            return path;
        }

        public static string GetCategoryFolderPath(Category category)
        {
            if (category == null)
                return null;

            return Categories + "/" + category.name;
        }

        public static string GetCategoryFolderPath(string category)
        {
            return Categories + "/" + category;
        }

        public static string GetCategoryEntryPath(string categoryName)
        {
            EnsureStructure();

            return Categories + "/" + categoryName + ".asset";
        }

        public static string GetFieldPath(string fieldName)
        {
            return Fields + "/" + fieldName + ".asset";
        }

        // Path for a new Library asset ("<Root>/Libraries/Tags.asset"); creates the folder.
        public static string GetLibraryPath(string assetName)
        {
            EnsureFolderPath(Libraries);

            return Libraries + "/" + assetName + ".asset";
        }

        // Library asset (Colors, Glyphs, Tags): found by type wherever the user keeps it, else at its default path
        // (also when the type search is not ready yet, e.g. during an import), else created there; without a
        // Project, null (nothing is created). initialize fills a new asset (e.g. default colors) before it is saved.
        public static T GetOrCreateLibraryAsset<T>(
            string assetName,
            System.Action<T> initialize = null)
            where T : ScriptableObject
        {
            T asset =
                FindLibraryAsset<T>();

            if (asset != null)
                return asset;

            // No Project yet (Welcome page): nothing is created until the user starts or imports one.
            if (FindAsset<LoreProject>() == null)
                return null;

            string path =
                GetLibraryPath(
                    assetName);

            asset =
                AssetDatabase.LoadAssetAtPath<T>(
                    path);

            if (asset != null)
                return asset;

            asset =
                ScriptableObject.CreateInstance<T>();

            asset.name = assetName;

            initialize?.Invoke(asset);

            // CreateAsset writes the file; no project-wide SaveAssets.
            AssetDatabase.CreateAsset(
                asset,
                path);

            return asset;
        }

        // Library asset (Tags, Glyphs, Colors): the one under the data root wins (e.g. an imported example Project
        // next to Libraries created earlier elsewhere), else the first one anywhere under Assets.
        public static T FindLibraryAsset<T>()
            where T : Object
        {
            string root = Root;

            if (AssetDatabase.IsValidFolder(root))
            {
                T asset = FindAsset<T>(root);

                if (asset != null)
                    return asset;
            }

            return FindAsset<T>();
        }

        // First asset of this type under folder (default: Assets).
        public static T FindAsset<T>(string folder = "Assets")
            where T : Object
        {
            foreach (string guid in
                     AssetDatabase.FindAssets(
                         $"t:{typeof(T).Name}",
                         new[] { folder }))
            {
                T asset =
                    AssetDatabase.LoadAssetAtPath<T>(
                        AssetDatabase.GUIDToAssetPath(
                            guid));

                if (asset != null)
                    return asset;
            }

            return null;
        }
        #endregion

        #region Helpers
        private static string FindRoot()
        {
            LoreProject project =
                FindAsset<LoreProject>();

            if (project == null)
                return DefaultRoot;

            string path =
                AssetDatabase.GetAssetPath(
                    project);

            int separator =
                path.LastIndexOf('/');

            return separator > 0
                ? path.Substring(0, separator)
                : DefaultRoot;
        }

        // Creates every missing folder of a path like "Assets/Game/Lore".
        public static void EnsureFolderPath(string path)
        {
            string[] parts =
                path.Split('/');

            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                EnsureFolder(
                    current,
                    parts[i]);

                current += "/" + parts[i];
            }
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
        #endregion
    }
}
