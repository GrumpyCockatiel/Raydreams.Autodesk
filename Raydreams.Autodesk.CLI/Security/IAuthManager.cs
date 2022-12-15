using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Raydreams.Autodesk.CLI.Model;

namespace Raydreams.Autodesk.CLI.Security
{
	/// <summary>Interface for A+A against BIM360</summary>
	public interface IAuthManager
	{
		/// <summary>Set the Autodesk scopes to apply</summary>
		ForgeScopes Scopes { set; }

		/// <summary>The URL to navigate to for login</summary>
		string Login { get; }

		/// <summary>Performs a 2 leg token handshake</summary>
		Task<APIResponse<Auth0Token>> GetAuthentication();

		/// <summary>Performs a 3 leg token handshake (Authorization + Access)</summary>
		Task<APIResponse<Auth0Token>> GetAuthorization( string code );

		/// <summary>Calls the get user info method</summary>
		Task<APIResponse<AutodeskUser>> GetUserInfo( string token );

		/// <summary>Calls the refresh method</summary>
		/// <param name="refreshToken">the refresh token string</param>
		/// <returns></returns>
		Task<APIResponse<Auth0Token>> GetRefreshToken( string refreshToken );

		/// <summary>Calls the method that gets the public key to check a JWT has been signed correctly</summary>
		Task<APIResponse<KeyChain>> GetKeys();
	}
}
