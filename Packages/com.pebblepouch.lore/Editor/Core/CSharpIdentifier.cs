using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PebblePouch.Lore.Editor
{
    // C# identifier rules for generated code (settings validation, Field constant names).
    public static class CSharpIdentifier
    {
        #region Fields
        private static readonly Regex IdentifierPattern = new(@"^[A-Za-z_][A-Za-z0-9_]*$");

        private static readonly HashSet<string> Keywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
            "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int",
            "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out",
            "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try",
            "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile",
            "while",
        };
        #endregion

        #region Public API
        public static bool IsKeyword(string value) => value != null && Keywords.Contains(value);

        public static bool IsValid(string value) =>
            !string.IsNullOrEmpty(value) &&
            IdentifierPattern.IsMatch(value) &&
            !IsKeyword(value);

        // Empty (global namespace) or identifiers separated by dots.
        public static bool IsValidNamespace(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            foreach (string part in value.Split('.'))
            {
                if (!IsValid(part))
                    return false;
            }

            return true;
        }
        #endregion
    }
}
