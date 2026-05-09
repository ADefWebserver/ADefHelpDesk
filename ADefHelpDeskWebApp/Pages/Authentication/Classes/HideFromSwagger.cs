using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HideFromSwaggerAttribute : Attribute
{
}

/// <summary>
/// Removes any operation whose route starts with "Account/" from the generated
/// OpenAPI document. Replaces the previous Swashbuckle-based filters that hid
/// the Identity UI endpoints.
/// </summary>
public class HideAccountEndpointsDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (document.Paths is null)
        {
            return Task.CompletedTask;
        }

        var pathsToRemove = document.Paths
            .Where(p => p.Key.TrimStart('/').StartsWith("Account/", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Key)
            .ToList();

        foreach (var path in pathsToRemove)
        {
            document.Paths.Remove(path);
        }

        return Task.CompletedTask;
    }
}
