using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class BrowserNavigationTarget
    {
        #region Properties
        public string Id { get; }
        public Rect Rect { get; }
        public Action Activate { get; }
        public Action NavigateLeft { get; }
        public Action NavigateRight { get; }
        public Action Edit { get; }
        public Action Delete { get; }
        public Action CreateChild { get; }
        #endregion

        #region Constructor
        public BrowserNavigationTarget(
            string id,
            Rect rect,
            Action activate,
            Action navigateLeft = null,
            Action navigateRight = null,
            Action edit = null,
            Action delete = null,
            Action createChild = null)
        {
            Id = id;
            Rect = rect;
            Activate = activate;
            NavigateLeft = navigateLeft;
            NavigateRight = navigateRight;
            Edit = edit;
            Delete = delete;
            CreateChild = createChild;
        }
        #endregion
    }
}