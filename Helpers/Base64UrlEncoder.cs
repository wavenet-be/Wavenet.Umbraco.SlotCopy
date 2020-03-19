// <copyright file="Base64UrlEncoder.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

#if UMB8
namespace Wavenet.Umbraco8.SlotCopy.Helpers
#else
namespace Wavenet.Umbraco7.SlotCopy.Helpers
#endif
{
    using System;
    using System.Text;

    /// <summary>
    /// Base 64 URL Encoder/Decoder.
    /// </summary>
    /// <remarks>
    /// Internal helper to avoid dependency.
    /// </remarks>
    internal static class Base64UrlEncoder
    {
        /// <summary>
        /// Decodes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// The BASE64 URL decoded version.
        /// </returns>
        public static byte[] Decode(string data)
        {
            data = data.Replace('_', '/').Replace('-', '+');
            switch (data.Length % 4)
            {
                case 2: data += "=="; break;
                case 3: data += "="; break;
            }

            return Convert.FromBase64String(data);
        }

        /// <summary>
        /// Decodes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// The BASE64 URL decoded version.
        /// </returns>
        public static string DecodeString(string data)
            => Encoding.UTF8.GetString(Decode(data));

        /// <summary>
        /// Encodes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The BASE64 URL encoded version.
        /// </returns>
        public static string Encode(string value)
            => Encode(Encoding.UTF8.GetBytes(value));

        /// <summary>
        /// Encodes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The BASE64 URL encoded version.
        /// </returns>
        public static string Encode(byte[] input)
            => Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}