using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Welcome page "Create Example Project": imports the package sample "Oz Demo" (a complete Lore Project) like
    // Package Manager > Lore > Samples > Import. The data root follows its Project asset, so it works wherever
    // Unity puts the sample (Assets/Samples/Lore/<version>/Oz Demo).
    public static class DemoSampleService
    {
        #region Fields
        // Must match the sample's "displayName" and "path" in package.json.
        private const string SampleDisplayName = "Oz Demo";
        private const string SampleFolder = "Samples~/OzDemo";

        private const string SamplesRoot = "Assets/Samples";
        #endregion

        #region Public API
        public static void Import()
        {
            Sample sample =
                Sample.FindByPackage(
                        LorePackage.Name,
                        LorePackage.Version)
                    .FirstOrDefault(candidate => candidate.displayName == SampleDisplayName);

            if (!string.IsNullOrEmpty(sample.resolvedPath))
            {
                if (!sample.Import(Sample.ImportOptions.OverridePreviousImports))
                    ShowNotFound();

                return;
            }

            // Not every package source lists its samples (e.g. embedded packages): copy it the same way.
            ImportByCopy();
        }
        #endregion

        #region Helpers
        private static void ImportByCopy()
        {
            string source = string.IsNullOrEmpty(LorePackage.ResolvedPath)
                ? string.Empty
                : Path.Combine(LorePackage.ResolvedPath, SampleFolder);

            if (!Directory.Exists(source))
            {
                ShowNotFound();

                return;
            }

            string target = $"{SamplesRoot}/{LorePackage.Info.displayName}/{LorePackage.Version}/{SampleDisplayName}";

            if (Directory.Exists(target))
            {
                Debug.LogError($"Lore: {target} already exists. Delete it to import the example Project again.");

                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? SamplesRoot);

            FileUtil.CopyFileOrDirectory(source, target);

            AssetDatabase.ImportAsset(
                target,
                ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ShowNotFound()
        {
            EditorUtility.DisplayDialog(
                LoreText.Welcome.DemoFailedTitle,
                LoreText.Welcome.DemoNotFoundMessage,
                LoreText.Common.OkButton);
        }
        #endregion
    }
}
