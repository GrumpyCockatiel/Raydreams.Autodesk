using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raydreams.Autodesk.CLI.Extensions;
using Raydreams.Autodesk.CLI.Serializers;

namespace Raydreams.Autodesk.CLI.Model
{
    /// <summary>Regions Codes</summary>
    public enum RegionCode
    {
        /// <summary>US</summary>
        US = 0,
        /// <summary>Europe</summary>
        EU = 1,
        /// <summary>Europe, Middle East and Africa</summary>
        EMEA = 2
    };

    /// <summary>A DTO tuple of basic Hub Account Information</summary>
    public class HubAccount
    {
        public HubAccount()
        { }

        public HubAccount( ForgeObject acct )
        {
            ID = new ForgeID( acct.ID );
            Name = acct.Attributes.GetName;
            Type = acct.Attributes.Extension.Type;
            Region = acct.Attributes.Region.GetEnumValue<RegionCode>(RegionCode.US, true);
        }

        /// <summary>The Name of the Hub/Account</summary>
        [JsonProperty( "name" )]
        public string Name { get; set; } = String.Empty;

        /// <summary>The ID of the Hub/Account</summary>
        [JsonProperty("id")]
        [JsonConverter(typeof(ForgeIDConverter))]
        public ForgeID ID { get; set; } = new ForgeID(Guid.Empty);

        /// <summary>The type of Account BIM360 or Core</summary>
        [JsonProperty("type")]
        public string Type { get; set; } = "core";

        /// <summary>What region is the Hub in</summary>
        [JsonProperty( "region" )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public RegionCode Region { get; set; }
    }

    /// <summary>A DTO for just Project ID/Name for Projects that come back from BIM360</summary>
    /// <remarks>Getting very similar to BIMrxProject object type</remarks>
    public class ProjectStub
    {
        public ProjectStub()
        { }

        public ProjectStub( ForgeObject proj )
        {
            ID = new ForgeID(proj.ID);
            Name = proj.Attributes.GetName;
            RootFolder = proj.Relationships.RootFolder.Data.ID;
            Platform = proj.Attributes.Extension.TryGetPlatform();
        }

        /// <summary>The Project ID enforced as a GUID</summary>
        /// <remarks>Change to AutodeskID</remarks>
        [JsonProperty("id")]
        [JsonConverter(typeof(ForgeIDConverter))]
        public ForgeID ID { get; set; } = new ForgeID(Guid.Empty);

        /// <summary>The Name of the Project</summary>
        [JsonProperty("name")]
        public string Name { get; set; } = String.Empty;

        /// <summary>The URN ID of the root folder this user can access</summary>
        [JsonProperty( "root" )]
        public string RootFolder { get; set; } = String.Empty;

        [JsonProperty( "platform" )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public PlatformType Platform { get; set; } = PlatformType.Undetermined;

        /// <summary>The current status of the project</summary>
        /// <remarks>May only get from BIM360</remarks>
        //[JsonProperty( "status" )]
        //[JsonConverter( typeof( StringEnumConverter ) )]
        //public ProjectStatus Status { get; set; }
    }
}

