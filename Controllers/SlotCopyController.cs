// <copyright file="SlotCopyController.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco7.SlotCopy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    using Newtonsoft.Json;

    using Umbraco.Core;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.WebApi;
    using Wavenet.Umbraco7.SlotCopy.Helpers;
    using Wavenet.Umbraco7.SlotCopy.Models;

    /// <summary>
    ///   <see cref="SlotCopyController" />.
    /// </summary>
    /// <seealso cref="Umbraco.Web.WebApi.UmbracoApiController" />
    /// <seealso cref="UmbracoApiController" />
    [PluginController("Wavenet")]
    public class SlotCopyController : UmbracoApiController
    {
        /// <summary>
        /// Downloads the requested file in the token.
        /// </summary>
        /// <returns>The requested file.</returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Download()
        {
            var url = JwtHelper.DecodeUrl(this.Request.Headers.Authorization?.Parameter);
            return !string.IsNullOrEmpty(url) ?
                new HttpResponseMessage
                {
                    Content = new StreamContent(File.OpenRead(HostingEnvironment.MapPath(url))),
                }
                : new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Executes asynchronously a single HTTP operation.
        /// </summary>
        /// <param name="controllerContext">The controller context for a single HTTP operation.</param>
        /// <param name="cancellationToken">The cancellation token assigned for the HTTP operation.</param>
        /// <returns>
        /// The newly started task.
        /// </returns>
        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
            => JwtHelper.HasKey ?
                base.ExecuteAsync(controllerContext, cancellationToken) :
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotImplemented) { ReasonPhrase = "Does not fulfill security requirements." });

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <returns>
        /// List of files to sync.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<FileInfoViewModel> GetFiles()
        {
            if (!JwtHelper.IsAuthenticate(this.Request.Headers.Authorization?.Parameter))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var root = HostingEnvironment.ApplicationPhysicalPath;
            string filesToSyncPattern = Settings.FilesToSyncPattern;
            return from path in Settings.PathsToSync.Select(p => HostingEnvironment.MapPath(p))
                   where Directory.Exists(path)
                   from fileName in Directory.EnumerateFiles(path, filesToSyncPattern, SearchOption.AllDirectories)
                   let fileInfo = new FileInfo(fileName)
                   select new FileInfoViewModel
                   {
                       Name = fileName.Substring(root.Length).Replace('\\', '/').EnsureStartsWith('/'),
                       Size = fileInfo.Length,
                       LastModified = DateHelper.ToEpoch(fileInfo.LastWriteTimeUtc),
                   };
        }

        /// <summary>
        /// Synchronizes files from target server.
        /// </summary>
        /// <returns>
        /// Number of item.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public int Sync()
        {
            var webServiceUrl = Settings.ServerToSync;
            if (string.IsNullOrEmpty(webServiceUrl))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            int updatedRessources = 0;
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Authorization] = $"Bearer {JwtHelper.GetAuthorizationToken()}";
                client.Headers[HttpRequestHeader.Cookie] = "x-ms-routing-name=self";
                var webServiceUri = new Uri(webServiceUrl);
                var files = GetFileList(client, webServiceUri);
                foreach (var file in files)
                {
                    string temp = null;
                    try
                    {
                        var local = new FileInfo(HostingEnvironment.MapPath(file.Name));
                        if (!local.Exists ||
                            DateHelper.ToEpoch(local.LastWriteTimeUtc) != file.LastModified ||
                            local.Length != file.Size)
                        {
                            temp = Path.GetTempFileName();
                            local.Directory.Create();
                            client.Headers[HttpRequestHeader.Authorization] = $"Bearer {JwtHelper.EncodeUrl(file.Name)}";
                            client.DownloadFile(new Uri(webServiceUri, "Download?x-ms-routing-name=self"), temp);
                            File.Copy(temp, local.FullName, true);
                            local.LastWriteTimeUtc = DateHelper.FromEpoch(file.LastModified);
                            updatedRessources++;
                        }
                    }
                    finally
                    {
                        if (temp != null)
                        {
                            File.Delete(temp);
                        }
                    }
                }

                return updatedRessources;
            }
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="webServiceUri">The web service URI.</param>
        /// <returns>
        /// The file list.
        /// </returns>
        private static IEnumerable<FileInfoViewModel> GetFileList(WebClient client, Uri webServiceUri)
            => JsonConvert.DeserializeObject<IEnumerable<FileInfoViewModel>>(Encoding.UTF8.GetString(client.DownloadData(webServiceUri + "?x-ms-routing-name=self")));
    }
}