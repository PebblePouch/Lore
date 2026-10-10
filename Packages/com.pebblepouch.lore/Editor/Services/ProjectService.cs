using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectService
    {
        #region Create
        // Welcome page "Start your Lore": a new Project in root (e.g. "Assets/Game/Lore") with its folders and
        // Libraries. Returns null when a Project already exists or the asset could not be created.
        public LoreProject CreateProject(string root)
        {
            if (FolderService.FindAsset<LoreProject>() != null)
            {
                Debug.LogError("Lore: This Unity project already has a Lore Project.");

                return null;
            }

            FolderService.EnsureFolderPath(root);

            var project = ScriptableObject.CreateInstance<LoreProject>();

            project.name = "Lore";

            LoreChangeTracker.MarkCreated(project);

            AssetDatabase.CreateAsset(project, root + "/" + FolderService.ProjectAssetName);

            if (!AssetDatabase.Contains(project))
            {
                Debug.LogError($"Failed to create Lore Project in:\n{root}");

                return null;
            }

            // The data root follows the Project asset: folders and Libraries are created next to it.
            FolderService.InvalidateRoot();
            FolderService.EnsureStructure();

            new TagLibraryService().GetCollection();
            new GlyphLibraryService().GetCollection();
            new ColorLibraryService().GetCollection();

            Save();

            return project;
        }
        #endregion

        #region Read
        public string GetDisplayTitle()
        {
            LoreProject project = LoreRepository.Instance.Project;

            if (project != null &&
                !string.IsNullOrWhiteSpace(project.Title))
            {
                return project.Title;
            }

            return PlayerSettings.productName;
        }

        public string GetGlyph()
        {
            LoreProject project = LoreRepository.Instance.Project;

            return project != null
                ? project.Glyph
                : LoreProject.DefaultGlyph;
        }

        public string GetDescription()
        {
            LoreProject project = LoreRepository.Instance.Project;

            return project != null
                ? project.Description
                : string.Empty;
        }
        #endregion

        #region Update
        public bool UpdateTitle(string title)
        {
            title = title?.Trim() ?? string.Empty;

            if (title == GetDisplayTitle())
                return true;

            LoreProject project = GetOrCreateProject();

            if (project == null)
                return false;

            LoreUndo.Record(project, LoreText.Undo.EditTitle);

            project.SetTitle(title);

            Save(project);

            return true;
        }

        public bool UpdateGlyph(string glyph)
        {
            if (glyph == GetGlyph())
                return true;

            LoreProject project = GetOrCreateProject();

            if (project == null)
                return false;

            LoreUndo.Record(project, LoreText.Undo.ChangeGlyph);

            project.SetGlyph(glyph);

            Save(project);

            return true;
        }

        public bool UpdateDescription(string description)
        {
            description ??= string.Empty;

            if (description == GetDescription())
                return true;

            LoreProject project = GetOrCreateProject();

            if (project == null)
                return false;

            LoreUndo.Record(project, LoreText.Undo.EditDescription);

            project.SetDescription(description);

            Save(project);

            return true;
        }
        #endregion

        #region Links
        private static readonly IReadOnlyList<ProjectLink> NoLinks = new List<ProjectLink>();

        public IReadOnlyList<ProjectLink> GetLinks()
        {
            LoreProject project = LoreRepository.Instance.Project;

            return project != null
                ? project.Links
                : NoLinks;
        }

        public ProjectLink AddLink()
        {
            LoreProject project = GetOrCreateProject();

            if (project == null)
                return null;

            var link =
                new ProjectLink(
                    LoreText.Defaults.NewLink,
                    string.Empty);

            LoreUndo.Record(project, LoreText.Undo.AddLink);

            project.AddLink(link);

            Save(project);

            return link;
        }

        public bool UpdateLink(
            ProjectLink link,
            string label,
            string url)
        {
            LoreProject project = LoreRepository.Instance.Project;

            if (project == null ||
                link == null)
            {
                return false;
            }

            LoreUndo.Record(project, LoreText.Undo.EditLink);

            link.SetLabel(label?.Trim());
            link.SetUrl(LoreUrl.Normalize(url));

            Save(project);

            return true;
        }

        public bool RemoveLink(ProjectLink link)
        {
            LoreProject project = LoreRepository.Instance.Project;

            if (project == null ||
                link == null)
            {
                return false;
            }

            project.RemoveLink(link);

            // Confirmed as "remove permanently": older steps must not bring the link back.
            LoreUndo.Forget(project);

            Save(project);

            return true;
        }

        public bool MoveLink(
            ProjectLink link,
            int insertionIndex)
        {
            LoreProject project = LoreRepository.Instance.Project;

            LoreUndo.Record(project, LoreText.Undo.ReorderLinks);

            if (project == null ||
                !project.MoveLink(
                    link,
                    insertionIndex))
            {
                return false;
            }

            Save(
                project,
                isChange: false);

            return true;
        }
        #endregion

        #region Helpers
        private static LoreProject GetOrCreateProject()
        {
            LoreProject project = LoreRepository.Instance.Project;

            if (project != null)
                return project;

            FolderService.EnsureStructure();

            project = ScriptableObject.CreateInstance<LoreProject>();

            project.name = "Lore";

            LoreChangeTracker.MarkCreated(project);

            AssetDatabase.CreateAsset(project, FolderService.ProjectAsset);

            if (!AssetDatabase.Contains(project))
            {
                Debug.LogError($"Failed to create Lore Project at:\n{FolderService.ProjectAsset}");

                return null;
            }

            return project;
        }

        // isChange = false for reordering (not a change for the Modified timestamp).
        private void Save(
            LoreProject project = null,
            bool isChange = true)
        {
            if (project != null)
            {
                if (isChange)
                    LoreChangeTracker.MarkModified(project);
                else
                    EditorUtility.SetDirty(project);
            }

            LoreRefreshService.Refresh();
            LoreRepaintService.Repaint();
        }
        #endregion
    }
}
