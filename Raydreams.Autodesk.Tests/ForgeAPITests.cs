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
        authMgr.Scopes = ForgeScopes.UserRead | ForgeScopes.UserProfileRead | ForgeScopes.AccountRead | ForgeScopes.DataRead | ForgeScopes.DataWrite | ForgeScopes.AccountWrite | ForgeScopes.DataCreate;

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
        var results = this.Repo.ListHubs().Result;

        Assert.IsNotNull( results );
    }

    /// <summary></summary>
    [TestMethod]
    public void ListProjectsTest()
    {
        var results = this.Repo.ListProjects( this.IDs.Account ).Result;

        Assert.IsNotNull( results );
        Assert.IsTrue( results.Count > 0 );
    }

    /// <summary></summary>
    [TestMethod]
    public void ListFilesTest()
    {
        List<ProjectItem> results = new List<ProjectItem>(); 

        ProjectBuilder proj = new ProjectBuilder( this.Repo );
        bool built = proj.Build( this.IDs ).Result;

        if ( built )
            results = proj.Project.Root.ToList( false );
        
        Assert.IsNotNull( results );
        Assert.IsTrue( results.Count > 0 );
    }

    /// <summary>The end to end process to upload a version 1 file to a project</summary>
    [TestMethod]
    public void UploadFileTest()
    {
        RawFileWrapper input = new RawFileWrapper();

        APIResponse<ForgeData> verRes = null;

        try
        {

            // read a local binary file
            input.Data = File.ReadAllBytes( Path.Combine( DesktopPath, "TestImages", "volvox.jpg" ) );
            input.Filename = "volvox.jpg";
            input.ContentType = "image/jpg";

            // need a folder ID and a filename
            ProjectBuilder proj = new ProjectBuilder( this.Repo );
            bool built = proj.Build( this.IDs ).Result;

            // get the folder to upload to
            var files = proj.Project.Root.FindByName( "Test Files" );

            // populate a new request item
            CreateStorageRequest item = new CreateStorageRequest( input.Filename, files[0].ID );

            // create a new storage location - POST
            APIResponse<ForgeData> meta = this.Repo.InsertStorage( this.IDs.Project, item ).Result;

            // parse the IDs
            ObjectIDs ids = new ObjectIDs( meta.Data.Result.ID );

            // get an upload link - STEP 4
            APIResponse<S3SignedUpload> s3Url = this.Repo.GetS3UploadLink( ids ).Result;

            // now upload the bytes themselves - STEP 5
            APIResponse<bool> results = this.Repo.PutObject( input, s3Url.Data.URLs[0] ).Result;

            // now complete the upload - STEP 6
            var complete = this.Repo.PostS3Upload( ids, s3Url.Data.UploadKey ).Result;

            // finally create the first version - STEP 7
            InsertItemRequest ver1 = new InsertItemRequest( input.Filename, files[0].ID, complete.Data.ObjectID );

            // now insert version 1
            verRes = this.Repo.InsertItem( this.IDs.Project, ver1 ).Result;
        }
        catch ( Exception exp )
        {
            _ = 5;
        }

        Assert.IsTrue( verRes?.IsSuccess );
    }
}
