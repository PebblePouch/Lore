using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Project page notice (neutral blue) while the generated Field constants no longer match the Fields or settings.
    // Hidden when constants were never generated. Cached per LoreDataVersion.Definitions (reads the file).
    public sealed class FieldConstantsNoticeSection
    {
        #region Fields
        private readonly IntegrityCalloutCard _card = new();
        private readonly List<IntegrityCalloutPiece> _pieces = new();

        private int _version;
        private bool _isOutOfDate;
        #endregion

        #region Public API
        public void Draw()
        {
            int version = LoreDataVersion.Definitions;

            // Only in Layout: Layout and the following events of a frame must agree on whether the card is drawn.
            if (_version != version &&
                Event.current.type == EventType.Layout)
            {
                _version = version;
                _isOutOfDate = FieldConstantsService.IsOutOfDate();
            }

            if (!_isOutOfDate)
                return;

            _pieces.Clear();

            IntegrityCalloutCard.AddTemplate(
                _pieces,
                LoreText.Inspector.FieldConstantsOutOfDate,
                () =>
                    _pieces.Add(
                        new IntegrityCalloutPiece(
                            LoreText.Inspector.FieldConstantsGenerate,
                            Generate)));

            _card.Draw(
                _pieces,
                InsightsStyle.NoticeCard,
                InsightsStyle.NoticeLink);
        }
        #endregion

        #region Helpers
        // After the GUI event: generating imports the script and starts a recompile.
        private static void Generate()
        {
            EditorApplication.delayCall += () =>
            {
                if (FieldConstantsService.GenerateAndLog().Status == FieldConstantsService.Status.Invalid)
                    SettingsService.OpenProjectSettings(LoreSettingsProvider.SettingsPath);
            };
        }
        #endregion
    }
}
