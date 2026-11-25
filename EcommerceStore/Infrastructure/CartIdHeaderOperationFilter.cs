using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EcommerceStore.Infrastructure
{
    /// <summary>Adds an optional X-Cart-Id header to the cart endpoints in Swagger so guest carts can be tested.</summary>
    public sealed class CartIdHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller)
                && controller == "Cart")
            {
                operation.Parameters ??= new List<OpenApiParameter>();
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Cart-Id",
                    In = ParameterLocation.Header,
                    Required = false,
                    Description = "Guest cart id (any GUID). Ignored when a Bearer token is sent.",
                    Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
                });
            }
        }
    }
}
