using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HideFromSwaggerAttribute : Attribute
{
}

public class HideFromSwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hideFromSwagger = (context.ApiDescription.RelativePath.StartsWith("Account/"));

        if (hideFromSwagger)
        {
            operation.Tags.Clear();
            operation.Responses.Clear();
            operation.Description = null;
            operation.Summary = null;
            operation.OperationId = null;
        }
    }
}

public class HideDefaultSectionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathsToRemove = swaggerDoc.Paths
            .Where(pathItem => pathItem.Value.Operations.Values.All(operation =>
                operation.Tags == null || operation.Tags.All(tag => tag.Name == "default")))
            .Select(pathItem => pathItem.Key)
            .ToList();

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }
    }
}