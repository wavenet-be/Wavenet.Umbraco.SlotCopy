// <copyright file="FileInfoViewModel.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

#if UMB8
namespace Wavenet.Umbraco8.SlotCopy.Models
#else
namespace Wavenet.Umbraco7.SlotCopy.Models
#endif
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The file info.
    /// </summary>
    [DataContract]
    public class FileInfoViewModel
    {
        /// <summary>
        /// Gets or sets the last modified.
        /// </summary>
        /// <value>
        /// The last modified.
        /// </value>
        [DataMember(Name = "lastModified", Order = 2)]
        public long LastModified { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        [DataMember(Name = "size", Order = 1)]
        public long Size { get; set; }
    }
}