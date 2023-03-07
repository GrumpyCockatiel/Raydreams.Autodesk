﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raydreams.Autodesk.Model;

namespace Raydreams.Autodesk.Data
{
	/// <summary>Adding in the S3 Specific Download and Upload methods</summary>
	public partial class DataManagerRepository : IDataManagerAPI
	{
		#region [ S3 Download ]

		/// <summary>Get the S3 download URL for an item</summary>
		/// <returns></returns>
		public async Task<APIResponse<S3SignedDownload>> GetS3DownloadLink( ObjectIDs ids )
		{
			if ( !ids.IsValid )
				return new APIResponse<S3SignedDownload>() { StatusCode = HttpStatusCode.BadRequest };

			string path = $"oss/v2/buckets/{ids.BucketKey}/objects/{ids.ObjectKey}/signeds3download";

			return await GetRequest<S3SignedDownload>( path, true );
		}

		/// <summary>Download an item using S3</summary>
		/// <param name="signedURL">S3 file URL</param>
		/// <param name="fullPath">Path to download to locally</param>
		/// <returns>File details as the file is downloaded to the path</returns>
		/// <remarks>Signed URLs have to begin within 2 minutes of getting the URL</remarks>
		public async Task<APIResponse<RawFileWrapper>> DownloadObject( S3SignedDownload signedURL, string fullPath )
		{
			HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Get, signedURL.URL );
			message.Headers.Clear();

			APIResponse<RawFileWrapper> results = new APIResponse<RawFileWrapper>()
			{ Data = new RawFileWrapper() };

			try
			{
				// set the timeout and buffer size
				using HttpClient client = new HttpClient() { Timeout = this.DownloadTimeout };

				// make the request async
				var cts = new CancellationTokenSource( this.DownloadTimeout );
				HttpResponseMessage httpResponse = await client.SendAsync( message, HttpCompletionOption.ResponseHeadersRead, cts.Token );

				// results - sadly Disposition does not include filename :-(
				results.StatusCode = httpResponse.StatusCode;
				results.Data.ContentType = signedURL.Parameters?.ContentType;

				using Stream readStream = await httpResponse.Content.ReadAsStreamAsync();

				// get the modification date {attachment; modification-date="Mon, 18 Mar 2019 21:49:59 +0000"}
				// this is also in the signedURL
				results.Data.ModificationDate = httpResponse.Content.Headers?.ContentDisposition?.ModificationDate;

				using Stream writeStream = File.Open( fullPath, FileMode.Create, FileAccess.Write );
				await readStream.CopyToAsync( writeStream );
			}
			catch ( System.Exception exp )
			{
				// timeouts throw - System.threading.tasks.TaskCanceledException
				// possibly handle HttpRequestException differently
				//this.Logger.Log( exp );
				results.Exception = exp;
			}

			return results;
		}

		#endregion [ S3 Download ]

		#region [ S3 Upload ]

		/// <summary>Get the S3 download URL for an item</summary>
		/// <returns></returns>
		public async Task<APIResponse<S3SignedUpload>> GetS3UploadLink( ObjectIDs ids )
		{
			if ( !ids.IsValid )
				return new APIResponse<S3SignedUpload>() { StatusCode = HttpStatusCode.BadRequest };

			string path = $"oss/v2/buckets/{ids.BucketKey}/objects/{ids.ObjectKey}/signeds3upload";

			return await GetRequest<S3SignedUpload>( path, true );
		}

		/// <summary>Puts a local file on S3</summary>
		/// <param name="file"></param>
		/// <param name="uploadURL"></param>
		/// <returns></returns>
		public async Task<APIResponse<bool>> PutObject( RawFileWrapper file, string uploadURL )
		{
			HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Put, $"{uploadURL}" );
			message.Headers.Clear();

			// read the file into memory which is limited to 2 GB, otherwise really should chunk the uploads

			// add the file body
			message.Content = new ByteArrayContent( file.Data );
			message.Content.Headers.ContentLength = file.Data.Length;

			// set MIME Type - careful any spaces will mess it up
			message.Content.Headers.ContentType = new MediaTypeHeaderValue( file.ContentType.Trim() );

			// create a reponse object
			APIResponse<bool> results = new APIResponse<bool>();

			try
			{
				using HttpClient client = new HttpClient() { Timeout = this.DownloadTimeout };

				// make the request async
				using var cts = new CancellationTokenSource( this.DownloadTimeout );
				HttpResponseMessage httpResponse = await client.SendAsync( message, cts.Token );

				// set the reponse
				results.StatusCode = httpResponse.StatusCode;
				string rawResponse = await httpResponse.Content.ReadAsStringAsync();

				// deserialize the response
				results.Data = results.StatusCode == HttpStatusCode.OK;
			}
			catch ( HttpRequestException exp )
			{
				throw exp;
			}
			catch ( System.Exception exp )
			{
                // timeouts throw - System.threading.tasks.TaskCanceledException
                results.Exception = exp;
            }

			return results;
		}

		#endregion [ S3 Upload ]
	}
}