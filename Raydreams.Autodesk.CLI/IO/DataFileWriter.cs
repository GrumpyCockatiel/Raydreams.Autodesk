using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Raydreams.Autodesk.CLI.IO
{
	/// <summary>Writes to a file</summary>
	public class DataFileWriter
	{
		#region [ Fields ]

		private string _filePath = null;

		private string _colDelim = ",";

		private string _newline = null;

		private string[] _headers = null;

		private char _fieldChar = Char.MinValue;

		#endregion [ Fields ]

		#region [ Constructors ]

		/// <summary></summary>
		/// <param name="path">Complete path including filename to write to</param>
		public DataFileWriter( string path )
		{
			this._filePath = path.Trim();
		}

		#endregion [ Constructors ]

		#region [ Properties ]

		/// <summary></summary>
		protected StreamWriter OutputStream { get; set; }

		/// <summary>String between each column</summary>
		public string ColDelimitor
		{
			get { return this._colDelim; }
			set { this._colDelim = value.Trim(); }
		}

		/// <summary>string at the end of a row if something other than an environment newline</summary>
		public string RowDelimitor
		{
			get { return this._newline; }
			set { this._newline = value.Trim(); }
		}

		/// <summary>Get the path being written to</summary>
		public string FilePath
		{
			get { return this._filePath; }
		}

		/// <summary>Field quote character to use if any</summary>
		public char FieldQuoteChar
		{
			get { return this._fieldChar; }
			set { this._fieldChar = value; }
		}

		#endregion [ Properties ]

		#region [ Methods ]

		/// <summary>Static convenience method to delete a directory</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static void DeleteDirectory( string path )
		{
			if ( !String.IsNullOrWhiteSpace( path ) && Directory.Exists( path ) )
				Directory.Delete( path, true );
		}

		/// <summary>Static convenience method to create a directory</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static DirectoryInfo CreateDirectory( string path )
		{
			if ( String.IsNullOrWhiteSpace( path ) )
				return null;

			return System.IO.Directory.CreateDirectory( path );
		}

		/// <summary>Open the output file</summary>
		public bool Open()
		{
			FileInfo fi = new FileInfo( this.FilePath );

			this.OutputStream = new StreamWriter( this.FilePath, false );

			return ( this.OutputStream != null );
		}

		/// <summary>Writes the list of headers to the file</summary>
		public int WriteHeader( IEnumerable<string> headers )
		{
			int cols = 0;

			if ( headers == null )
				return cols;

			// keep the headers for later
			this._headers = headers.ToArray<string>();

			StringBuilder sb = new StringBuilder();

			foreach ( string s in this._headers )
			{
				if ( this.FieldQuoteChar == Char.MinValue )
					sb.AppendFormat( "{0}{1}", s, this.ColDelimitor );
				else
					sb.AppendFormat( "{2}{0}{2}{1}", s, this.ColDelimitor, this.FieldQuoteChar );
				++cols;
			}

			if (sb.Length > 0)
				--sb.Length;

			// write the row delim if any even if there are no headers
			if ( !String.IsNullOrWhiteSpace( this.RowDelimitor ) )
				sb.Append( this.RowDelimitor );

			this.OutputStream.WriteLine( sb.ToString() );

			return cols;
		}

		/// <summary>Writes a single line to the file.</summary>
		/// <param name="line">The string to write.</param>
		/// <returns></returns>
		public void WriteRawLine( string line )
		{
			if ( line == null )
				return;

			this.OutputStream.WriteLine( line );
		}

		/// <summary>Writes the given string list to a delimited line</summary>
		/// <param name="values"></param>
		public void WriteValuesToLine( IEnumerable<string> values )
		{
			StringBuilder sb = new StringBuilder();

			foreach ( string s in values )
			{
				if ( this.FieldQuoteChar == Char.MinValue )
					sb.AppendFormat( "{0}{1}", ( s == null ) ? String.Empty : s, this.ColDelimitor );
				else
					sb.AppendFormat( "{2}{0}{2}{1}", ( s == null ) ? String.Empty : s, this.ColDelimitor, this.FieldQuoteChar );
			}

			sb.Length = sb.Length - this.ColDelimitor.Length;

			if ( !String.IsNullOrWhiteSpace( this.RowDelimitor ) )
				sb.Append( this.RowDelimitor );

			this.OutputStream.WriteLine( sb.ToString() );
		}

		/// <summary>Close the file stream</summary>
		public void Close()
		{
			if ( this.OutputStream != null )
			{
				this.OutputStream.Flush();
				this.OutputStream.Close();
			}
		}

		#endregion [ Methods ]

	}
}
