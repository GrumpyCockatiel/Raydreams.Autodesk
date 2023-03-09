using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raydreams.Autodesk.Model;
using Raydreams.Autodesk.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Raydreams.Autodesk.Data
{
	/// <summary>Autodesk BIM360 Sepcific API calls</summary>
	/// <remarks>https://forge.autodesk.com/en/docs/bim360/v1/reference/http/document-management-projects-project_id-folders-folder_id-permissionsbatch-create-POST/</remarks>
	public class BIM360Repository : IBIM360Repository
	{
		#region [ Fields ]

		private string _xuser;

		private RegionCode _region = RegionCode.US;

		public const string TenantBase = "https://developer.api.autodesk.com";

		/// <summary>The User ID authorization header can be set multiple ways</summary>
		public const string XUserIDField = "x-user-id";

		/// <summary>The OTHER impersonation user ID</summary>
		public const string UserIDField = "User-Id";

		/// <summary>The max number of calls that will ever be made for bulk lookups</summary>
		public const int MaxLoops = 20;

		#endregion [ Fields ]

		#region [ Constructor ]

		/// <summary></summary>
		/// <param name="tokener">Token Manager to use</param>
		/// <param name="reg">Which region to use US or EU</param>
		public BIM360Repository( ITokenManager tokener, RegionCode reg = RegionCode.US )
		{
			this.Tokener = tokener;
			this._region = reg;
		}

		#endregion [ Constructor ]

		#region [ Properties ]

		/// <summary>Get the region part of the URL which is empty string if US</summary>
		public string Region => ( this._region == RegionCode.US ) ? String.Empty : $"regions/{this._region}".ToLowerInvariant();

		/// <summary>Handles retrieving a token</summary>
		public ITokenManager Tokener { get; set; }

		/// <summary>allows hardcoding a token</summary>
		public async Task<string> Token() => await this.Tokener?.GetTokenAsync();

		/// <summary>When set, this field will be added to x-user-id and UserID header</summary>
		public string Impersonation {
			get => this._xuser;
			set => this._xuser = ( !String.IsNullOrWhiteSpace( value ) ) ? value.Trim() : null;
		}

		#endregion [ Properties ]

		#region [ Methods ]

		/// <summary>Get a list of all users for the specified account/hub with their subject ID</summary>
		/// <param name="accountID">Account to get users for</param>
		/// <param name="limit">The max number of records to return which has a max value of 100 and will default to 10</param>
		/// <param name="offset">Page offset defaulting to 0</param>
		/// <returns></returns>
		/// <remarks>GET (account:read) app only (2-leg)</remarks>
		public async Task<APIResponse<List<AccountUser>>> ListAccountUsers( string accountID, int limit = -1, int offset = -1 )
		{
			ForgeID acct = new ForgeID( accountID);

			// valiadate the input
			if ( !acct.IsValid )
				return new APIResponse<List<AccountUser>>() { StatusCode = HttpStatusCode.BadRequest };

			// format the path
			string path = $"hq/v1/{this.Region}accounts/{acct.BIM360}/users";

			Dictionary<string, string> vars = this.GetOptions( limit, offset );

			return await GetRequest<List<AccountUser>>( path, true, vars );
		}

		/// <summary>Returns ALL the account users in the specified account</summary>
		/// <param name="account">The account to get users from</param>
		/// <returns>List of Account Users as Subjects with ID and Name only</returns>
		/// <remarks>GET (account:read) app only (2-leg)</remarks>
		public async Task<List<AccountUser>> ListAllAccountUsers( ForgeID account )
		{
			List<AccountUser> results = new List<AccountUser>();

			// valiadate the input
			if ( !account.IsValid )
				return results;

			// format the path
			string path = $"hq/v1/{this.Region}accounts/{account.BIM360}/users";

			// append base options
			int offset = 0;
			int loops = 0;

			// get accounts
			APIResponse<List<AccountUser>> resp = null;

			do
			{
				Dictionary<string, string> vars = this.GetOptions( 100, offset );

				resp = await GetRequest<List<AccountUser>>( path, true, vars );

				if ( resp.IsSuccess && resp.Data != null && resp.Data.Count > 0 )
					results.AddRange( resp.Data );
				else
					break;

				// uptick
				offset += 100;
				++loops;
			} while ( loops < MaxLoops );

			return results;
		}

		/// <summary>List all the account users on the specified project that match the search</summary>
		/// <param name="filters">Filters to use</param>
		/// <param name="accountID">Account to get users for</param>
		/// <param name="limit">The max number of records to return which has a max value of 100 and will default to 10</param>
		/// <param name="offset">Page offset defaulting to 0</param>
		/// <returns></returns>
		/// <remarks>GET (account:read) app only (2-leg)</remarks>
		public async Task<APIResponse<List<AccountUser>>> SearchAccountUsers( string accountID, AccountUserFilters filters, int limit = -1, int offset = -1 )
		{
            ForgeID acct = new ForgeID( accountID);

			// valiadate the input and do list if there are no filters
			if ( filters == null || !filters.IsValid )
				return await this.ListAccountUsers( accountID, limit, offset );

			// format the path
			string path = $"hq/v1/{this.Region}accounts/{acct.BIM360}/users/search";

			// add all the filters
			Dictionary<string, string> vars = this.GetOptions( limit, offset );
			vars.Add( "operator", filters.Operator.ToString() );
			vars.Add( "partial", filters.Partial.ToString().ToLowerInvariant() );
			if ( !String.IsNullOrWhiteSpace( filters.Name ) )
				vars.Add( "name", filters.Name );
			if ( !String.IsNullOrWhiteSpace( filters.Email ) )
				vars.Add( "email", filters.Email );
			if ( !String.IsNullOrWhiteSpace( filters.CompanyName ) )
				vars.Add( "company_name", filters.CompanyName );

			return await GetRequest<List<AccountUser>>( path, true, vars );
		}

		/// <summary>Get details on a specific account user</summary>
		/// <param name="accountID">Account/HUB ID</param>
		/// <param name="userID">This is the GUID account ID not the Autodesk User ID</param>
		/// <returns></returns>
		/// <remarks>GET (account:read) app only</remarks>
		public async Task<APIResponse<AccountUser>> GetAccountUser( string accountID, string userID )
		{
            ForgeID acct = new ForgeID( accountID );

			// valiadate the input
			if ( !acct.IsValid || String.IsNullOrWhiteSpace( userID ) )
				return new APIResponse<AccountUser>() { StatusCode = HttpStatusCode.BadRequest };

			userID = userID.Trim();

			// format the path
			string path = $"hq/v1/{this.Region}accounts/{acct.BIM360}/users/{userID}";

			return await GetRequest<AccountUser>( path, true );
		}

		/// <summary>Searches all account users for the specified autodesk user ID</summary>
		/// <param name="accountID">Account/HUB ID</param>
		/// <param name="autodeskID">This is the 12 alphanumeric ID assigned by Forge to a user</param>
		/// <returns>An Autodesk User Account info</returns>
		public async Task<APIResponse<AccountUser>> GetAccountUserByID( string accountID, string autodeskID )
		{
            ForgeID acct = new ForgeID( accountID );

			// valiadate the input
			if ( !acct.IsValid || String.IsNullOrWhiteSpace( autodeskID ) )
				return null;

			autodeskID = autodeskID.Trim();

			// format the path
			string path = $"hq/v1/{this.Region}accounts/{acct.BIM360}/users";

			// append base options
			int offset = 0;
			int loops = 0;

			do
			{
				Dictionary<string, string> vars = this.GetOptions( 100, offset );

				APIResponse<List<AccountUser>> resp = await GetRequest<List<AccountUser>>( path, true, vars );

				if ( resp.IsSuccess && resp.Data != null && resp.Data.Count > 0 )
				{
					var user = resp.Data.Where( u => u.UserID == autodeskID ).FirstOrDefault();
					
					if ( user != null )
						return new APIResponse<AccountUser> { Data = user, StatusCode = HttpStatusCode.OK };
				}
				else
					break;

				// uptick
				offset += 100;
				++loops;

			} while ( loops < MaxLoops );

			return null;
		}

		/// <summary>Insert a new Account User into a project</summary>
		/// <param name="user"></param>
		/// <returns></returns>
		/// <remarks>Call this right after creating a new project so someone has access</remarks>
		public async Task<APIResponse<AccountUser>> InsertAccountUser( AccountUser user )
		{
			// validate the input
			if ( user == null || !user.AccountID.IsValid || String.IsNullOrWhiteSpace( user.Email ) )
				return new APIResponse<AccountUser>() { StatusCode = HttpStatusCode.BadRequest };

			string path = $"hq/v1/{this.Region}accounts/{user.AccountID}/users";

			// serialize the body
			string body = JsonConvert.SerializeObject( user );

			// get the results
			APIResponse<AccountUser> results = await PostRequest<AccountUser>( path, body, true );

			return results;
		}

		#endregion [ Methods ]

		#region [ Protected Methods ]

		/// <summary>Gets the Limit and Offset options for now</summary>
		/// <param name="limit"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		protected Dictionary<string, string> GetOptions( int limit, int offset )
		{
			Dictionary<string, string> vars = new Dictionary<string, string>();

			// append options
			if ( limit > 0 )
			{
				if ( limit > 100 )
					limit = 100;
				vars.Add( "limit", limit.ToString() );
			}

			if ( offset > -1 )
				vars.Add( "offset", offset.ToString() );

			return vars;
		}

		/// <summary>Forge Data Manager GET Request</summary>
		/// <param name="authenticate">If true then add the bearer token to the request header</param>
		/// <returns></returns>
		protected async Task<APIResponse<T>> GetRequest<T>( string path, bool authenticate, Dictionary<string, string> param = null )
		{
			if ( String.IsNullOrWhiteSpace( path ) )
				return null;

			UriBuilder sb = new UriBuilder( $"{TenantBase}/{path}" );

			if ( param != null && param.Count > 0 )
			{
				foreach ( KeyValuePair<string, string> kvp in param )
				{
					if ( String.IsNullOrWhiteSpace( kvp.Key ) || String.IsNullOrWhiteSpace( kvp.Value ) )
						continue;

					sb.Query += $"{kvp.Key.Trim()}={kvp.Value.Trim()}&";
				}

				sb.Query = sb.Query.TrimEnd( '&' );
			}

			APIResponse<T> results = new APIResponse<T>();

			HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Get, sb.Uri );
			message.Headers.Clear();

			// check for an existing token
			if ( authenticate && !String.IsNullOrWhiteSpace( await this.Token() ) )
				message.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", await this.Token() );

			// do we need to set the x-user
			if ( this.Impersonation != null )
			{
				message.Headers.Add( XUserIDField, this.Impersonation );

				//var user = await this.Tokener.GetUser();
				//if ( user != null && !String.IsNullOrWhiteSpace( user.UserID ) )
				//{
					//message.Headers.Add( XUserIDField, user.UserID );
					//message.Headers.Add( UserIDField, user.UserID );
				//}
			}

			// hold the reponse
			string rawResponse = null;

			try
			{
				using HttpResponseMessage httpResponse = await new HttpClient().SendAsync( message );
				results.StatusCode = httpResponse.StatusCode;
				rawResponse = await httpResponse.Content.ReadAsStringAsync();

				if ( !results.IsSuccess )
				{
					// test for the kind of response object
					dynamic temp = JsonConvert.DeserializeObject( rawResponse );

					// BUG - BIM360 should not have this
					if ( !temp.ContainsKey( "jsonapi" ) )
					{
						// the response is probably a ForgeError Object like Unauthorized
						AuthenticationError error = JsonConvert.DeserializeObject<AuthenticationError>( rawResponse );
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

				results.Exception = exp;
				return results;
			}
			// serialization issues need special care
			catch ( JsonSerializationException exp )
			{
				results.Exception = exp;
				results.Debug = rawResponse;
				return results;
			}
			catch ( System.Exception exp )
			{
				results.Exception = exp;
				return results;
			}

			return results;
		}

		/// <summary>Formats a POST, PUT or PATCH request</summary>
		/// <typeparam name="T">reponse type</typeparam>
		/// <param name="path"></param>
		/// <param name="body"></param>
		/// <param name="authenticate">Whether to add in the token</param>
		/// <returns></returns>
		protected async Task<APIResponse<T>> PostRequest<T>( string path, string body, bool authenticate, string method = "POST" )
		{
			if ( String.IsNullOrWhiteSpace( path ) )
				return null;

			HttpMethod met = ( String.IsNullOrWhiteSpace( method ) ) ? HttpMethod.Post : new HttpMethod( method.Trim() );

			// should validate we have only POST, PUT and PATCH
			if ( met != HttpMethod.Post && met != HttpMethod.Patch && met != HttpMethod.Put )
				return null;

			APIResponse<T> results = new APIResponse<T>();

			HttpRequestMessage message = new HttpRequestMessage( met, $"{TenantBase}/{path}" );
			message.Headers.Clear();

			// check for an existing token
			if ( authenticate && !String.IsNullOrWhiteSpace( await this.Token() ) )
				message.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", await this.Token() );

			// do we need to set the x-user
			if ( this.Impersonation != null )
				message.Headers.Add( XUserIDField, this.Impersonation );

			// do we need to set the x-user
			//if ( this.Impersonate )
			//{
			//	var user = await this.Tokener.GetUser();
			//	if ( user != null && !String.IsNullOrWhiteSpace( user.UserID ) )
			//	{
			//		message.Headers.Add( XUserIDField, user.UserID );
			//		message.Headers.Add( UserIDField, user.UserID );
			//	}
			//}

			// add the body
			message.Content = new StringContent( body, Encoding.UTF8, "application/json" );
			byte[] bytes = Encoding.UTF8.GetBytes( body );
			message.Content.Headers.Add( "Content-Length", bytes.Length.ToString() );

			// hold the reponse
			string rawResponse = null;

			try
			{
				using HttpResponseMessage httpResponse = await new HttpClient().SendAsync( message );
				results.StatusCode = httpResponse.StatusCode;
				rawResponse = await httpResponse.Content.ReadAsStringAsync();

				if ( !results.IsSuccess )
				{
					// test for the kind of response object
					//dynamic temp = JsonConvert.DeserializeObject( rawResponse );
					results.Debug = rawResponse;
					return results;
				}

				// deserialize the response if results.Data is an actual object
				if ( !String.IsNullOrWhiteSpace( rawResponse ) )
					results.Data = JsonConvert.DeserializeObject<T>( rawResponse );
				
			}
			catch ( System.Exception exp )
			{
				results.Exception = exp;
				return results;
			}

			return results;
		}

		#endregion [ Protected Methods ]
	}
}
