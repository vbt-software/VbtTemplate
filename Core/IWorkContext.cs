using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public interface IWorkContext
    {
        /// <summary>
        ///     Gets or sets the current User
        /// </summary>
        int CurrentUserId { get; set; }

        bool IsMobile { get; set; }
        bool IsAdmin { get; set; }
    }
}
