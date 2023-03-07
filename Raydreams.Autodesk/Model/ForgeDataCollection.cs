using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Raydreams.Autodesk.Extensions;

namespace Raydreams.Autodesk.Model
{
    /// <summary>Project Platform type</summary>
    public enum PlatformType
    {
        /// <summary>Not set or parsed</summary>
        Undetermined = 0,
        /// <summary>A BIM360 item</summary>
        BIM360 = 1,
        /// <summary>An ACC item</summary>
        ACC = 2
    }

    /// <summary>The different type of forge object types</summary>
    /// <remarks>Forge expects all lowered values</remarks>
    public enum ForgeObjectType
    {
        [EnumMember(Value = "unknown")]
        Unknown = 0,
        [EnumMember(Value = "hubs")]
        Hubs = 1,
        [EnumMember(Value = "projects")]
        Projects = 2,
        [EnumMember(Value = "items")]
        Items = 3,
        [EnumMember(Value = "folders")]
        Folders = 4,
        [EnumMember(Value = "versions")]
        Versions = 5,
        [EnumMember(Value = "buckets")]
        Buckets = 6,
        [EnumMember(Value = "objects")]
        Objects = 7
    }

    /// <summary>A root level Forge Response Object that will have a collection of data in the Result</summary>
    /// <remarks>These are more specifically Data Management generic objects</remarks>
    public class ForgeDataCollection
    {
        [JsonProperty("jsonapi")]
        public JSONAPI jsonapi { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        /// <summary>Sadly this can be either a single object or an array of data objects depending on the call</summary>
        [JsonProperty("data")]
        public List<ForgeObject> Result { get; set; }

        [JsonProperty("meta")]
        public Meta Metadata { get; set; }

        [JsonProperty("errors")]
        public Error[] Errors { get; set; }
    }

    /// <summary>A root level Forge Response Object from data management that has one ONE data object as the result</summary>
    /// <remarks>Seems to be a single unified JSON structure at least with Data Mangement</remarks>
    public class ForgeData
    {
        public ForgeData()
        {
            this.JsonAPI = new JSONAPI();
            this.Result = new ForgeObject();
            this.Included = new List<ForgeObject>();
        }

        [JsonProperty("jsonapi")]
        public JSONAPI JsonAPI { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        /// <summary>Sadly this can be either a single object or an array of data objects depending on the call</summary>
        [JsonProperty("data")]
        public ForgeObject Result { get; set; }

        [JsonProperty("included")]
        public List<ForgeObject> Included { get; set; }

        [JsonProperty("meta")]
        public Meta Metadata { get; set; }

        [JsonProperty("errors")]
        public Error[] Errors { get; set; }
    }

    /// <summary>An error in Data Management</summary>
    public class Error
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
    }

    /// <summary></summary>
    public class ForgeObject
    {
        public ForgeObject()
        {
            this.Attributes = new Attributes();
            // remove this and see if everything still works
            this.Relationships = new Relationships();
        }

        /// <summary>The Forge Object Type name should match an enum value</summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ForgeObjectType Type { get; set; } = ForgeObjectType.Unknown;

        /// <summary>The object ID</summary>
        /// <example></example>
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("relationships")]
        public Relationships Relationships { get; set; }
    }

