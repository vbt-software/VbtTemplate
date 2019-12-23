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
                Schema=new OpenApiSchema { Type="int"},
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "IsMobile",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "bool" },
                Required = false
            });
        }
    }
}
