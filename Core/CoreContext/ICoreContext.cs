using System;
using System.Collections.Generic;
using System.Text;

namespace Core.CoreContext
{
    public interface ICoreContext
    {
        string CdnUrl { get; }
        string CdnPath { get; }
        string MobileVersion { get; }
        string EnvironmentName { get; }
        int TokenExpireTime { get; }
        int RefreshTokenExpireTime { get; }
        bool Contains(string source, string destination);
    }
}
