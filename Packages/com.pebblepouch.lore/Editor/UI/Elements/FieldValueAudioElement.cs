using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Audio value: AudioClip field + play button (stop while playing, see AudioPreviewService).
    // Without the preview API the clip field takes the whole width.
    public sealed class FieldValueAudioElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            GetRects(
                rect,
                out Rect clipRect,
                out Rect buttonRect);

            EntryAssignmentContext.CheckInteraction(
                clipRect);

            EditorGUI.BeginChangeCheck();

            AudioClip value =
                (AudioClip)EditorGUI.ObjectField(
                    clipRect,
                    GUIContent.none,
                    _fieldValue.AudioClipValue,
                    typeof(AudioClip),
                    false);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetAudioClip(
                    value);

                Save();
            }

            if (!AudioPreviewService.IsAvailable)
                return;

            AudioClip clip = _fieldValue.AudioClipValue;

            if (AudioPreviewService.IsPlaying(clip))
            {
                ButtonDrawer.Stop(
                    buttonRect,
                    AudioPreviewService.Stop,
                    ButtonDrawer.DisplayMode.Glyph,
                    InspectorStyle.FieldButton);

                return;
            }

            using (new EditorGUI.DisabledScope(clip == null))
            {
                ButtonDrawer.Play(
                    buttonRect,
                    () => AudioPreviewService.Play(clip),
                    ButtonDrawer.DisplayMode.Glyph,
                    InspectorStyle.FieldButton);
            }
        }
        #endregion

        #region Helpers
        private static void GetRects(
            Rect rect,
            out Rect clipRect,
            out Rect buttonRect)
        {
            if (!AudioPreviewService.IsAvailable)
            {
                clipRect = rect;
                buttonRect = Rect.zero;

                return;
            }

            float buttonWidth =
                Mathf.Min(
                    InspectorStyle.FieldButtonColumnWidth,
                    rect.width);

            float clipWidth =
                Mathf.Max(
                    0f,
                    rect.width -
                        buttonWidth -
                        InspectorStyle.FieldValueAudioButtonSpacing);

            clipRect =
                new Rect(
                    rect.x,
                    rect.y,
                    clipWidth,
                    rect.height);

            buttonRect =
                new Rect(
                    rect.xMax - buttonWidth,
                    rect.y,
                    buttonWidth,
                    rect.height);
        }
        #endregion
    }
}
