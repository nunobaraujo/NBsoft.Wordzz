using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace NBsoft.Wordzz.Infrastructure
{
    public class SessionKeyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
            var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);
            if (isAuthorized && !allowAnonymous)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<OpenApiParameter>();
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "api-key",
                    In = ParameterLocation.Header,
                    Description = "api access key",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
                //operation.Parameters.Add(new NonBodyParameter
                //{
                //    Name = "session-token",
                //    In = "header",
                //    Description = "session token",
                //    Required = true,
                //    Type = "string"
                //});
            }
        }

    }
}
