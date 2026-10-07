using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Project-wide Lore settings (Project Settings > Lore), stored in ProjectSettings/LoreSettings.asset so the whole
    // team shares them (unlike LorePreferences, which are per machine).
    [FilePath("ProjectSettings/LoreSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class LoreSettings : ScriptableSingleton<LoreSettings>
    {
        #region Constants
        public const string DefaultFieldConstantsClassName = "LoreFields";

        private const string GeneratedFolderName = "Generated";
        private const string AssetsFolderPrefix = "Assets/";
        private const string ScriptExtension = ".cs";
        #endregion

        #region Fields
        // Empty = <data root>/Generated/<class name>.cs
        [SerializeField]
        private string _fieldConstantsPath = string.Empty;

        // Empty = global namespace.
        [SerializeField]
        private string _fieldConstantsNamespace = string.Empty;

        // Empty = DefaultFieldConstantsClassName.
        [SerializeField]
        private string _fieldConstantsClassName = string.Empty;

        // File of the last generation (empty = never generated): the Project page compares it with the current
        // Fields and settings (outdated-constants notice).
        [SerializeField]
        private string _lastFieldConstantsPath = string.Empty;
        #endregion

        #region Properties
        public string FieldConstantsPathSetting => _fieldConstantsPath ?? string.Empty;

        public string FieldConstantsNamespace => (_fieldConstantsNamespace ?? string.Empty).Trim();

        public string FieldConstantsClassNameSetting => _fieldConstantsClassName ?? string.Empty;

        public string FieldConstantsClassName =>
            string.IsNullOrWhiteSpace(_fieldConstantsClassName)
                ? DefaultFieldConstantsClassName
                : _fieldConstantsClassName.Trim();

        public string FieldConstantsPath =>
            string.IsNullOrWhiteSpace(_fieldConstantsPath)
                ? DefaultFieldConstantsPath
                : _fieldConstantsPath.Trim().Replace('\\', '/');

        public string LastFieldConstantsPath => _lastFieldConstantsPath ?? string.Empty;

        public string DefaultFieldConstantsPath =>
            $"{FolderService.Root}/{GeneratedFolderName}/{FieldConstantsClassName}{ScriptExtension}";
        #endregion

        #region Public API
        public void SetFieldConstantsPath(string path) =>
            Set(ref _fieldConstantsPath, path);

        public void SetFieldConstantsNamespace(string value) =>
            Set(ref _fieldConstantsNamespace, value);

        public void SetFieldConstantsClassName(string value) =>
            Set(ref _fieldConstantsClassName, value);

        public void SetLastFieldConstantsPath(string path) =>
            Set(ref _lastFieldConstantsPath, path);

        // First problem of the Field constants settings, or null when they are valid.
        public string GetFieldConstantsIssue()
        {
            string path = FieldConstantsPath;

            if (!path.StartsWith(AssetsFolderPrefix))
                return LoreText.Settings.PathOutsideAssets;

            if (!path.EndsWith(ScriptExtension))
                return LoreText.Settings.PathNotScript;

            if (!CSharpIdentifier.IsValidNamespace(FieldConstantsNamespace))
                return string.Format(LoreText.Settings.InvalidNamespace, FieldConstantsNamespace);

            if (!CSharpIdentifier.IsValid(FieldConstantsClassName))
                return string.Format(LoreText.Settings.InvalidClassName, FieldConstantsClassName);

            return null;
        }
        #endregion

        #region Helpers
        private void Set(
            ref string field,
            string value)
        {
            value ??= string.Empty;

            if (field == value)
                return;

            field = value;

            Save(true);

            // Settings feed cached analyses (outdated-constants notice).
            LoreDataVersion.Increment();

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}
