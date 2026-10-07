using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Types for ScriptableObject Fields: which classes can be chosen, their script assets and display names.
    public static class ScriptableObjectTypeService
    {
        #region Fields
        // Assemblies of Unity and Lore itself: their ScriptableObjects are not the user's own data types.
        private static readonly string[] ExcludedAssemblyPrefixes =
        {
            "Unity.",
            "UnityEngine.",
            "UnityEditor.",
            "PebblePouch.Lore"
        };

        // Rebuilt after every domain reload (static state is reset on recompile).
        private static List<Type> _candidates;
        #endregion

        #region Public API
        // ScriptableObject classes from the project's player assemblies (Assembly-CSharp, own asmdefs, third-party
        // packages), abstract base classes included; no editor-only, test, Unity or Lore assemblies; sorted by name.
        public static IReadOnlyList<Type> GetCandidates()
        {
            if (_candidates != null)
                return _candidates;

            var playerAssemblies =
                new HashSet<string>(
                    CompilationPipeline
                        .GetAssemblies(
                            AssembliesType.PlayerWithoutTestAssemblies)
                        .Select(
                            assembly => assembly.name));

            _candidates =
                TypeCache
                    .GetTypesDerivedFrom<ScriptableObject>()
                    .Where(
                        type =>
                            !type.ContainsGenericParameters &&
                            IsUserAssembly(
                                type.Assembly.GetName().Name,
                                playerAssemblies))
                    .OrderBy(
                        type => type.Name,
                        StringComparer.OrdinalIgnoreCase)
                    .ThenBy(
                        type => type.Namespace,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            return _candidates;
        }

        // GUID of the script asset that defines the type; empty if it has none (e.g. types in precompiled DLLs).
        public static string FindScriptGuid(Type type)
        {
            if (type == null)
                return string.Empty;

            foreach (MonoScript script in MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (script == null ||
                    script.GetClass() != type)
                {
                    continue;
                }

                return AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(
                        script));
            }

            return string.Empty;
        }

        // Type no longer resolves (class renamed together with its file, namespace changed, moved to another
        // assembly): follows the stored script GUID to the class and stores its new name. Exact match (same script
        // asset), so it runs silently. Derived data: SetDirty only, Modified stays. Returns true if repaired.
        public static bool RepairTypeName(Field field)
        {
            if (field == null ||
                field.Type != FieldType.ScriptableObject ||
                field.ObjectType != null ||
                string.IsNullOrEmpty(field.ObjectScriptGuid))
            {
                return false;
            }

            MonoScript script =
                AssetDatabase.LoadAssetAtPath<MonoScript>(
                    AssetDatabase.GUIDToAssetPath(
                        field.ObjectScriptGuid));

            Type type =
                script != null
                    ? script.GetClass()
                    : null;

            if (type == null ||
                !typeof(ScriptableObject).IsAssignableFrom(type))
            {
                return false;
            }

            if (!field.SetObjectTypeName(
                    Field.GetTypeName(
                        type)))
            {
                return false;
            }

            EditorUtility.SetDirty(field);

            return true;
        }

        // "DialogDefinition" from a stored type name ("MyGame.DialogDefinition, Assembly-CSharp"),
        // also when the type no longer exists.
        public static string GetShortName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return string.Empty;

            int assemblySeparator =
                typeName.IndexOf(',');

            string fullName =
                assemblySeparator >= 0
                    ? typeName.Substring(0, assemblySeparator)
                    : typeName;

            int namespaceSeparator =
                Math.Max(
                    fullName.LastIndexOf('.'),
                    fullName.LastIndexOf('+'));

            return fullName
                .Substring(namespaceSeparator + 1)
                .Trim();
        }

        // Field type for display: "Text", or "DialogDefinition (SO)" ("ScriptableObject" while no type is set).
        public static string GetFieldTypeLabel(Field field)
        {
            return field != null
                ? GetFieldTypeLabel(
                    field.Type,
                    field.ObjectTypeName)
                : string.Empty;
        }

        public static string GetFieldTypeLabel(
            FieldType type,
            string objectTypeName)
        {
            string label =
                LoreText.FieldTypes.Get(
                    type);

            if (type != FieldType.ScriptableObject ||
                string.IsNullOrEmpty(objectTypeName))
            {
                return label;
            }

            return string.Format(
                LoreText.FieldTypes.WithObjectType,
                GetShortName(
                    objectTypeName),
                LoreText.FieldTypes.ScriptableObjectSuffix);
        }

        // Type label with the full name as tooltip (for narrow type columns that clip the label):
        // "DialogDefinition (SO)" / tooltip "MyGame.DialogDefinition (SO)".
        public static GUIContent GetFieldTypeContent(Field field)
        {
            string label =
                GetFieldTypeLabel(
                    field);

            if (field == null ||
                field.Type != FieldType.ScriptableObject ||
                string.IsNullOrEmpty(field.ObjectTypeName))
            {
                return new GUIContent(
                    label,
                    label);
            }

            string typeName = field.ObjectTypeName;

            int assemblySeparator =
                typeName.IndexOf(',');

            string fullName =
                assemblySeparator >= 0
                    ? typeName.Substring(0, assemblySeparator).Trim()
                    : typeName;

            return new GUIContent(
                label,
                string.Format(
                    LoreText.FieldTypes.WithObjectType,
                    fullName,
                    LoreText.FieldTypes.ScriptableObjectSuffix));
        }

        // Dropdown item: "DialogDefinition  (MyGame.Dialog, abstract)".
        public static string GetCandidateLabel(Type type)
        {
            var details = new List<string>();

            if (!string.IsNullOrEmpty(type.Namespace))
                details.Add(type.Namespace);

            if (type.IsAbstract)
                details.Add(LoreText.Library.ObjectTypeAbstract);

            return details.Count == 0
                ? type.Name
                : string.Format(
                    LoreText.Library.ObjectTypeDetails,
                    type.Name,
                    string.Join(
                        LoreText.Library.ObjectTypeDetailSeparator,
                        details));
        }
        #endregion

        #region Helpers
        private static bool IsUserAssembly(
            string assemblyName,
            HashSet<string> playerAssemblies)
        {
            if (!playerAssemblies.Contains(assemblyName))
                return false;

            foreach (string prefix in ExcludedAssemblyPrefixes)
            {
                if (assemblyName.StartsWith(
                        prefix,
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
