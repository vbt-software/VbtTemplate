using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models.Users
{
    public class UserModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Gsm { get; set; }
        public bool IsDeleted { get; set; }
    }
}
