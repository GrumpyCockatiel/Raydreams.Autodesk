using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Raydreams.Autodesk.CLI.Model;
using Newtonsoft.Json;
using Raydreams.Autodesk.CLI.Extensions;

namespace Raydreams.Autodesk.CLI.Security
{
	/// <summary>Handles API calls with the Forge Authentication APIs</summary>
	/// <remarks>This is a very conflicting name, so lets change it to AutodeskAuthManager</remarks>
	public class AuthenticationManager : IAuthManager
	{
		#region [ Fields ]

		public const string TenantBase = "https://developer.api.autodesk.com";

        public const string TenantRoute = "authentication/v2";

        /// <summary>covers all scopes for now</summary>
        /// <remarks>This is a bad idea and needs to be removed.</remarks>
        //private string _scopes = "account:read%20account:write%20data:read%20data:write%20data:create";
        private string _scopes = "data:read%20account:read";

		#endregion [ Fields ]

		/// <summary>Specify client details for using Forge</summary>
		public AuthenticationManager( ForgeAppClient client )
		{
			this.Client = client;

			// should be read only by default
			this.Scopes = ForgeScopes.DataRead | ForgeScopes.DataWrite | ForgeScopes.DataCreate | ForgeScopes.AccountRead | ForgeScopes.AccountWrite;
		}

		#region [ Properties ]

		/// <summary>The client connection being used</summary>
		protected ForgeAppClient Client { get; set; }

		/// <summary>Change the scopes after creation</summary>
		public ForgeScopes Scopes
		{
			set
			{
				this._scopes = FormatScopes(value);
			}
		}

		/// <summary>Generates the Autodesk OAuth login URL based on the current Properties for a client</summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public string Login => $"{TenantBase}/{TenantRoute}/authorize?client_id={this.Client.ClientID}&redirect_uri={this.Client.CallbackURL}&response_type=code&scope={this._scopes}";

		/// <summary>Generates the logout URL</summary>
		/// <remarks>{TenantBase}/v2/logout</remarks>
		public static string Logout => @"https://accounts.autodesk.com/Authentication/LogOut";

		#endregion [ Properties ]

		#region [ Methods ]

		/// <summary>Reformats the scopes in the correct string format based on the selected flags</summary>
		public static string FormatScopes( ForgeScopes scope )
		{
			StringBuilder sb = new StringBuilder();

			Array all = Enum.GetValues( typeof( ForgeScopes ) );

			foreach ( ForgeScopes flag in all )
			{
				if ( scope.HasFlag( flag ) )
                {
					string desc = flag.GetDescription();
					if ( !String.IsNullOrWhiteSpace(desc) )
						sb.Append( $"{desc}%20" );
				}
			}

			sb.Length -= 3;
			return sb.ToString().ToLowerInvariant();
		}

		/// <summary>Calls this for the 2-Leg token only</summary>
		/// <returns></returns>
		public async Task<APIResponse<Auth0Token>> GetAuthentication()
		{
			APIResponse<Auth0Token> results = new APIResponse<Auth0Token>();

			string body = $"grant_type=client_credentials&scope={_scopes}";

            //&client_id={this.Client.ClientID}&client_secret={this.Client.Secret}

			// start a message request
            HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Post, $"{TenantBase}/{TenantRoute}/token" );
			message.Headers.Clear();
            message.Headers.Add( "Accept", "application/json" );

            // set Authorization
            string authToken = Convert.ToBase64String( Encoding.UTF8.GetBytes( $"{this.Client.ClientID}:{this.Client.Secret}" ) );
            message.Headers.Authorization = new AuthenticationHeaderValue( "Basic", authToken );

			// set the body
            message.Content = new StringContent( body, Encoding.UTF8, "application/x-www-form-urlencoded" );
			byte[] bytes = Encoding.UTF8.GetBytes( body );
			message.Content.Headers.Add( "Content-Length", bytes.Length.ToString() );

			try
			{
				HttpResponseMessage httpResponse = await new HttpClient().SendAsync( message );
				results.StatusCode = httpResponse.StatusCode;
				string response = await httpResponse.Content.ReadAsStringAsync();

				// deserialize the response
				results.Data = JsonConvert.DeserializeObject<Auth0Token>( response );
			}
			catch ( System.Exception exp )
			{
				results.Exception = exp;
				return results;
			}

			return results;
		}

		/// <summary>Call this for the 3-leg token Authorization with a supplied login code</summary>
		/// <returns></returns>
		/// <remarks>User ID is in the JWT</remarks>
		public async Task<APIResponse<Auth0Token>> GetAuthorization( string code )
		{
            APIResponse<Auth0Token> results = new APIResponse<Auth0Token>();

            if ( String.IsNullOrWhiteSpace( code ) )
				return results;

			string body = $"grant_type=authorization_code&code={code.Trim()}&redirect_uri={this.Client.CallbackURL}&scope={this._scopes}";

            // start a message request
            HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Post, $"{TenantBase}/{TenantRoute}/token" );
			message.Headers.Clear();
			message.Headers.Add("Accept", "application/json");

