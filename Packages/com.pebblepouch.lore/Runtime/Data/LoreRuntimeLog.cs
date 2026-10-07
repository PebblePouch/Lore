using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore
{
    // Warnings of the runtime API (missing Field, wrong type). Editor and development builds only,
    // each message once per session, so per-frame lookups do not flood the console.
    internal static class LoreRuntimeLog
    {
        #region Fields
        private static readonly HashSet<string> Logged = new();
        #endregion

        #region Public API
        public static void WarnOnce(
            string message,
            Object context = null)
        {
            if (!Debug.isDebugBuild)
                return;

            if (!Logged.Add(message))
                return;

            Debug.LogWarning(
                $"[Lore] {message}",
                context);
        }
        #endregion

        #region Unity
        // Clears the "once" memory per Play session also with Enter Play Mode Options (no domain reload).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetLogged() => Logged.Clear();
        #endregion
    }
}
