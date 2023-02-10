using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raydreams.Autodesk.CLI.Data;
using Raydreams.Autodesk.CLI.Extensions;
using Raydreams.Autodesk.CLI.IO;
using Raydreams.Autodesk.CLI.Logic;
using Raydreams.Autodesk.CLI.Model;
using Raydreams.Autodesk.CLI.Security;

namespace Raydreams.Autodesk.CLI
{
    /// <summary></summary>
    public class AutodeskCLI : BackgroundService
    {
        /// <summary></summary>
        /// <param name="client">App client credentials injected at startup</param>
        public AutodeskCLI( ForgeAppClient client )
        {
            this.Client = client;
        }

        /// <summary>The root path of where the app reads and writes to</summary>
        public static DirectoryInfo AppDataRoot { get; set; } = new DirectoryInfo( Path.Combine( IOHelpers.DesktopPath, "Forge" ) );

        /// <summary></summary>
        protected ForgeAppClient Client { get; set; }

        /// <summary></summary>
        protected override Task<int> ExecuteAsync(CancellationToken stoppingToken)
        {
            int res = 0;

            // lets do the thing
            try
            {
                res = this.Run();
            }
            catch (System.Exception)
            {
                //this.LogException(exp);
                return Task.FromResult(-1);
            }

            return Task.FromResult(res);
        }

        /// <summary></summary>
        /// <returns></returns>
        public int Run()
        {
            // create an auth manager
            AuthenticationManager authMgr = new AuthenticationManager(this.Client);
            authMgr.Scopes = ForgeScopes.UserRead | ForgeScopes.UserProfileRead | ForgeScopes.AccountRead | ForgeScopes.DataRead;

            // load the default IDs for testing
            ForgeIDs ids = new ForgeIDs( this.Client.PrimaryHubID, this.Client.DefaultProjectID );

            // where to save the token
            string tokenPath = Path.Combine( AppDataRoot.FullName, "autodesk.txt" );
            //HTTPTokenManager tokenMgr = new HTTPTokenManager(authMgr, tokenPath, 50001);
            //tokenMgr.Writer = new FileTokenIO();

            // create a 2 leg token
            ITokenManager tokenMgr = new TwoLegTokenManager(authMgr);
            //AutodeskUser user = tokenMgr.GetUser().GetAwaiter().GetResult();

            // make a Data Manager Repo
            IDataManagerAPI repo = new DataManagerRepository(tokenMgr);

            //string? token = tokenMgr.GetTokenAsync().GetAwaiter().GetResult();

            // test getting all the hubs
            var hub = this.ListHubs(repo).GetAwaiter().GetResult();

            // test getting a project tree
            //var projects = this.ListProjects(repo, acctID).GetAwaiter().GetResult();
            var dir = this.GetProjectTree(repo, ids.Account, ids.Project );

            // extract all the files from the tree
            List<ProjectFile> files = dir.Root.GetFiles();

            // test downloading the first file in files
            APIResponse<ForgeData> meta = repo.GetItemMetadata(ids.Project, files[0].ID).GetAwaiter().GetResult();
            Relationship storage = meta.Data?.Included[meta.Data.Included.Count - 1].Relationships.Storage;
            
            // storage will be null if the user does not have access to DL
            var storageID = storage?.Data?.ID;

            // encapsulate the IDs of the file to DL
            var obj = new ObjectIDs( storageID, files[0].Name );
            _ = this.DownloadFile( repo, obj);

            //Console.WriteLine(token);
            return 0;
        }

        /// <summary>List all the hubs in a project</summary>
        protected async Task<List<HubAccount>> ListHubs( IDataManagerAPI repo )
        {
            var hubs = await repo.ListHubs();

            hubs.ForEach(h => Console.WriteLine($"[{h.ID}] {h.Name}"));

            return hubs;
        }

        /// <summary>Downloads directly to the file system and returns the metadata</summary>
        /// <param name="repo">DM repo to use</param>
        /// <param name="ids">Bucket and object key</param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        protected async Task<RawFileWrapper> DownloadFile( IDataManagerAPI repo, ObjectIDs ids )
        {
            var dlLink = repo.GetS3DownloadLink( ids ).GetAwaiter().GetResult();

            // use the original filename to save as locally
            var resp = repo.DownloadObject( dlLink.Data, Path.Combine( AppDataRoot.FullName, ids.Name ) ).GetAwaiter().GetResult();
            return resp.Data;
        }

        /// <summary>List Projects in the account</summary>
        /// <param name="repo"></param>
        /// <param name="acctID"></param>
        /// <returns></returns>
        protected async Task<List<ProjectStub>> ListProjects( IDataManagerAPI repo, ForgeID acctID )
        {
            var projs = await repo.ListProjects( acctID );

            // write them all to a desktop file
            DataFileWriter file = new DataFileWriter( Path.Combine( AppDataRoot.FullName, "projects.csv" ) ).Open();
            file.WriteHeader(new string[] { "ID,Name,Root,Platform" });

            projs.ForEach( p => {
                var items = p.SerializeToList();
                file.WriteValuesToLine(items);
            });

            return projs;
        }

        /// <summary>Get a project</summary>
        protected ForgeProject GetProjectTree( IDataManagerAPI repo, ForgeID acctID, ForgeID projID )
        {
            ProjectBuilder proj = new ProjectBuilder( repo );
            var built = proj.Build( new ForgeIDs( acctID, projID ) ).GetAwaiter().GetResult();

            if ( built )
            {
                List<ProjectItem> items = proj.Project.Root.ToList(false);

                DataFileWriter file = new DataFileWriter( Path.Combine( AppDataRoot.FullName, "files.csv" ) ).Open();

                file.WriteHeader( new string[] { "ID,Name,Depth,Last Modified" } );

                items.ForEach( p => {
                    string[] items = { p.ID, p.Name, p.Depth.ToString(), p.LastModified.ToString()  };
                    file.WriteValuesToLine( items );
                } );
            }

            return proj.Project;
        }
    }
}