    /// <summary>The API version</summary>
    public class JSONAPI
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";
    }

    /// <summary>Additional Info on Links to resources</summary>
    public class Links
    {
        [JsonProperty("self")]
        public Reference Self { get; set; }

        /// <summary>Path web URL to view this item in BIM360</summary>
        [JsonProperty("webView")]
        public Reference WebView { get; set; }

        [JsonProperty("related")]
        public Reference Related { get; set; }

        [JsonProperty("first")]
        public Reference First { get; set; }

        [JsonProperty("previous")]
        public Reference Previous { get; set; }

        [JsonProperty("next")]
        public Reference Next { get; set; }
    }

    /// <summary>Anything with JUST an HREF string</summary>
    /// <remarks>WebView, Link, Schema, Related and Self 
    /// all fit this object</remarks>
    public class Reference
    {
        [JsonProperty("href")]
        public string HREF { get; set; }
    }

    /// <summary></summary>
    public class Attributes
    {
        public Attributes()
        {
            this.Extension = new Extension();
            this.CreateTime = DateTimeOffset.MinValue;
            this.LastModifiedTime = DateTimeOffset.MinValue;
        }

        /// <summary>Get whichever name is populated</summary>
        [JsonIgnore()]
        public string GetName => (!String.IsNullOrWhiteSpace(this.Name)) ? this.Name : this.DisplayName;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>The path in the remote project from root</summary>
        /// <remarks>Query string param has to be set to true</remarks>
        [JsonProperty("pathInProject")]
        public string PathInProject { get; set; }

        [JsonProperty("scopes")]
        public string[] Scopes { get; set; }

        [JsonProperty("extension")]
        public Extension Extension { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("createTime")]
        public DateTimeOffset CreateTime { get; set; }

        [JsonProperty("createUserId")]
        public string CreateUserID { get; set; }

        [JsonProperty("createUserName")]
        public string CreateUserName { get; set; }

        [JsonProperty("lastModifiedTime")]
        public DateTimeOffset LastModifiedTime { get; set; }

        [JsonProperty("lastModifiedUserId")]
        public string LastModifiedUserID { get; set; }

        [JsonProperty("lastModifiedUserName")]
        public string LastModifiedUserName { get; set; }

        [JsonProperty("LastModifiedTimeRollup")]
        public DateTimeOffset LastModifiedTimeRollup { get; set; }

        [JsonProperty("objectCount")]
        public int ObjectCount { get; set; }

        [JsonProperty("hidden")]
        public bool Hidden { get; set; }
    }

    /// <summary></summary>
    public class Relationships
    {
        [JsonProperty("projects")]
        public Relationship Projects { get; set; }

        [JsonProperty("hub")]
        public Relationship Hub { get; set; }

        [JsonProperty("rootFolder")]
        public Relationship RootFolder { get; set; }

        [JsonProperty("topFolders")]
        public Relationship TopFolders { get; set; }

        [JsonProperty("issues")]
        public Relationship Issues { get; set; }

        [JsonProperty("submittals")]
        public Relationship Submittals { get; set; }

        [JsonProperty("rfis")]
        public Relationship RFIs { get; set; }

        [JsonProperty("markups")]
        public Relationship Markups { get; set; }

        [JsonProperty("checklists")]
        public Relationship Checklists { get; set; }

        [JsonProperty("cost")]
        public Relationship Cost { get; set; }

        [JsonProperty("locations")]
        public Relationship Locations { get; set; }

        [JsonProperty("content")]
        public Relationship Content { get; set; }

        [JsonProperty("parent")]
        public Relationship Parent { get; set; }

        [JsonProperty("refs")]
        public Relationship Refs { get; set; }

        [JsonProperty("links")]
        public Relationship Links { get; set; }

        /// <summary>This is the last/latest version of the file</summary>
        [JsonProperty("tip")]
        public Relationship Tip { get; set; }

        [JsonProperty("versions")]
        public Relationship Versions { get; set; }

        /// <summary>contains the object bucket ID</summary>
        /// <remarks>storage.data.i = urn:adsk.objects:os.object:wip.dm.prod/c5cc9bf7-c06a-407e-8fc0-fe43c71fcaa1.pdf"</remarks>
        [JsonProperty("storage")]
        public Relationship Storage { get; set; }

        [JsonProperty("item")]
        public Relationship Item { get; set; }

        [JsonProperty("target")]
        public Relationship Target { get; set; }

        [JsonProperty("thumbnails")]
        public Relationship Thumbnails { get; set; }

        [JsonProperty("derivatives")]
        public Relationship Derivatives { get; set; }

        [JsonProperty("downloadFormats")]
        public Relationship DownloadFormats { get; set; }
    }

    /// <summary>Child of relationships</summary>
    public class Relationship
    {
        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    /// <summary></summary>
    public class Extension
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>Holds a Project Version as a string such as</summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("schema")]
        public Reference Schema { get; set; }

        /// <summary></summary>
        /// <remarks>
        /// "visibleTypes": ["items:autodesk.bim360:File"],
        /// "allowedTypes": ["items:autodesk.bim360:File", "folders:autodesk.bim360:Folder"]
        /// </remarks>
        [JsonProperty("data")]
        public object Data { get; set; }

        /// <summary>try to get the actual project type</summary>
        /// <returns></returns>
        public PlatformType TryGetPlatform()
        {
            JObject data = this.Data as JObject;
            if (data != null && data.ContainsKey("projectType"))
                return data["projectType"].ToString().GetEnumValue<PlatformType>(true);

            return PlatformType.Undetermined;
        }
    }

    /// <summary></summary>
    public class Meta
    {
        [JsonProperty("link")]
        public Reference Link { get; set; }

        [JsonProperty("warnings")]
        public Warning[] Warnings { get; set; }
    }

    /// <summary>Relationship data</summary>
    public class Data
    {
        /// <summary>What is it</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>Whats the ID of the object</summary>
        [JsonProperty("id")]
        public string ID { get; set; }
    }

    /// <summary></summary>
    public class Warning
    {
        [JsonProperty("Id")]
        public object ID { get; set; }

        [JsonProperty("HttpStatusCode")]
        public string HttpStatusCode { get; set; }

        [JsonProperty("ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Detail")]
        public string Detail { get; set; }

        [JsonProperty("AboutLink")]
        public object AboutLink { get; set; }

        [JsonProperty("Source")]
        public object[] Source { get; set; }

        [JsonProperty("meta")]
        public object[] Metadata { get; set; }
    }
}

