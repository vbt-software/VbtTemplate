using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateProject.Infrastructure
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "UserId",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "int" },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "IsMobile",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "bool" },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "RefreshToken",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "string" },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "MobileVersion",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "string" },
                Required = false
            });
            //Mobile için RedisKeyler'de UnqDeviceId eklenir.
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "UnqDeviceId",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "string" },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "BeHalfOfToken",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "string" },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "BeHalfOfUserId",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "int" },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "BeHalfOfPassword",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "string" },
                Required = false
            });
        }
    }
}
