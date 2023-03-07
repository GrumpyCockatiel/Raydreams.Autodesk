using System;
using Newtonsoft.Json;

namespace Raydreams.Autodesk.Model
{
    /// <summary>A tuple of Autodesk IDs to pair the Account ID and Project ID together</summary>
    public struct ForgeIDs
    {
        public ForgeIDs( ForgeID acctID, ForgeID projID )
        {
            this.Account = acctID;
            this.Project = projID;
        }

        public ForgeIDs( Guid acctID, Guid projID )
        {
            this.Account = new ForgeID( acctID );
            this.Project = new ForgeID( projID );
        }

        public ForgeIDs( string acctID, string projID )
        {
            this.Account = new ForgeID( acctID );
            this.Project = new ForgeID( projID );
        }

        [JsonProperty( "acctID" )]
        public ForgeID Account { get; }

        [JsonProperty( "projID" )]
        public ForgeID Project { get; }

        /// <summary></summary>
        public bool IsValid => ( this.Account != null && this.Project != null && this.Account.IsValid && this.Project.IsValid );
    }

    /// <summary>Wraps a Account/Hub ID or Project ID as a raw string</summary>
    /// <remarks>Try to use this over a raw string since it will guarantee the correct format</remarks>
    //[JsonConverter( typeof( AutodeskIDConverter ) )]
    public class ForgeID : IEquatable<ForgeID>
    {
        /// <summary>The true underlying value</summary>
        private Guid _id = Guid.Empty;

        #region [ Constructors ]

        public ForgeID( string? raw )
        {
            this._id = this.Parse( raw );
        }

        public ForgeID( Guid raw )
        {
            this._id = raw;
        }

        #endregion [ Constructors ]

        /// <summary>The opposite of IsEmpty</summary>
        public bool IsValid => ( this._id != Guid.Empty );

        /// <summary>The ID as a GUID</summary>
        public Guid ID => this._id;

        /// <summary>Always writes out the BIM360 ID format</summary>
        public override string ToString() => this.BIM360;

        /// <summary>Get a DataManager formatted ID string</summary>
        public string DM => $"b.{this._id}".ToLowerInvariant();

        /// <summary>Get a BIM360 formatted ID string</summary>
        public string BIM360 => this._id.ToString().ToLowerInvariant();

        /// <summary>Parses a raw string into an Autodesk ID</summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        private Guid Parse( string? raw )
        {
            if ( String.IsNullOrWhiteSpace( raw ) )
                return Guid.Empty;

            raw = raw.Trim();

            if ( raw.StartsWith( "b.", StringComparison.OrdinalIgnoreCase ) )
                raw = raw.Substring( 2 );

            if ( !Guid.TryParse( raw, out Guid id ) )
                return Guid.Empty;

            return id;
        }

        /// <summary>Reference equality to another ID</summary>
        public bool Equals( ForgeID? other )
        {
            // other is null
            if ( ReferenceEquals( other, null ) )
                return false;

            return ReferenceEquals( this, other );
        }

        /// <summary>Override equivalence</summary>
        public static bool operator ==( ForgeID a, ForgeID b )
        {
            // 2 nulls are equivalent
            if ( a is null && b is null )
                return true;

            // one is null, the other is not
            if ( ( a is null && !( b is null ) ) || ( b is null && !( a is null ) ) )
                return false;

            return a._id == b._id;
        }

        /// <summary>Override not equivalent</summary>
        public static bool operator !=( ForgeID a, ForgeID b )
        {
            // 2 nulls are not equivalent
            if ( a is null && b is null )
                return false;

            // one is null, the other is not
            if ( ( a is null && !( b is null ) ) || ( b is null && !( a is null ) ) )
                return true;

            return a._id != b._id;
        }

        /// <summary>This overrides value equality</summary>
        public override bool Equals( object? obj )
        {
            if ( obj is null || this.GetType() != obj.GetType() )
                return false;

            ForgeID? other = obj as ForgeID;

            if ( other is null )
                return false;

            return other._id == this._id;
        }

        /// <summary></summary>
        public override int GetHashCode()
        {
            return this._id.GetHashCode();
        }

    }
}

