using System.IO;
using AspNetSecurity.Data;
using AspNetSecurity.Repositories;
using AspNetSecurity.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NWebsec.AspNetCore.Middleware;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetSecurity
{
    public class Startup
    {
        private readonly IHostingEnvironment env;

        public Startup(IHostingEnvironment env)
        {
            this.env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDataProtection();

            // Setup Access-Control-Allow-Origin header
            services.AddCors(options =>
            {
                options.AddPolicy("AllowBankCom",
                    c => c.WithOrigins("https://bank.com"));
            });

            if (!env.IsDevelopment())
                services.Configure<MvcOptions>(o =>
                    o.Filters.Add(new RequireHttpsAttribute()));

            services.AddSingleton<ConferenceRepo>();
            services.AddSingleton<ProposalRepo>();
            services.AddSingleton<AttendeeRepo>();
            services.AddSingleton<PurposeStringConstants>();

            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                //.AddUserSecrets() // disable because of error during ef migration
                ;
            var configuration = builder.Build();
            // use dotnet cli to set secrets, ex:
            // > dotnet user-secrets set databasepwd secret
            var databasepwd = configuration["databasepwd"];

            services.AddDbContext<ConfArchDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("ConfArchConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly("AspNetSecurity")));

            services.AddIdentity<ConfArchUser, IdentityRole>(
                    opt => opt.Password.RequireNonAlphanumeric = false)
                .AddEntityFrameworkStores<ConfArchDbContext>()
                // Token providers generate codes to do: email verification, email reset
                // 2fa and phone number confirmation.
                .AddDefaultTokenProviders()
                
                // use custom token provider:
                //.AddTokenProvider<TopSecurityStampBasedTokenProvider<ConfArchUser>>("Custom token provider")
                ;

            // Migrate new database, use Microsoft.EntityFrameworkCore.Tools.DotNet (see csproj file).
            // The open command line:
            // > dotnet ef migrations add initial
            // > dotnet ef database update

            // Use custom claims principle factory to include the birthday claim
            services.AddTransient<IUserClaimsPrincipalFactory<ConfArchUser>,
                ConfArchUserClaimsPrincipalFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            // Csp prevents loading inline javascript, preventing XSS attacks.
            app.UseCsp(options => options.DefaultSources(s => s.Self())
                .StyleSources(s => s.Self().CustomSources("maxcdn.bootstrapcdn.com"))
                .FontSources(s => s.Self().CustomSources("maxcdn.bootstrapcdn.com"))
            );

            // Sets the X-Frame-Options header to prevent click jacking attacks.
            app.UseXfo(o => o.Deny());

            app.UseDeveloperExceptionPage();

            //Add the HSTS header in order to enforce ssl!
            if (!env.IsDevelopment())
                app.UseHsts(c => c.MaxAge(days: 365)
                    //The preload option will enforce ssl even
                    //with first call ever to the site.
                    //First register your domain first at
                    //https://hstspreload.appspot.com
                    .Preload());

            app.UseStaticFiles();

            // Setup of cookie middleware needed to handle the cookie
            // that the entity framework uses.
            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Conference}/{action=Index}/{id?}");
            });
        }
    }
}