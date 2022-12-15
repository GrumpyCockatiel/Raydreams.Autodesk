using Newtonsoft.Json;
using Raydreams.Autodesk.CLI.Extensions;
using System;

namespace Raydreams.Autodesk.CLI.Model
{
	/// <summary>An Auth0 Token</summary>
	/// <remarks>Class name needs to be changed. This came from some other Auth0 code
	/// Reconcile with TokenReponse - we don't need both
	/// ALL these properties come from the JWT itself except the Refresh Token code
	/// </remarks>
	public class Auth0Token
	{
		#region [ Fields ]

		/// <summary>The JWT token</summary>
		private string _access;

		#endregion [ Fields ]

		/// <summary>A token ID from the JWT</summary>
		[JsonProperty( "token_id" )]
		public string ID
		{
			get => ( this.JWT != null ) ? this.JWT.TokenID : null;
		}

		/// <summary>The User ID scoped in the token</summary>
		[JsonProperty( "user_id" )]
		public string UserID
		{
			get => ( this.JWT != null ) ? this.JWT.UserID : null;
		}

		/// <summary>The primary access token itself</summary>
		/// <remarks>When this property is set, the JWT will be decoded to get other values</remarks>
		[JsonProperty( "access_token" )]
		public string Access
		{
			get => this._access;
			set
			{
				if ( !String.IsNullOrWhiteSpace(value) )
				{
					this._access = value;
					this.JWT = ADJWT.Decode( value );
				}
			}
		}

		/// <summary>The refresh token ID to use to get a new token instead of re-logging back in</summary>
		/// <remarks>2 leg tokens will not have a refresh token</remarks>
		[JsonProperty( "refresh_token" )]
		public string Refresh { get; set; }

		/// <summary>Should be Bearer</summary>
		[JsonProperty( "token_type" )]
		public string Type { get; set; } = "Bearer";

		/// <summary>Number of seconds before the token expires WHEN it is fetched set by the Autodesk server.</summary>
		/// <remarks>This is never a good value to use because it has no reference. Use ExpiresOn which comes from the token itself. This may be 0 when coming from the proxy</remarks>
		[JsonProperty( "expires_in" )]
		public int Expires { get; set; }

		/// <summary>Date when the token will actually expire as found inside the JWT</summary>
		/// <remarks>Changing this in the file doesn't work since its pulled from the JWT itself</remarks>
		[JsonProperty( "expires_on" )]
		public DateTimeOffset ExpiresOn
		{
			get => ( this.JWT != null ) ? DateTimeOffset.FromUnixTimeSeconds( this.JWT.Expires ) : DateTimeOffset.MinValue;
		}

		/// <summary>The decoded JWT</summary>
		[JsonIgnore]
		public ADJWT JWT { get; protected set; }
	}

	/// <summary>Autodesk JWT is decoded from the token string itself</summary>
	public class ADJWT
	{
		/// <summary>The App Client ID used to connect</summary>
		[JsonProperty( "client_id" )]
		public string ClientID { get; set; }

		/// <summary>The audience the token is good for which is Autodesk</summary>
		[JsonProperty( "aud" )]
		public string Audience { get; set; }

		/// <summary>Autodesk's assigned token ID</summary>
		[JsonProperty( "jti" )]
		public string TokenID { get; set; }

		/// <summary>The Autodesk User ID</summary>
		[JsonProperty( "userid" )]
		public string UserID { get; set; }

		/// <summary>The scopes used on this token</summary>
		[JsonProperty( "scope" )]
		public string[] Scopes { get; set; }

		/// <summary>When the token expires in terms of Epoch Time in seconds</summary>
		[JsonProperty( "exp" )]
		public int Expires { get; set; }

		[JsonProperty( "expires_on" )]
		public DateTimeOffset ExpiresOn => DateTimeOffset.FromUnixTimeSeconds( this.Expires );

		/// <summary>Decodes/deserializes an Autodesk JWT Base64 URL encoded token</summary>
		/// <param name="encoded">The FULL JWT encoded token</param>
		/// <returns></returns>
		/// <remarks>Only the payloaded is returned</remarks>
		public static ADJWT Decode(string encoded)
		{
			if ( String.IsNullOrWhiteSpace( encoded ) )
				return null;

			string[] parts = encoded.Split( '.' );

			// only care about the payload for now
			if ( parts.Length < 2 )
				return null;

			string json = System.Text.Encoding.UTF8.GetString( parts[1].BASE64UrlDecode() );
			return JsonConvert.DeserializeObject<ADJWT>( json );
		}
	}
}
