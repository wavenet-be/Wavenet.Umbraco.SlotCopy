// <copyright file="SlotCopyController.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

#if UMB8
namespace Wavenet.Umbraco8.SlotCopy.Controllers
#else
namespace Wavenet.Umbraco7.SlotCopy.Controllers
#endif
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

#if UMB8
    using Wavenet.Umbraco8.SlotCopy.Helpers;
    using Wavenet.Umbraco8.SlotCopy.Models;
#else
    using Wavenet.Umbraco7.SlotCopy.Helpers;
    using Wavenet.Umbraco7.SlotCopy.Models;
#endif

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
        /// <returns>
        /// The requested file.
        /// </returns>
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
        /// Synchronises files from source server.
        /// </summary>
        /// <returns>
        /// Synchronisation status.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Sync()
        {
            var webServiceUrl = Settings.ServerToSync;
            if (string.IsNullOrEmpty(webServiceUrl))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage
            {
                Content = new PushStreamContent((Action<Stream, HttpContent, TransportContext>)this.ProcessSync, "text/plain"),
            };
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

        /// <summary>
        /// Writes the <paramref name="line"/> on the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="line">The line.</param>
        private static void WriteLine(Stream stream, string line)
        {
            var buffer = Encoding.UTF8.GetBytes(line + Environment.NewLine);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        /// <summary>
        /// Processes the synchronisation.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="content">The content.</param>
        /// <param name="transportContext">The transport context.</param>
        private void ProcessSync(Stream stream, HttpContent content, TransportContext transportContext)
        {
            try
            {
                using (stream)
                using (var client = new WebClient())
                {
                    // Ensure TLS 1.2 is enabled.
                    ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12;
                    int updatedRessources = 0;

                    client.Headers[HttpRequestHeader.Authorization] = $"Bearer {JwtHelper.GetAuthorizationToken()}";
                    client.Headers[HttpRequestHeader.Cookie] = "x-ms-routing-name=self";
                    var webServiceUri = new Uri(Settings.ServerToSync);
                    var files = GetFileList(client, webServiceUri);
                    WriteLine(stream, $"Files to sync: {files.Count()}");
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
                                File.SetLastWriteTimeUtc(temp, DateHelper.FromEpoch(file.LastModified));
                                File.Copy(temp, local.FullName, true);
                                updatedRessources++;
                                WriteLine(stream, $"Sync: {file.Name}");
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

                    WriteLine(stream, $"Success - Updated: {updatedRessources} files.");
                }
            }
            catch (Exception exception)
            {
                WriteLine(stream, $"Error: {exception}");
            }
        }
    }
}
