using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using products.Model;

namespace products
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
            services.AddSingleton(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            });           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, JsonSerializerOptions serializerOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }            

            app.UseRouting();            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("list", Items);                
            });

            async Task Items(HttpContext context)
            {
                var items = new ProductItem[]        
                {
                    new ProductItem { Name = "Zaraza", Price = 25m, StockQuantity = 10000 },
                    new ProductItem { Name = "Melassa", Price = 20m, StockQuantity = 3000 }
                };        

                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(context.Response.Body, items, serializerOptions);
            }    
        }               
    }
}
