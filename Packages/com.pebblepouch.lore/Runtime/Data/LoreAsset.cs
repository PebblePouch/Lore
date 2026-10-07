using UnityEngine;

namespace PebblePouch.Lore
{
    public abstract class LoreAsset : ScriptableObject
    {
        #region Fields
        [SerializeField, HideInInspector]
        private string _id;

        // UTC ticks (DateTime cannot be serialized). Set by the editor (LoreChangeTracker), never 0 in a project:
        // created once on creation; modified on every change except reordering.
        [SerializeField, HideInInspector]
        private long _createdTicks;

        [SerializeField, HideInInspector]
        private long _modifiedTicks;
        #endregion

        #region Properties
        public string Id => _id;

        public System.DateTime Created =>
            new(_createdTicks, System.DateTimeKind.Utc);

        public System.DateTime Modified =>
            new(_modifiedTicks, System.DateTimeKind.Utc);
        #endregion

        #region Public API
        internal void RegenerateId()
        {
            _id =
                System.Guid
                    .NewGuid()
                    .ToString();
        }

        // New or duplicated asset: created = modified = now.
        internal void InitializeTimestamps(long ticks)
        {
            _createdTicks = ticks;
            _modifiedTicks = ticks;
        }

        // Existing asset without timestamps: fills only the missing ones. Returns true if something was set.
        internal bool EnsureTimestamps(long ticks)
        {
            bool changed = false;

            if (_createdTicks <= 0)
            {
                _createdTicks = ticks;
                changed = true;
            }

            if (_modifiedTicks <= 0)
            {
                _modifiedTicks = ticks;
                changed = true;
            }

            return changed;
        }

        internal void MarkModified(long ticks)
        {
            EnsureTimestamps(ticks);

            _modifiedTicks = ticks;
        }
        #endregion

        #region Unity
        protected virtual void OnEnable() => EnsureId();

#if UNITY_EDITOR
        protected virtual void OnValidate() => EnsureId();
#endif
        #endregion

        #region Helpers
        private void EnsureId()
        {
            if (!string.IsNullOrEmpty(_id))
            {
                return;
            }

            RegenerateId();
        }
        #endregion
    }
}