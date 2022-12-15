using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raydreams.Autodesk.CLI.Data;
using Raydreams.Autodesk.CLI.IO;
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

            string tokenPath = Path.Combine(IOHelpers.DesktopPath, "tokens", "autodesk.txt");
            //HTTPTokenManager tokenMgr = new HTTPTokenManager(authMgr, tokenPath, 50001);
            //tokenMgr.Writer = new FileTokenIO();

            ITokenManager tokenMgr = new TwoLegTokenManager(authMgr);

            //AutodeskUser user = tokenMgr.GetUser().GetAwaiter().GetResult();

            IDataManagerAPI repo = new DataManagerRepository(tokenMgr);
            var results1 = repo.ListHubs().GetAwaiter().GetResult();

            var results2 = repo.ListProjects( new ForgeID(this.Client.PrimaryHubID) ).GetAwaiter().GetResult();

            //string? token = tokenMgr.GetTokenAsync().GetAwaiter().GetResult();

            //Console.WriteLine(token);
            return 0;
        }
    }
}

