// <copyright file="DateHelper.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

#if UMB8
namespace Wavenet.Umbraco8.SlotCopy.Helpers
#else
namespace Wavenet.Umbraco7.SlotCopy.Helpers
#endif
{
    using System;

    /// <summary>
    /// Helper for date conversion between UTC and Epoch.
    /// </summary>
    internal class DateHelper
    {
        /// <summary>
        /// The epoch date.
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts the specified <paramref name="epochDate" /> to date.
        /// </summary>
        /// <param name="epochDate">The epoch date.</param>
        /// <returns>
        /// The date from the specified <paramref name="epochDate" />.
        /// </returns>
        public static DateTime FromEpoch(long epochDate)
            => Epoch.AddSeconds(epochDate);

        /// <summary>
        /// Converts the specified date to epoch.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// The epoch date.
        /// </returns>
        public static long ToEpoch(DateTime date)
            => (long)(date - Epoch).TotalSeconds;
    }
}