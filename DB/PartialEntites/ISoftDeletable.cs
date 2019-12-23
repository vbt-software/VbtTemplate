using System;
using System.Collections.Generic;
using System.Text;

namespace DB.PartialEntites
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
}
