using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace SCAA_API
{
    public static class Extensions
    {
        public static void AddControllers(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
        }

        public static void AddSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SCAA API",
                    Version = "v1",
                    Description = "API for SCAA final project"
                });

                #region Security Header
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Just paste your token below (without the 'Bearer ' prefix)"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
                #endregion

                #region XML Documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
                #endregion

                #region Examples
                options.ExampleFilters();
                #endregion

            });

            builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();
        }

    }
}
