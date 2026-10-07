using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class DropLineDrawer
    {
        public static void Draw(
            Rect firstCard,
            Rect lastCard,
            Rect targetCard,
            bool firstPosition,
            bool lastPosition)
        {
            Rect lineRect;

            if (firstPosition)
            {
                lineRect = new Rect(
                    firstCard.x,
                    firstCard.y + BrowserStyle.DropLineVerticalOffset,
                    firstCard.width,
                    BrowserStyle.DropLineHeight);
            }
            else if (lastPosition)
            {
                lineRect = new Rect(
                    lastCard.x,
                    lastCard.yMax +
                    BrowserStyle.DropLineHeight +
                    BrowserStyle.DropLineVerticalOffset,
                    lastCard.width,
                    BrowserStyle.DropLineHeight);
            }
            else
            {
                lineRect = new Rect(
                    targetCard.x,
                    targetCard.y +
                    BrowserStyle.DropLineVerticalOffset,
                    targetCard.width,
                    BrowserStyle.DropLineHeight);
            }

            EditorGUI.DrawRect(
                lineRect,
                BrowserStyle.DropLineColor);
        }
    }
}