			// set Authorization
			string authToken = Convert.ToBase64String( Encoding.UTF8.GetBytes( $"{this.Client.ClientID}:{this.Client.Secret}") );
            message.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken );

			// set content
			message.Content = new StringContent( body, Encoding.UTF8, "application/x-www-form-urlencoded" );
			byte[] bytes = Encoding.UTF8.GetBytes( body );
			message.Content.Headers.Add( "Content-Length", bytes.Length.ToString() );

			try
			{
				HttpResponseMessage httpResponse = await new HttpClient().SendAsync( message );
				results.StatusCode = httpResponse.StatusCode;
				string response = await httpResponse.Content.ReadAsStringAsync();

				if ( httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest )
				{
					AuthenticationError? error = JsonConvert.DeserializeObject<AuthenticationError>( response );
					results.Debug = $"{error?.Message}";
					return results;
				}

				// deserialize the response
				results.Data = JsonConvert.DeserializeObject<Auth0Token>( response );

				// set the expire date if token was returned
				if ( !String.IsNullOrWhiteSpace( results.Data?.Access ) )
				{
					;
				}
			}
			catch ( System.Exception exp )
			{
				results.Exception = exp;
				return results;
			}

			return results;
		}

		/// <summary>Gets the User's Info from Autodesk Cloud</summary>
		/// <param name="token"></param>
		/// <returns></returns>
		/// <remarks>Get (none) user context required</remarks>
		public async Task<APIResponse<AutodeskUser>> GetUserInfo( string token )
		{
			if ( String.IsNullOrWhiteSpace( token ) )
				return null;

			APIResponse<AutodeskUser> results = new APIResponse<AutodeskUser>();

			HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Get, $"{TenantBase}/userprofile/v1/users/@me" );
			message.Headers.Clear();
			message.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", token );

			try
			{
				HttpResponseMessage httpResponse = await new HttpClient().SendAsync( message );
				results.StatusCode = httpResponse.StatusCode;
				string response = await httpResponse.Content.ReadAsStringAsync();

				if ( !results.IsSuccess )
				{
					AuthenticationError error = JsonConvert.DeserializeObject<AuthenticationError>( response );
					return results;
				}

				// deserialize the response
				results.Data = JsonConvert.DeserializeObject<AutodeskUser>( response );
			}
			catch ( System.Exception exp )
			{
				results.Exception = exp;
				return results;
			}

			return results;
		}

		/// <summary>Uses the current refresh token to aquire a new refresh token after a token has expired</summary>
		/// <param name="refreshToken">The current refresh token</param>
		/// <returns></returns>
		public async Task<APIResponse<Auth0Token>> GetRefreshToken( string refreshToken )
		{
			if ( String.IsNullOrWhiteSpace( refreshToken ) )
				return null;

			APIResponse<Auth0Token> results = new APIResponse<Auth0Token>();

			string body = $"grant_type=refresh_token&client_id={this.Client.ClientID}&client_secret={this.Client.Secret}&refresh_token={refreshToken.Trim()}";

			HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Post, $"{TenantBase}/authentication/v1/refreshtoken" );
			message.Headers.Clear();
			message.Content = new StringContent( body, Encoding.UTF8, "application/x-www-form-urlencoded" );
			byte[] bytes = Encoding.UTF8.GetBytes( body );
			message.Content.Headers.Add( "Content-Length", bytes.Length.ToString() );

			try
			{
				HttpResponseMessage httpResponse = await new HttpClient().SendAsync( message );
				results.StatusCode = httpResponse.StatusCode;
				string response = await httpResponse.Content.ReadAsStringAsync();

				// will return BadRequest if it can't be refreshed any longer
				if ( httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest )
				{
					AuthenticationError error = JsonConvert.DeserializeObject<AuthenticationError>( response );
					results.Debug = $"{error.Message}";
					return results;
				}

				// deserialize the response
				results.Data = JsonConvert.DeserializeObject<Auth0Token>( response );

				// set the token if one was returned
				if ( !String.IsNullOrWhiteSpace( results?.Data.Access ) )
				{
					;
				}

			}
			catch ( System.Exception exp )
			{
				results.Exception = exp;
				return results;
			}

			return results;
		}

		/// <summary>Retrieves the JWKS public keys </summary>
        /// <returns></returns>
		public async Task<APIResponse<KeyChain>> GetKeys()
        {
			APIResponse<KeyChain> results = new APIResponse<KeyChain>();

			HttpRequestMessage message = new HttpRequestMessage( HttpMethod.Get, $"{TenantBase}/authentication/v2/keys" );
			message.Headers.Clear();

			try
			{
				HttpResponseMessage httpResponse = await new HttpClient().SendAsync( message );
				results.StatusCode = httpResponse.StatusCode;
				string response = await httpResponse.Content.ReadAsStringAsync();

				if ( httpResponse.IsSuccessStatusCode )
					results.Data = JsonConvert.DeserializeObject<KeyChain>( response );
			}
			catch ( System.Exception exp )
			{
				results.Exception = exp;
				return results;
			}

			return results;
		}

		#endregion [ Methods ]
	}
}
