using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Raydreams.Autodesk.CLI.Model;

namespace Raydreams.Autodesk.CLI.Security
{
	/// <summary>Token Manager Interface</summary>
	public interface ITokenManager
	{
		/// <summary>The actual token once retrieved</summary>
		Auth0Token? Token { get; }

		/// <summary>Get and retain a token of some kind</summary>
		Task<string?> GetTokenAsync();

		/// <summary>Gets the user's Autodesk ID</summary>
		Task<AutodeskUser> GetUser();

		/// <summary>Clear the token</summary>
		void Reset();
	}
}
