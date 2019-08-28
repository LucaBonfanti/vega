using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using vega.Persistence;
using vega.Core;
using AutoMapper;
using vega.Core.Models;
using vega.Controllers;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Vega
{
  public class Startup
  {

        // public Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder AllowAnyOrigin { get; }

        public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

        if (env.IsDevelopment())
            builder = builder.AddUserSecrets<Startup>();

        builder = builder.AddEnvironmentVariables();
        Configuration = builder.Build();
    }

    readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
    
    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<PhotoSettings>(Configuration.GetSection("PhotoSettings"));

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient<IPhotoService, PhotoService>();
        services.AddTransient<IPhotoStorage, FileSystemPhotoStorage>();

        services.AddAutoMapper();

        services.AddDbContext<VegaDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

    /*  
        services.AddCors(options => {
            options.AddPolicy(MyAllowSpecificOrigins,
            builder => {
                builder.WithOrigins("http://*:5000")
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        });
    */
        services.AddCors();
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
        // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            
            services.AddAuthentication(x =>
            {
                // x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                // const string V = "https://api.vega.com";
                // x.Audience = V;
                // x.ClaimsIssuer = "Issuer";
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                //  IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                //  ValidAudience = V,
                //  ValidIssuer = "Issuer",
                    ValidateIssuer = false,
                    ValidateAudience = false
                //  ValidateLifetime = true,
                //  RequireSignedTokens = true,
                //  ClockSkew = System.TimeSpan.FromMinutes(0)
                };
            });


        // configure DI for application services
        // services.AddScoped<IUserService, UserService>();

        // Add framework services.
        // services.AddMvc();
    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        loggerFactory.AddConsole(Configuration.GetSection("Logging"));
        loggerFactory.AddDebug();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                HotModuleReplacement = true,
                HotModuleReplacementEndpoint = "/dist/__webpack_hmr"
            });
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();

        // app.UseCors(MyAllowSpecificOrigins); 
           app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

        /* 'UseJwtBearerAuthentication is obsolete. Configure JwtBearer authentication with AddAuthentication().AddJwtBearer in ConfigureServices. 
        var options = new JwtBearerOptions
        {
            Audience = "https://api.vega.com",
            Authority = "https://vegaproject.auth0.com/"
        };
        app.UseJwtBearerAuthentication(options);
        */
        app.UseAuthentication();
        
        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");

            routes.MapSpaFallbackRoute(
                name: "spa-fallback",
                defaults: new { controller = "Home", action = "Index" });
        });
    }
  }
}
