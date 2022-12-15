using System;
using Newtonsoft.Json;

namespace Raydreams.Autodesk.CLI.Model
{
    /// <summary>Encapsulates App Client info that can be stored in the BIMrx Cloud</summary>
    public class ForgeAppClient
    {
        #region [ Fields ]

        #endregion [ Fields ]

        #region [ Constructors ]

        public ForgeAppClient() : this(null, null, null)
        { }

        public ForgeAppClient(string? id, string? secret, string? redirect)
        {
            this.ClientID = id ?? String.Empty;
            this.Secret = secret ?? String.Empty;
            this.CallbackURL = redirect ?? String.Empty;
        }

        #endregion [ Constructors ]

        #region [ Properties ]

        /// <summary>Descriptive name of the client</summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>Forge App Client ID</summary>
        [JsonProperty("clientID")]
        public string ClientID { get; set; }

        /// <summary>Client secret</summary>
        [JsonProperty("secret")]
        public string Secret { get; set; }

        /// <summary>Callback URL</summary>
        [JsonProperty("callbackURL")]
        public string CallbackURL { get; set; }

        /// <summary>A description of the app client</summary>
        [JsonProperty("description")]
        public string? Description { get; set; }

        /// <summary>The primary hub/account to use with this client as a default</summary>
        [JsonProperty("primaryHubID")]
        public string? PrimaryHubID { get; set; }

        /// <summary>Contact email</summary>
        [JsonProperty("ownerEmail")]
        public string? OwnerEmail { get; set; }

        /// <summary>when the record was originally created</summary>
        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        #endregion [ Properties ]

    }
}

