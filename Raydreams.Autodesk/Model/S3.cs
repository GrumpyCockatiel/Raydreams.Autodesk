using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Raydreams.Autodesk.Model
{
    /// <summary></summary>
    public enum S3Status
    {
        /// <summary></summary>
        Complete = 0,

        /// <summary></summary>
        Chunked = 1,

        /// <summary></summary>
        Fallback = 2,
    }

    /// <summary>A download link</summary>
    public class S3SignedDownload
    {
        [JsonConverter( typeof( StringEnumConverter ) )]
        [JsonProperty( "status" )]
        public S3Status Status { get; set; }

        /// <summary>The download URL</summary>
        [JsonProperty( "url" )]
        public string URL { get; set; }

        [JsonProperty( "params" )]
        public FileParams Parameters { get; set; }

        /// <summary>File Size</summary>
        [JsonProperty( "size" )]
        public long Size { get; set; }

        /// <summary>SHA1 Sig of the file</summary>
        [JsonProperty( "sha1" )]
        public string Signature { get; set; }
    }

    /// <summary></summary>
    public class FileParams
    {
        [JsonProperty( "content-type" )]
        public string ContentType { get; set; }

        [JsonProperty( "content-disposition" )]
        public string ContentDisposition { get; set; }

        [JsonProperty( "cache-control" )]
        public string CacheControl { get; set; }
    }

    /// <summary></summary>
    public class S3SignedUpload
    {
        /// <summary>Need this to complete the upload</summary>
        [JsonProperty( "uploadKey" )]
        public string UploadKey { get; set; }

        [JsonProperty( "uploadExpiration" )]
        public DateTimeOffset UploadExpiration { get; set; }

        [JsonProperty( "urlExpiration" )]
        public DateTimeOffset URLExpiration { get; set; }

        [JsonProperty( "urls" )]
        public List<string> URLs { get; set; }
    }

    /// <summary></summary>
    public class S3SignedUploadComplete
    {
        /// <summary></summary>
        [JsonProperty( "bucketKey" )]
        public string BucketKey { get; set; }

        /// <summary></summary>
        [JsonProperty( "objectId" )]
        public string ObjectID { get; set; }

        /// <summary></summary>
        [JsonProperty( "objectKey" )]
        public string ObjectKey { get; set; }

        [JsonProperty( "content-type" )]
        public string ContentType { get; set; }

        [JsonProperty( "size" )]
        public long Size { get; set; } = 0;

        [JsonProperty( "location" )]
        public string Location { get; set; }
    }

}
