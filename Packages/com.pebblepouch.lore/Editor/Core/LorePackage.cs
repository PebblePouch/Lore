using System.IO;

using UnityEngine;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace PebblePouch.Lore.Editor
{
    // The installed Lore package (embedded, Git or local): name, version and its folder on disk. Folders with a
    // trailing "~" (Documentation~, Samples~) are not imported by Unity, so their files are read from disk.
    public static class LorePackage
    {
        #region Fields
        private const string DocumentationFolder = "Documentation~";

        private static PackageInfo _info;
        private static bool _resolved;
        #endregion

        #region Properties
        public static PackageInfo Info
        {
            get
            {
                if (!_resolved)
                {
                    _resolved = true;

                    _info = PackageInfo.FindForAssembly(typeof(LorePackage).Assembly);
                }

                return _info;
            }
        }

        public static string Name => Info?.name ?? string.Empty;

        public static string Version => Info?.version ?? string.Empty;

        // Absolute folder of the package; empty when Lore is not installed as a package.
        public static string ResolvedPath => Info?.resolvedPath ?? string.Empty;
        #endregion

        #region Public API
        // Text of a file in Documentation~ (e.g. "Manual.md"); null when it cannot be read.
        public static string ReadDocumentation(string fileName)
        {
            if (string.IsNullOrEmpty(ResolvedPath))
                return null;

            string path = Path.Combine(ResolvedPath, DocumentationFolder, fileName);

            try
            {
                return File.Exists(path)
                    ? File.ReadAllText(path)
                    : null;
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"Lore: Could not read {path}: {exception.Message}");

                return null;
            }
        }
        #endregion
    }
}
