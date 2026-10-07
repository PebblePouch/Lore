using System;

namespace PebblePouch.Lore.Editor
{
    // One cached result per key (e.g. the shown Entry), recalculated when the key or LoreDataVersion changes.
    // Also keeps Layout and Repaint consistent: both events of a frame see the same result.
    public sealed class LoreCache<TKey, TValue>
        where TKey : class
    {
        #region Fields
        private TKey _key;
        private TValue _value;
        private int _version;
        #endregion

        #region Public API
        public TValue Get(
            TKey key,
            Func<TKey, TValue> calculate)
        {
            int version =
                LoreDataVersion.Current;

            if (_version != version ||
                !ReferenceEquals(_key, key))
            {
                _value = calculate(key);
                _key = key;
                _version = version;
            }

            return _value;
        }
        #endregion
    }
}
