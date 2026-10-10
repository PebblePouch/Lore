using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Types for ScriptableObject and Object Fields: which classes can be chosen, their script assets and display
    // names.
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

        // Asset types with a Field type of their own: Object Fields leave them out.
        private static readonly Type[] OwnFieldTypes =
        {
            typeof(GameObject),
            typeof(Sprite),
            typeof(AudioClip)
        };

        // Rebuilt after every domain reload (static state is reset on recompile).
        private static List<Type> _candidates;

        private static List<Type> _objectCandidates;
        #endregion

        #region Public API
        // Field types that store a type per Field (Field.ObjectType).
        public static bool HasObjectType(FieldType type) =>
            type == FieldType.ScriptableObject ||
            type == FieldType.Object;

        // Types that can be chosen for the Field type: ScriptableObject classes or Object types.
        public static IReadOnlyList<Type> GetCandidates(FieldType type) =>
            type == FieldType.Object
                ? GetObjectCandidates()
                : GetCandidates();

        // True when the stored type name (Field.GetTypeName form) resolves to a candidate of the Field type.
        public static bool IsCandidate(
            FieldType type,
            string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return false;

            Type resolved =
                Type.GetType(
                    typeName,
                    false);

            return resolved != null &&
                   GetCandidates(type).Contains(resolved);
        }

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

        // Asset types for Object Fields: Unity's own types (Texture2D, Material, abstract bases like Texture,
        // components like Rigidbody, which pick prefabs) and the project's other classes (e.g. MonoBehaviours), from
        // engine modules and player assemblies only; sorted by name. Left out: UnityEngine.Object itself, obsolete and
        // non-public Unity types, types with a Field type of their own (GameObject, Sprite, AudioClip) and the classes
        // of ScriptableObject Fields.
        public static IReadOnlyList<Type> GetObjectCandidates()
        {
            if (_objectCandidates != null)
                return _objectCandidates;

            var playerAssemblies =
                new HashSet<string>(
                    CompilationPipeline
                        .GetAssemblies(
                            AssembliesType.PlayerWithoutTestAssemblies)
                        .Select(
                            assembly => assembly.name));

            var scriptableObjectTypes =
                new HashSet<Type>(
                    GetCandidates());

            _objectCandidates =
                TypeCache
                    .GetTypesDerivedFrom<UnityEngine.Object>()
                    .Where(
                        type =>
                            !type.ContainsGenericParameters &&
                            !scriptableObjectTypes.Contains(type) &&
                            !IsOwnFieldType(type) &&
                            !type.IsDefined(typeof(ObsoleteAttribute), false) &&
                            IsObjectAssembly(
                                type,
                                playerAssemblies))
                    .OrderBy(
                        type => type.Name,
                        StringComparer.OrdinalIgnoreCase)
                    .ThenBy(
                        type => type.Namespace,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            return _objectCandidates;
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
        // asset), so it runs silently. Types without a script (Unity's own, e.g. moved to another engine module in a
        // newer Unity): the only Object candidate with the same full name. Derived data: SetDirty only, Modified
        // stays. Returns true if repaired.
        public static bool RepairTypeName(Field field)
        {
            if (field == null ||
                !HasObjectType(field.Type) ||
                field.ObjectType != null ||
                string.IsNullOrEmpty(field.ObjectTypeName))
            {
                return false;
            }

            Type type;

            if (!string.IsNullOrEmpty(field.ObjectScriptGuid))
            {
                MonoScript script =
                    AssetDatabase.LoadAssetAtPath<MonoScript>(
                        AssetDatabase.GUIDToAssetPath(
                            field.ObjectScriptGuid));

                type =
                    script != null
                        ? script.GetClass()
                        : null;
            }
            else if (field.Type == FieldType.Object)
            {
                string fullName =
                    GetFullName(
                        field.ObjectTypeName);

                List<Type> matches =
                    GetObjectCandidates()
                        .Where(candidate => candidate.FullName == fullName)
                        .ToList();

                type =
                    matches.Count == 1
                        ? matches[0]
                        : null;
            }
            else
            {
                return false;
            }

            Type baseType =
                field.Type == FieldType.Object
                    ? typeof(UnityEngine.Object)
                    : typeof(ScriptableObject);

            if (type == null ||
                !baseType.IsAssignableFrom(type))
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

        // Field type for display: "Text", "DialogDefinition (SO)", "Texture2D (OBJ)" ("ScriptableObject" / "Object"
        // while no type is set).
        public static string GetFieldTypeLabel(Field field)
        {
            // Vector Fields show their dimension: "Vector2", "Vector3".
            if (field != null &&
                field.Type == FieldType.Vector)
            {
                return GetVectorLabel(field.VectorDimension);
            }

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

            if (!HasObjectType(type) ||
                string.IsNullOrEmpty(objectTypeName))
            {
                return label;
            }

            return string.Format(
                LoreText.FieldTypes.WithObjectType,
                GetShortName(
                    objectTypeName),
                GetObjectTypeSuffix(
                    type));
        }

        // "(SO)" or "(OBJ)".
        public static string GetObjectTypeSuffix(FieldType type) =>
            type == FieldType.Object
                ? LoreText.FieldTypes.ObjectSuffix
                : LoreText.FieldTypes.ScriptableObjectSuffix;

        public static string GetVectorLabel(int dimension) =>
            dimension == 2
                ? LoreText.FieldTypes.Vector2
                : LoreText.FieldTypes.Vector3;

        // Type label with the full name as tooltip (for narrow type columns that clip the label):
        // "DialogDefinition (SO)" / tooltip "MyGame.DialogDefinition (SO)".
        public static GUIContent GetFieldTypeContent(Field field)
        {
            string label =
                GetFieldTypeLabel(
                    field);

            if (field == null ||
                !HasObjectType(field.Type) ||
                string.IsNullOrEmpty(field.ObjectTypeName))
            {
                return new GUIContent(
                    label,
                    label);
            }

            return new GUIContent(
                label,
                string.Format(
                    LoreText.FieldTypes.WithObjectType,
                    GetFullName(
                        field.ObjectTypeName),
                    GetObjectTypeSuffix(
                        field.Type)));
        }

        // Dropdown item: "DialogDefinition  (MyGame.Dialog, abstract)".
        public static string GetCandidateLabel(Type type)
        {
            var details = new List<string>();

            // Plain "UnityEngine" would follow most of Unity's types without telling anything.
            if (!string.IsNullOrEmpty(type.Namespace) &&
                type.Namespace != "UnityEngine")
            {
                details.Add(type.Namespace);
            }

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
        // "MyGame.DialogDefinition" from "MyGame.DialogDefinition, Assembly-CSharp".
        private static string GetFullName(string typeName)
        {
            int assemblySeparator =
                typeName.IndexOf(',');

            return assemblySeparator >= 0
                ? typeName.Substring(0, assemblySeparator).Trim()
                : typeName;
        }

        private static bool IsOwnFieldType(Type type)
        {
            if (type == typeof(UnityEngine.Object))
                return true;

            foreach (Type ownType in OwnFieldTypes)
            {
                if (ownType.IsAssignableFrom(type))
                    return true;
            }

            return false;
        }

        // Unity's engine modules (not part of the compilation pipeline) and player script assemblies. Unity's types
        // only when public; Lore's own types never.
        private static bool IsObjectAssembly(
            Type type,
            HashSet<string> playerAssemblies)
        {
            string assemblyName = type.Assembly.GetName().Name;

            if (assemblyName.StartsWith("PebblePouch.Lore", StringComparison.Ordinal))
                return false;

            bool engineModule =
                assemblyName == "UnityEngine" ||
                assemblyName.StartsWith("UnityEngine.", StringComparison.Ordinal) &&
                assemblyName.EndsWith("Module", StringComparison.Ordinal);

            bool unityAssembly =
                engineModule ||
                assemblyName.StartsWith("UnityEngine.", StringComparison.Ordinal) ||
                assemblyName.StartsWith("Unity.", StringComparison.Ordinal);

            if (unityAssembly &&
                !type.IsVisible)
            {
                return false;
            }

            return engineModule ||
                   playerAssemblies.Contains(assemblyName);
        }

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
