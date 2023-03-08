using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raydreams.Autodesk.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Raydreams.Autodesk.Model
{
	/// <summary>Specifies which Project Type to use in the URN IDs</summary>
	/// <remarks>
	/// hubs:autodesk.core:Hub (team hubs), hubs:autodesk.bim360:Account (BIM 360 Docs accounts), hubs:autodesk.a360:PersonalHub
	/// </remarks>
	public enum ProjectType
	{
		[Description( "items:autodesk.core:File" )]
		Core = 0,
		[Description( "items:autodesk.bim360:File" )]
		BIM360 = 1,
		//[Description( " hubs:autodesk.a360:PersonalHub" )]
		//Personal = 2,
	}

	/// <summary>Forge Object specific to inserting version 1 of an item</summary>
	public class InsertItemRequest
	{
		/// <summary></summary>
		public InsertItemRequest()
		{
			this.JsonAPI = new JSONAPI();
			this.Data = new InsertItemData();
			this.Included = new List<IncludeData>();
		}

        /// <summary></summary>
        /// <param name="objID">urn:adsk.objects:os.object:wip.dm.prod/2a6d61f2-49df-4d7b-9aed-439586d61df7.jpg</param>
        public InsertItemRequest( string fileName, string folderID, string objID, ProjectType type = ProjectType.BIM360 )
		{
			if ( new string[] { fileName, folderID, objID }.IsAnyNullOrWhiteSpace() )
				throw new System.ArgumentNullException( "All arguments are required." );

			string pt = $"autodesk.{type.ToString().ToLowerInvariant()}";

			this.JsonAPI = new JSONAPI();
			this.Data = new InsertItemData();
			this.Included = new List<IncludeData>();
			this.Data.Type = ForgeObjectType.Items;
			this.Data.Attributes.DisplayName = fileName;
			this.Data.Attributes.Extension.Type = $"items:{pt}:File";
			this.Data.Attributes.Extension.Version = "1.0";
			this.Data.Relationships.Tip.Data = new Data { Type = "versions", ID = "1" };
			this.Data.Relationships.Parent.Data = new Data { Type = "folders", ID = folderID };

			IncludeData store = new IncludeData() { Type = ForgeObjectType.Versions, ID = "1" };
			store.Attributes.Name = fileName;
			store.Attributes.Extension = new InsertItemExtension { Type = $"versions:{pt}:File", Version = "1.0" };
			store.Relationships.Storage = new DataRelationship { Data = new Data { Type = "objects", ID = objID } };
			this.Included.Add( store );
		}

		[JsonProperty( "jsonapi" )]
		public JSONAPI JsonAPI { get; set; }

		[JsonProperty( "data" )]
		public InsertItemData Data { get; set; }

		[JsonProperty( "included" )]
		public List<IncludeData> Included { get; set; }
	}

	/// <summary></summary>
	public class InsertItemData
	{
		public InsertItemData()
		{
			this.Type = ForgeObjectType.Items;
			this.Attributes = new InsertItemAttributes();
			this.Relationships = new InsertItemRelationships();
		}

		/// <summary>The Forge Object Type name should match an enum value</summary>
		[JsonProperty( "type" )]
		[JsonConverter( typeof( StringEnumConverter ) )]
		public ForgeObjectType Type { get; set; }

		[JsonProperty( "attributes" )]
		public InsertItemAttributes Attributes { get; set; }

		[JsonProperty( "relationships" )]
		public InsertItemRelationships Relationships { get; set; }
	}

	/// <summary></summary>
	public class InsertItemAttributes
	{
		public InsertItemAttributes()
		{
			this.Extension = new InsertItemExtension();
		}

		[JsonProperty( "displayName" )]
		public string DisplayName { get; set; }

		[JsonProperty( "extension" )]
		public InsertItemExtension Extension { get; set; }
	}

	/// <summary></summary>
	public class InsertItemExtension
	{
		[JsonProperty( "type" )]
		public string Type { get; set; }

		[JsonProperty( "version" )]
		public string Version { get; set; }
	}

	/// <summary></summary>
	public class InsertItemRelationships
	{
		public InsertItemRelationships()
		{
			this.Tip = new DataRelationship();
			this.Parent = new DataRelationship();
		}

		/// <summary>This is the last/latest version of the file</summary>
		[JsonProperty( "tip" )]
		public DataRelationship Tip { get; set; }

		[JsonProperty( "parent" )]
		public DataRelationship Parent { get; set; }
	}

	/// <summary></summary>
	public class IncludeData
	{
		public IncludeData()
		{
			this.ID = "1";
			this.Type = ForgeObjectType.Versions;
			this.Attributes = new IncludeAttributes();
			this.Relationships = new IncludeRelationships();
		}

		[JsonProperty( "id" )]
		public string ID { get; set; }

		/// <summary>The Forge Object Type name should match an enum value</summary>
		[JsonProperty( "type" )]
		[JsonConverter( typeof( StringEnumConverter ) )]
		public ForgeObjectType Type { get; set; }

		[JsonProperty( "attributes" )]
		public IncludeAttributes Attributes { get; set; }

		[JsonProperty( "relationships" )]
		public IncludeRelationships Relationships { get; set; }
	}

	/// <summary></summary>
	public class IncludeAttributes
	{
		public IncludeAttributes()
		{
			this.Extension = new InsertItemExtension();
		}

		[JsonProperty( "name" )]
		public string Name { get; set; }

		[JsonProperty( "extension" )]
		public InsertItemExtension Extension { get; set; }
	}

	/// <summary></summary>
	public class IncludeRelationships
	{
		public IncludeRelationships()
		{
			this.Storage = new DataRelationship();
		}

		[JsonProperty( "storage" )]
		public DataRelationship Storage { get; set; }
	}
}
