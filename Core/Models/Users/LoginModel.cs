using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models.Users
{
    public class LoginModel : BaseModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsMobile { get; set; } = false; //Set Default Value
        public string UnqDeviceId { get; set; }
       
    }

    public class LoginResultModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public long CreatedTokenTime { get; set; }
        public bool HasChangePassword { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
