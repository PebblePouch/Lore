using System;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

namespace PebblePouch.Lore
{
    // Runtime changes of field values (Entry.SetInt, …), keyed by Entry ID + Field ID. Never written to the Lore
    // assets: the Entry getters return the changed value, Entry.GetOriginal… the value from Lore.
    // Cleared at the start of every Play session (also without domain reload) and when leaving Play mode.
    public static class LoreState
    {
        #region Constants
        private const int SaveVersion = 1;
        #endregion

        #region Fields
        private static readonly Dictionary<(string EntryId, string FieldId), LoreStateValue> Values = new();

        // Entries currently loaded (registered in Entry.OnEnable), to resolve Entry values loaded from JSON by ID.
        // Not cleared with the values: without domain reload, loaded assets stay enabled across Play sessions.
        private static readonly Dictionary<string, Entry> LoadedEntries = new();
        #endregion

        #region Save Data
        // JSON layout of SaveToJson. Values are text in the invariant culture ("5", "true", "1.5", "1;2;3", colors as
        // "r;g;b;a"); Enum values store the option ID, Entry values the Entry ID, so renames are safe.
        [Serializable]
        private class SaveData
        {
            public int version = SaveVersion;
            public List<SaveRecord> values = new();
        }

        [Serializable]
        private class SaveRecord
        {
            public string entry;
            public string field;
            public string type;
            public string value;
        }
        #endregion

        #region Events
        // A changed value was set or reset to the original. Entry and Field are null when many values changed at
        // once (ResetAllToOriginal, LoadFromJson).
        public static event Action<Entry, Field> Changed;
        #endregion

        #region Properties
        // True while any value differs from the original.
        public static bool HasChanges => Values.Count > 0;
        #endregion

        #region Public API
        public static void ResetAllToOriginal()
        {
            if (Values.Count == 0)
                return;

            Values.Clear();

            Changed?.Invoke(null, null);
        }

        // All changes as JSON, e.g. for a save game; the game decides where to store it. Changes of Entries or
        // Fields that no longer exist are kept, so saving again loses nothing.
        public static string SaveToJson(bool prettyPrint = false)
        {
            var data = new SaveData();

            foreach (var pair in Values)
            {
                data.values.Add(
                    new SaveRecord
                    {
                        entry = pair.Key.EntryId,
                        field = pair.Key.FieldId,
                        type = pair.Value.Type.ToString(),
                        value = pair.Value.ToSaveText(),
                    });
            }

            return JsonUtility.ToJson(data, prettyPrint);
        }

        // Replaces all changes with the ones in the JSON (null or empty = no changes). Invalid JSON returns false
        // and keeps the current changes; unreadable records are skipped (both with a warning). Raises Changed
        // (null, null) unless nothing changed before and after.
        public static bool LoadFromJson(string json)
        {
            var loaded = new Dictionary<(string EntryId, string FieldId), LoreStateValue>();

            if (!string.IsNullOrWhiteSpace(json))
            {
                SaveData data;

                try
                {
                    data = JsonUtility.FromJson<SaveData>(json);
                }
                catch (ArgumentException exception)
                {
                    LoreRuntimeLog.WarnOnce($"Could not load the runtime changes: {exception.Message}");

                    return false;
                }

                foreach (SaveRecord record in data?.values ?? new List<SaveRecord>())
                {
                    if (record == null ||
                        string.IsNullOrEmpty(record.entry) ||
                        string.IsNullOrEmpty(record.field) ||
                        !Enum.TryParse(record.type, out FieldType type) ||
                        !LoreStateValue.TryParse(type, record.value, out LoreStateValue value))
                    {
                        LoreRuntimeLog.WarnOnce(
                            $"Skipped an unreadable runtime change (type '{record?.type}', value '{record?.value}').");

                        continue;
                    }

                    loaded[(record.entry, record.field)] = value;
                }
            }

            if (Values.Count == 0 &&
                loaded.Count == 0)
            {
                return true;
            }

            Values.Clear();

            foreach (var pair in loaded)
                Values[pair.Key] = pair.Value;

            Changed?.Invoke(null, null);

            return true;
        }
        #endregion

