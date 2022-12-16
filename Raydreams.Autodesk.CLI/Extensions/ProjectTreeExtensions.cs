using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Raydreams.Autodesk.CLI.Model;

namespace Raydreams.Autodesk.CLI.Extensions
{
    /// <summary>Extensions on the ProjectItem DTOs to walk the tree</summary>
    public static class ProjectTreeExtensions
    {
        #region [ Fields ]

        /// <summary>The fullname or prefix of a special folder that is often excluded</summary>
        public static readonly string[] SpecialFolders = new string[] { "submittals-attachments", "checklist_", "dailylog_", "issue_", "ProjectTb", "COST Root Folder" };

        public const string RootFolder = "root-folder";

        #endregion [ Fields ]

        /// <summary>will test a folder name to see if it is a special folder</summary>
        /// <param name="dir">The project Folder</param>
        /// <param name="projID">The Project ID</param>
        /// <returns></returns>
        /// <remarks>Some special folder contains GUID that are not the project ID</remarks>
        public static bool IsSpecialFolder ( this ProjectFolder dir, string projID )
		{
            // if the name starts with
            if ( SpecialFolders.Where( f => dir.Name.StartsWith( f ) ).Count() > 0 )
                return true;

            ForgeID id = new ForgeID( projID );

            if ( !id.IsValid )
                return false;

            // if the name contains the project ID
            return dir.Name.ToLowerInvariant().Contains( id.ToString() );
        }

        /// <summary>Check is the root folder by name not NULL parent</summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static bool IsRootFolder( this ProjectFolder dir ) => dir.Name.Contains( RootFolder );

        /// <summary>Gets the depth of an element by walking up the tree</summary>
		public static int Depth( this ProjectItem child )
        {
            if ( child == null )
                throw new ArgumentNullException("A null element can't have any depth.");

            int depth = 0;

            if ( child.Parent == null )
                return depth;

            var current = child.Parent;

            while ( current != null )
            {
                current = current.Parent;
                ++depth;
            }

            return depth;
        }

        /// <summary>Finds a node given a path from specified starting node in the format of '\ProjectFiles\MyDoc.txt' where the path separators are environment specific for now</summary>
        /// <param name="start">The folder to start from</param>
        /// <param name="partialPath">The path to search down</param>
        /// <returns></returns>
        public static ProjectItem FindByPath( ProjectFolder start, string partialPath )
        {
            if ( start == null || String.IsNullOrWhiteSpace( partialPath ) )
                return null;

            // get the path parts
            string[] parts = partialPath.Split( new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries );

            if ( parts == null || parts.Length < 1 )
                return start;

            // init current
            ProjectFolder current = start;

            // should find a match before we finish looping
            for ( int i = 0; i < parts.Length; ++i )
            {
                // stopping condition
                if ( i >= parts.Length - 1 )
                {
                    // may return null
                    return current.Contents.Where( n => n.Name.ToLowerInvariant() == parts[i].ToLowerInvariant() ).FirstOrDefault();
                }

                if ( current.Contents == null || current.Contents.Count < 1 )
                    return null;

                // go down one level
                current = current.Contents.Where( n => n.Name.ToLowerInvariant() == parts[i].ToLowerInvariant() && n.GetType() == typeof( ProjectFolder ) ).FirstOrDefault() as ProjectFolder;
            }

            return null;
        }

        /// <summary>Recurses a directory tree looking for an item of the specified ID</summary>
        /// <param name="start"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static ProjectItem FindByID( ProjectFolder start, string itemID )
		{
            if ( String.IsNullOrWhiteSpace( itemID ) )
                return null;

            itemID = itemID.Trim();

            var enu = Preorder( start ).GetEnumerator();

            while ( enu.MoveNext() )
            {
                if ( enu.Current.ID.Equals( itemID ) )
                    return enu.Current;
            }

            return null;
        }

