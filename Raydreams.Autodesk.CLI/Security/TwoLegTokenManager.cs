using System;
using System.Threading.Tasks;
using Raydreams.Autodesk.CLI.Model;

namespace Raydreams.Autodesk.CLI.Security
{
	/// <summary>Handles the 2-Leg token auth workflow</summary>
	/// <remarks>Does not save the token to file right now since it has to be retrieved every hour any way.</remarks>
	public class TwoLegTokenManager : ITokenManager
	{
		public TwoLegTokenManager( IAuthManager mgr )
		{
			this.AuthRepo = mgr;
		}

		#region [ Properties ]

		/// <summary>The auth repo is the data comm class that handles getting the token</summary>
		public IAuthManager AuthRepo { get; set; }

		/// <summary>Current in memory token</summary>
		/// <remarks>Lock setting the token</remarks>
		public Auth0Token? Token { get; protected set; }

		/// <summary>Looks at the current in memory token to see if its still valid</summary>
		public bool IsTokenValid => this.Token != null && !String.IsNullOrWhiteSpace( this.Token.Access )
			&& ( DateTimeOffset.UtcNow < this.Token.ExpiresOn );

		/// <summary></summary>
		//public ILogger Logger { get; set; } = new NullLogger();

		#endregion [ Properties ]

		/// <summary>Adds an ILogger to this instance</summary>
		/// <param name="logger"></param>
		/// <returns></returns>
		//public TwoLegTokenManager AddLogger( ILogger logger )
		//{
		//	if ( logger != null )
		//		this.Logger = logger;

		//	return this;
		//}

		/// <summary>Get token async</summary>
		/// <remarks>Tokens are only stored in memory for now since there is really no point to store them locally</remarks>
		public async Task<string?> GetTokenAsync()
		{
			// check the local token
			if ( this.IsTokenValid )
				return this.Token?.Access;

			// get a new token
			//this.Logger.Log( "Getting a new 2-leg token." );
			APIResponse<Auth0Token> login = await this.AuthRepo.GetAuthentication();

			// save the token locally if all went well
			// usually expires in 1 hour
			if ( login.IsSuccess && login.Data != null && !String.IsNullOrWhiteSpace( login.Data.Access ) )
			{
				this.Token = login.Data;

				return this.Token.Access;
			}

			// never could get a token
			return null;
		}

		/// <summary>There's no user for a 2-leg token so better to return a null to tell the repo to NOT add this</summary>
		/// <returns>Right throws an exception so we can see where this might occur</returns>
		public async Task<AutodeskUser> GetUser()
		{
			//this.Logger.Log( "Call to GetUser from a two leg token manager which is invalid. Investigate why this happened.", LogLevel.Error );
			
			return await Task.FromResult<AutodeskUser>(null);
		}

		/// <summary>Removes any existing token</summary>
		public void Reset()
		{
			this.Token = null;
		}
	}
}

/// <summary>Get token sync</summary>
/// <remarks>Tokens are only stored in memory for now</remarks>
//public string GetToken()
//{
//	if ( this.IsTokenValid )
//		return this.Token.Access;

//	// authorize
//	this.Logger.Log( "Getting a new 2-leg token." );
//	APIResponse<Auth0Token> login = this.AuthRepo.GetAuthentication().GetAwaiter().GetResult();

//	// save the token locally if all went well
//	if ( login.IsSuccess && login.Data != null && !String.IsNullOrWhiteSpace( login.Data.Access ) )
//	{
//		this.Token = login.Data;
//		// write token here
//		return this.Token.Access;
//	}

//	// never could get a token
//	return null;
//}
