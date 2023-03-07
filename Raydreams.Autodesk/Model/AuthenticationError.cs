using System;
using Newtonsoft.Json;

namespace Raydreams.Autodesk.Model
{
    /// <summary>Seems to be the basic request level error object</summary>
    /// <remarks>https://forge.autodesk.com/en/docs/oauth/v2/developers_guide/error_handling/</remarks>
    public class AuthenticationError
    {
        [JsonProperty("developerMessage")]
        public string Message { get; set; }

        [JsonProperty("userMessage")]
        public string UserMessage { get; set; }

        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("moreinfo")]
        public string MoreInfo { get; set; }
    }
}

