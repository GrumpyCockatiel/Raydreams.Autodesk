using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Raydreams.Autodesk.Model
{
	/// <summary>An Auth0 User</summary>
    /// <remarks>
	/// This is a terrible name
	/// Basing this on IdentityUser is good for a web app but causes several issues with Azure Functions
    /// To use as the IdentityUser you need AspNetCore.Identity and Extensions.IdentityStores
    /// </remarks>
	public class AutodeskUser
	{
		/// <summary>The user's primary ID</summary>
		[JsonProperty( "userId" )]
		public string UserID { get; set; }

		/// <summary>The user's name</summary>
		[JsonProperty( "userName" )]
		public string Name { get; set; }

		/// <summary>first name</summary>
		[JsonProperty( "firstName" )]
		public string FirstName { get; set; }

		/// <summary>last name</summary>
		[JsonProperty( "lastName" )]
		public string LastName { get; set; }

		/// <summary>email</summary>
		[JsonProperty( "emailId" )]
		//public override string Email { get { return base.Email; } set { base.Email = value; } }
		public string Email { get; set; }

		/// <summary>Any claims to store</summary>
		/// <remarks>Currently not used but we could append claims</remarks>
		[JsonIgnore]
		public Claim[] Claims => new[] { new Claim( ClaimTypes.Email, this.Email ),
			new Claim( ClaimTypes.Name, this.Name ) };
	}
}
