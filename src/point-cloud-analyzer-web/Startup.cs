using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Firewall;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace point_cloud_analyzer_web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //var allowedIPs =
            //    new List<IPAddress>
            //    {
            //        IPAddress.Parse("10.20.30.40"),
            //        IPAddress.Parse("1.2.3.4"),
            //        IPAddress.Parse("5.6.7.8")
            //    };

            //var allowedCIDRs =
            //    new List<CIDRNotation>
            //    {
            //        CIDRNotation.Parse("110.40.88.12/28"),
            //        CIDRNotation.Parse("88.77.99.11/8")
            //    };

            app.UseFirewall(
                FirewallRulesEngine
                    .DenyAllAccess()
                //  .ExceptFromIPAddressRanges(allowedCIDRs)
                //  .ExceptFromIPAddresses(allowedIPs)
                .ExceptFromLocalhost()
                .ExceptWhen(ctx=>ctx.Connection.RemoteIpAddress.ToString().StartsWith("140"))
            );

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}