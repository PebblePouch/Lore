using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class AssetNameAvailabilityValidator
    {
        public static bool IsAvailable<T>(string folder, string name, Object ignore = null) where T : Object
        {
            string path = $"{folder}/{name}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existing == null)
                return true;

            if (existing == ignore)
                return true;

            return false;
        }
    }
}