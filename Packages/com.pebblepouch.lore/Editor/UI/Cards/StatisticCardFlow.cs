using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class StatisticCardFlow
    {
        #region Public API
        public void Draw(
            IReadOnlyList<StatisticCardItem> items)
        {
            if (items == null ||
                items.Count == 0)
            {
                return;
            }

            for (int i = 0;
                 i < items.Count;
                 i++)
            {
                StatisticCard.Draw(
                    items[i]);

                if (i < items.Count - 1)
                {
                    GUILayout.Space(
                        InsightsStyle.StatisticCardSpacing);
                }
            }
        }
        #endregion
    }
}