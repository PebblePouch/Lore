using System;
using System.Reflection;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    // Shared checks on serialized object references (used by the integrity analyzers and repair).
    public static class SerializedReferenceUtility
    {
        #region Fields
        // The stored object ID: objectReferenceEntityIdValue (newer Unity 6 versions, EntityId) replaces
        // objectReferenceInstanceIDValue (int; obsolete as error in newer versions). Read by reflection, so the
        // package compiles on every supported Unity version.
        private static readonly PropertyInfo StoredIdProperty =
            typeof(SerializedProperty).GetProperty("objectReferenceEntityIdValue") ??
            typeof(SerializedProperty).GetProperty("objectReferenceInstanceIDValue");

        private static readonly object EmptyId =
            StoredIdProperty != null &&
            StoredIdProperty.PropertyType.IsValueType
                ? Activator.CreateInstance(StoredIdProperty.PropertyType)
                : null;
        #endregion

        #region Public API
        // A reference is stored (object ID set), but the object cannot be loaded anymore:
        // what Unity shows as "Missing" in object fields.
        public static bool IsMissingReference(
            SerializedProperty property)
        {
            return
                property != null &&
                property.propertyType == SerializedPropertyType.ObjectReference &&
                property.objectReferenceValue == null &&
                HasStoredId(property);
        }
        #endregion

        #region Helpers
        private static bool HasStoredId(SerializedProperty property)
        {
            if (StoredIdProperty == null)
                return false;

            object id =
                StoredIdProperty.GetValue(
                    property);

            return id != null &&
                   !id.Equals(EmptyId);
        }
        #endregion
    }
}
