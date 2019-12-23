using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ApiResponse
{
    public interface IServiceResponse<T>
    {
        IList<T> List { get; set; }
        T Entity { get; set; }

        int Count { get; set; }

        bool IsValid { get;}

        bool IsSuccessful { get; set; }
    }
}
