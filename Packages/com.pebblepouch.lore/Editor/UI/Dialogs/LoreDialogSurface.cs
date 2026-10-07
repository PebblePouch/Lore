using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LoreDialogSurface
    {
        #region Public API
        public static void HandleInput(
            Rect windowRect)
        {
            if (!LoreDialog.IsOpen)
                return;

            LoreDialogRequest request =
                LoreDialog.Current;

            if (request == null)
                return;

            Event current =
                Event.current;

            if (current.type ==
                EventType.KeyDown)
            {
                HandleKeyboard(
                    request);

                return;
            }

            if (current.type !=
                EventType.MouseDown)
            {
                return;
            }

            Rect dialogRect =
                GetDialogRect(
                    windowRect,
                    request);

            Rect confirmRect =
                GetConfirmButtonRect(
                    dialogRect,
                    request);

            Rect cancelRect =
                GetCancelButtonRect(
                    dialogRect,
                    request);

            if (confirmRect.Contains(
                    current.mousePosition))
            {
                current.Use();

                LoreDialog.Confirm();

                return;
            }

            if (request.HasCancel &&
                cancelRect.Contains(
                    current.mousePosition))
            {
                current.Use();

                LoreDialog.Cancel();

                return;
            }

            // Every other click is swallowed while
            // the modal surface is active.
            current.Use();
        }

        public static void Draw(
            Rect windowRect)
        {
            if (!LoreDialog.IsOpen)
                return;

            LoreDialogRequest request =
                LoreDialog.Current;

            if (request == null)
                return;

            DrawBlocker(
                windowRect);

            Rect dialogRect =
                GetDialogRect(
                    windowRect,
                    request);

            DrawBorder(
                GetBorderRect(
                    dialogRect));

            DrawBackground(
                dialogRect);

            DrawHeader(
                dialogRect,
                request);

            DrawMessage(
                dialogRect,
                request);

            DrawButtons(
                dialogRect,
                request);
        }
        #endregion

        #region Draw
        private static void DrawBlocker(Rect rect)
        {
            EditorGUI.DrawRect(
                rect,
                DialogStyle.Blocker);
        }

        private static void DrawBorder(Rect rect)
        {
            EditorGUI.DrawRect(
                rect,
                DialogStyle.Border);
        }

        private static void DrawBackground(Rect rect)
        {
            EditorGUI.DrawRect(
                rect,
                DialogStyle.Background);
        }

        private static void DrawHeader(
            Rect dialogRect,
            LoreDialogRequest request)
        {
            Rect headerRect =
                new(
                    dialogRect.x,
                    dialogRect.y,
                    dialogRect.width,
                    DialogStyle.TitleHeight);

            EditorGUI.DrawRect(
                headerRect,
                DialogStyle.HeaderBackground);

            Rect titleRect =
                new(
                    headerRect.x
                        + DialogStyle.EdgeMargin,
                    headerRect.y,
                    headerRect.width
                        - DialogStyle.EdgeMargin * 2f,
                    headerRect.height);

            GUI.Label(
                titleRect,
                request.Title,
                DialogStyle.Title);
        }

        private static void DrawMessage(
            Rect dialogRect,
            LoreDialogRequest request)
        {
            float top =
                dialogRect.y
                + DialogStyle.TitleHeight
                + DialogStyle.EdgeMargin;

            float bottom =
                dialogRect.yMax
                - DialogStyle.EdgeMargin
                - DialogStyle.ButtonHeight
                - DialogStyle.SectionSpacing;

            Rect messageRect =
                new(
                    dialogRect.x
                        + DialogStyle.EdgeMargin,
                    top,
                    dialogRect.width
                        - DialogStyle.EdgeMargin * 2f,
                    Mathf.Max(
                        0f,
                        bottom - top));

            GUI.Label(
                messageRect,
                request.Message,
                DialogStyle.Message);
        }

        private static void DrawButtons(
            Rect dialogRect,
            LoreDialogRequest request)
        {
            Rect confirmRect =
                GetConfirmButtonRect(
                    dialogRect,
                    request);

            DrawButton(
                confirmRect,
                request.ConfirmLabel,
                request.Mode ==
                LoreDialogMode.Destructive);

            if (!request.HasCancel)
                return;

            Rect cancelRect =
                GetCancelButtonRect(
                    dialogRect,
                    request);

            DrawButton(
                cancelRect,
                request.CancelLabel,
                false);
        }

        private static void DrawButton(
            Rect rect,
            string label,
            bool destructive)
        {
            bool hovered =
                LoreHover.Contains(rect);

            Color color;

            if (destructive)
            {
                color =
                    hovered
                        ? DialogStyle.DestructiveButtonHover
                        : DialogStyle.DestructiveButton;
            }
            else
            {
                color =
                    hovered
                        ? DialogStyle.ButtonHover
                        : DialogStyle.ButtonNormal;
            }

            EditorGUI.DrawRect(
                rect,
                color);

            GUI.Label(
                rect,
                label,
                DialogStyle.Button);

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);
        }
        #endregion

        #region Helpers
        private static Rect GetDialogRect(
            Rect windowRect,
            LoreDialogRequest request)
        {
            float height =
                Mathf.Min(
                    Mathf.Max(
                        DialogStyle.Height,
                        GetContentHeight(
                            request)),
                    windowRect.height
                        - DialogStyle.EdgeMargin * 2f);

            return new Rect(
                windowRect.center.x
                    - DialogStyle.Width * 0.5f,
                windowRect.center.y
                    - height * 0.5f,
                DialogStyle.Width,
                height);
        }

        // Title, message (wrapped at the dialog width) and buttons, so long messages are not cut off.
        private static float GetContentHeight(
            LoreDialogRequest request)
        {
            float messageHeight =
                DialogStyle.Message.CalcHeight(
                    new GUIContent(
                        request.Message),
                    DialogStyle.Width
                        - DialogStyle.EdgeMargin * 2f);

            return DialogStyle.TitleHeight
                + DialogStyle.EdgeMargin
                + messageHeight
                + DialogStyle.SectionSpacing
                + DialogStyle.ButtonHeight
                + DialogStyle.EdgeMargin;
        }

        private static Rect GetBorderRect(Rect dialogRect)
        {
            return new Rect(
                dialogRect.x
                    - DialogStyle.BorderWidth,
                dialogRect.y
                    - DialogStyle.BorderWidth,
                dialogRect.width
                    + DialogStyle.BorderWidth * 2f,
                dialogRect.height
                    + DialogStyle.BorderWidth * 2f);
        }

        private static Rect GetConfirmButtonRect(
            Rect dialogRect,
            LoreDialogRequest request)
        {
            float availableWidth =
                dialogRect.width
                - DialogStyle.EdgeMargin * 2f;

            float buttonWidth =
                request.HasCancel
                    ? (availableWidth
                        - DialogStyle.ButtonSpacing) * 0.5f
                    : availableWidth;

            return new Rect(
                dialogRect.x
                    + DialogStyle.EdgeMargin,
                dialogRect.yMax
                    - DialogStyle.EdgeMargin
                    - DialogStyle.ButtonHeight,
                buttonWidth,
                DialogStyle.ButtonHeight);
        }

        private static Rect GetCancelButtonRect(
            Rect dialogRect,
            LoreDialogRequest request)
        {
            Rect confirmRect =
                GetConfirmButtonRect(
                    dialogRect,
                    request);

            return new Rect(
                confirmRect.xMax
                    + DialogStyle.ButtonSpacing,
                confirmRect.y,
                confirmRect.width,
                confirmRect.height);
        }

        private static void HandleKeyboard(
            LoreDialogRequest request)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    Event.current.Use();

                    LoreDialog.Confirm();

                    break;

                case KeyCode.Escape:
                    Event.current.Use();

                    if (request.HasCancel)
                    {
                        LoreDialog.Cancel();
                    }
                    else
                    {
                        LoreDialog.Confirm();
                    }

                    break;

                default:
                    Event.current.Use();
                    break;
            }
        }
        #endregion
    }
}