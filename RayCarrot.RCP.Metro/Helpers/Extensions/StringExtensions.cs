using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="String"/>
    /// </summary>
    public static class StringExtensions
    {
        //
        // NOTE: I'd like to find an alternative to using this when getting Registry values which are not correctly NULL-terminated (like the Legends resolution strings)
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/ddf0e860-0fcb-45b8-96db-b275255e644b/registrykey-getvalue-function-returns-string-with-invalid-unicode-char?forum=clr
        // https://docs.microsoft.com/en-us/windows/desktop/api/winreg/nf-winreg-reggetvaluea
        // http://www.pinvoke.net/default.aspx/advapi32.regqueryvalueex
        //

        /// <summary>
        /// Gets the first digits in a string and removed invalid characters
        /// </summary>
        /// <param name="input">The string to get the first digits from</param>
        /// <returns>The new string with the first digits</returns>
        public static string KeepFirstDigitsOnly(this string input)
        {
            // Get the null termination strings
            var nullString = Convert.ToChar(0x0).ToString();

            // Remove invalid characters
            if (input.Contains(nullString))
                input = input.Substring(0, input.IndexOf(nullString, StringComparison.Ordinal));

            // Keep only first digits
            return input.TakeWhile(Char.IsDigit).Aggregate(String.Empty, (current, c) => current + c);
        }

        /// <summary>
        /// Returns true if the string is null or an empty string
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>True if the string is null or empty, false if not</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Returns true if the string is null or an empty string or just whitespace
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>True if the string is null, empty or just white space, false if not</returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Replaces every occurrence of the old characters with the new string
        /// </summary>
        /// <param name="value">The string to replace the characters in</param>
        /// <param name="oldChars">The old characters to replace</param>
        /// <param name="newString">The string to replace the old characters with</param>
        /// <returns>The string with the characters replaced</returns>
        /// <exception cref="ArgumentNullException"/>
        public static string Replace(this string value, char[] oldChars, string newString)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (oldChars == null)
                throw new ArgumentNullException(nameof(oldChars));

            if (newString == null)
                throw new ArgumentNullException(nameof(newString));

            return String.Join(newString, value.Split(oldChars, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Gets all indexes of a specific value in a string
        /// </summary>
        /// <param name="str">The string to get the indexes from</param>
        /// <param name="value">The value to search for</param>
        /// <returns>The found indexes</returns>
        public static IEnumerable<int> AllIndexesOf(this string str, string value)
        {
            if (value.IsNullOrEmpty())
                throw new ArgumentException("The string to find may not be empty", nameof(value));

            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);

                if (index == -1)
                    break;

                yield return index;
            }
        }

        /// <summary>
        /// Removes the specified parts of a string
        /// </summary>
        /// <param name="input">The string to remove the parts from</param>
        /// <param name="toRemove">The parts of the string to remove</param>
        /// <returns>The new string with the specified parts removed</returns>
        public static string Remove(this string input, IEnumerable<string> toRemove)
        {
            return toRemove.Where(x => !x.IsNullOrWhiteSpace()).Aggregate(input, (current, s) => current.Replace(s, String.Empty));
        }

        /// <summary>
        /// Truncates a string to have the specified maximum length
        /// </summary>
        /// <param name="value">The string to truncate</param>
        /// <param name="maxLength">The max string length</param>
        /// <returns>The truncated string</returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (value.IsNullOrEmpty())
                return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}