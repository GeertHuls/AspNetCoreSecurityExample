using System;
using AspNetSecurity_m1.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NWebsec.AspNetCore.Middleware;

namespace AspNetSecurity_m1
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
          
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Conference}/{action=Index}/{id?}");
            });
        }
    }
}