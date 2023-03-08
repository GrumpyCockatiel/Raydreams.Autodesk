using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Raydreams.Autodesk.Model
{
    /// <summary>Forge DM Object specific to creating a storage request</summary>
    /// <remarks>We need a Factory class to spits out the correct Forge Object JSON based on the operation</remarks>
    public class CreateStorageRequest
    {
        /// <summary></summary>
        public CreateStorageRequest()
        {
            this.JsonAPI = new JSONAPI();
            this.Data = new CreateObjectData();
        }

        /// <summary></summary>
        /// <param name="fileName">The name to use as the filename on the uploaded bytes</param>
        /// <param name="folderID">ID of the folder to store it in</param>
        public CreateStorageRequest( string fileName, string folderID )
        {
            if ( String.IsNullOrWhiteSpace( fileName ) || String.IsNullOrWhiteSpace( fileName ) )
                throw new System.ArgumentNullException( "All arguments are required." );

            this.JsonAPI = new JSONAPI();
            this.Data = new CreateObjectData();
            this.Data.Attributes.Name = fileName;
            this.Data.Relationships.Target.Data = new Data { Type = "folders", ID = folderID };
        }

        [JsonProperty( "jsonapi" )]
        public JSONAPI JsonAPI { get; set; }

        [JsonProperty( "data" )]
        public CreateObjectData Data { get; set; }
    }

    /// <summary></summary>
    public class CreateObjectData
    {
        public CreateObjectData()
        {
            this.Type = ForgeObjectType.Objects;
            this.Attributes = new CreateAttributes();
            this.Relationships = new CreateRelationships();
        }

        /// <summary>The Forge Object Type name should match an enum value</summary>
        [JsonProperty( "type" )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ForgeObjectType Type { get; set; } = ForgeObjectType.Unknown;

        [JsonProperty( "attributes" )]
        public CreateAttributes Attributes { get; set; }

        [JsonProperty( "relationships" )]
        public CreateRelationships Relationships { get; set; }
    }

    /// <summary></summary>
    public class CreateAttributes
    {
        [JsonProperty( "name" )]
        public string Name { get; set; }
    }

    /// <summary></summary>
    public class CreateRelationships
    {
        public CreateRelationships()
        {
            this.Target = new DataRelationship();
        }

        [JsonProperty( "target" )]
        public DataRelationship Target { get; set; }
    }

    /// <summary>Only has a Data property only</summary>
    public class DataRelationship
    {
        [JsonProperty( "data" )]
        public Data Data { get; set; }
    }
}

