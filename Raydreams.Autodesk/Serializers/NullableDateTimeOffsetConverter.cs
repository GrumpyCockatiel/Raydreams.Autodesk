using System;
using Newtonsoft.Json;

namespace Raydreams.Autodesk.Serializers
{
    public class NullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        /// <summary>read from JSON</summary>
        public override DateTimeOffset? ReadJson( JsonReader reader, Type objectType, DateTimeOffset? existingValue, bool hasExistingValue, JsonSerializer serializer )
        {
            string s = reader.Value?.ToString();

            if ( String.IsNullOrWhiteSpace( s ) )
                return null;

            if ( !DateTimeOffset.TryParse( s, out DateTimeOffset dt ) )
                return null;

            return dt;
        }

        /// <summary>Write to JSON</summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson( JsonWriter writer, DateTimeOffset? value, JsonSerializer serializer )
        {
            if ( value == null )
                writer.WriteNull();

            DateTimeOffset dt = (DateTimeOffset)value;

            writer.WriteValue( dt.ToString( "o" ) );
        }
    }
}