        /// <summary>Flattens a project into a simple list of all the project items using a level walk</summary>
        /// <param name="noFiles">Folders only, don't include any files</param>
        /// <remarks>You can also use GetFolders if you do not need the files</remarks>
        public static List<ProjectItem> ToList( this ProjectFolder start, bool noFiles = true )
        {
            List<ProjectItem> results = new List<ProjectItem>();

            if ( start == null || start.Contents == null )
                return results;

            // use a level order enumerator
            var enu = Levelorder( start ).GetEnumerator();

            // iterate
            while ( enu.MoveNext() )
            {
                ProjectFolder dir = enu.Current as ProjectFolder;

                if ( noFiles && dir != null )
                    results.Add( dir );
                else
                    results.Add(enu.Current);
            }

            return results;
        }

        /// <summary>Performs a Level Order walk of an existing tree, reconnecting the childs to parent, filling in the path and removing files if specified. Use this to reorg a tree after getting it from the cache</summary>
        /// <param name="start">Folder to start from. Usually just the root folder</param>
        /// <param name="depth">Max depth to return back where 0 is the root</param>
        /// <param name="noFiles">true to not include files</param>
        public static void Repath( ProjectFolder start, int depth = -1, bool noFiles = false )
        {
            // queue to visit
            Queue<ProjectItem> toVisit = new Queue<ProjectItem>();

            // temp queue
            Queue<ProjectItem> temp = new Queue<ProjectItem>();

            // init the stack with the root
            toVisit.Enqueue( start );

            // while both to visit and temp are both NOT empty
            while ( toVisit.Count > 0 || temp.Count > 0 )
            {
                while ( toVisit.Count > 0 )
                {
                    // make the top of the stack the current node
                    ProjectItem current = toVisit.Dequeue();

                    // current has to be a folder
                    ProjectFolder dir = current as ProjectFolder;

                    // push any children
                    if ( dir != null && dir.Contents != null )
                    {
                        // only calc depth if it is NOT unlimited
                        int curDepth = ( depth != -1 ) ? Depth( dir ) : -100;

                        // this remvoes children from the output
                        if ( curDepth >= depth )
                            dir.Contents = new ObservableCollection<ProjectItem>();
                        else if ( noFiles ) // remove files - no need to sort
                            dir.Contents = new ObservableCollection<ProjectItem>( dir.Contents.Where( i => i.Type == ForgeObjectType.Folders ) );
                        else // sort files first Items = 3 Folders = 4
                            dir.Contents = new ObservableCollection<ProjectItem>( dir.Contents.OrderBy( i => i.Type ) );

                        foreach ( ProjectItem child in dir.Contents )
                        {
                            // join up the child with its parent
                            child.Parent = dir;

                            // rebuild the path
                            if ( child.Parent != null )
                            {
                                child.PathSegments = new List<string>();
                                child.PathSegments.AddRange( child.Parent.PathSegments );
                                child.PathSegments.Add( child.Parent.Name );
                            }

                            // add any future nodes to process
                            temp.Enqueue( child );
                        }
                    }
                }

                // move children over to the to visit queue
                while ( temp.Count > 0 )
                    toVisit.Enqueue( temp.Dequeue() );
            }
        }

        /// <summary>Convenience method to call on a Project object instead of the root</summary>
  //      public static Dictionary<string, ProjectItem> IndexByID( BIMProject start )
		//{
  //          if ( start == null )
  //              return new Dictionary<string, ProjectItem>();

  //          return start.Root.IndexByID();
		//}

        /// <summary>Does a tree walk and creates an index by item ID on every item</summary>
        /// <param name="start">HubProject to start the walk from</param>
        /// <returns>A dictionary of Item URN ID and ProjectItem</returns>
        public static Dictionary<string, ProjectItem> IndexByID( this ProjectFolder start )
		{
            // index to return
            Dictionary<string, ProjectItem> index = new Dictionary<string, ProjectItem>();

            if ( start == null )
                return index;

            IEnumerator<ProjectItem> downEnum = Preorder( start ).GetEnumerator();

            // add each item to the index
            while ( downEnum.MoveNext() )
                index.Add( downEnum.Current.ID, downEnum.Current );

            return index;
        }

