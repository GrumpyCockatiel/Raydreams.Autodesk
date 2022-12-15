﻿using System;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Raydreams.Autodesk.CLI.Model;
using Raydreams.Autodesk.CLI.Security;
using System.Net;
using System.Text;

namespace Raydreams.Autodesk.CLI.Data
{
    /// <summary></summary>
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
        public async Task<APIResponse<ForgeDataCollection>> ListHubs()
        {
            string path = $"project/v1/hubs";

            return await GetRequest<ForgeDataCollection>(path, true);
        }

        /// <summary>Gets all the projects in the sepcified Hub the token can access</summary>
        /// <param name="hubID">Hub/Account to get projects from</param>
        /// <param name="nameFilter">A CASE sensitive name contains filter. Use null to get 200 projects</param>
        /// <returns>A max of 200 matching projects</returns>
        /// <remarks>GET (data:read) user context optional
        /// filters 
        /// filter on array of project IDs somehow
        /// ?page[number]=1&page[limit]=2
        /// </remarks>
        public async Task<APIResponse<ForgeDataCollection>> ListProjects( ForgeID id, string? nameFilter = null )
        {
            if ( !id.IsValid )
                return new APIResponse<ForgeDataCollection>() { StatusCode = HttpStatusCode.BadRequest };

            StringBuilder path = new StringBuilder( $"project/v1/hubs/{id.DM}/projects" );

            // apply the filter as a parameter
            if ( !String.IsNullOrWhiteSpace( nameFilter ) )
            {
                // consider a min filter length here
                if ( nameFilter.Length < 3 )
                    return new APIResponse<ForgeDataCollection>() { Data = new ForgeDataCollection() };

                nameFilter = nameFilter.Trim();
                string filter = $"?filter[name]-contains={nameFilter}";
                path.Append( filter );
            }

            return await GetRequest<ForgeDataCollection>( path.ToString(), true );
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

    }
}

