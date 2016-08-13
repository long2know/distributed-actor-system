using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Common.Models;

namespace Worker2
{
    public class Startup
    {
        public IServiceProvider Services { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Add our worker node
            services.AddWorkerNode();

            Services = services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //lifetime.ApplicationStarted.Register(Started);
            //lifetime.ApplicationStopping.Register(Stopping);
            //lifetime.ApplicationStopped.Register(Stopped);
        }

        //public void Started()
        //{
        //    // Tell "cluster" I'm ready.
        //    var workerStatus = Services.GetRequiredService<IWorkerSatus<WorkerStatusMessage>>();

        //    workerStatus.AddCallback("test", action =>
        //    {
        //        workerStatus.Next.Tell(action.Message);
        //    });

        //    var cancellationTokenSource = new CancellationTokenSource();
        //    TaskRepeater.Interval(TimeSpan.FromSeconds(30), () =>
        //    {
        //        try
        //        {
        //            var apiService = Services.GetRequiredService<IApiService>();
        //            apiService.GetFromApi<string>("http://localhost:9000/api/worker/ready/localhost%3A9002");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Couldn't join cluster.  Keep trying?");
        //            Console.WriteLine(ex);
        //        }
        //    }, cancellationTokenSource.Token, true);
        //}

        //public void Stopping()
        //{
        //    var workerStatus = Services.GetRequiredService<IWorkerSatus<WorkerStatusMessage>>();
        //    workerStatus.AddCallback("test", action =>
        //    {
        //        Console.WriteLine(action.TimeStamp);
        //    });
        //    Console.WriteLine();
        //}

        //public void Stopped()
        //{
        //    // Tell "cluster" I'm gone!.
        //    try
        //    {
        //        var workerStatus = Services.GetRequiredService<IWorkerSatus<WorkerStatusMessage>>();
        //        var apiService = Services.GetRequiredService<IApiService>();
        //        apiService.GetFromApi<string>("http://localhost:9000/api/worker/left/localhost%3A9001");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Couldn't join cluster.  Keep trying?");
        //        Console.WriteLine(ex);
        //    }
        //}
    }
}
