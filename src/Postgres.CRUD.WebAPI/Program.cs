
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Postgres.CRUD.DataAccess.Configs;
using Postgres.CRUD.DataAccess.DatabaseContext;
using Postgres.CRUD.DataAccess.Repository;
using Postgres.CRUD.WebAPI.Configurations;

namespace Postgres.CRUD.WebAPI
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      var pgsqlSettings = builder.Configuration.GetSection("PostgresSettings").Get<PostgresSettings>();
      var entraIdConfig = builder.Configuration.GetSection("EntraIdSettings").Get<EntraIdSettings>();
      builder.Services.Configure<PostgresSettings>(builder.Configuration.GetSection("PostgresSettings"));
      builder.Services.Configure<PostgresSettings>(builder.Configuration.GetSection("EntraIdSettings"));

      builder.Services.AddControllers();
      builder.Services.AddOpenApi();
      builder.Services.AddScoped<IBookRepository, BookRepository>();
      builder.Services.AddScoped<DbAuthInterceptor>();
      builder.Services.AddPostgreSql(pgsqlSettings, builder.Environment.IsDevelopment());
      builder.Services.AddSingleton(sp =>
      new ClientSecretCredential(
        entraIdConfig.TenantId,
        entraIdConfig.ServicePrincipalId,
        pgsqlSettings.ClientSecret));

      builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraIdSettings"));

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.MapOpenApi();
      }

      app.UseHttpsRedirection();

      app.UseAuthentication();
      app.UseAuthorization();


      app.MapControllers();

      app.Run();
    }
  }
}
