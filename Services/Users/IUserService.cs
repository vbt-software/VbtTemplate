using Core.ApiResponse;
using Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Users
{
    public interface IUserService : IEntityService<UserModel>
    {
        public IServiceResponse<UserModel> GetById(int userId);
        public IServiceResponse<UserModel> UpdateAdmin(int userId, bool isAdmin = true);        
    }
}
