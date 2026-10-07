using System;

namespace PebblePouch.Lore
{
    // Identifies a Field for value lookups: a Field reference (rename-safe), a Field ID (rename-safe, used by the
    // generated Field constants) or a Field name (case-insensitive). Converts implicitly, so lookups accept all:
    //   entry.GetInt("Health");
    //   entry.GetInt(healthField);
    //   entry.GetInt(LoreFields.Health);
    public readonly struct FieldKey
    {
        #region Fields
        private readonly Field _field;
        private readonly string _id;
        private readonly string _name;
        #endregion

        #region Constructor
        private FieldKey(
            Field field,
            string id,
            string name)
        {
            _field = field;
            _id = id;
            _name = name;
        }
        #endregion

        #region Conversions
        public static implicit operator FieldKey(Field field) => new(field, null, null);

        public static implicit operator FieldKey(string name) => new(null, null, name);
        #endregion

        #region Public API
        // Key by Field ID; name is only used in messages (e.g. the title when the constant was generated).
        public static FieldKey FromId(
            string id,
            string name = null) =>
            new(null, id, name);

        public bool Matches(Field field)
        {
            if (field == null)
                return false;

            if (!ReferenceEquals(_field, null))
                return field == _field;

            if (_id != null)
                return _id.Length > 0 && string.Equals(field.Id, _id, StringComparison.Ordinal);

            return !string.IsNullOrEmpty(_name) &&
                   string.Equals(
                       field.Title,
                       _name,
                       StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            if (!ReferenceEquals(_field, null))
                return _field != null ? _field.Title : "(missing Field)";

            if (_id != null)
            {
                return string.IsNullOrEmpty(_name)
                    ? $"(Field ID {_id})"
                    : _name;
            }

            return string.IsNullOrEmpty(_name)
                ? "(no Field)"
                : _name;
        }
        #endregion
    }
}
