// <copyright file="JwtHelper.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

#if UMB8
namespace Wavenet.Umbraco8.SlotCopy.Helpers
#else
namespace Wavenet.Umbraco7.SlotCopy.Helpers
#endif
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Text;

    using Newtonsoft.Json;

#if UMB8
    using static Wavenet.Umbraco8.SlotCopy.Helpers.DateHelper;
#else
    using static Wavenet.Umbraco7.SlotCopy.Helpers.DateHelper;
#endif

    /// <summary>
    /// JWT helper to sign and validate requests.
    /// </summary>
    /// <remarks>
    /// Internal helper to avoid dependency.
    /// </remarks>
    internal static class JwtHelper
    {
        /// <summary>
        /// The header.
        /// </summary>
        /// <remarks>Equivalent to: {"alg": "HS256","typ": "JWT"}.</remarks>
        private const string Header = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";

        /// <summary>
        /// The maximum validity in minutes.
        /// </summary>
        private const int MaxValidityInMinutes = 10;

        /// <summary>
        /// The issuer.
        /// </summary>
        private static readonly string Issuer = typeof(JwtHelper).Namespace;

        /// <summary>
        /// Gets a value indicating whether this application has key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this application has key; otherwise, <c>false</c>.
        /// </value>
        public static bool HasKey => Settings.ValidationKey != null;

        /// <summary>
        /// Decodes the URL.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The decoded URL.
        /// </returns>
        public static string DecodeUrl(string token)
            => Decode(token)?.Url;

        /// <summary>
        /// Encodes the request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        /// The JWT Token.
        /// </returns>
        public static string EncodeUrl(string url)
        {
            return GenerateToken(new Token
            {
                Issuer = Issuer,
                Expiration = ToEpoch(DateTime.UtcNow.AddMinutes(MaxValidityInMinutes)),
                Url = url,
            });
        }

        /// <summary>
        /// Gets the authorization token.
        /// </summary>
        /// <returns>The authorization token.</returns>
        public static object GetAuthorizationToken()
            => GenerateToken(new Token
            {
                Issuer = Issuer,
                Expiration = ToEpoch(DateTime.UtcNow.AddMinutes(MaxValidityInMinutes)),
            });

        /// <summary>
        /// Determines whether the specified token is authenticate.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        ///   <c>true</c> if the specified token is authenticate; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAuthenticate(string token) => Decode(token) != null;

        /// <summary>
        /// Decodes the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The decoded token; Or null if it's invalid.</returns>
        private static Token Decode(string token)
        {
            var parts = token?.Split('.') ?? new string[0];
            using (var hash = new HMACSHA256(Settings.ValidationKey))
            {
                if (parts.Length != 3 || parts[0] != Header || Base64UrlEncoder.Encode(hash.ComputeHash(Encoding.Default.GetBytes($"{parts[0]}.{parts[1]}"))) != parts[2])
                {
                    return null;
                }

                var payload = JsonConvert.DeserializeObject<Token>(Base64UrlEncoder.DecodeString(parts[1]));
                if (payload.Issuer != Issuer || Epoch.AddSeconds(payload.Expiration) < DateTime.UtcNow)
                {
                    return null;
                }

                return payload;
            }
        }

        /// <summary>
        /// Generates the token.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The JWT Token.</returns>
        private static string GenerateToken(Token data)
        {
            var payload = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(data, Formatting.None));
            var token = new StringBuilder($"{Header}.{payload}");
            using (var hash = new HMACSHA256(Settings.ValidationKey))
            {
                var signature = Base64UrlEncoder.Encode(hash.ComputeHash(Encoding.Default.GetBytes(token.ToString())));
                token.Append('.').Append(signature);
            }

            return token.ToString();
        }

        /// <summary>
        ///   <see cref="Token" />.
        /// </summary>
        [DataContract]
        private class Token
        {
            /// <summary>
            /// Gets or sets the expiration.
            /// </summary>
            /// <value>
            /// The expiration.
            /// </value>
            [DataMember(Name = "exp", Order = 1)]
            public long Expiration { get; set; }

            /// <summary>
            /// Gets or sets the issuer.
            /// </summary>
            /// <value>
            /// The issuer.
            /// </value>
            [DataMember(Name = "iss", Order = 0)]
            public string Issuer { get; set; }

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            /// <value>
            /// The URL.
            /// </value>
            [DataMember(Name = "url", Order = 2, EmitDefaultValue = false)]
            public string Url { get; set; }
        }
    }
}