using System;
using Raydreams.Autodesk.CLI.Model;

namespace Raydreams.Autodesk.CLI.Data
{
    /// <summary>The interface for interacting with the Data Manager APIs</summary>
    public interface IDataManagerAPI
    {
        /// <summary></summary>
        Task<List<HubAccount>> ListHubs();

        /// <summary></summary>
        Task<List<ProjectStub>> FilterProjects(ForgeID id, string nameFilter);

        /// <summary></summary>
        Task<List<ProjectStub>> ListProjects(ForgeID hubID);

        /// <summary></summary>
        Task<APIResponse<ForgeData>> GetProject( ForgeIDs ids );

        /// <summary></summary>
        Task<APIResponse<ForgeData>> GetFolderByProject(ForgeID projectID, string folderID);

        /// <summary></summary>
        Task<APIResponse<ForgeDataCollection>> GetFolderContents(ForgeID projectID, string folderID);

        /// <summary></summary>
        Task<APIResponse<ForgeData>> GetItemMetadata(ForgeID projectID, string itemID);

        /// <summary></summary>
        Task<APIResponse<S3SignedDownload>> GetS3DownloadLink(ObjectIDs ids);

        /// <summary></summary>
        Task<APIResponse<RawFileWrapper>> DownloadObject(S3SignedDownload signedURL, string fullPath);

        /// <summary></summary>
        Task<APIResponse<S3SignedUpload>> GetS3UploadLink(ObjectIDs ids);
    }
}
