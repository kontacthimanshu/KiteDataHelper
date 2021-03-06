using KiteDataHelper.Common.Interfaces.Services;
using KiteDataHelper.Models;
using KiteDataHelper.Service;
using KiteDataHelper.Web.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KiteDataHelper.Web
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
            services.AddMemoryCache(delegate (MemoryCacheOptions memoryCacheOptions) {
                memoryCacheOptions.SizeLimit = 1000000;
            });

            services.AddControllersWithViews();
            services.AddResponseCaching();
            services.AddHttpClient<IKiteAuthenticationService, KiteAuthenticationService>();
            services.AddHttpClient<IMarketDataAccessService, MarketDataAccessService>();
            services.AddSingleton<KiteInstruments>();
            services.AddSingleton<FifteenMinuteTimer>();
            services.AddSingleton<TickCache>();
            services.AddSingleton<Ticker>();

            services.AddSignalR(opt =>
            {
                opt.EnableDetailedErrors = true;
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseResponseCaching();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<TickerHub>("/tickers");
            });
        }
    }
}
