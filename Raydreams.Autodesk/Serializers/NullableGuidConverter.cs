using System;
using Newtonsoft.Json;

namespace Raydreams.Autodesk.Serializers
{
    /// <summary>(De)Serializes a nullable GUID</summary>
    public class NullableGuidConverter : JsonConverter<Guid?>
    {
        /// <summary>read from JSON</summary>
        public override Guid? ReadJson( JsonReader reader, Type objectType, Guid? existingValue, bool hasExistingValue, JsonSerializer serializer )
        {
            string s = reader.Value?.ToString();

            if ( String.IsNullOrWhiteSpace( s ) )
                return null;

            if ( !Guid.TryParse( s, out Guid id ) )
                return null;

            return id;
        }

        /// <summary>Write to JSON</summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson( JsonWriter writer, Guid? value, JsonSerializer serializer )
        {
            if ( value == null )
                writer.WriteNull();

            Guid id = (Guid)value;

            writer.WriteValue( id.ToString() );
        }
    }
}

