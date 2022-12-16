using System;
using Raydreams.Autodesk.CLI.Model;

namespace Raydreams.Autodesk.CLI.Data
{
    /// <summary></summary>
    public interface IDataManagerAPI
    {
        Task<APIResponse<ForgeDataCollection>> ListHubs();

        Task<List<ProjectStub>> FilterProjects(ForgeID id, string nameFilter);

        Task<List<ProjectStub>> ListProjects(ForgeID hubID);

        Task<APIResponse<ForgeData>> GetProject( ForgeIDs ids );

        Task<APIResponse<ForgeData>> GetFolderByProject(ForgeID projectID, string folderID);

        Task<APIResponse<ForgeDataCollection>> GetFolderContents(ForgeID projectID, string folderID);
    }
}

