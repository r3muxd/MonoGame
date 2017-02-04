// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TwoMGFX.EffectParsing
{
    /// <summary>
    /// A collection of utility string extension methods for the parsing system.
    /// </summary>
    public static class StringEx
    {
        public static bool IsEqualTo(this string s1, string s2, bool ignoreCase = true)
        {
            return s1.Equals(s2, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        public static bool FirstWordIs(this string text, string expected, bool ignoreCase = true)
        {
            var words = text.Split((char[]) null, 2, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return false;

            return words[0].IsEqualTo(expected, ignoreCase);
        }

        private static readonly char[] EqualsSeparators = { '\t', '\n', '\r', '=', ' ' };

        public static string GetFirstIdentifier(this string text)
        {
            var words = text.Split(EqualsSeparators, 2, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                return string.Empty;
            return words[0];
        }

        public static bool BeforeEqualsIs(this string text, string expected, bool ignoreCase = true)
        {
            var length = text.IndexOf("=", StringComparison.InvariantCulture);
            if (length == -1)
                return false;

            return text.Substring(0, length).Trim().IsEqualTo(expected, ignoreCase);
        }

        public static bool AfterEqualsIs(this string text, string expected, bool ignoreCase = true)
        {
            return text.GetAfterEquals().IsEqualTo(expected, ignoreCase);
        }

        public static string GetAfterEquals(this string text)
        {
            var startPos = text.IndexOf("=", StringComparison.InvariantCulture) + 1;
            if (startPos == 0 || startPos >= text.Length)
                return string.Empty;

            return text.Substring(startPos, text.Length - startPos).Trim();
        }

        public static bool IsValidIdentifier(this string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            if (!char.IsLetter(identifier, 0) && identifier[0] != '_')
                return false;

            foreach (var c in identifier)
            {
                if (!char.IsLetterOrDigit(c) && identifier[0] != '_')
                    return false;
            }

            return true;
        }
    }
}
