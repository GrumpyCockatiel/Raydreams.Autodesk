using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raydreams.Autodesk.CLI.Data;
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
        public AutodeskCLI(ForgeAppClient client)
        {
            this.Client = client;
        }

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
            AuthenticationManager authMgr = new AuthenticationManager(this.Client);
            authMgr.Scopes = ForgeScopes.UserRead | ForgeScopes.UserProfileRead | ForgeScopes.AccountRead | ForgeScopes.DataRead;

            ForgeID acctID = new ForgeID( this.Client.PrimaryHubID);

            string tokenPath = Path.Combine( IOHelpers.DesktopPath, "tokens", "autodesk.txt" );
            //HTTPTokenManager tokenMgr = new HTTPTokenManager(authMgr, tokenPath, 50001);
            //tokenMgr.Writer = new FileTokenIO();

            ITokenManager tokenMgr = new TwoLegTokenManager(authMgr);
            //AutodeskUser user = tokenMgr.GetUser().GetAwaiter().GetResult();

            IDataManagerAPI repo = new DataManagerRepository(tokenMgr);

            //string? token = tokenMgr.GetTokenAsync().GetAwaiter().GetResult();

            // test getting a project tree
            var projects = this.ListProjects(repo, acctID).GetAwaiter().GetResult();

            //Console.WriteLine(token);
            return 0;
        }

        protected void ListHubs( IDataManagerAPI repo )
        {
            var results1 = repo.ListHubs().GetAwaiter().GetResult();
        }

        protected async Task<List<ProjectStub>> ListProjects( IDataManagerAPI repo, ForgeID acctID )
        {
            return await repo.ListProjects( acctID );
        }

        protected void GetProjectTree( IDataManagerAPI repo, ForgeID acctID, ForgeID projID )
        {
            ProjectBuilder proj = new ProjectBuilder( repo );
            var tree = proj.Build( new ForgeIDs( acctID, new ForgeID( "b.4fcf928a-f8be-4243-aff5-bbb56fd105d4" ) ) ).GetAwaiter().GetResult();
        }
    }
}

