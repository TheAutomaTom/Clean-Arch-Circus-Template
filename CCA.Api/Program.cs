using CCA.Api.Config;
using CCA.Api.Config.Routing;
using CCA.Api.Config.Swagger;
using CCA.Api.Middleware;
using CCA.Core.Application.Config;
using CCA.Data.Infra.Emails.Config;
using CCA.Data.Persistence.Config;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;
using System.Text.Json.Serialization;
using CCA.Data.Infra.Auth.Config;

namespace CCA.Api
{
  public class Program
  {
    public static void Main(string[] args)
    {
      //******************************************************************************************//
      var builder = WebApplication.CreateBuilder(args);
      //******************************************************************************************//

      var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      if (env == null)
      {
        // Set the default environment to Development
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        env ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      }

      var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env}.json", optional: true
        ).Build();


      builder.Services.AddLogger(config, env);
      builder.Host.UseSerilog();

      builder.Services.AddCorsPolicy(builder.Configuration);
      
      builder.Services.AddAuthService(builder.Configuration);
      builder.Services.AddAuth(builder.Configuration);

      builder.Services.AddControllers();
      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();

      //builder.Services.AddLocalOutputCache(config);
      builder.Services.AddDistributedCache(config);

      // Internal services
      builder.Services.AddDbContexts(builder.Configuration);
      builder.Services.AddMediatorSupport();
      builder.Services.AddElasticsearch(config);
      builder.Services.AddEmailService(builder.Configuration);

      // Exposed features
      builder.Services.AddGraphQL(builder.Configuration);

      builder.Services.AddControllers()
											.AddJsonOptions(o => { 
												o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); 
											});

			builder.Services.AddControllersWithViews(o =>
      {
        o.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
      });

      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwagger();


      // New .Net 8 replacement for custom Exception Middleware
      builder.Services.AddExceptionHandler<ExceptionHandlerConfig>();
      builder.Services.AddProblemDetails();

      // Required for some Docker server stuff.
      builder.Services.AddDataProtection()
          .PersistKeysToFileSystem(new DirectoryInfo(@"..\Docker\keys"));


      //******************************************************************************************//
      var app = builder.Build();
      //******************************************************************************************//

      app.UseExceptionHandler();

      //app.UseOutputCache();

      app.UseCors(CorsConfig.Policy);
      app.UseHttpsRedirection();
      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseSwagger();
      app.UseSwaggerUI();

      app.MapControllers();
      app.MapGraphQL();

      app.Run();
    }
  }
}
