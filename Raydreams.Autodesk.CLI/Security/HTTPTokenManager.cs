using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Raydreams.Autodesk.IO;
using Raydreams.Autodesk.Model;
using Raydreams.Autodesk.CLI.Web;
using Raydreams.Autodesk.Security;

namespace Raydreams.Autodesk.CLI.Security
{
	/// <summary>Handles getting a token using the local web server HTTP redirect method routed through the proxy's login</summary>
	/// <remarks>Equivalent to a 3-leg token should be called the ProxyTokenManager</remarks>
	public class HTTPTokenManager : ITokenManager
	{
		#region [ Construction ]

		/// <summary></summary>
		/// <param name="api">API repo class for calling the BIMrx proxy</param>
		/// <param name="filePath">Full path (with file name) to the token file including file name</param>
		/// <param name="port">The port to run the web server on which we could get from the callback URL in the Auth Manager</param>
		public HTTPTokenManager( IAuthManager api, string filePath, int port = 5005 )
		{
			this.ProxyManager = api;
			this.TokenFilePath = filePath;
			this.Port = port;
		}

		#endregion [ Construction ]

		#region [ Properties ]

		/// <summary>The in memory token</summary>
		public Auth0Token? Token { get; protected set; }

		/// <summary>The Auth Manager</summary>
		public IAuthManager ProxyManager { get; set; }

		/// <summary>port to listen on</summary>
		public int Port { get; set; } = 5005;

		/// <summary></summary>
		public bool IsTokenValid => this.Token != null && !String.IsNullOrWhiteSpace( this.Token.Access )
			&& ( DateTimeOffset.UtcNow < this.Token.ExpiresOn );

		/// <summary>The token writer to use</summary>
		/// <remarks>When Encrypted = false the token will be written in plain text</remarks>
		public ITokenIO Writer { get; set; } = new MemoryTokenIO();

		/// <summary>Full path to get/put the token file</summary>
		public string TokenFilePath { get; set; }

		/// <summary>URL to browse to the proxy for login</summary>
		public string URL => $"{this.ProxyManager.Login}&m=redirect&p={this.Port}";

		#endregion [ Properties ]

		#region [ Methods ]

		/// <summary>Calls getting a token</summary>
		public async Task<string?> GetTokenAsync()
		{
			if ( this.IsTokenValid )
				return this.Token?.Access;

			// read an existing token file
			if ( ( this.Token = Writer.ReadToken( this.TokenFilePath ) ) != null )
			{
				// is no longer valid then try refresh
				if ( DateTimeOffset.UtcNow > this.Token.ExpiresOn )
				{
					//this.Logger.Log( "Attempting to refresh the current token" );

					// if refresh doesn't work then try login again
					if ( !await this.Refresh( this.Token.Refresh ) )
					{
						//this.Logger.Log( "Could not refresh token. Getting a new 3-leg token." );
						this.Token = await this.Authorize();
					}
				}

				return this.Token?.Access;
			}
			else // file does not exist - login again
			{
				//this.Logger.Log( "Getting a new 3-leg token." );
				this.Token = await this.Authorize();

				return this.Token?.Access;
			}
		}

		/// <summary>Do 3-leg auth login routed through the proxy server</summary>
		/// <returns>Token</returns>
		/// <remarks>This only works going through the proxy</remarks>
		protected async Task<Auth0Token> Authorize()
		{
			// run a mini web server in the background - need to validate the port
			MoreServer server = new MoreServer(this.Port);// { Logger = this.Logger };

			// when the redirect is returned
			string? code = null;
			server.AuthRedirect += ( string? c ) => {
				code = c;
				server.Shutdown();
			};

			// run the server in the background
			Task serverTask = Task.Run( async () =>
			{
				await server.Serve();
			} );

			// spawn the default web browser to the proxy Login URL (doesn't block)
			Process? process = Process.Start( new ProcessStartInfo( this.URL ) { UseShellExecute = true } );

			// wait for the mini server to be shutdown
			serverTask.Wait();

			// do a POST to get the final token
			if ( !String.IsNullOrWhiteSpace(code) )
			{
				APIResponse<Auth0Token> results = await this.ProxyManager.GetAuthorization(code);

				if (results.IsSuccess && results.Data != null)
				{
                    // write the token to a file
                    //this.Logger.Log( $"Writing 3-leg token to file {this.TokenFilePath}" );
                    await this.Writer.WriteToken(results.Data, this.TokenFilePath);
					return results.Data;
				}
            }

			return new Auth0Token();
			
		}

		/// <summary>Get the Users Info using the @me method</summary>
		/// <returns></returns>
		public async Task<AutodeskUser> GetUser()
		{
			if ( this.ProxyManager == null )
				return null;

			// if the token is not set we need to get it again
			await this.GetTokenAsync();
			APIResponse<AutodeskUser> results = await this.ProxyManager.GetUserInfo( this.Token.Access );

			return ( results.IsSuccess ) ? results.Data : null;
		}

		/// <summary>Removes any existing token</summary>
		public void Reset()
		{
			this.Token = null;
		}

		/// <summary>Similar to Authorize but no need to prompt for credentials</summary>
		/// <param name="token"></param>
		/// <returns></returns>
		protected async Task<bool> Refresh( string token )
		{
			if ( this.ProxyManager == null )
				return false;

			// call refresh via the proxy itsef
			APIResponse<Auth0Token> replacement = await this.ProxyManager.GetRefreshToken( token );

			// save the token locally if all went well
			if ( replacement.IsSuccess && replacement.Data != null && !String.IsNullOrWhiteSpace( replacement.Data.Access ) )
			{
				// write the token in the background and return true
				this.Token = replacement.Data;
				_ = this.Writer.WriteToken( replacement.Data, this.TokenFilePath );
				return true;
			}

			// have to login in again
			return false;
		}

		#endregion [ Methods ]
	}
}
