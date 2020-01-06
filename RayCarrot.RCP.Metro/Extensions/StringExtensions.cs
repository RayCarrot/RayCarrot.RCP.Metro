using System;
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
    }
}