using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryReference
    {
        #region Properties
        public Category SourceCategory { get; }
        public Category TargetCategory { get; }

        public int ReferenceCount { get; }
        public IReadOnlyList<CategoryReferenceField> Fields { get; }
        #endregion

        #region Constructor
        public CategoryReference(
            Category sourceCategory,
            Category targetCategory,
            int referenceCount,
            IReadOnlyList<CategoryReferenceField> fields)
        {
            SourceCategory = sourceCategory;
            TargetCategory = targetCategory;
            ReferenceCount = referenceCount;
            Fields = fields;
        }
        #endregion
    }
}