using System;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Raydreams.Autodesk.CLI.Model;
using Raydreams.Autodesk.CLI.Security;
using System.Net;
using System.Text;

namespace Raydreams.Autodesk.CLI.Data
{
    /// <summary>Concrete implementation for interacting with the Data Manager APIs</summary>
    public partial class DataManagerRepository : IDataManagerAPI
    {
        #region [ Fields ]

        /// <summary>Default seconds before a request will timeout</summary>
        private int _timeout = 100;

        /// <summary>Base URL to the API</summary>
        public const string TenantBase = "https://developer.api.autodesk.com";

        /// <summary>Impersonation override field</summary>
        public const string XUserIDField = "x-user-id";

        /// <summary>The max number of calls that will ever be made for bulk lookups</summary>
		public const int MaxLoops = 20;

        #endregion [ Fields ]

        /// <summary></summary>
        /// <param name="tokener">The token manager to use</param>
        /// <param name="timeout">Seconds before a download will timeout as taking too long</param>
        public DataManagerRepository(ITokenManager tokener, int timeout = 100)
        {
            this.Tokener = tokener;

            this._timeout = Math.Clamp(timeout, 10, 1200);
        }

        #region [ Properties ]

        /// <summary>Sets the default HTTP client timeout for Uploads and Downloads ONLY for now</summary>
        public TimeSpan DownloadTimeout => TimeSpan.FromSeconds(this._timeout);

        /// <summary>Handles retrieving a token</summary>
        public ITokenManager Tokener { get; set; }

        /// <summary>Add x-user-id to the header</summary>
        public bool IsXUser { get; set; }

        #endregion [ Properties ]

        /// <summary>Retrieves the token from the token manager</summary>
        /// <returns>The token as a string. If the Tokener is null, the the results will be null</returns>
        protected async Task<string?> Token() => await this.Tokener.GetTokenAsync();

        /// <summary>Gets all the hubs this token has access to</summary>
        /// <returns></returns>
        /// <remarks>
        /// GET (data:read) user context optional
        /// The user will see ANY hubs they have ANY access to
        /// </remarks>
        public async Task<List<HubAccount>> ListHubs()
        {
            List<HubAccount> results = new List<HubAccount>();

            string path = $"project/v1/hubs";

            var resp = await GetRequest<ForgeDataCollection>(path, true);

            if ( resp.IsSuccess && resp.Data != null && resp.Data.Result != null )
                resp.Data.Result.ForEach( h => results.Add( new HubAccount( h ) ) );

            return results;
        }

        /// <summary>Gets all the projects </summary>
        /// <param name="hubID">Hub/Account to get projects from</param>
        /// <param name="nameFilter">A CASE sensitive name contains filter. Use null to get 200 projects</param>
        /// <returns>A max of 200 matching projects</returns>
        /// <remarks>GET (data:read) user context optional
        /// filters 
        /// filter on array of project IDs somehow
        /// ?page[number]=1&page[limit]=2
        /// </remarks>
        public async Task<List<ProjectStub>> FilterProjects( ForgeID id, string nameFilter )
        {
            List<ProjectStub> results = new List<ProjectStub>();

            if ( !id.IsValid || !String.IsNullOrWhiteSpace( nameFilter ) )
                return results;

            // consider a min filter length here
            if ( nameFilter.Length < 3 )
                return results;

            nameFilter = nameFilter.Trim();

            string path = $"project/v1/hubs/{id.DM}/projects?filter[name]-contains={nameFilter}";
            
            APIResponse<ForgeDataCollection> resp = await GetRequest<ForgeDataCollection>( path, true );

            if ( resp.IsSuccess && resp.Data != null && resp.Data.Result != null )
                resp.Data.Result.ForEach( p => results.Add( new ProjectStub( p ) ) );

            return results;
        }

        /// <summary>Gets all the projects in a hub</summary>
        /// <param name="hubID">The account ID</param>
        /// <param name="allStatuses">There's no status property in DM</param>
        /// <returns></returns>
        public async Task<List<ProjectStub>> ListProjects( ForgeID hubID )
        {
            int limit = 200;

            // valiadate the input
            if ( !hubID.IsValid )
                return new List<ProjectStub>();

            int page = 0;

            APIResponse<ForgeDataCollection> resp = null;
            List<ProjectStub> results = new List<ProjectStub>();

            // limit to MaxLoops * 100 projects
            do
            {
                // format the path
                string path = $"project/v1/hubs/{hubID.DM}/projects?page[number]={page}&page[limit]={limit}";

                resp = await GetRequest<ForgeDataCollection>( path, true );

                if ( resp.IsSuccess && resp.Data != null && resp.Data.Result != null )
                {
                    resp.Data.Result.ForEach(p => results.Add(new ProjectStub(p)));
                }

                // uptick
                ++page;

            } while ( page < MaxLoops && resp.IsSuccess && resp.Data != null && resp.Data.Result != null );

            // filter out non active??

            return results.OrderBy( p => p.Name ).ToList();
        }

        /// <summary>Gets details on a specific project inclulding the top level folder</summary>
        /// <remarks>GET (data:read) user context optional</remarks>
        public async Task<APIResponse<ForgeData>> GetProject( ForgeIDs ids )
        {
            if ( !ids.IsValid )
                return new APIResponse<ForgeData>() { StatusCode = HttpStatusCode.BadRequest };

            string path = $"project/v1/hubs/{ids.Account.DM}/projects/{ids.Project.DM}";

            return await GetRequest<ForgeData>( path, true );
        }

        /// <summary>Gets a specific folder by its project ID and Folder ID</summary>
        /// <remarks>GET (data:read) user context optional
        /// Will return Forbidden 403 if you try to use on a folder you dont have rights to
        /// </remarks>
        public async Task<APIResponse<ForgeData>> GetFolderByProject( ForgeID projectID, string folderID )
        {
            
            if ( !projectID.IsValid || String.IsNullOrWhiteSpace( folderID ) )
                return new APIResponse<ForgeData>() { StatusCode = HttpStatusCode.BadRequest };

            string path = $"data/v1/projects/{projectID.DM}/folders/{folderID}";

            return await GetRequest<ForgeData>( path, true );
        }

        /// <summary>Get the content of a folder including child files/folders</summary>
        /// <remarks>GET (data:read) user context optional
        /// Will return Forbidden 403 if you try to use on a folder you dont have rights to
        /// </remarks>
        public async Task<APIResponse<ForgeDataCollection>> GetFolderContents( ForgeID projectID, string folderID )
        {
            if ( !projectID.IsValid || String.IsNullOrWhiteSpace( folderID ) )
                return new APIResponse<ForgeDataCollection>() { StatusCode = HttpStatusCode.BadRequest };

            string path = $"data/v1/projects/{projectID.DM}/folders/{folderID}/contents?includeHidden=false";

            return await GetRequest<ForgeDataCollection>( path, true );
        }

        /// <summary>Get the detailed metadata on a single item. Use ListItems for multiple items</summary>
        /// <remarks>GET (data:read) user context optional
        /// Will return Forbidden 403 if you try to use on a folder you dont have rights to
        /// </remarks>
        public async Task<APIResponse<ForgeData>> GetItemMetadata(ForgeID projectID, string itemID)
        {

            if (!projectID.IsValid || String.IsNullOrWhiteSpace(itemID))
                return new APIResponse<ForgeData>() { StatusCode = HttpStatusCode.BadRequest };

            string path = $"data/v1/projects/{projectID.DM}/items/{itemID.Trim()}?includePathInProject=true";

            return await GetRequest<ForgeData>(path, true);
        }

    }
}

