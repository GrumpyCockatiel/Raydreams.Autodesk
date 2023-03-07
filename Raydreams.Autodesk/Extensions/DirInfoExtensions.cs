using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Raydreams.Autodesk.Extensions
{
	/// <summary>Directory Info Extensions</summary>
	public static class DirInfoExtensions
	{
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

        /// <summary>Given just the parent directory path and full file path, returns JUST the part of the path after the project directory name including the file name. No root folder or project name is included.</summary>
        /// <returns></returns>
        public static string PathDiff( this DirectoryInfo dir, FileInfo fi)
		{
			int begin = fi.FullName.IndexOf( dir.FullName, StringComparison.InvariantCultureIgnoreCase );

			if ( begin < 0 )
				return null;

			return fi.FullName.Substring( dir.FullName.Length );
		}

		/// <summary>Just returns the top level child folders in the specified folder</summary>
		/// <param name="dir"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		public static DirectoryInfo[] GetAllRootFolders( this DirectoryInfo dir, string filter = "*" )
		{
			// no filter will match everything
			if ( String.IsNullOrWhiteSpace( filter ) )
				filter = "*";

			if ( !dir.Exists )
				return new DirectoryInfo[] { };

			return dir.GetDirectories( "*", SearchOption.TopDirectoryOnly );
		}

		/// <summary>Only gets files in the specified folder - no children</summary>
		/// <param name="dir">directory</param>
		/// <param name="filter">A file filter where null gets every file</param>
		/// <returns>A list of matching files</returns>
		public static FileInfo[] GetAllRootFiles(this DirectoryInfo dir, string filter)
		{
			// no filter will match everything
			if (String.IsNullOrWhiteSpace(filter))
				filter = "*";

			if ( !dir.Exists )
				return new FileInfo[] { };

			return dir.GetFiles(filter, SearchOption.TopDirectoryOnly);
		}

		/// <summary>Return ALL files (including child folders) in the specified folder path that match the specified filter.</summary>
		/// <param name="dir">Root directory</param>
		/// <param name="filter">A file filter where null gets every file</param>
		/// <returns>A list of matching files</returns>
		public static FileInfo[] GetAllFiles( this DirectoryInfo dir, string filter )
		{
			// no filter will match everything
			if ( String.IsNullOrWhiteSpace( filter ) )
				filter = "*";

			if ( dir == null || !dir.Exists )
				return new FileInfo[] { };

			return dir.GetFiles( filter, SearchOption.AllDirectories );
		}

		/// <summary>Returns the full path to the last created file with the given filter. Only searches in the input path, no children</summary>
		/// <param name="filter">Filter to use on filtering files such as MyFile*</param>
		/// <returns>Full file path</returns>
		public static FileInfo LatestFile(this DirectoryInfo dir, string filter)
		{
			if (dir == null || !dir.Exists)
				return null;

			if (String.IsNullOrWhiteSpace(filter))
				filter = "*";

			return dir.GetFiles(filter).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
		}

		/// <summary>The directory to purge all files from</summary>
		/// <param name="dir">The directory in which to purge files, only matches the top level</param>
		/// <param name="filter">The filename filter to use in matching. Will match all if none specified.</param>
		/// <param name="createdBefore">Only matches files created before the specifed date. Pass DateTime.Max to match all.</param>
		/// <returns>A int tuple of number of files deleted, total number of files that match the filter in the specified directory</returns>
		public static Tuple<int, int> PurgeDirectory(this DirectoryInfo dir, string filter, DateTime createdBefore)
		{
			int count = 0;

			if (!dir.Exists)
				return new Tuple<int, int>(count, -1);

			// get the files
			FileInfo[] files = dir.GetAllRootFiles(filter).Where(f => f.CreationTime < createdBefore).ToArray();

			// delete each file
			foreach (FileInfo file in files)
			{
				try
				{
					file.Delete();
					++count;
				}
				catch
				{
					// if an error occurs skip and continue
				}
			}

			return new Tuple<int, int>(count, files.Length);
		}
	}
}
