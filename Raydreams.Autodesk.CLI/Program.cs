using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raydreams.Autodesk.CLI.Data;
using Raydreams.Autodesk.CLI.IO;
using Raydreams.Autodesk.CLI.Model;
using Raydreams.Autodesk.CLI.Security;

namespace Raydreams.Autodesk.CLI
{
    public static class Program
    {
        /// <summary>Main entry class</summary>
        /// <param name="args">any future command line args</param>
        /// <returns>exit value</returns>
        public static int Main(string[] args)
        {
            Console.WriteLine("Starting...");

            // check if running on macOS
            bool isMac = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            // get the environment var
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // inject all the input
            IHostBuilder builder = new HostBuilder()
            .ConfigureLogging((ctx, logging) =>
            {
                logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                logging.ClearProviders();
                logging.AddDebug();
                logging.AddConsole();
            })
            .ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddJsonFile($"appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true);
                config.AddEnvironmentVariables();

                if (args != null)
                    config.AddCommandLine(args);
            })
            .ConfigureServices((ctx, services) =>
            {
                services.AddOptions();

                // get the app config file
                //services.AddScoped<AppConfig>( p =>
                //{
                //    return ctx.Configuration.GetSection( "AppConfig" ).Get<AppConfig>();
                //} );

                // add the App Client Info
                services.AddScoped<ForgeAppClient>(p => {
                    return ctx.Configuration.GetSection("ForgeAppClient").Get<ForgeAppClient>();
                });

                // add the logger
                services.AddLogging();

                // add hosted service
                services.AddHostedService<AutodeskCLI>();
            });

            // run the host sync
            // using just Build gives the Worker you can pass a cancellation token to
            builder.Build().Start();

            Console.WriteLine("Stopping...");

            return 0;
        }
    }
}


//{
//    ForgeAppClient clClient = new("RP0lMVSy9ncgMzMuzyvuy1L2d6Mqbr7E", "MAM2h4yAA9j9yQkw", "http://localhost:8000/forge-callback");

//    ForgeAppClient testClient = new("pM6UgwvZ4b9fJQrXhLt3t7kueTchfA5w", "GbL7KohlHZq0gNCr", "http://localhost:50001/token");

//    AuthenticationManager authMgr = new AuthenticationManager(clClient);
//    authMgr.Scopes = ForgeScopes.UserRead | ForgeScopes.UserProfileRead | ForgeScopes.DataRead;

//    string tokenPath = Path.Combine(IOHelpers.DesktopPath, "tokens", "autodesk.txt");
//    HTTPTokenManager tokenMgr = new HTTPTokenManager(authMgr, tokenPath, 50001);
//    tokenMgr.Writer = new FileTokenIO();

//    AutodeskUser user = tokenMgr.GetUser().GetAwaiter().GetResult();

//    IDataManagerRepository repo = new DataManagerRepository(tokenMgr);
//    var results = repo.ListHubs().GetAwaiter().GetResult();

//    //string? token = tokenMgr.GetTokenAsync().GetAwaiter().GetResult();

//    //Console.WriteLine(token);
//    _ = 5;
//}