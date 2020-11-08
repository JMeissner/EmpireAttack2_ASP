using System;
using EmpireAttack2_ASP.Game;
using EmpireAttack2_ASP.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmpireAttack2_ASP
{
    public class Startup
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddWebSocketManager();
            services.AddRazorPages();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseWebSockets();

            app.UseHttpsRedirection();
            app.UseFileServer();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<GameHub>("/game");
            });

            // Use this if you want App_Data off your project root folder
            string baseDir = env.ContentRootPath;

            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(baseDir, "App_Data"));

            ServiceProvider = serviceProvider;

            GameHub.Current = app.ApplicationServices.GetService<IHubContext<GameHub>>();

            //Initialize the Game (No of Players hardcoded as 2 for testing)
            GameManager.Instance.Initilize(2);
            System.Diagnostics.Debug.WriteLine("Initialization Completed.");
        }
    }
}
