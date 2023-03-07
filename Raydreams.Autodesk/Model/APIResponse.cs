using System.Net;

namespace Raydreams.Autodesk.Model
{
    /// <summary>Wraps a response back from an API to include the Status Code and any exception</summary>
    /// <typeparam name="T">The success data type sent back from the service</typeparam>
    /// <remarks>Possibly add Authentication Error type as a property</remarks>
    public class APIResponse<T>
    {
        public APIResponse()
        {
            this.StatusCode = 0;
            this.Data = default;
        }

        /// <summary>HTTP Status code returned by the service</summary>
		public HttpStatusCode StatusCode { get; set; } = 0;

        /// <summary>The results back from the API</summary>
        public T? Data { get; set; }

        /// <summary>Any optional Debug Info</summary>
        public string? Debug { get; set; }

        /// <summary>Is HTTP Status a 2nn with no exception</summary>
        /// <remarks>You sill have to check the Data is valid</remarks>
        public bool IsSuccess => !this.IsException && this.StatusCode >= HttpStatusCode.OK 
            && this.StatusCode < HttpStatusCode.Ambiguous;

        /// <summary>Did any exception occur</summary>
        public bool IsException => this.Exception != null;

        /// <summary>Any exception that occured</summary>
        public Exception? Exception { get; set; }
    }
}
