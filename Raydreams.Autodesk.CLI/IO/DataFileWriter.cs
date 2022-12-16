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

        /// <summary>file lock</summary>
        private readonly object _fileLock = new object();

        private string _colDelim = ",";

		private string _newline = Environment.NewLine;

		private string[] _headers = new string[0];

		private char _fieldChar = Char.MinValue;

		#endregion [ Fields ]

		#region [ Constructors ]

		/// <summary></summary>
		/// <param name="path">Complete path including filename to write to</param>
		public DataFileWriter( FileInfo path )
		{
			this.Path = path;
		}

		/// <summary></summary>
		/// <param name="path">Complete path including filename to write to</param>
		public DataFileWriter(string path) : this( new FileInfo(path) )
		{ }

        #endregion [ Constructors ]

        #region [ Properties ]

        protected FileInfo Path { get; set; }

		/// <summary></summary>
		protected StreamWriter? OutputStream { get; set; }

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

		/// <summary>Field quote character to use if any</summary>
		public char FieldQuoteChar
		{
			get { return this._fieldChar; }
			set { this._fieldChar = value; }
		}

		#endregion [ Properties ]

		#region [ Methods ]

		/// <summary>Open the output file</summary>
		public DataFileWriter Open()
		{
			this.OutputStream = new StreamWriter( this.Path.FullName, false );

			return this;
		}

		/// <summary>Writes the list of headers to the file</summary>
		public int WriteHeader( IEnumerable<string> headers )
		{
            int cols = 0;

			if ( this.OutputStream == null || headers == null )
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

			lock( _fileLock )
			{
                this.OutputStream.WriteLine( sb.ToString() );
            }
			
			return cols;
		}

		/// <summary>Writes a single line to the file.</summary>
		/// <param name="line">The string to write.</param>
		/// <returns></returns>
		public void WriteRawLine( string line )
		{
			if ( this.OutputStream == null || line == null )
				return;

			lock (_fileLock)
			{
				this.OutputStream.WriteLine(line);
			}
		}

		/// <summary>Writes the given string list to a delimited line</summary>
		/// <param name="values"></param>
		public void WriteValuesToLine( IEnumerable<string> values )
		{
            if ( this.OutputStream == null || values.Count() < 0 )
                return;

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

			lock (_fileLock)
			{
				this.OutputStream.WriteLine(sb.ToString());
			}
		}

		/// <summary>Close the file stream</summary>
		public void Close()
		{
			if ( this.OutputStream != null )
			{
				lock (_fileLock)
				{
					this.OutputStream.Flush();
					this.OutputStream.Close();
				}
			}
		}

		#endregion [ Methods ]

	}
}