        #region Internal API
        internal static bool TryGet(
            Entry entry,
            Field field,
            out LoreStateValue value)
        {
            if (Values.Count == 0 ||
                entry == null ||
                field == null)
            {
                value = default;

                return false;
            }

            return Values.TryGetValue(
                (entry.Id, field.Id),
                out value);
        }

        // Stores the value; a value equal to the original removes the change. Raises Changed only when the
        // effective value changes.
        internal static void Set(
            Entry entry,
            Field field,
            LoreStateValue value,
            LoreStateValue original)
        {
            var key = (entry.Id, field.Id);

            bool hadValue = Values.TryGetValue(
                key,
                out LoreStateValue current);

            LoreStateValue effective =
                hadValue
                    ? current
                    : original;

            if (value.Equals(original))
                Values.Remove(key);
            else
                Values[key] = value;

            if (!value.Equals(effective))
                Changed?.Invoke(entry, field);
        }

        internal static void Reset(
            Entry entry,
            Field field)
        {
            if (entry == null ||
                field == null)
            {
                return;
            }

            if (Values.Remove((entry.Id, field.Id)))
                Changed?.Invoke(entry, field);
        }

        internal static void RegisterEntry(Entry entry)
        {
            if (entry != null &&
                !string.IsNullOrEmpty(entry.Id))
            {
                LoadedEntries[entry.Id] = entry;
            }
        }

        internal static void UnregisterEntry(Entry entry)
        {
            if (entry != null &&
                !string.IsNullOrEmpty(entry.Id) &&
                LoadedEntries.TryGetValue(entry.Id, out Entry registered) &&
                registered == entry)
            {
                LoadedEntries.Remove(entry.Id);
            }
        }

        // The Entry of an Entry value: the one set in this session, otherwise the loaded Entry with that ID
        // (null when it is not loaded or was deleted).
        internal static Entry ResolveEntry(LoreStateValue value)
        {
            if (value.Entry != null)
                return value.Entry;

            if (string.IsNullOrEmpty(value.Text))
                return null;

            // The ID check skips Entries whose ID changed after registering (editor: duplicated assets get a new ID).
            return LoadedEntries.TryGetValue(value.Text, out Entry entry) &&
                   entry != null &&
                   entry.Id == value.Text
                ? entry
                : null;
        }

        // Leaving Play mode (editor): drops values and subscribers of the ended session without raising Changed.
        internal static void Clear()
        {
            Values.Clear();

            Changed = null;
        }
        #endregion

        #region Unity
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => Clear();
        #endregion
    }

