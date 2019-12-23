using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Filters
{
    public class BaseFilter
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int Skip => PageNo * PageSize;

        [JsonIgnore]
        public string OrderByCondition { get; set; }

        [JsonIgnore]
        public ListSortDirection SortDirection { get; set; }

        public BaseFilter()
        {
            if (PageSize == 0)
                PageSize = 20;
        }
    }
}
