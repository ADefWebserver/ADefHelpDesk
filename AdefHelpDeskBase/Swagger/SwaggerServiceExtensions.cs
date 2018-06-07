using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdefHelpDeskBase
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("internal", new Info { Title = "Internal API", Version = "v1" });
                c.SwaggerDoc("external",
                    new Info
                    {
                        Title = "External API",
                        Version = "v1",
                        Description = "ADefHelpDesk Web API"
                    });

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new string[] { } }
                });

                // Set the comments path for the Swagger JSON and UI.                
                var xmlPath = Path.GetFullPath(@"ADefHelpDeskApp\ADefHelpDeskApp.xml");
                c.IncludeXmlComments(xmlPath);
                c.OperationFilter<FileUploadOperation>(); //Register File Upload Operation Filter
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/external/swagger.json", "ADefHelpDesk External API V1");
            });

            return app;
        }

        public class FileUploadOperation : IOperationFilter
        {
            public void Apply(Operation operation, OperationFilterContext context)
            {
                if (operation.Parameters != null)
                {
                    // If the method has a paramater "objFile"
                    // rewrite the parameters to create an upload control in swagger
                    if (operation.Parameters.Where(x => x.Name == "objFile").FirstOrDefault() != null)
                    {
                        // Create a collection that does not have objFile
                        // because it will be added with an upload control
                        List<IParameter> colIParameter = new List<IParameter>();
                        foreach (var item in operation.Parameters)
                        {
                            if (item.Name != "objFile")
                            {
                                colIParameter.Add(item);
                            }
                        }

                        operation.Parameters.Clear();
                        operation.Parameters = colIParameter;

                        operation.Parameters.Add(new NonBodyParameter
                        {
                            Name = "objFile",
                            In = "formData",
                            Description = "Upload File",
                            Required = false,
                            Type = "file"
                        });
                        operation.Consumes.Add("multipart/form-data");
                    }
                }
            }
        }
    }
}