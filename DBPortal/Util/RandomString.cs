using System;

namespace DBPortal.Util
{
    public static class RandomString
    {
        public const string AlphaNumericCharset = "abcdefghijklmnopqrstuvwxyz0123456789";
        
        /// <summary>
        /// Generates a random string with a given length using character from a given set.
        /// </summary>
        /// <param name="length">Length of the resultant string.</param>
        /// <param name="charset">Characters to use in the random string.</param>
        /// <returns>A random string.</returns>
        public static string Generate(uint length, string charset)
        {
            var rng = new Random(DateTime.Now.GetHashCode());
            var builder = "";
            for (var i = 0; i < length; ++i)
            {
                builder += charset[rng.Next(charset.Length)];
            }

            return builder;
        }
    }
}