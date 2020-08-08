using System;
using System.Text.RegularExpressions;

namespace KekeDataStore
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

        /// <summary>
        /// Checks if filename is valid
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsValidFilename(this string fileName)
        {
            if (fileName.IsEmpty()) 
                return false;

            var strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidFileNameChars());
            var regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(fileName)) 
                return false;

            return true;
        }

        /// <summary>
        /// Checks if T is Serializable
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsSerializable<T>(this T obj)
        {
            var type = obj.GetType();

            return type.IsSerializable;
        }
    }
}
