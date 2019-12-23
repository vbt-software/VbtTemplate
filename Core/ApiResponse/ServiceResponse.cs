using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApiResponse
{
    [Serializable]
    public class ServiceResponse<T> : IServiceResponse<T>
    {
        public bool HasExceptionError { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ExceptionMessage { get; set; }

        public List<string> ValidationErrorList { get; set; }

        public IList<T> List { get; set; }

        [JsonProperty]
        public T Entity { get; set; }

        public int Count { get; set; }

        public bool IsValid => !HasExceptionError && !ValidationErrorList.Any() && string.IsNullOrEmpty(ExceptionMessage);

        public bool IsSuccessful { get; set; }

        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public long CreatedTokenTime { get; set; }

        public ServiceResponse(HttpContext context)
        {
            List = new List<T>();
            ValidationErrorList = new List<string>();

            if (context?.Items["token"] != null)
            {
                Token = (string)context.Items["token"];
            }

            if (context?.Items["refreshToken"] != null)
            {
                RefreshToken = (string)context.Items["refreshToken"];
            }

            if (context?.Items["createdTokenTime"] != null)
            {
                CreatedTokenTime = (long)context.Items["createdTokenTime"];
            }
        }
    }
}
