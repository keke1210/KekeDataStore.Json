using System;

namespace KekeDataStore.Json
{
    /// <summary>
    /// Helper extension methods
    /// </summary>
    internal static class HelperExtensions
    {
        /// <summary>
        /// Checks if a string is null, whitespace or empty
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string text) => string.IsNullOrWhiteSpace(text) || string.Empty == text;

        /// <summary>
        /// Checks if string is a valid guid
        /// </summary>
        /// <param name="stringGuid"></param>
        /// <returns></returns>
        public static bool IsEmptyGuid(this string stringGuid) => string.IsNullOrEmpty(stringGuid) || stringGuid == Guid.Empty.ToString();
    }
}
