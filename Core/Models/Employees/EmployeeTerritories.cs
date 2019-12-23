using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class EmployeeTerritory : BaseModel
    {
        public int EmployeeId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string Region { get; set; }
        public string Territory { get; set; }
    }
}
