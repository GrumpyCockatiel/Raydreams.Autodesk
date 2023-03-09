using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raydreams.Autodesk.Extensions;
using Raydreams.Autodesk.Serializers;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Raydreams.Autodesk.Model
{
    /// <summary>Types of logical operators</summary>
    /// <remarks>Integer Value is the precedence where AND, OR and NOT always beat left</remarks>
    public enum OperatorType
    {
        OR = 0,
        /// <summary>AND operator </summary>
        AND = 1,
        /// <summary>NOT operator</summary>
        /// <remarks>For now, Not is handled by the filter type</remarks>
        NOT = 2
    }

    /// <summary>Account User Roles</summary>
    public enum AccountUserRole
	{
		[EnumMember( Value = "" )]
		None = 0,
		[EnumMember( Value = "project_admin" )]
		ProjectAdmin,
		[EnumMember( Value = "account_user" )]
		AccountUser,
		[EnumMember( Value = "account_admin" )]
		AccountAdmin
	}

	/// <summary>A Page results of project users</summary>
	public class AccountUserFilters
	{
		[JsonProperty( "email" )]
		public string Email { get; set; }

		[JsonProperty( "name" )]
		public string Name { get; set; }

		[JsonProperty( "company_name" )]
		public string CompanyName { get; set; }

		[JsonProperty( "operator" )]
		[JsonConverter( typeof( StringEnumConverter ) )]
		public OperatorType Operator { get; set; } = OperatorType.OR;

		[JsonProperty( "partial" )]
		public bool Partial { get; set; } = true;

		/// <summary>Needs at least 1 filter to be valid</summary>
		public bool IsValid => !new string[] { this.Email, this.Name, this.CompanyName }.IsAllNullOrWhiteSpace();
	}

	/// <summary>A BIM360 Account User Object</summary>
	public class AccountUser
	{
		[JsonProperty( "id" )]
		public Guid ID { get; set; }

		/// <summary>Hub/Account ID</summary>
		[JsonProperty( "account_id" )]
		[JsonConverter( typeof( ForgeIDConverter ) )]
		public ForgeID AccountID { get; set; }

		[JsonProperty( "status" )]
		public string Status { get; set; }

		[JsonProperty( "role" )]
		public string Role { get; set; }

		[JsonProperty( "company_id" )]
		[JsonConverter( typeof( NullableGuidConverter ) )]
		public Guid? CompanyID { get; set; }

		[JsonProperty( "company_name" )]
		public string CompanyName { get; set; }

		[JsonProperty( "last_sign_in" )]
		[JsonConverter( typeof( NullableDateTimeOffsetConverter ) )]
		public DateTimeOffset? LastSignIn { get; set; }

		[JsonProperty( "email" )]
		public string Email { get; set; }

		[JsonProperty( "name" )]
		public string Name { get; set; }

		[JsonProperty( "nickname" )]
		public string Nickname { get; set; }

		[JsonProperty( "first_name" )]
		public string FirstName { get; set; }

		[JsonProperty( "last_name" )]
		public string LastName { get; set; }

		[JsonProperty( "uid" )]
		public string UserID { get; set; }

		[JsonProperty( "image_url" )]
		public string ImageURL { get; set; }

		[JsonProperty( "address_line_1" )]
		public string Address1 { get; set; }

		[JsonProperty( "address_line_2" )]
		public string Address2 { get; set; }

		[JsonProperty( "city" )]
		public string City { get; set; }

		[JsonProperty( "state_or_province" )]
		public string StateProvince { get; set; }

		[JsonProperty( "postal_code" )]
		public string PostalCode { get; set; }

		[JsonProperty( "country" )]
		public string Country { get; set; }

		[JsonProperty( "phone" )]
		public string Phone { get; set; }

		[JsonProperty( "company" )]
		public string Company { get; set; }

		[JsonProperty( "job_title" )]
		public string JobTitle { get; set; }

		[JsonProperty( "industry" )]
		public string Industry { get; set; }

		[JsonProperty( "about_me" )]
		public string AboutMe { get; set; }

		[JsonProperty( "created_at" )]
		public DateTimeOffset Created { get; set; }

		[JsonProperty( "updated_at" )]
		public DateTimeOffset Updated { get; set; }

		[JsonProperty( "default_role" )]
		public string DefaultRole { get; set; }

		[JsonProperty( "default_role_id" )]
		[JsonConverter( typeof( NullableGuidConverter ) )]
		public Guid? DefaultRoleID { get; set; }
	}

	/// <summary>The results of a bulk insert of account users</summary>
	public class AccountUsersInsert
	{
		[JsonProperty( "success" )]
		public int Success { get; set; }

		[JsonProperty( "failure" )]
		public int Failure { get; set; }

		[JsonProperty( "success_items" )]
		public List<AccountUser> SuccessItems { get; set; }

		[JsonProperty( "failure_items" )]
		public List<AccountUser> FailureItems { get; set; }
	}
}
