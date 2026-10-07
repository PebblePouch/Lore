using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public static class NavigationHistoryService
    {
        // Session-only Back/Forward history of selection changes (Project, Category, Entry).

        #region Fields
        private const int Capacity = 50;

        private static readonly List<NavigationLocation> _back = new();
        private static readonly List<NavigationLocation> _forward = new();

        private static NavigationLocation _current;

        private static bool _isNavigating;
        #endregion

        #region Properties
        public static bool CanGoBack => HasTarget(_back);
        public static bool CanGoForward => HasTarget(_forward);
        #endregion

        #region Public API
        public static void Record(
            Category category,
            Entry entry)
        {
            if (_isNavigating)
                return;

            var location =
                new NavigationLocation(
                    category,
                    entry);

            if (location.Matches(_current))
                return;

            if (_current != null &&
                _current.IsValid)
            {
                _back.Add(_current);

                if (_back.Count > Capacity)
                    _back.RemoveAt(0);
            }

            _forward.Clear();

            _current = location;
        }

        // Copy of the history state, for temporary technical selection changes that are not navigation
        // (e.g. a browser rename that selects the item and restores the previous selection afterwards).
        // Restore after the selection is back: the steps recorded in between are dropped.
        public sealed class Snapshot
        {
            internal readonly List<NavigationLocation> Back;
            internal readonly List<NavigationLocation> Forward;
            internal readonly NavigationLocation Current;

            internal Snapshot()
            {
                Back = new List<NavigationLocation>(_back);
                Forward = new List<NavigationLocation>(_forward);
                Current = _current;
            }
        }

        public static Snapshot CreateSnapshot()
        {
            return new Snapshot();
        }

        public static void Restore(Snapshot snapshot)
        {
            if (snapshot == null)
                return;

            _back.Clear();
            _back.AddRange(snapshot.Back);

            _forward.Clear();
            _forward.AddRange(snapshot.Forward);

            _current = snapshot.Current;
        }

        public static bool GoBack()
        {
            return Step(_back, _forward);
        }

        public static bool GoForward()
        {
            return Step(_forward, _back);
        }
        #endregion

        #region Helpers
        private static bool Step(
            List<NavigationLocation> from,
            List<NavigationLocation> to)
        {
            while (from.Count > 0)
            {
                NavigationLocation target = from[from.Count - 1];

                from.RemoveAt(from.Count - 1);

                if (!target.IsValid ||
                    target.Matches(_current))
                {
                    continue;
                }

                if (_current != null &&
                    _current.IsValid)
                {
                    to.Add(_current);
                }

                _current = target;

                Apply(target);

                return true;
            }

            return false;
        }

        private static void Apply(NavigationLocation location)
        {
            _isNavigating = true;

            try
            {
                if (location.Entry != null)
                {
                    SelectionService.SelectEntry(location.Entry);

                    ContentBrowserRequestService.RequestEntryReveal(location.Entry);
                }
                else if (location.Category != null)
                {
                    SelectionService.SelectCategory(location.Category);

                    ContentBrowserRequestService.RequestCategoryReveal(location.Category);
                }
                else
                {
                    SelectionService.SelectProject();
                }
            }
            finally
            {
                _isNavigating = false;
            }

            LoreRepaintService.Repaint();
        }

        private static bool HasTarget(
            List<NavigationLocation> locations)
        {
            foreach (NavigationLocation location in locations)
            {
                if (location.IsValid &&
                    !location.Matches(_current))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
