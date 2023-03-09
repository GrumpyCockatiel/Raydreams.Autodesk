using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Raydreams.Autodesk.Model;

namespace Raydreams.Autodesk.Data
{
	/// <summary>BIM 360 Interface Class</summary>
	/// <remarks>Needs to be renamed to IBIM360API</remarks>
	public interface IBIM360Repository
	{
		string Impersonation { get; set; }

		/// <summary>List all the account users on the specified account</summary>
		Task<APIResponse<List<AccountUser>>> ListAccountUsers( string accountID, int limit = -1, int offset = -1 );

		/// <summary>Returns ALL the subjects in the specified account</summary>
		Task<List<AccountUser>> ListAllAccountUsers( ForgeID account );

		/// <summary>Get the details of a specific account user from an account</summary>
		Task<APIResponse<AccountUser>> GetAccountUser( string accountID, string userID );

		/// <summary>Searches all account users for the specified autodesk user ID</summary>
		/// <param name="accountID">Account/HUB ID</param>
		/// <param name="autodeskID">This is the 12 alphanumeric ID assigned by Forge to a user</param>
		/// <returns>An Autodesk User Account info</returns>
		Task<APIResponse<AccountUser>> GetAccountUserByID( string accountID, string autodeskID );

		/// <summary>List all the project users on the specified project that match the search</summary>
		Task<APIResponse<List<AccountUser>>> SearchAccountUsers( string accountID, AccountUserFilters filters, int limit = -1, int offset = -1 );
	}
}
								