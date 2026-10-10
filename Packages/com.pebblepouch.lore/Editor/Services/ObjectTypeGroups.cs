using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Groups of the Object type dropdown by function. Unity's types are matched by base type, namespace, engine module
    // ("UnityEngine.PhysicsModule") or package assembly ("Unity.Timeline"); the first matching group wins, so every
    // type is in exactly one group. Unity types without a group go to General, the project's own types (and
    // third-party packages) to Project.
    public static class ObjectTypeGroups
    {
        #region Types
        public sealed class Group
        {
            public string Label { get; }

            public IReadOnlyList<Type> Types { get; }

            public Group(
                string label,
                IReadOnlyList<Type> types)
            {
                Label = label;
                Types = types;
            }
        }

        private sealed class Rule
        {
            public string Label { get; }

            public Func<Type, bool> Matches { get; }

            public Rule(
                string label,
                Func<Type, bool> matches)
            {
                Label = label;
                Matches = matches;
            }
        }
        #endregion

        #region Fields
        // Match order = display order. Where groups overlap, the earlier one wins: 2D and Effects take their renderers
        // before Rendering, Text takes TextMeshPro before UI.
        private static readonly Rule[] Rules =
        {
            new(LoreText.Library.ObjectTypeGroup2D,
                type =>
                    IsModule(type, "Tilemap", "Grid", "SpriteShape", "SpriteMask") ||
                    IsAssembly(type, "Unity.2D.") ||
                    IsNamespace(type, "UnityEngine.U2D") ||
                    Is(type, typeof(SpriteRenderer))),

            new(LoreText.Library.ObjectTypeGroupAnimation,
                type =>
                    IsModule(type, "Animation", "Director") ||
                    IsAssembly(type, "Unity.Timeline", "Unity.Animation")),

            new(LoreText.Library.ObjectTypeGroupAudioVideo,
                type =>
                    IsModule(type, "Audio", "Video", "DSPGraph")),

            new(LoreText.Library.ObjectTypeGroupEffects,
                type =>
                    IsModule(type, "ParticleSystem", "VFX") ||
                    IsAssembly(type, "Unity.VisualEffectGraph") ||
                    Is(type, typeof(LineRenderer), typeof(TrailRenderer), typeof(Flare), typeof(LensFlare),
                        typeof(Projector))),

            new(LoreText.Library.ObjectTypeGroupInput,
                type =>
                    IsModule(type, "Input", "InputLegacy", "InputForUI") ||
                    IsAssembly(type, "Unity.InputSystem")),

            new(LoreText.Library.ObjectTypeGroupMaterials,
                type =>
                    Is(type, typeof(Material), typeof(Shader), typeof(ComputeShader),
                        typeof(ShaderVariantCollection))),

            new(LoreText.Library.ObjectTypeGroupNavigation,
                type =>
                    IsModule(type, "AI") ||
                    IsAssembly(type, "Unity.AI.Navigation")),

            new(LoreText.Library.ObjectTypeGroupPhysics,
                type =>
                    IsModule(type, "Physics", "Vehicles", "TerrainPhysics", "Cloth")),

            new(LoreText.Library.ObjectTypeGroupPhysics2D,
                type =>
                    IsModule(type, "Physics2D")),

            new(LoreText.Library.ObjectTypeGroupRendering,
                type =>
                    IsAssembly(type, "Unity.RenderPipelines.") ||
                    IsNamespace(type, "UnityEngine.Rendering") ||
                    Is(type, typeof(Renderer), typeof(Mesh), typeof(MeshFilter), typeof(Camera), typeof(Light),
                        typeof(LightProbes), typeof(LightProbeGroup), typeof(ReflectionProbe), typeof(LODGroup),
                        typeof(Skybox), typeof(LightingSettings), typeof(OcclusionArea), typeof(OcclusionPortal))),

            new(LoreText.Library.ObjectTypeGroupTerrain,
                type =>
                    IsModule(type, "Terrain", "Wind")),

            new(LoreText.Library.ObjectTypeGroupText,
                type =>
                    IsModule(type, "TextRendering", "TextCoreFontEngine", "TextCoreTextEngine") ||
                    IsAssembly(type, "Unity.TextMeshPro") ||
                    Is(type, typeof(TextAsset))),

            new(LoreText.Library.ObjectTypeGroupTextures,
                type =>
                    Is(type, typeof(Texture))),

            new(LoreText.Library.ObjectTypeGroupUI,
                type =>
                    IsModule(type, "UI", "UIElements", "IMGUI") ||
                    IsAssembly(type, "UnityEngine.UI") ||
                    Is(type, typeof(RectTransform)))
        };

        // Rebuilt after every domain reload, like the candidates.
        private static List<Group> _groups;
        #endregion

        #region Public API
        // Project first, then the function groups, General last; empty groups are left out. Types keep the candidate
        // order (by name).
        public static IReadOnlyList<Group> GetGroups()
        {
            if (_groups != null)
                return _groups;

            var project = new List<Type>();
            var general = new List<Type>();

            var grouped =
                Rules.ToDictionary(
                    rule => rule,
                    _ => new List<Type>());

            foreach (Type type in ScriptableObjectTypeService.GetObjectCandidates())
            {
                if (!IsUnityType(type))
                {
                    project.Add(type);

                    continue;
                }

                Rule rule =
                    Rules.FirstOrDefault(
                        candidate => candidate.Matches(type));

                if (rule != null)
                    grouped[rule].Add(type);
                else
                    general.Add(type);
            }

            _groups = new List<Group>();

            Add(LoreText.Library.ObjectTypeGroupProject, project);

            foreach (Rule rule in Rules)
                Add(rule.Label, grouped[rule]);

            Add(LoreText.Library.ObjectTypeGroupGeneral, general);

            return _groups;
        }
        #endregion

        #region Helpers
        private static void Add(
            string label,
            List<Type> types)
        {
            if (types.Count > 0)
                _groups.Add(new Group(label, types));
        }

        private static string GetAssemblyName(Type type) => type.Assembly.GetName().Name;

        private static bool IsUnityType(Type type)
        {
            string assemblyName = GetAssemblyName(type);

            return assemblyName == "UnityEngine" ||
                   assemblyName.StartsWith("UnityEngine.", StringComparison.Ordinal) ||
                   assemblyName.StartsWith("Unity.", StringComparison.Ordinal);
        }

        // Engine module by short name: "Physics" = UnityEngine.PhysicsModule.
        private static bool IsModule(
            Type type,
            params string[] modules)
        {
            string assemblyName = GetAssemblyName(type);

            return modules.Any(module => assemblyName == $"UnityEngine.{module}Module");
        }

        // Package assembly: exact name or prefix ("Unity.2D." covers all 2D package assemblies).
        private static bool IsAssembly(
            Type type,
            params string[] prefixes)
        {
            string assemblyName = GetAssemblyName(type);

            return prefixes.Any(
                prefix =>
                    prefix.EndsWith(".")
                        ? assemblyName.StartsWith(prefix, StringComparison.Ordinal)
                        : assemblyName == prefix ||
                          assemblyName.StartsWith(prefix + ".", StringComparison.Ordinal));
        }

        private static bool IsNamespace(
            Type type,
            string ns)
        {
            return type.Namespace != null &&
                   (type.Namespace == ns ||
                    type.Namespace.StartsWith(ns + ".", StringComparison.Ordinal));
        }

        private static bool Is(
            Type type,
            params Type[] baseTypes)
        {
            return baseTypes.Any(baseType => baseType.IsAssignableFrom(type));
        }
        #endregion
    }
}
