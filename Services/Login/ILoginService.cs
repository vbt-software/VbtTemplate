using Core.ApiResponse;
using Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Login
{
    public interface ILoginService : IEntityService<LoginModel>
    {
        ServiceResponse<LoginResultModel> CheckLogin(LoginModel model);
        ServiceResponse<LoginResultModel> GetOnBeHalfofPassword(int userId);
        ServiceResponse<LoginResultModel> CheckBeHalfofPassword(string beHalfOfPassword);
    }
}
