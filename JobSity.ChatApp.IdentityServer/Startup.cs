using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JobSity.ChatApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;

namespace JobSity.ChatApp.IdentityServer
{
    public class Startup
    {
        IConfiguration _configuration;
        ILogger<Startup> _logger;
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var identityDbConnectionString = _configuration.GetSection("ConnectionStrings:IdentityDb").Value;

            services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.AddSeq(_configuration.GetSection("Seq"));
                    });

            services.AddDbContext<IdentityChatDbContext>(options => {
                options.UseSqlServer(identityDbConnectionString);
            });

            services.AddIdentity<IdentityUser, IdentityRole>(config =>{
                config.Password.RequiredLength = 4;
                // keep it simple for dev purpose
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<IdentityChatDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(config => {
                config.Cookie.Name = "ChatIdentityCookie";
                config.LoginPath = "/Authentication/Login";
            });

            

            services.AddIdentityServer()
                .AddAspNetIdentity<IdentityUser>()
                .AddInMemoryIdentityResources(Configuration.GetIdentityResources())
                .AddInMemoryApiResources(Configuration.GetApisResources())
                .AddInMemoryApiScopes(Configuration.GetApiScopes())
                .AddInMemoryClients(Configuration.GetClients())
                .AddDeveloperSigningCredential();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }*/

            app.UseExceptionHandler(new ExceptionHandlerOptions{
                ExceptionHandler = (c) => {

                    var exception = c.Features.Get<IExceptionHandlerFeature>();

                    _logger.LogError(exception.Error.Message);

                    var statusCode = exception.Error.GetType().Name switch {
                        "ArgumentException" => HttpStatusCode.BadRequest,
                        _ => HttpStatusCode.ServiceUnavailable 
                    };

                    c.Response.StatusCode = (int)statusCode;

                    var content = Encoding.UTF8.GetBytes(exception.Error.Message);
                    
                    c.Response.Body.WriteAsync(content, 0, content.Length);

                    return Task.CompletedTask;
                }
            });
            
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Identity Server");
                });
            });
        }
    }
}
