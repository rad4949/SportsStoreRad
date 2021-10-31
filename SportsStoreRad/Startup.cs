using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SportsStoreRad.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsStoreRad
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
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IProductRepository, EFProductRepository>();
            services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddControllersWithViews();
            services.AddMvc();
            services.AddMemoryCache();
            services.AddSession();
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
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();
            //app.UseMvc();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "pagination",
                   pattern: "{category}/Page{productPage}",
                   defaults: new { Controller = "Product", action = "List" });

                endpoints.MapControllerRoute(
                   name: "null",
                   pattern: "Page{productPage:int}",
                   defaults: new { Controller = "Product", action = "List", productPage = 1 });

                endpoints.MapControllerRoute(
                   name: "null",
                   pattern: "{category}",
                   defaults: new { Controller = "Product", action = "List", productPage = 1 });

                endpoints.MapControllerRoute(
                   name: "null",
                   pattern: "",
                   defaults: new { Controller = "Product", action = "List", productPage = 1 });

                endpoints.MapControllerRoute(
                   name: "null",
                   pattern: "{controller}/{action}/{id}");
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                ApplicationDbContext content = scope.ServiceProvider.GetService<ApplicationDbContext>();
                SeedData.Initial(content);
            }
        }
    }
}