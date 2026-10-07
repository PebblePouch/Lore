using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace PebblePouch.Lore.Editor
{
    // Refreshes and saves (Lore assets only) the derived runtime data before every player build (Project index,
    // scene paths), so a build never ships a stale list of Categories/Entries or outdated scene paths.
    public sealed class LoreBuildPreprocessor : IPreprocessBuildWithReport
    {
        #region Properties
        public int callbackOrder => 0;
        #endregion

        #region IPreprocessBuildWithReport
        public void OnPreprocessBuild(BuildReport report)
        {
            LoreRepository.Instance.Refresh();

            LoreRefreshService.SaveLoreAssets();
        }
        #endregion
    }
}
