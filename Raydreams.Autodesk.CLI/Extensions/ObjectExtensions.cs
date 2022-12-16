using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Raydreams.Autodesk.CLI.Extensions
{
	/// <summary>Generic Object extensions mainly to just hash fields on an instance</summary>
	public static class ObjectExtensions
	{
		/// <summary>Turns all the properties of a dynamic object to a dict</summary>
		/// <param name="dynObj"></param>
		/// <returns></returns>
		public static Dictionary<string, string> DynToDict( dynamic dynObj )
		{
			var dict = new Dictionary<string, string>();

			foreach ( PropertyDescriptor prop in TypeDescriptor.GetProperties( dynObj ) )
			{
				object obj = prop.GetValue( dynObj );
				dict.Add( prop.Name, obj?.ToString() );
			}

			return dict;
		}

		/// <summary>Gets all the public readable property infos on an object</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static PropertyInfo[] GetPropertyInfo<T>( this T obj ) => typeof( T ).GetProperties( BindingFlags.Public | BindingFlags.Instance ).Where( p => p.CanRead ).ToArray();

		/// <summary>Gets a single property name where the name match is case sensitive for now</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="propName"></param>
		/// <returns></returns>
		public static PropertyInfo GetPropertyInfo<T>( this T obj, string propName ) => typeof( T ).GetProperties( BindingFlags.Public | BindingFlags.Instance ).Where( p => p.CanRead && p.Name.Equals( propName.Trim() ) ).FirstOrDefault();

		/// <summary>Get just the field/property names on a specific object as a string list</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static List<string> GetHeaders<T>( this T obj )
		{
			var dict = new List<string>();

			if ( obj == null )
				return dict;

			// get all the properties in the class
			PropertyInfo[] props = obj.GetPropertyInfo();

			if ( props.Length < 1 )
				return dict;

			Array.ForEach( props, i => dict.Add( i.Name ) );

			return dict;
		}

		/// <summary>Just serializes all the objects properties to a string array of their values</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static List<string> SerializeToList<T>( this T obj )
		{
			var dict = new List<string>();

			if ( obj == null )
				return dict;

			// get all the properties in the class
			PropertyInfo[] props = obj.GetPropertyInfo();

			if ( props.Length < 1 )
				return dict;

			// iterate all properties
			foreach ( PropertyInfo prop in props )
			{
				object value = prop.GetValue( obj );
				dict.Add( ( value == null ) ? String.Empty : value.ToString() );
			}

			return dict;
		}

		/// <summary>Converts the entire object to a string string dictionary</summary>
		public static Dictionary<string, string> ToKeyValuePair<T>( this T obj )
		{
			var dict = new Dictionary<string, string>();

			if ( obj == null )
				return dict;

			// get all the properties in the class
			PropertyInfo[] props = obj.GetPropertyInfo();

			if ( props.Length < 1 )
				return dict;

			// iterate all properties
			foreach ( PropertyInfo prop in props )
			{
				object value = prop.GetValue( obj );
				dict.Add( prop.Name, ( value == null ) ? String.Empty : value.ToString() );
			}

			return dict;
		}
	}
}
