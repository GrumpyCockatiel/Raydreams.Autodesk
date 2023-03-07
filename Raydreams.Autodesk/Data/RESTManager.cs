using System;
using Newtonsoft.Json;
using Raydreams.Autodesk.Model;
using System.Net.Http.Headers;

namespace Raydreams.Autodesk.Data
{
    /// <summary>Eventual HTTPClient Wrapper because RESTSharp SUCKS!</summary>
    public partial class DataManagerRepository : IDataManagerAPI
    {
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
    }
}

