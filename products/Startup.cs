using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using products.Model;
using Dapr.Client;

namespace products
{
    public class Startup
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDaprClient(client =>
            {
                client.UseJsonSerializationOptions(options);
            });                   
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }            

            app.UseRouting();            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("list", Items); 
                endpoints.MapPost("checkBooking", CheckBooking).WithTopic("bookingToCheck");               
            });

            async Task Items(HttpContext context)
            {
                var items = new ProductItem[]        
                {
                    new ProductItem { Name = "Zaraza", Price = 25m, StockQuantity = 10000 },
                    new ProductItem { Name = "Melassa", Price = 20m, StockQuantity = 3000 }
                };        

                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(context.Response.Body, items, options);
            } 

            async Task CheckBooking(HttpContext context)
            {
                Console.WriteLine("Check booking");
                
                var client = context.RequestServices.GetRequiredService<DaprClient>();
                
                var message = await JsonSerializer.DeserializeAsync<BookingToCheck>(context.Request.Body, options);

                Console.WriteLine("BookingId={0}, ProductId={1}, Quantity={2}", message.BookingId, message.ProductId, message.Quantity); 

                //to do
                bool check = true;

                if (check)
                {
                    await client.PublishEventAsync("bookingStockConfirmed", new BookingStockConfirmation
                    {
                        BookingId = message.BookingId                    
                    });

                    Console.WriteLine("Sent confirmation for booking {0}", message.BookingId); 
                }
                else
                {
                    await client.PublishEventAsync("bookingStockRejected", new BookingStockRejection
                    {
                        BookingId = message.BookingId                    
                    });

                    Console.WriteLine("Sent rejection for booking {0}", message.BookingId); 
                }
                                           
            }
        }               
    }
}
