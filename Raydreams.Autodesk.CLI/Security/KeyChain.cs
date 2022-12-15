using System;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Raydreams.Autodesk.CLI.Extensions;

namespace Raydreams.Autodesk.CLI.Security
{
    /// <summary>A collection of Keys</summary>
    public class KeyChain
    {
        [JsonProperty("keys")]
        public List<RSAKey> Keys { get; set; }

        public RSAKey GetKey(string id) => (this.Keys != null) ? this.Keys.Where(k => k.ID == id).FirstOrDefault() : null;
    }

    /// <summary>An RSA Public Key</summary>
    public class RSAKey
    {
        [JsonProperty("kty")]
        public string Algorithm { get; set; }

        [JsonProperty("kid")]
        public string ID { get; set; }

        [JsonProperty("use")]
        public string Usage { get; set; }

        [JsonProperty("n")]
        public string Modulus { get; set; }

        [JsonProperty("e")]
        public string Exponent { get; set; }

        /// <summary>From the BASE64 rep back into the byte representation</summary>
        public RSAParameters Parameters
        {
            get
            {
                return new RSAParameters
                {
                    Modulus = this.Modulus.BASE64UrlDecode(),
                    Exponent = this.Exponent.BASE64UrlDecode()
                };
            }
        }
    }
}

