using System.Collections.Generic;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class GUITextureUtility
    {
        private static readonly Dictionary<Color, Texture2D> _cache = new();

        public static Texture2D GetTexture(Color color)
        {
            if (_cache.TryGetValue(color, out Texture2D texture))
                return texture;

            texture = new Texture2D(1, 1);

            texture.SetPixel(0, 0, color);
            texture.Apply();

            texture.hideFlags = HideFlags.HideAndDontSave;

            _cache.Add(color, texture);

            return texture;
        }
    }
}