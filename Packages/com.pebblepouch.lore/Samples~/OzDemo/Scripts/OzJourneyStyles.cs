using UnityEngine;

namespace PebblePouch.Lore.Samples.OzDemo
{
    // Colors, GUIStyles and generated textures of OzJourneyView. Styles need GUI.skin, so they are created on the
    // first OnGUI call (Ensure), never in a static constructor.
    internal static class OzJourneyStyles
    {
        #region Fields
        public static readonly Color Panel = new(0.17f, 0.22f, 0.19f, 0.96f);
        public static readonly Color MapGround = new(0.22f, 0.30f, 0.22f);
        public static readonly Color Text = new(0.93f, 0.93f, 0.88f);
        public static readonly Color Dim = new(0.65f, 0.70f, 0.65f);
        public static readonly Color Accent = new(0.98f, 0.82f, 0.25f);   // Yellow brick
        public static readonly Color Link = new(0.55f, 0.85f, 1f);
        public static readonly Color Danger = new(0.90f, 0.30f, 0.25f);
        public static readonly Color Changed = new(1f, 0.55f, 0.15f);
        public static readonly Color Completed = new(0.35f, 0.80f, 0.40f);
        public static readonly Color Upcoming = new(0.45f, 0.50f, 0.45f);
        public static readonly Color HealthBar = new(0.80f, 0.25f, 0.30f);
        public static readonly Color HealthLost = new(0.30f, 0.20f, 0.20f);

        public const string LinkHex = "#8CD9FF";

        private const int DiscSize = 64;

        private static bool _created;
        #endregion

        #region Properties
        public static GUIStyle Title { get; private set; }

        public static GUIStyle Heading { get; private set; }

        public static GUIStyle Label { get; private set; }

        public static GUIStyle Small { get; private set; }

        public static GUIStyle DimLabel { get; private set; }

        public static GUIStyle MapLabel { get; private set; }

        public static GUIStyle LinkButton { get; private set; }

        public static GUIStyle Button { get; private set; }

        public static GUIStyle Chip { get; private set; }

        public static Texture2D White => Texture2D.whiteTexture;

        // A white filled circle; tinted with GUI.color.
        public static Texture2D Disc { get; private set; }
        #endregion

        #region Methods
        public static void Ensure()
        {
            if (_created && Disc != null)
                return;

            _created = true;

            Label = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                richText = true,
                wordWrap = true,
                normal = { textColor = Text },
            };

            Title = new GUIStyle(Label) { fontSize = 20, fontStyle = FontStyle.Bold, wordWrap = false };
            Heading = new GUIStyle(Label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = Accent } };
            Small = new GUIStyle(Label) { fontSize = 11 };
            DimLabel = new GUIStyle(Small) { normal = { textColor = Dim } };

            MapLabel = new GUIStyle(Small)
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
            };

            LinkButton = new GUIStyle(Label)
            {
                wordWrap = false,
                padding = new RectOffset(0, 0, 1, 1),
                normal = { textColor = Link },
                hover = { textColor = Color.white },
            };

            Button = new GUIStyle(GUI.skin.button) { fontSize = 13, richText = true };

            Chip = new GUIStyle(Small)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                padding = new RectOffset(6, 6, 1, 2),
                normal = { textColor = Color.white },
            };

            Disc = CreateDisc();
        }

        // A filled rectangle in the given color.
        public static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;

            GUI.color = color;
            GUI.DrawTexture(rect, White);
            GUI.color = previous;
        }

        public static void DrawDisc(Rect rect, Color color)
        {
            Color previous = GUI.color;

            GUI.color = color;
            GUI.DrawTexture(rect, Disc);
            GUI.color = previous;
        }

        // A Sprite may be part of an atlas: draw only its own rectangle of the texture.
        public static void DrawSprite(Rect rect, Sprite sprite)
        {
            if (sprite == null)
                return;

            Texture2D texture = sprite.texture;
            Rect source = sprite.textureRect;

            GUI.DrawTextureWithTexCoords(
                rect,
                texture,
                new Rect(
                    source.x / texture.width,
                    source.y / texture.height,
                    source.width / texture.width,
                    source.height / texture.height));
        }

        private static Texture2D CreateDisc()
        {
            var texture = new Texture2D(DiscSize, DiscSize, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Bilinear,
            };

            float center = (DiscSize - 1) / 2f;
            float radius = DiscSize / 2f;

            for (int y = 0; y < DiscSize; y++)
            {
                for (int x = 0; x < DiscSize; x++)
                {
                    // One pixel of soft edge.
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - distance);

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();

            return texture;
        }
        #endregion
    }
}
