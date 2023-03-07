using System;
using Raydreams.Autodesk.Data;
using Raydreams.Autodesk.Extensions;
using Raydreams.Autodesk.Model;

namespace Raydreams.Autodesk.Logic
{
    /// <summary></summary>
	public class ProjectBuilder
	{
        protected bool _noFiles = false;

        public bool _includeSpecial = false;

        public ProjectBuilder( IDataManagerAPI repo )
		{
            this.Repo = repo;
		}

        /// <summary></summary>
        protected IDataManagerAPI Repo { get; set; }

        /// <summary>The Project as a tree</summary>
        public ForgeProject Project { get; protected set; }

        /// <summary></summary>
        public ProjectBuilder IncludeSpecial( bool includeSpecial )
        {
            this._includeSpecial = includeSpecial;
            return this;
        }

        /// <summary></summary>
        public ProjectBuilder NoFiles( bool noFiles )
        {
            this._noFiles = noFiles;
            return this;
        }

        /// <summary>Populates the tree starting with the root</summary>
        /// <param name="depth">depth is how far down to populate from the root. Where 0 is root only and < 0 is all the way</param>
        /// <returns></returns>
        /// <remarks>
        /// This needs to be called explicitly after the root is set.
        /// Will call GetRootFolder to make a root from the API
        /// </remarks>
        public async Task<bool> Build( ForgeIDs ids, int depth = -1 )
        {
            if ( !ids.IsValid )
                return false;

            //this.OnLog( $"Getting root of project '{projID}'" );

            // create a root from the repo
            this.Project = await this.GetRootFolder( ids );

            // make sure we got the root before moving on
            if ( this.Project.Root == null )
                return false;

            //this.OnLog( $"Begin walking project '{this.ProjectName}' ({this.ProjectID}) to depth {depth}" );

            bool results = await this.Fill( this.Project.Root, depth );

            //if ( results )
                //this.OnLog( $"Successfully recursed project '{this.ProjectName}' ({this.ProjectID})" );

            return results;
        }

        /// <summary>Init the tree with a hub and project ID to create the root folder</summary>
        /// <param name="hubID">Hub/Account ID</param>
        /// <param name="projID">project ID</param>
        /// <returns></returns>
        protected async Task<ForgeProject> GetRootFolder( ForgeIDs ids )
        {
            // get the base project info
            APIResponse<ForgeData> proj = await this.Repo.GetProject( ids );

            if ( !proj.IsSuccess || proj.Data == null || proj.Data.Result == null )
                return null;

            // remember the internal root values
            //this.ProjectID = proj.Data.Result.ID;

            ForgeProject project = new ForgeProject() { ID = ids.Project, AccountID = ids.Account };

            if ( proj.Data.Result.Attributes != null )
            {
                project.Name = proj.Data.Result.Attributes.GetName;
                //this.Project.Version = proj.Data.Result.Attributes.Extension.Version;
                // try to get the actual project type
                project.Platform = proj.Data.Result.Attributes.Extension.TryGetPlatform();
            }

            // get the contents of the top folder
            string rootFolderID = proj.Data.Result.Relationships.RootFolder.Data.ID;
            APIResponse<ForgeData> root = await this.Repo.GetFolderByProject( ids.Project, rootFolderID );

            if ( !root.IsSuccess || root.Data == null || root.Data.Result == null || root.Data.Result.Type != ForgeObjectType.Folders )
                return null;

            // set the root from a forge data
            project.Root = ProjectItem.Create( root.Data.Result ) as ProjectFolder;

            // create a new local directory tree
            return project;
        }

        /// <summary>Appends the directory tree starting from the specified parent node</summary>
        /// <param name="parent">The node to start from</param>
        /// <param name="depth">depth is how far down to populate from the root. Where 0 is root only and < 0 is all the way</param>
        /// <returns>Tree if node was recursed further, else false if returns without further recursion</returns>
        protected async Task<bool> Fill( ProjectItem parent, int depth = -1 )
        {
            if ( parent == null || String.IsNullOrWhiteSpace( parent.ID ) )
                return false;

            // check the depth where -1 is all the way down
            if ( depth > -1 && parent.Depth >= depth )
                return false;

            // only descend folders
            if ( parent.Type != ForgeObjectType.Folders )
                return false;

            // query all child contents -> is there anyway to know there is no children w/o query
            APIResponse<ForgeDataCollection> children = await this.Repo.GetFolderContents( this.Project.ID, parent.ID );

            // if not children.IsSuccess then do some logging
            if ( children.IsSuccess )
            {
                // do we really have children
                if ( children.Data == null || children.Data.Result == null || children.Data.Result.Count < 1 )
                    return false;

                // iterate all children
                foreach ( ForgeObject c in children.Data.Result )
                {
                    // make a new node with the Forge data
                    ProjectItem child = ProjectItem.Create( c );

                    // no data then move along
                    if ( parent is not ProjectFolder || child == null )
                        return false;

                    // set the parent
                    child.Parent = (ProjectFolder)parent;

                    // populate the path segments from the parent paths
                    child.PathSegments.AddRange( parent.PathSegments );
                    // and parent itself
                    child.PathSegments.Add( parent.Name );

                    // Trace log the walk
                    //this.OnLog( $"Walking... {child.Data.Name} ({child.Data.ID})", LogLevel.Trace );

                    // filter out special & Guid folders include GUID root
                    if ( !this._includeSpecial && parent.Name.Contains( ProjectTreeExtensions.RootFolder ) && child.Type == ForgeObjectType.Folders )
                    {
                        if ( ProjectTreeExtensions.SpecialFolders.Where( f => child.Name.StartsWith( f ) ).Count() > 0 )
                            continue;
                    }

                    // skip files is NoFiles is true
                    if ( this._noFiles && child.Type == ForgeObjectType.Items )
                        continue;

                    // actually add the child to the parent
                    this.Add( (ProjectFolder)parent, child );

                    // now walk the child with the same project ID
                    _ = await this.Fill( child, depth );
                }
            }

            return true;
        }

        /// <summary>Add a new node to the tree</summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public void Add( ProjectFolder parent, ProjectItem child )
        {
            // add the child to the parent
            parent.Contents.Add( child );

            // add the child to the list
            //this.Directory.Add( child );
        }
    }
}

