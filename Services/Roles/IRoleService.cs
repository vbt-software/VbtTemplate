using Core.ApiResponse;
using Core.Models.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Roles
{
    public interface IRoleService : IEntityService<RoleModel>
    {
        public IServiceResponse<RoleModel> GetRoleById(int userId, int roleGroupID, Int64 roleID);
        public IServiceResponse<RoleModel> GetRoleListByGroupId(int userId, int roleGroupID);
    }
}
