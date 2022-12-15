using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raydreams.Autodesk.CLI.Extensions;

namespace Raydreams.Autodesk.CLI.IO
{
	/// <summary>IO Helpers are just IO utility classes for common IO opertations.</summary>
	public static class IOHelpers
	{
		#region [ Methods ]

		/// <summary>Path to the user's desktop folder</summary>
		public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

		/// <summary>local app storage page</summary>
		public static string AppDataPath => (EnvironmentExtensions.IsMacOS()) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Applications") : Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

		/// <summary>Reads every line into a string array</summary>
		/// <param name="path">Path to a physical file to test</param>
		/// <remarks>Just move this to IO Helpers</remarks>
		public static List<string> ReadFile(string path, bool trimLines = true)
		{
			List<string> data = new List<string>();

			if (String.IsNullOrWhiteSpace(path))
				return data;

			FileInfo fi = new FileInfo(path);

			if (!fi.Exists)
				return data;

			using ( StreamReader reader = new StreamReader( path, Encoding.UTF8 ) )
			{
				string next = null;

				while ( ( next = reader.ReadLine( ) ) != null )
				{
					if ( String.IsNullOrWhiteSpace( next ) )
						continue;

					if ( trimLines )
						next = next.Trim( );

					data.Add( next );
				}
			}

			return data;
		}

		/// <summary>Makes a copy of the source file with the same name + the specified suffix in the destination folder</summary>
		/// <param name="suffix">an additional suffix to add to the end of the file name</param>
		public static int CopyFile(string srcPath, string destPath, string suffix = "", bool overwrite = true)
		{
			if (String.IsNullOrWhiteSpace(suffix))
				suffix = String.Empty;

			FileInfo fi = new FileInfo(srcPath);
			DirectoryInfo di = new DirectoryInfo(destPath);

			if (!fi.Exists || !di.Exists)
				return 0;

			// Rename the file
			var filePart = Path.GetFileNameWithoutExtension(fi.FullName);
			var filePartExt = Path.GetExtension(fi.FullName);
			var targetPath = Path.Combine(destPath, String.Format("{0}{1}{2}", filePart, suffix, filePartExt));

			try
			{
				File.Copy(fi.FullName, targetPath, overwrite);
			}
			catch (System.Exception)
			{
				return 0;
			}

			return 1;
		}

		/// <summary>Moves a file from one folder to another the <see cref="SourceFileName"/> to the <see cref="ArchiveFolder"/></summary>
		/// <param name="suffix">an additional suffix to add to the end of the file name</param>
		public static int MoveFile(string srcPath, string destPath, string suffix = "_bkup")
		{
			if (String.IsNullOrWhiteSpace(suffix))
				suffix = String.Empty;

			FileInfo fi = new FileInfo(srcPath);
			DirectoryInfo di = new DirectoryInfo(destPath);

			if (!fi.Exists || !di.Exists)
				return 0;

			// Rename the file
			var filePart = Path.GetFileNameWithoutExtension(fi.FullName);
			var filePartExt = Path.GetExtension(fi.FullName);
			var targetPath = Path.Combine(destPath, String.Format("{0}{1}{2}", filePart, suffix, filePartExt));

			try
			{
				File.Move(fi.FullName, targetPath);
			}
			catch (System.Exception)
			{
				return 0;
			}

			return 1;
		}

		/// <summary>Deletes the directory at the specified path if you have persmission</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static void DeleteDirectory(string path)
		{
			if (!String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
				Directory.Delete(path, true);
		}

		/// <summary>Creates a new folder at the specified path if you have permission</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static DirectoryInfo CreateDirectory(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				return null;

			return Directory.CreateDirectory(path);
		}

		/// <summary>Loads an XML file into a Dataset given the specified physical file path</summary>
		/// <param name="name">Optional name to set on the DataSet</param>
		/// <returns>A populated DataSet object</returns>
		public static DataSet LoadXMLFile(string srcPath, string name = null)
		{
			// also need to check the file exists and it is an XML file
			if (String.IsNullOrWhiteSpace(srcPath))
				return new DataSet();

			FileInfo fi = new FileInfo(srcPath);

			// check it is an XML file
			if (!fi.Exists || fi.Extension.ToLower() != ".xml")
				return new DataSet();

			// name is optional
			name = (String.IsNullOrWhiteSpace(name)) ? Guid.NewGuid().ToString() : name.Trim();

			// create the data set
			DataSet ds = new DataSet(name);
			_ = ds.ReadXml(srcPath, XmlReadMode.InferSchema);

			// return
			return ds;
		}

		#endregion [ Methods ]
	}
}
