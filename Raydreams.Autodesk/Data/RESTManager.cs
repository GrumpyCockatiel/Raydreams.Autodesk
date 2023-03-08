using System;
using Newtonsoft.Json;
using Raydreams.Autodesk.Model;
using System.Net.Http.Headers;
using System.Text;
using System.Net;

namespace Raydreams.Autodesk.Data
{
    /// <summary>Eventual HTTPClient Wrapper because RESTSharp SUCKS!</summary>
    public partial class DataManagerRepository : IDataManagerAPI
    {
        //"application/vnd.api+json"
        public string ContentType { get; set; } = "application/json";

        /// <summary>Forge Data Manager GET Request</summary>
        /// <param name="authenticate">If true then add the bearer token to the request header</param>
        /// <returns></returns>
        protected async Task<APIResponse<T>> GetRequest<T>( string path, bool authenticate )
        {
            if ( String.IsNullOrWhiteSpace( path ) )
                return default!;

            APIResponse<T> results = new APIResponse<T>();

            HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Get, $"{TenantBase}/{path}" );
            message.Headers.Clear();

            // check for an existing token
            if ( authenticate )
                message.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", await this.Token() );

            // TODO - Need the user ID here
            if ( this.IsXUser )
                message.Headers.Add( XUserIDField, "" );

            // hold the reponse
            string rawResponse = String.Empty;

            try
            {
                HttpResponseMessage httpResponse = await new HttpClient().SendAsync( message );

                results.StatusCode = httpResponse.StatusCode;
                rawResponse = await httpResponse.Content.ReadAsStringAsync();

                // TO DO - find a way to grab a 429 response
                RetryConditionHeaderValue? retry = httpResponse.Headers.RetryAfter;

                //if (retry != null)
                //this.Logger.Log("Retry encountered");

                if ( !results.IsSuccess )
                {
                    // test for the kind of response object
                    dynamic? temp = JsonConvert.DeserializeObject( rawResponse );

                    if ( !temp?.ContainsKey( "jsonapi" ) )
                    {
                        // the response is probably a ForgeError Object like Unauthorized
                        AuthenticationError? error = JsonConvert.DeserializeObject<AuthenticationError>( rawResponse );
                        results.Debug = rawResponse;
                        return results;
                    }
                }

                // deserialize the response
                results.Data = JsonConvert.DeserializeObject<T>( rawResponse );

            }
            // usually some kind of network issue we want to treat differently like retry
            catch ( HttpRequestException exp )
            {
                // we want to add some resliancy here
                //this.Logger.Log(exp);
                results.Exception = exp;
                return results;
            }
            // serialization issues need special care
            catch ( JsonSerializationException exp )
            {
                //this.Logger.Log(exp);
                results.Exception = exp;
                results.Debug = rawResponse;
                return results;
            }
            catch ( System.Exception exp )
            {
                //this.Logger.Log(exp);
                results.Exception = exp;
                return results;
            }

            return results;
        }

        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="authenticate"></param>
        /// <returns></returns>
        protected async Task<APIResponse<T>> PostRequest<T>( string path, string body, bool authenticate )
        {
            APIResponse<T> results = new APIResponse<T>();

            if ( String.IsNullOrWhiteSpace( path ) )
                return results;

            HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Post, $"{TenantBase}/{path}" );
            message.Headers.Clear();

            // check for an existing token
            if ( authenticate )
                message.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", await this.Token() );

            // TODO - Need the user ID here
            if ( this.IsXUser )
                message.Headers.Add( XUserIDField, "" );

            // add the body
            message.Content = new StringContent( body, Encoding.UTF8 );
            byte[] bytes = Encoding.UTF8.GetBytes( body );
            message.Content.Headers.ContentLength = bytes.Length;
            message.Content.Headers.ContentType = new MediaTypeHeaderValue( this.ContentType );

            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage httpResponse = await client.SendAsync( message );

                results.StatusCode = httpResponse.StatusCode;
                string rawResponse = await httpResponse.Content.ReadAsStringAsync();

                // deserialize the response
                results.Data = JsonConvert.DeserializeObject<T>( rawResponse );
            }
            catch ( System.Exception exp )
            {
                results.Exception = exp;
                return results;
            }

            return results;
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
    }
}
