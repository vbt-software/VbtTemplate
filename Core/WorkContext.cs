using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class WorkContext : IWorkContext
    {
        public int CurrentUserId { get; set; }
        public bool IsMobile { get; set; }
    }
}
