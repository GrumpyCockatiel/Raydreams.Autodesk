using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Raydreams.Autodesk.Extensions;
using Raydreams.Autodesk.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raydreams.Autodesk.Serializers
{
	/// <summary>Handles converting abstract base classes</summary>
	public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
	{
		protected override JsonConverter ResolveContractConverter( Type objectType )
		{
			if ( typeof( ProjectItem ).IsAssignableFrom( objectType ) && !objectType.IsAbstract )
				return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
			
			return base.ResolveContractConverter( objectType );
		}
	}

	/// <summary>Handles serializing the ProjectItem abstract base class</summary>
	public class ProjectItemConverter : JsonConverter<ProjectItem>
    {
		static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };

		/// <summary>Read the JSON and convert it to an object instance</summary>
		/// <returns></returns>
		public override ProjectItem ReadJson( JsonReader reader, Type objectType, ProjectItem existingValue, bool hasExistingValue, JsonSerializer serializer )
		{
			// nothing to parse
			if ( reader.TokenType == JsonToken.Null )
				return null;

			JObject obj = JObject.Load( reader );

			if ( !obj.ContainsKey( "type" ) )
				return null;

			ForgeObjectType type = obj["type"].ToString().GetEnumValue<ForgeObjectType>( true );

			switch ( type )
			{
				case ForgeObjectType.Folders:
					return JsonConvert.DeserializeObject<ProjectFolder>( obj.ToString(), SpecifiedSubclassConversion );
				case ForgeObjectType.Items:
					return JsonConvert.DeserializeObject<ProjectFile>( obj.ToString(), SpecifiedSubclassConversion );
				//case ForgeObjectType.Projects:
					//return JsonConvert.DeserializeObject<HubProject>( obj.ToString(), SpecifiedSubclassConversion );
				default:
					return null;
			}
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		/// <summary>How to write the JSON back out</summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		/// <remarks>For now we don't need to do this</remarks>
		public override void WriteJson( JsonWriter writer, ProjectItem value, JsonSerializer serializer )
		{
			throw new NotImplementedException();
		}

    }

}
