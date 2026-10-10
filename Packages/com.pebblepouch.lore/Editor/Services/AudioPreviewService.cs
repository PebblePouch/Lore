using System;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Plays AudioClips in the editor (Audio Field preview button), one at a time.
    // Unity has no public API for this; the long-standing internal UnityEditor.AudioUtil (PlayPreviewClip,
    // StopAllPreviewClips, IsPreviewClipPlaying) is called via reflection. If a Unity update renames it,
    // IsAvailable is false and callers hide the button.
    public static class AudioPreviewService
    {
        #region Fields
        private static readonly Type AudioUtilType =
            typeof(EditorGUI).Assembly.GetType(
                "UnityEditor.AudioUtil");

        private static readonly MethodInfo PlayMethod = FindMethod("PlayPreviewClip", typeof(AudioClip));
        private static readonly MethodInfo StopAllMethod = FindMethod("StopAllPreviewClips");
        private static readonly MethodInfo IsPlayingMethod = FindMethod("IsPreviewClipPlaying");

        private static AudioClip _playingClip;
        #endregion

        #region Properties
        public static bool IsAvailable =>
            PlayMethod != null &&
            StopAllMethod != null &&
            IsPlayingMethod != null;
        #endregion

        #region Public API
        public static bool IsPlaying(AudioClip clip)
        {
            return clip != null &&
                   clip == _playingClip;
        }

        public static void Play(AudioClip clip)
        {
            if (!IsAvailable ||
                clip == null)
            {
                return;
            }

            Stop();

            // PlayPreviewClip(clip, startSample = 0, loop = false): later parameters get their defaults.
            ParameterInfo[] parameters = PlayMethod.GetParameters();

            object[] arguments = new object[parameters.Length];

            arguments[0] = clip;

            for (int i = 1; i < parameters.Length; i++)
            {
                arguments[i] =
                    parameters[i].HasDefaultValue
                        ? parameters[i].DefaultValue
                        : GetDefault(parameters[i].ParameterType);
            }

            if (!TryInvoke(
                    PlayMethod,
                    arguments,
                    out _))
            {
                return;
            }

            _playingClip = clip;

            // Switches the button back to Play when the clip ends.
            EditorApplication.update -= WatchPlayback;
            EditorApplication.update += WatchPlayback;
        }

        public static void Stop()
        {
            if (!IsAvailable)
                return;

            TryInvoke(
                StopAllMethod,
                Array.Empty<object>(),
                out _);

            EndPlayback();
        }
        #endregion

        #region Helpers
        // firstParameter: the type of the first parameter; null = a method without parameters.
        private static MethodInfo FindMethod(
            string name,
            Type firstParameter = null)
        {
            if (AudioUtilType == null)
                return null;

            return AudioUtilType
                .GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static)
                .FirstOrDefault(method =>
                {
                    if (method.Name != name)
                        return false;

                    ParameterInfo[] parameters = method.GetParameters();

                    return firstParameter == null
                        ? parameters.Length == 0
                        : parameters.Length > 0 && parameters[0].ParameterType == firstParameter;
                });
        }

        private static bool TryInvoke(
            MethodInfo method,
            object[] arguments,
            out object result)
        {
            result = null;

            try
            {
                result = method.Invoke(null, arguments);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static object GetDefault(Type type)
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        private static void WatchPlayback()
        {
            if (_playingClip != null &&
                TryInvoke(
                    IsPlayingMethod,
                    Array.Empty<object>(),
                    out object playing) &&
                playing is true)
            {
                return;
            }

            EndPlayback();
        }

        private static void EndPlayback()
        {
            EditorApplication.update -= WatchPlayback;

            if (_playingClip == null)
                return;

            _playingClip = null;

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}
