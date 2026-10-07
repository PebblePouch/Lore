using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class TagDeleteAnalyzer
    {
        #region Public API
        public TagDeleteReport Analyze(
            TagDefinition tag)
        {
            if (tag == null)
            {
                return new TagDeleteReport(
                    null,
                    0);
            }

            int affectedEntryCount =
                LoreRepository.Instance
                    .Entries
                    .Count(
                        entry =>
                            entry.HasTag(
                                tag.Id));

            return new TagDeleteReport(
                tag,
                affectedEntryCount);
        }
        #endregion
    }
}