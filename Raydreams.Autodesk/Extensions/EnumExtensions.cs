using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Raydreams.Autodesk.Extensions
{
	/// <summary>Enum extensions</summary>
	public static class EnumExtensions
	{
		/// <summary>Gets the <see cref="DescriptionAttribute"/> of the value, otherwise returns the string value of the value</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetDescription( this Enum value )
		{
			FieldInfo fieldInfo = value.GetType( ).GetField( value.ToString( ) );

			DescriptionAttribute attr = Attribute.GetCustomAttribute( fieldInfo, typeof( DescriptionAttribute ) ) as DescriptionAttribute;

			return (attr == null) ? value.ToString( ) : attr.Description;
		}

		/// <summary>Turns the specified enum in T into a Dictionary list if possible</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Dictionary<int,string> ToDict<T>() where T : struct, IConvertible
		{
			Type type = typeof( T );

			if ( !type.IsEnum )
				return new Dictionary<int, string>();

			return Enum.GetValues( type ).Cast<T>().ToDictionary( i => Convert.ToInt32(i), i => i.ToString() );
		}

	}
}
