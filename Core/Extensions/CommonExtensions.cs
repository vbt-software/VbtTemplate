using Core.ApiResponse;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Extensions
{
    public static class CommonExtensions
    {
        public static void Merge<T>(this ServiceResponse<T> controllerResponse, ServiceResponse<T> serviceResponse)
        {
            controllerResponse.Entity = serviceResponse.Entity;
            controllerResponse.List = serviceResponse.List;
            controllerResponse.IsSuccessful = serviceResponse.IsSuccessful;
            controllerResponse.ValidationErrorList = serviceResponse.ValidationErrorList;
            controllerResponse.HasExceptionError = serviceResponse.HasExceptionError;
            controllerResponse.Count = serviceResponse.Count;
            controllerResponse.ExceptionMessage = serviceResponse.ExceptionMessage;
        }
    }
}
