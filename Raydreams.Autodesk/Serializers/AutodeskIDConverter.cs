using Newtonsoft.Json;
using Raydreams.Autodesk.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raydreams.Autodesk.Serializers
{
	/// <summary>JSON serializer to handle AutodeskID</summary>
	public class ForgeIDConverter : JsonConverter<ForgeID>
	{
		public override ForgeID ReadJson( JsonReader reader, Type objectType, ForgeID existingValue, bool hasExistingValue, JsonSerializer serializer )
		{
			string s = reader.Value?.ToString();

			return new ForgeID( s );
		}

		public override void WriteJson( JsonWriter writer, ForgeID value, JsonSerializer serializer )
		{
			if ( value is null  )
				writer.WriteValue( Guid.Empty.ToString() );

			writer.WriteValue( value.ToString() );
		}
	}
}
