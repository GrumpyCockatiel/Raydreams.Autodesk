using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Raydreams.Autodesk.CLI.Model
{
    /// <summary>Flag Enum of the various Autodesk scopes</summary>
    /// <remarks>Not all scopes are included yet since they are not used
    /// https://forge.autodesk.com/en/docs/oauth/v2/developers_guide/scopes/
    /// </remarks>
    [FlagsAttribute]
    public enum ForgeScopes
    {
        /// <summary>No scopes defined</summary>
        [Description( "" )]
        None = 0,
        /// <summary></summary>
        [Description( "user:read" )]
        UserRead = 1,
        /// <summary></summary>
        [Description( "user-profile:read" )]
        UserProfileRead = 2,
        /// <summary></summary>
        [Description( "data:read" )]
        DataRead = 4,
        /// <summary>Allows overwriting existing items</summary>
        [Description( "data:write" )]
        DataWrite = 8,
        /// <summary>Allows creating but NOT overwriting items</summary>
        [Description( "data:create" )]
        DataCreate = 16,
        /// <summary></summary>
        [Description( "account:read" )]
        AccountRead = 32,
        /// <summary></summary>
        [Description( "account:write" )]
        AccountWrite = 64
    }
}
