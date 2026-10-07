using System;
using System.Text;

namespace PebblePouch.Lore.Editor
{
    public static class LoreNameValidator
    {
        #region Fields
        private static readonly char[] InvalidCharacters =
        {
            '<',
            '>',
            ':',
            '"',
            '/',
            '\\',
            '|',
            '?',
            '*'
        };

        private static readonly string[] ReservedNames =
        {
            "CON",
            "PRN",
            "AUX",
            "NUL",

            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",

            "LPT1",
            "LPT2",
            "LPT3",
            "LPT4",
            "LPT5",
            "LPT6",
            "LPT7",
            "LPT8",
            "LPT9"
        };

        private const int MaxByteLength = 255;
        #endregion

        #region Public API
        public static bool IsValid(string name)
        {
            return Validate(name).Success;
        }

        public static LoreOperationResult Validate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return LoreOperationResult.Failed(LoreText.Validation.EnterName);
            }

            if (name != name.Trim())
            {
                return LoreOperationResult.Failed(LoreText.Validation.NoLeadingOrTrailingSpace);
            }

            if (name.EndsWith("."))
            {
                return LoreOperationResult.Failed(LoreText.Validation.NoTrailingPeriod);
            }

            if (name.IndexOfAny(InvalidCharacters) >= 0)
            {
                return LoreOperationResult.Failed(LoreText.Validation.InvalidCharacters);
            }

            foreach (char character in name)
            {
                if (character == '\0' || char.IsControl(character))
                {
                    return LoreOperationResult.Failed(LoreText.Validation.NoControlCharacters);
                }
            }

            if (Encoding.UTF8.GetByteCount(name) > MaxByteLength)
            {
                return LoreOperationResult.Failed(LoreText.Validation.NameTooLong);
            }

            string baseName = GetBaseName(name);

            foreach (string reservedName in ReservedNames)
            {
                if (!string.Equals(
                        baseName,
                        reservedName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return LoreOperationResult.Failed(string.Format(LoreText.Validation.ReservedName, baseName));
            }

            return LoreOperationResult.Succeeded();
        }
        #endregion

        #region Helpers
        private static string GetBaseName(string name)
        {
            int extensionIndex = name.IndexOf('.');

            if (extensionIndex < 0)
                return name;

            return name.Substring(
                0,
                extensionIndex);
        }
        #endregion
    }
}