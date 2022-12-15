using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raydreams.Autodesk.CLI.Extensions;
using System.Collections.ObjectModel;
using Raydreams.Autodesk.CLI.Serializers;

namespace Raydreams.Autodesk.CLI.Model
{
    /// <summary>An abstract base object for project files and folders in a tree</summary>
    /// <remarks>Needs a path property from Root to child like path = "/SomeFolder/ChildFolder
    /// </remarks>
    [JsonConverter( typeof( ProjectItemConverter ) )]
    public abstract class ProjectItem
    {
        private string _lastModedBy = null;

        /// <summary>Transform to a ProjectItem from a ForgeObject</summary>
        /// <param name="obj">The Autodesk Forge object to extract from</param>
        /// <returns></returns>
        public static ProjectItem Create( ForgeObject obj )
        {
            if ( obj.Type == ForgeObjectType.Folders )
                return new ProjectFolder
                {
                    ID = obj.ID,
                    LastModified = obj.Attributes.LastModifiedTime,
                    Name = obj.Attributes.GetName,
                    LastModifiedBy = obj.Attributes.LastModifiedUserName
                };

            if ( obj.Type == ForgeObjectType.Items )
                return new ProjectFile
                {
                    ID = obj.ID,
                    LastModified = obj.Attributes.LastModifiedTime,
                    Name = obj.Attributes.GetName,
                    Version = obj.Relationships.Tip.Data.ID.ParseVersion(),
                    LastModifiedBy = obj.Attributes.LastModifiedUserName
                };

            return null;
        }

        /// <summary>The refrential parent of this item</summary>
        /// <remarks>This may be null until a tree walk is called</remarks>
        [JsonIgnore]
        public virtual ProjectFolder Parent { get; set; }

        /// <summary>The ID of the item</summary>
        [JsonProperty( PropertyName = "id", Order = 1 )]
        public string ID { get; set; }

        /// <summary>The name of the item</summary>
        [JsonProperty( PropertyName = "name", Order = 2 )]
        public string Name { get; set; }

        /// <summary>The Autodesk type of the item</summary>
        [JsonProperty( PropertyName = "type", Order = 3 )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public abstract ForgeObjectType Type { get; }

        /// <summary>The date the item was last modified</summary>
        [JsonProperty( PropertyName = "lastModified", Order = 4 )]
        public DateTimeOffset LastModified { get; set; }

        /// <summary>The date the item was last modified</summary>
        [JsonProperty( PropertyName = "lastModifiedBy", Order = 5 )]
        public string LastModifiedBy
        {
            get => this._lastModedBy;
            set => this._lastModedBy = !String.IsNullOrWhiteSpace( value ) ? value.Trim() : null;
        }

        /// <summary>Stores the path of this item in the remote project NOT including the item itself</summary>
        /// <remarks>Will include the GUID root folder</remarks>
        [JsonProperty( PropertyName = "path", Order = 6 )]
        public virtual List<string> PathSegments { get; set; } = new List<string>();

        /// <summary>Convenience method that drops the GUID root folder from the path start</summary>
        /// <remarks>Use this to rebuild a string path</remarks>
        /// <example>
        /// String.Join( Path.DirectorySeparatorChar, item.PathInProject )
        /// </example>
        [JsonIgnore]
        public string[] PathInProject => this.PathSegments.FindAll( p => !p.IsGUIDRoot() ).ToArray();

        /// <summary>Just returns the depth of the item where 0 is the GUID root folder so the first user folders should be 1</summary>
        [JsonIgnore]
        public int Depth => this.PathSegments.Count;
    }

    /// <summary>A folder in a project</summary>
    public class ProjectFolder : ProjectItem
    {
        /// <summary>The item type</summary>
        [JsonProperty( PropertyName = "type", Order = 3 )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public override ForgeObjectType Type => ForgeObjectType.Folders;

        /// <summary>The child contents of the item if it is a folder</summary>
        /// <remarks>Originall a List<ProjectItems> was made obserrable to work with WPF</remarks>
        [JsonProperty( PropertyName = "contents", Order = 10 )]
        public ObservableCollection<ProjectItem> Contents { get; set; } = new ObservableCollection<ProjectItem>();

        /// <summary>Gets a list of the currently empty folders</summary>
        /// <returns></returns>
        public List<ProjectFolder> GetEmptyFolders()
        {
            if ( this.Contents == null || this.Contents.Count < 1 )
                return new List<ProjectFolder>();

            // use the GetFolders extension
            return this.GetFolders( true );
        }
    }

    /// <summary>A file in a project</summary>
    public class ProjectFile : ProjectItem
    {
        /// <summary>The item type</summary>
        [JsonProperty( PropertyName = "type", Order = 3 )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public override ForgeObjectType Type => ForgeObjectType.Items;

        /// <summary>the version of this item</summary>
        [JsonProperty( PropertyName = "version", Order = 9 )]
        public int Version { get; set; } = 0;
    }
}

