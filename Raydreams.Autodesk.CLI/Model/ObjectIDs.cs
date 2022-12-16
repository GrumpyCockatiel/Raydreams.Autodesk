using System;
using Newtonsoft.Json;
using Raydreams.Autodesk.CLI.Extensions;

namespace Raydreams.Autodesk.CLI.Model
{
    /// <summary>For passing around just the Bucket/Object Key tuple</summary>
    /// <remarks>A full URN ID contains several parts
    /// urn:adsk.objects:os.object:wip.dm.prod/720cc53f-f7b5-432c-9227-aed23729dbe2.png
    /// </remarks>
    public struct ObjectIDs
    {
        /// <summary>Init with a single URN ID which will be parsed</summary>
        /// <param name="urnID"></param>
        /// <param name="name">Setting the name is optional</param>
        public ObjectIDs( string urnID, string? name = null )
        {
            if ( String.IsNullOrWhiteSpace( urnID ) || !urnID.StartsWith( "urn:", StringComparison.InvariantCultureIgnoreCase ) )
                throw new System.ArgumentNullException( "URN ID required", nameof( urnID ) );

            string parts = urnID.Trim().GetLastAfter( ':' );

            if ( String.IsNullOrWhiteSpace( parts ) )
                throw new System.ArgumentNullException( "URN ID invalid", nameof( urnID ) );

            string[] ids = parts.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries );

            if ( ids.Length < 2 )
                throw new System.ArgumentNullException( "URN ID invalid", nameof( urnID ) );

            this.BucketKey = ids[0];
            this.ObjectKey = ids[1];

            this.Name = name?.Trim();
        }

        /// <summary>The Object's Bucket key</summary>
        /// <remarks>e.g. wip.dm.prod</remarks>
        [JsonProperty( "bucketKey" )]
        public string BucketKey { get; set; }

        /// <summary>The Object Key which usually a GUID with a file extension</summary>
        /// <remarks>e.g. 720aa03f-f7b5-432c-9227-ae123729dbe2.png</remarks>
        [JsonProperty( "objectKey" )]
        public string ObjectKey { get; set; }

        /// <summary>The friendly name of the item</summary>
        /// <remarks>Does not have to be assigned to be valid</remarks>
        [JsonProperty( "name" )]
        public string? Name { get; set; }

        /// <summary>Test for valid IDs</summary>
        public bool IsValid => !new string[] { this.BucketKey, this.ObjectKey }.IsAnyNullOrWhiteSpace();
    }
}

