using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raydreams.Autodesk.Extensions;
using Raydreams.Autodesk.Serializers;

namespace Raydreams.Autodesk.Model
{
    /// <summary>This is the primary BIMrx entity for holding Project information in the cache</summary>

    public class ForgeProject
    {
        /// <summary>Locks the contents will new items are added</summary>
        private readonly object dirLock = new object();

        #region [ Constructors ]

        public ForgeProject() : this( Guid.Empty )
        {
        }

        public ForgeProject( Guid id )
        {
            this.ID = new ForgeID( id );
        }

        public ForgeProject( string id )
        {
            this.ID = new ForgeID( id );
        }

        #endregion [ Constructors ]

        /// <summary>The project ID as an Autodesk ID</summary>
        [JsonProperty( PropertyName = "id", Order = 1 )]
        [JsonConverter( typeof( ForgeIDConverter ) )]
        public ForgeID ID { get; set; }

        /// <summary>The name of the Project</summary>
        [JsonProperty( PropertyName = "name", Order = 2 )]
        public string Name { get; set; }

        /// <summary>The account this project belongs to</summary>
        [JsonProperty( "account", Order = 3 )]
        [JsonConverter( typeof( ForgeIDConverter ) )]
        public ForgeID AccountID { get; set; }

        /// <summary>The item type</summary>
        /// <remarks>Probably not necessary but will match the Autodesk Type identifiers</remarks>
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ForgeObjectType Type => ForgeObjectType.Projects;

        /// <summary>The top level root folder</summary>
        /// <remarks>Name is usually like <GUID>-root-folder</remarks>
        [JsonProperty( PropertyName = "root", Order = 10 )]
        public ProjectFolder Root { get; set; }

        /// <summary>The date the project was last modified</summary>
        /// <remarks>Inited to MaxValue otherwise ACC projects will always be stale </remarks>
        [JsonProperty( PropertyName = "lastModified", Order = 4 )]
        public DateTimeOffset LastModified { get; set; } = DateTimeOffset.MaxValue;

        /// <summary></summary>
        [JsonProperty( PropertyName = "platform", Order = 5 )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public PlatformType Platform { get; set; } = PlatformType.Undetermined;

        /// <summary>The date last written to the BIMrx cache</summary>
        /// <remarks>Defaults to now</remarks>
        [JsonProperty( PropertyName = "cached", Order = 5 )]
        public DateTimeOffset CacheUpdated { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>Quick test to see if this instance contains any folders/files before iterating</summary>
        [JsonIgnore]
        public bool IsEmpty => ( this.Root == null || this.Root.Contents == null || this.Root.Contents.Count < 1 );

        /// <summary>Add all the children to the specified parent folder</summary>
        /// <param name="parentID"></param>
        /// <param name="child"></param>
        /// <returns>True if new nodes were appended, otherwise false</returns>
        /// <remarks>Not thread safe</remarks>
        public int AppendNodes( ProjectFolder parent, IEnumerable<ProjectItem> childs )
        {
            if ( parent == null || childs == null )
                return 0;

            int added = 0;

            // lock while new nodes are added
            lock ( dirLock )
            {
                // get an enumerator to recurse the tree
                var enu = ProjectTreeExtensions.Preorder( this.Root ).GetEnumerator();

                while ( enu.MoveNext() )
                {
                    if ( enu.Current is ProjectFolder )
                    {
                        ProjectFolder dir = enu.Current as ProjectFolder;

                        if ( dir.ID == parent.ID )
                        {
                            added = dir.Contents.AddRange( childs );
                        }
                    }
                }
            }

            return added;
        }
    }
}

