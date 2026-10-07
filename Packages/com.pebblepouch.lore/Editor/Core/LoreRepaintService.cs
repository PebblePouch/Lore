using UnityEditorInternal;

namespace PebblePouch.Lore.Editor
{
    public static class LoreRepaintService
    {
        #region Public API
        public static void Repaint()
        {
            InternalEditorUtility.RepaintAllViews();
        }
        #endregion
    }
}