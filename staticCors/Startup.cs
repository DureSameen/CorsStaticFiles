using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace staticCors
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public CorsPolicy Policy { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            Policy = new CorsPolicy();
            Policy.Origins.Add("https://localhost:44313");

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins("https://localhost:44313")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("CorsPolicy");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    if (context.File.Name.ToLower().EndsWith(".json"))
                    {
                        var origin = context.Context.Request.Headers[CorsConstants.Origin];
                        var requestHeaders = context.Context.Request.Headers;

                        var isOptionsRequest = string.Equals(context.Context.Request.Method, CorsConstants.PreflightHttpMethod, StringComparison.OrdinalIgnoreCase);
                        var isPreflightRequest = isOptionsRequest && requestHeaders.ContainsKey(CorsConstants.AccessControlRequestMethod);

                        var corsResult = new CorsResult
                        {
                            IsPreflightRequest = isPreflightRequest,
                            IsOriginAllowed = IsOriginAllowed(Policy, origin),
                        };

                   if (!corsResult.IsOriginAllowed)
                        { context.Context.Response.StatusCode = 204;
                           }
                    }
                    
                }
            });
            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private bool IsOriginAllowed(CorsPolicy policy, StringValues origin)
        {
            if (StringValues.IsNullOrEmpty(origin))
            {
                
                return false;
            }

          
            if (policy.AllowAnyOrigin || policy.IsOriginAllowed(origin))
            {
                
                return true;
            }
           
            return false;
        }
    }
}
