using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Tools > Lore > Export / Import Project: the Lore data as a .unitypackage (assets with their .meta files and
    // GUIDs, so all references survive). Export = data root + Libraries + generated Field constants (wherever they
    // live); import = Unity's own import window. ProjectSettings (LoreSettings) cannot be part of a package.
    public static class ProjectPackageService
    {
        #region Constants
        public const string PackageExtension = "unitypackage";
        private const string DefaultPackageName = "Lore";
        #endregion

        #region Menu
        [MenuItem(LoreMenu.ExportProject, false, LoreMenu.ExportProjectPriority)]
        private static void ExportFromMenu()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                LoreText.ProjectPackage.ExportDialogTitle,
                LoreText.ProjectPackage.ExportDialogMessage,
                LoreText.ProjectPackage.LoreDataOnlyButton,
                LoreText.Common.CancelButton,
                LoreText.ProjectPackage.WithReferencedAssetsButton);

            // 0 = first button, 1 = cancel, 2 = alternative.
            if (choice == 1)
                return;

            string path = EditorUtility.SaveFilePanel(
                LoreText.ProjectPackage.SaveTitle,
                string.Empty,
                GetDefaultFileName(),
                PackageExtension);

            if (string.IsNullOrEmpty(path))
                return;

            Export(path, includeReferencedAssets: choice == 2);
        }

        [MenuItem(LoreMenu.ImportProject, false, LoreMenu.ImportProjectPriority)]
        private static void ImportFromMenu() => Import();
        #endregion

        #region Public API
        // Asks for a package and opens Unity's import window (also the Welcome page's "Import Unity Package").
        public static void Import()
        {
            string path = EditorUtility.OpenFilePanel(
                LoreText.ProjectPackage.OpenTitle,
                string.Empty,
                PackageExtension);

            if (string.IsNullOrEmpty(path))
                return;

            if (FolderService.FindAsset<LoreProject>() != null &&
                !EditorUtility.DisplayDialog(
                    LoreText.ProjectPackage.ImportWarningTitle,
                    LoreText.ProjectPackage.ImportWarningMessage,
                    LoreText.ProjectPackage.ImportAnywayButton,
                    LoreText.Common.CancelButton))
            {
                return;
            }

            // Unity's import window: shows every file and lets the user deselect some.
            AssetDatabase.ImportPackage(path, true);
        }

        // One of several Projects (Multiple Projects window): only its own folder, Libraries inside it included.
        // Returns false when the export failed.
        public static bool ExportFolder(
            string folder,
            string path)
        {
            LoreRefreshService.SaveLoreAssets();

            try
            {
                AssetDatabase.ExportPackage(folder, path, ExportPackageOptions.Recurse);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Lore: Could not export {folder} to {path}: {exception.Message}");

                return false;
            }

            Debug.Log($"Lore: Exported {folder} to {path}.");

            return File.Exists(path);
        }

        public static void Export(
            string path,
            bool includeReferencedAssets)
        {
            // Unsaved Lore changes go into the package too.
            LoreRefreshService.SaveLoreAssets();

            string[] assetPaths = GetAssetPaths();

            ExportPackageOptions options = ExportPackageOptions.Recurse;

            if (includeReferencedAssets)
                options |= ExportPackageOptions.IncludeDependencies;

            try
            {
                AssetDatabase.ExportPackage(assetPaths, path, options);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Lore: Could not export the project to {path}: {exception.Message}");

                return;
            }

            Debug.Log(
                $"Lore: Exported the project to {path} " +
                $"({(includeReferencedAssets ? "with referenced assets" : "Lore data only")}).");

            EditorUtility.RevealInFinder(path);
        }
        #endregion

        #region Helpers
        // Data root, plus Libraries and the generated Field constants when they live outside it.
        private static string[] GetAssetPaths()
        {
            string root = FolderService.Root;

            List<string> paths = new() { root };

            AddOutsideRoot(paths, root, GetPath(FolderService.FindLibraryAsset<TagCollection>()));
            AddOutsideRoot(paths, root, GetPath(FolderService.FindLibraryAsset<GlyphLibraryCollection>()));
            AddOutsideRoot(paths, root, GetPath(FolderService.FindLibraryAsset<ColorLibraryCollection>()));

            string constantsPath = LoreSettings.instance.LastFieldConstantsPath;

            if (File.Exists(constantsPath))
                AddOutsideRoot(paths, root, constantsPath);

            return paths.ToArray();
        }

        private static string GetPath(Object asset) =>
            asset != null
                ? AssetDatabase.GetAssetPath(asset)
                : string.Empty;

        private static void AddOutsideRoot(
            List<string> paths,
            string root,
            string path)
        {
            if (string.IsNullOrEmpty(path) ||
                path.StartsWith(root + "/") ||
                paths.Contains(path))
            {
                return;
            }

            paths.Add(path);
        }

        // "<Project title>.unitypackage" without characters file names cannot have.
        private static string GetDefaultFileName()
        {
            LoreProject project = LoreRepository.Instance.Project;

            return GetFileName(project != null ? project.Title : null);
        }

        // "<title>.unitypackage" (default name for an empty title).
        public static string GetFileName(string title)
        {
            title = !string.IsNullOrWhiteSpace(title)
                ? title.Trim()
                : DefaultPackageName;

            char[] invalid = Path.GetInvalidFileNameChars();

            title = new string(title.Where(character => !invalid.Contains(character)).ToArray()).Trim();

            return (title.Length > 0 ? title : DefaultPackageName) + "." + PackageExtension;
        }
        #endregion
    }
}
