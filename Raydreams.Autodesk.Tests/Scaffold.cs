using Microsoft.Extensions.Configuration;
using Raydreams.Autodesk.Model;

namespace Raydreams.Autodesk.Tests;

/// <summary>Injects local config settings into the tests</summary>
public class Scaffold
{
    private static Scaffold _default = new Scaffold();

    static Scaffold()
    {
        // load the configuarion file
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddJsonFile( $"appsettings.json", true, true );

        IConfigurationRoot configurationRoot = builder.Build();
        _default = configurationRoot.GetSection( nameof( Scaffold ) ).Get<Scaffold>();
    }

    /// <summary>Get the loaded values</summary>
    public static Scaffold Default => _default;

    /// <summary>Azure Data Storage Blob Connection String</summary>
    public ForgeAppClient AppClient { get; set; } = new ForgeAppClient();

    /// <summary>Mongo Connection String</summary>
    //public string MongoConnectionStr { get; set; } = String.Empty;
}