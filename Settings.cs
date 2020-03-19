// <copyright file="Settings.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

#if UMB8
namespace Wavenet.Umbraco8.SlotCopy.Helpers
#else
namespace Wavenet.Umbraco7.SlotCopy.Helpers
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Text;
    using System.Web.Configuration;

    /// <summary>
    /// SlotCopy module <see cref="Settings" />.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// The configuration root.
        /// </summary>
        private const string ConfigurationRoot = "UmbracoSlotCopy::";

        /// <summary>
        /// The validation key.
        /// </summary>
        private static readonly Lazy<byte[]> LazyValidationKey = new Lazy<byte[]>(() =>
                {
                    var key = ConfigurationManager.AppSettings[ConfigurationRoot + nameof(ValidationKey)];
                    if (!string.IsNullOrEmpty(key))
                    {
                        return Encoding.UTF8.GetBytes(key);
                    }

                    var configSection = (MachineKeySection)ConfigurationManager.GetSection("system.web/machineKey");
                    return TryParseHex(configSection.ValidationKey);
                });

        /// <summary>
        /// Gets the files to synchronize pattern.
        /// </summary>
        /// <value>
        /// The files to synchronize pattern.
        /// </value>
        public static string FilesToSyncPattern => ConfigurationManager.AppSettings[ConfigurationRoot + nameof(FilesToSyncPattern)] ?? "*.*";

        /// <summary>
        /// Gets the paths to synchronize.
        /// </summary>
        /// <value>
        /// The paths to synchronize.
        /// </value>
        public static IEnumerable<string> PathsToSync => (ConfigurationManager.AppSettings[ConfigurationRoot + nameof(PathsToSync)] ?? "~/media,~/css,~/App_Data/UmbracoForms").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Gets the server to synchronize.
        /// </summary>
        /// <value>
        /// The server to synchronize.
        /// </value>
        public static string ServerToSync => ConfigurationManager.AppSettings[ConfigurationRoot + nameof(ServerToSync)];

        /// <summary>
        /// Gets the validation key.
        /// </summary>
        /// <value>
        /// The validation key.
        /// </value>
        public static byte[] ValidationKey => LazyValidationKey.Value;

        /// <summary>
        /// Tries the parse hexadecimal.
        /// </summary>
        /// <param name="validationKey">The validation key.</param>
        /// <returns>The hexadecimal representation of <paramref name="validationKey"/>; - or - null if the key is not hexadecimal.</returns>
        private static byte[] TryParseHex(string validationKey)
        {
            if (validationKey.Length % 2 != 0)
            {
                return null;
            }

            var result = new byte[validationKey.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                var byteValue = validationKey.Substring(i << 1, 2);
                if (!byte.TryParse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var data))
                {
                    return null;
                }

                result[i] = data;
            }

            return result;
        }
    }
}