    // One changed value. Type is the Field type it was set for: after a type change the value is ignored.
    internal readonly struct LoreStateValue : IEquatable<LoreStateValue>
    {
        #region Fields
        public readonly FieldType Type;
        public readonly bool Bool;
        public readonly int Int;
        public readonly float Float;
        public readonly Vector3 Vector;
        public readonly Color Color;

        // Text fields: the text. Enum fields: the option ID. Entry fields: the Entry ID (empty = none).
        public readonly string Text;

        // Entry fields: the Entry when set in this session (null after loading; then resolved by ID). Not part of
        // equality, the ID is.
        public readonly Entry Entry;
        #endregion

        #region Constructor
        private LoreStateValue(
            FieldType type,
            bool boolValue = false,
            int intValue = 0,
            float floatValue = 0f,
            Vector3 vectorValue = default,
            Color colorValue = default,
            string textValue = null,
            Entry entryValue = null)
        {
            Type = type;
            Bool = boolValue;
            Int = intValue;
            Float = floatValue;
            Vector = vectorValue;
            Color = colorValue;
            Text = textValue ?? string.Empty;
            Entry = entryValue;
        }
        #endregion

        #region Factories
        public static LoreStateValue FromBool(bool value) => new(FieldType.Boolean, boolValue: value);

        public static LoreStateValue FromInt(int value) => new(FieldType.Integer, intValue: value);

        public static LoreStateValue FromFloat(float value) => new(FieldType.Float, floatValue: value);

        public static LoreStateValue FromVector(Vector3 value) => new(FieldType.Vector, vectorValue: value);

        public static LoreStateValue FromColor(Color value) => new(FieldType.Color, colorValue: value);

        public static LoreStateValue FromText(string value) => new(FieldType.Text, textValue: value);

        public static LoreStateValue FromEnum(string optionId) => new(FieldType.Enum, textValue: optionId);

        // Missing (deleted) Entries count as none.
        public static LoreStateValue FromEntry(Entry value) =>
            value != null
                ? new(FieldType.Entry, textValue: value.Id, entryValue: value)
                : new(FieldType.Entry);

        public static LoreStateValue FromEntryId(string entryId) => new(FieldType.Entry, textValue: entryId);
        #endregion

        #region Save Text
        // Invariant culture; floats round-trip exactly.
        public string ToSaveText()
        {
            return Type switch
            {
                FieldType.Boolean => Bool ? "true" : "false",
                FieldType.Integer => Int.ToString(CultureInfo.InvariantCulture),
                FieldType.Float => ToText(Float),
                FieldType.Vector => $"{ToText(Vector.x)};{ToText(Vector.y)};{ToText(Vector.z)}",
                FieldType.Color => $"{ToText(Color.r)};{ToText(Color.g)};{ToText(Color.b)};{ToText(Color.a)}",
                FieldType.Text or FieldType.Enum or FieldType.Entry => Text,
                _ => string.Empty,
            };
        }

        public static bool TryParse(
            FieldType type,
            string text,
            out LoreStateValue value)
        {
            value = default;

            if (text == null)
                return false;

            switch (type)
            {
                case FieldType.Boolean when bool.TryParse(text, out bool boolValue):
                    value = FromBool(boolValue);
                    return true;

                case FieldType.Integer when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue):
                    value = FromInt(intValue);
                    return true;

                case FieldType.Float when TryParseFloat(text, out float floatValue):
                    value = FromFloat(floatValue);
                    return true;

                case FieldType.Vector:
                    string[] parts = text.Split(';');

                    if (parts.Length == 3 &&
                        TryParseFloat(parts[0], out float x) &&
                        TryParseFloat(parts[1], out float y) &&
                        TryParseFloat(parts[2], out float z))
                    {
                        value = FromVector(new Vector3(x, y, z));
                        return true;
                    }

                    return false;

                case FieldType.Color:
                    string[] channels = text.Split(';');

                    if (channels.Length == 4 &&
                        TryParseFloat(channels[0], out float r) &&
                        TryParseFloat(channels[1], out float g) &&
                        TryParseFloat(channels[2], out float b) &&
                        TryParseFloat(channels[3], out float a))
                    {
                        value = FromColor(new Color(r, g, b, a));
                        return true;
                    }

                    return false;

                case FieldType.Text:
                    value = FromText(text);
                    return true;

                case FieldType.Enum:
                    value = FromEnum(text);
                    return true;

                case FieldType.Entry:
                    value = FromEntryId(text);
                    return true;

                default:
                    return false;
            }
        }

        private static string ToText(float value) => value.ToString("R", CultureInfo.InvariantCulture);

        private static bool TryParseFloat(
            string text,
            out float value)
        {
            return float.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }
        #endregion

        #region Equality
        // Exact comparison (Vector3 == would treat tiny differences as equal).
        public bool Equals(LoreStateValue other) =>
            Type == other.Type &&
            Bool == other.Bool &&
            Int == other.Int &&
            Float.Equals(other.Float) &&
            Vector.Equals(other.Vector) &&
            Color.Equals(other.Color) &&
            string.Equals(Text, other.Text, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is LoreStateValue other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Type, Bool, Int, Float, Vector, Color, Text);
        #endregion
    }
}
