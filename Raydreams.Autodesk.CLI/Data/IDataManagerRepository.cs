using System;
using Raydreams.Autodesk.CLI.Model;

namespace Raydreams.Autodesk.CLI.Data
{
    /// <summary></summary>
    public interface IDataManagerAPI
    {
        Task<APIResponse<ForgeDataCollection>> ListHubs();

        Task<APIResponse<ForgeDataCollection>> ListProjects( ForgeID id, string? nameFilter = null );

        Task<APIResponse<ForgeData>> GetProject( ForgeIDs ids );
    }
}

