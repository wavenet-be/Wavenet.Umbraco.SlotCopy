// <copyright file="Settings.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco7.SlotCopy
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// SlotCopy module <see cref="Settings"/>.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// The configuration root.
        /// </summary>
        private const string ConfigurationRoot = "UmbracoSlotCopy::";

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
    }
}