        /// <summary>Returns only a list of ALL the Project Folders</summary>
        /// <param name="start">The folder to start searching from</param>
        /// <param name="emptyOnly">When set to true ONLY empty foldes will be returned</param>
        /// <returns></returns>
        public static List<ProjectFolder> GetFolders( this ProjectFolder start, bool emptyOnly = false )
		{
            List<ProjectFolder> results = new List<ProjectFolder>();

            if ( start == null )
                return results;

            var enu = Preorder( start ).GetEnumerator();

            while ( enu.MoveNext() )
            {
                if ( enu.Current is ProjectFolder )
				{
                    ProjectFolder dir = enu.Current as ProjectFolder;

                    if ( emptyOnly && ( dir.Contents == null || dir.Contents.Count < 1 ) )
                        results.Add( dir );
                    else
                        results.Add( dir );
                }
            }

            // strip any content references before returning
            //results.ForEach( d => d.Contents = null );

            return results;
        }

        /// <summary>Returns only a list of leaf files</summary>
        /// <param name="start">The folder to start searching from</param>
        /// <returns></returns>
        public static List<ProjectFile> GetFiles( this ProjectFolder start )
        {
            List<ProjectFile> results = new List<ProjectFile>();

            if ( start == null )
                return results;

            var enu = Preorder( start ).GetEnumerator();

            while ( enu.MoveNext() )
            {
                if ( enu.Current is ProjectFile )
                    results.Add( enu.Current as ProjectFile );
            }

            return results;
        }

        /// <summary>Iterator that goes up the tree from a child to the root</summary>
        /// <returns>A list of Project Items starting from the child and going to the root</returns>
        public static IEnumerable<ProjectItem> ToRoot( ProjectItem item )
        {
            ProjectItem current = item;

            // need a fool proof breaking condition
            while ( current.Parent != null )
            {
                current = current.Parent;

                // return the current node
                yield return current;
            }
        }

        /// <summary>Creates a preorder enumerator on a hub project</summary>
        /// <remarks>Will also relink the Parent property of child nodes</remarks>
        public static IEnumerable<ProjectItem> Preorder( ProjectFolder root )
        {
            // create a state stack
            Stack<ProjectItem> toVisit = new Stack<ProjectItem>();
            ProjectItem current = null;

            // init the stack with the root
            toVisit.Push( root );

            while ( toVisit.Count > 0 )
            {
                // make the top of the stack the current node
                current = toVisit.Pop();

                // current has to be a folder
                ProjectFolder dir = current as ProjectFolder;

                // push any children
                if ( dir != null && dir.Contents != null )
                {
                    // iterate backward through the children
                    for ( int i = dir.Contents.Count - 1; i > -1; --i )
                    {
                        // join up the child with its parent
                        dir.Contents[i].Parent = dir;

                        // push the children onto the stack left to right
                        toVisit.Push( dir.Contents[i] );
                    }
                }

                // return the current node
                yield return current;
            }

        }

        /// <summary>Creates a level-order enumerator on a project folder which searches top across before going down one level</summary>
        public static IEnumerable<ProjectItem> Levelorder( ProjectFolder root )
		{
            // queue to visit
            Queue<ProjectItem> toVisit = new Queue<ProjectItem>();
            
            // temp queue
            Queue<ProjectItem> temp = new Queue<ProjectItem>();

            // init the stack with the root
            toVisit.Enqueue( root );

            while ( toVisit.Count > 0 || temp.Count > 0 )
            {
                while ( toVisit.Count > 0 )
                {
                    // make the top of the stack the current node
                    ProjectItem current = toVisit.Dequeue();

                    // current has to be a folder
                    ProjectFolder dir = current as ProjectFolder;

                    // push any children
                    if ( dir != null && dir.Contents != null )
                    {
                        foreach ( ProjectItem child in dir.Contents )
                        {
                            // join up the child with its parent
                            child.Parent = dir;

                            temp.Enqueue( child );
                        }
                    }

                    // return the current node
                    yield return current;
                }

                // move children over to the to visit queue
                while ( temp.Count > 0 )
                    toVisit.Enqueue( temp.Dequeue() );
            }
        }
    }
}
