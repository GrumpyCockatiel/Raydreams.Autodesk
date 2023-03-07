using Raydreams.Autodesk.Data;
using Raydreams.Autodesk.IO;
using Raydreams.Autodesk.Logic;
using Raydreams.Autodesk.Model;
using Raydreams.Autodesk.Security;
using Raydreams.Autodesk.Extensions;

namespace Raydreams.Autodesk.Tests;

[TestClass]
public class ForgeAPITests
{
    public static readonly string DesktopPath = Environment.GetFolderPath( Environment.SpecialFolder.DesktopDirectory );

    protected static Scaffold Config = default!;

    [ClassInitialize()]
    public static void ClassInit( TestContext context )
    {
        Config = Scaffold.Default;
    }

    [TestInitialize()]
    public void Initialize()
    {
        AuthenticationManager authMgr = new AuthenticationManager( Config.AppClient );
        authMgr.Scopes = ForgeScopes.UserRead | ForgeScopes.UserProfileRead | ForgeScopes.AccountRead | ForgeScopes.DataRead;

        // create a 2 leg token
        ITokenManager tokenMgr = new TwoLegTokenManager( authMgr );

        // make a Data Manager Repo
        this.Repo = new DataManagerRepository( tokenMgr );

        this.IDs = new ForgeIDs( Config.AppClient.PrimaryHubID, Config.AppClient.DefaultProjectID );
    }

    public IDataManagerAPI Repo { get; set; }

    // load the default IDs for testing
    public ForgeIDs IDs { get; set; }

    /// <summary>Test to list all hubs the client can access</summary>
    [TestMethod]
    public void ListHubsTest()
    {
        var hubs = this.Repo.ListHubs().Result;

        Assert.IsNotNull(hubs);
    }

    /// <summary>Test to list all hubs the client can access</summary>
    [TestMethod]
    public void ListFilesTest()
    {
        List<ProjectItem> results = new List<ProjectItem>(); 

        ProjectBuilder proj = new ProjectBuilder( this.Repo );
        var built = proj.Build( this.IDs ).Result;

        if ( built )
            results = proj.Project.Root.ToList( false );
        
        Assert.IsNotNull( results );
        Assert.IsTrue( results.Count > 0 );
    }
}
