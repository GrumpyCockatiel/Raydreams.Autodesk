using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Raydreams.Autodesk.Extensions
{
    /// <summary>String Extensions</summary>
	public static partial class StringExtensions
	{
		/// <summary>Given a string file path and array path - test if they are in the same path</summary>
		/// <returns></returns>
		public static bool IsPathsEqual( this string path, string[] pathInProject, char delim, bool caseSensitive = false )
		{
            // validate input
			if ( path == null || pathInProject == null )
				return false;

            if ( delim == 0x0 )
                delim = Path.DirectorySeparatorChar;

            path = path.Trim();

            // both root
            if ( path == String.Empty && pathInProject.Length < 1 )
                return true;

			StringComparison compare = caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

			return $"{delim}{String.Join( delim, pathInProject )}".Equals(path, compare);
		}

		/// <summary>Simple string GUID match for only the format of ce3efb9e-8f7e-4d01-b8a6-8855b526465c</summary>
		/// <param name="guid">The folder name string to test</param>
		/// <param name="suffix">Additional suffix to append to the end of the GUId though for is always -root-folder</param>
		/// <returns></returns>
		public static bool IsGUIDRoot(this string name, string suffix = "-root-folder" )
		{
            if ( String.IsNullOrEmpty( name ) )
                return false;

            suffix = ( String.IsNullOrEmpty( suffix ) ) ? String.Empty : suffix.Trim();

            string pattern = $"^[A-Z0-9]{{8}}-([A-Z0-9]{{4}}-){{3}}[A-Z0-9]{{12}}{suffix}$";

            Regex regex = new Regex( pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline );
            return regex.IsMatch( name.Trim() );
        }

        /// <summary>Simply test a string is a valid Hub/Account ID or Project ID</summary>
        /// <param name="id">ID as a string</param>
        /// <returns></returns>
        public static bool IsValidID( this string id )
        {
            Regex regex = new Regex( @"^(b.)?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$" );

            return regex.IsMatch( id );
        }

        /// <summary>Removes all whitespace characters from the string</summary>
        public static string RemoveSpaces( this string str )
        {
            StringBuilder temp = new StringBuilder();

            foreach ( char c in str )
            {
                if ( !Char.IsWhiteSpace( c ) )
                    temp.Append( c );
            }

            return temp.ToString();
        }

        /// <summary>Parses the version number off a version URN</summary>
        /// <param name=""></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <remarks> urn:adsk.wipprod:fs.file:vf.XeIA4-UDT1OMLRodWtVa9g?version=1</remarks>
        public static int ParseVersion(this string version)
		{
            string parts = version.GetLastAfter( '?' );

            if ( String.IsNullOrWhiteSpace( parts ) )
                return 0;

            string[] ids = parts.Split( new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries );

            if ( ids.Length < 2 || !Int32.TryParse(ids[1], out int ver ) )
                return 0;

            return ver;
        }

        /// <summary>Truncates a string to the the specified length or less</summary>
        public static string Truncate( this string str, int length, bool trim = true )
        {
            // if greater than length
            if ( str.Length > length )
                return ( trim ) ? str.Trim().Substring( 0, length ) : str.Substring( 0, length );

            return ( trim ) ? str.Trim() : str;
        }

        /// <summary>Returns all of a string after the specified LAST occurance of the specified token</summary>
        /// <returns>The substring</returns>
        public static string GetLastAfter( this string str, char token )
        {
            if ( str.Length < 1 )
                return String.Empty;

            int idx = str.LastIndexOf( token );

            if ( idx < 0 || idx >= str.Length - 1 )
                return String.Empty;

            return str.Substring( idx + 1, str.Length - idx - 1 ).Trim();
        }

        /// <summary>Gets all of a string from the beginning up but not including the last instance of the token</summary>
        /// <param name="str"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <remarks>\Project Files\Docs\Test3.txt should return just \Project Files\Docs</remarks>
        public static string GetAllBeforeLast( this string str, char token )
		{
            if ( str.Length < 1 )
                return String.Empty;

            int idx = str.LastIndexOf( token );

            if ( idx < 0 )
                return String.Empty;

            return str.Substring( 0, idx ).Trim();
        }

        /// <summary>If any string is null or white space return true.</summary>
        /// <param name="strs">list of strings to test</param>
        /// <returns></returns>
        public static bool IsAnyNullOrWhiteSpace( this string[] strs )
        {
            return strs.Count( s => String.IsNullOrWhiteSpace( s ) ) > 0;
        }

        /// <summary>If ALL strings are null or white space return true.</summary>
        /// <param name="strs">list of strings to test</param>
        /// <returns></returns>
        public static bool IsAllNullOrWhiteSpace( this string[] strs )
        {
            if ( strs == null )
                return true;

            return strs.Count( s => String.IsNullOrWhiteSpace( s ) ) > ( strs.Length - 1);
        }

        /// <summary>Cleans a string of all non digit characters and collapses to a string with no spaces.</summary>
        public static string RemoveNonDigits( this string str )
        {
            StringBuilder temp = new StringBuilder( );

            foreach ( char c in str )
            {
                if ( Char.IsDigit( c ) )
                    temp.Append( c );
            }

            return temp.ToString( );
        }

		/// <summary>Picks digits out of the specified string and return them as a list of numbers.</summary>
		/// <remarks>This can be done with LINQ now.</remarks>
		public static List<Int32> ParseDigits( this string str )
        {
            List<Int32> digits = new List<Int32>( );
            StringBuilder temp = new StringBuilder( );

            for ( int i = 0; i < str.Length; ++i )
            {
                while ( i < str.Length && Char.IsDigit( str[i] ) )
                {
                    temp.Append( str[i++] );
                }

                if ( temp.Length > 0 )
                {
                    digits.Add( Int32.Parse( temp.ToString( ) ) );
                    temp.Length = 0;
                }
            }

            return digits;
        }

        /// <summary>Converts a string to an enum value of enum T failing to default(T)</summary>
        /// <returns></returns>
        /// <remarks>Case is always ignored</remarks>
        public static T GetEnumValue<T>( this string value, bool ignoreCase = true ) where T : struct, IConvertible
        {
            T result = default( T );

            if ( String.IsNullOrWhiteSpace( value ) )
                return result;

            if ( Enum.TryParse<T>( value, ignoreCase, out result ) )
                return result;

            return default( T );
        }

        /// <summary>Converts a string to an enum value with the specified default on fail</summary>
        /// <param name="def">Default value if parsing fails</param>
        /// <param name="ignoreCase">Ignore case by default</param>
        /// <returns></returns>
        public static T GetEnumValue<T>( this string value, T def, bool ignoreCase = true ) where T : struct, IConvertible
        {
            T result = def;

            if ( String.IsNullOrWhiteSpace( value ) )
                return result;

            if ( Enum.TryParse<T>( value, ignoreCase, out result ) )
                return result;

            return def;
        }

        /// <summary>Returns the enum value by the description string on the enum member</summary>
        /// <returns>The enum value that matches by the Description Attribute, otherwise the default value</returns>
        public static T EnumByDescription<T>( this string desc ) where T : struct, IConvertible
        {
            Type type = typeof( T );

            // is it an enum
            if ( !type.IsEnum )
                throw new System.ArgumentException( "Type must be an enum." );

            foreach ( string field in Enum.GetNames( type ) )
            {
                MemberInfo[] infos = type.GetMember( field );

                foreach ( MemberInfo info in infos )
                {
                    DescriptionAttribute attr = info.GetCustomAttribute<DescriptionAttribute>( false );

                    if ( attr.Description.Equals( desc, StringComparison.InvariantCultureIgnoreCase ) )
                        return field.GetEnumValue<T>( true );
                }
            }

            return default( T );
        }

        /// <summary>Hash the string to an MD5 digest</summary>
        /// <param name="str">single input string to hash</param>
        /// <returns>byte array</returns>
        public static byte[] HashToMD5( this string str )
        {
            // calculate the hash value of the object
            byte[] hash;

            using ( MD5 hasher = MD5.Create( ) )
                hash = hasher.ComputeHash( Encoding.UTF8.GetBytes( str ) );

            return hash;
        }

        /// <summary>Hash the string to a SHA1 digest</summary>
        /// <param name="str">single input string to hash</param>
        /// <returns>byte array</returns>
        public static byte[] HashToSHA1( this string str )
        {
            // calculate the hash value of the object
            byte[] hash;

            using ( SHA1 haser = SHA1.Create( ) )
                hash = haser.ComputeHash( Encoding.UTF8.GetBytes( str ) );

            return hash;
        }

        /// <summary>Hash the string to a SHA256 digest</summary>
        /// <param name="str">single input string to hash</param>
        /// <returns>byte array</returns>
        public static byte[] HashToSHA256( this string str )
        {
            // calculate the hash value of the object
            byte[] hash;

            using ( SHA256 haser = SHA256.Create( ) )
                hash = haser.ComputeHash( Encoding.UTF8.GetBytes( str ) );

            return hash;
        }

        /// <summary>Hash the string to a SHA256 digest</summary>
        /// <param name="str">single input string to hash</param>
        /// <param name="key">The key SHOULD be 64 bytes exactly but will still work if it is not</param>
        /// <returns>byte array</returns>
        public static byte[] HashToHMACSHA256( this string str, byte[] key )
        {
            if ( key.Length != 64 )
                throw new System.ArgumentException("The key should be exactly 64 bytes.", nameof(key));

            // calculate the hash value of the object
            byte[] hash;

            using ( HMACSHA256 haser = new HMACSHA256( key ) )
                hash = haser.ComputeHash( Encoding.UTF8.GetBytes( str ) );

            return hash;
        }

        /// <summary>Encodes a byte array as BASE64 URL encoded</summary>
        /// <remarks>This needs to move to Array Extensions</remarks>
        public static string BASE64UrlEncode( byte[] arg )
        {
            string s = Convert.ToBase64String( arg ); // Regular base64 encoder
            //s = s.Split( '=' )[0];
            s = s.TrimEnd( '=' ); // remove any trailing =
            s = s.Replace( '+', '-' ); // 62nd char of encoding
            s = s.Replace( '/', '_' ); // 63rd char of encoding
            return s;
        }

        /// <summary>Decodes a BASE64 URL encoded string back to its original bytes</summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static byte[] BASE64UrlDecode( this string str )
        {
            str = str.Replace( '-', '+' ); // 62nd char of encoding
            str = str.Replace( '_', '/' ); // 63rd char of encoding

            // the modulus of the length by 4 can not be remainder 1
            switch ( str.Length % 4 )
            {
                // no padding necessary if it divides evenly
                case 0:
                    break;
                // pad with two =
                case 2:
                    str += "==";
                    break;
                // pad once
                case 3:
                    str += "=";
                    break;
                // hopefully this does not happen
                default:
                    throw new System.Exception( "Illegal BASE64URL string!" );
            }

            return Convert.FromBase64String( str ); // Standard base64 decoder
        }

        /// <summary>Given a string in the format key1=value1,key2=value2,key3=value3 splits into a dictionary</summary>
        public static Dictionary<string, string> PairsToDictionary( this string str, bool stripQuotes = true )
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            StringBuilder temp = new StringBuilder();

            foreach ( char c in str )
            {
                if ( c == ',' )
                {
                    string[] parts = temp.ToString().Split( '=', StringSplitOptions.None );
                    
                    if ( parts != null && parts.Length > 0 && !String.IsNullOrWhiteSpace( parts[0] ) )
                    {
                        parts[1] = ( parts.Length < 2 || String.IsNullOrWhiteSpace( parts[1] ) ) ? String.Empty : parts[1].Trim();
                        
                        if ( stripQuotes )
                            parts[1] = parts[1].Replace( "\"", "" );
                        
                        results.Add( parts[0].Trim(), parts[1] );
                    }

                    temp = new StringBuilder();
                }
                else
                {
                    temp.Append( c );
                }
            }

            return results;
        }

    }
}
