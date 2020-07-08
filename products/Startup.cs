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
        public const string StoreName = "default";

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

            app.UseCloudEvents();           

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();

                endpoints.MapGet("list", Items); 
                endpoints.MapPost("checkBooking", CheckBooking).WithTopic("bookingToCheck");               
            });

            async Task Items(HttpContext context)
            {
                var client = context.RequestServices.GetRequiredService<DaprClient>(); 

                var item1 = await GetOrCreateProduct(client, 1, new ProductItem { Name = "Zaraza", Price = 25m, StockQuantity = 10000 });
                var item2 = await GetOrCreateProduct(client, 2, new ProductItem { Name = "Melassa", Price = 20m, StockQuantity = 3000 });

                var items = new ProductItem[] { item1, item2 };                      

                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(context.Response.Body, items, options);
            } 

            async Task<ProductItem> GetProduct(DaprClient client, int id)
            {
                return await client.GetStateAsync<ProductItem>(StoreName, id.ToString());
            }

            async Task SaveProduct(DaprClient client, int id, ProductItem item)
            {
                await client.SaveStateAsync(StoreName, id.ToString(), item);

                Console.WriteLine("Saved product {0}", id);
            }

            async Task<ProductItem> GetOrCreateProduct(DaprClient client, int id, ProductItem item)
            {
                var product = await GetProduct(client, id);

                if (product != null)
                    return product;

                item.Id = id;
                await SaveProduct(client, id, item);
                return item;
            }

            async Task CheckBooking(HttpContext context)
            {
                Console.WriteLine("Check booking");
                
                var client = context.RequestServices.GetRequiredService<DaprClient>();
                
                var message = await JsonSerializer.DeserializeAsync<BookingToCheck>(context.Request.Body, options);

                Console.WriteLine("BookingId={0}, ProductId={1}, Quantity={2}", message.BookingId, message.ProductId, message.Quantity); 

                
                var productItem = await GetProduct(client, message.ProductId);


                if (productItem != null)
                {
                    productItem.RemoveStock(message.Quantity);
                    await SaveProduct(client, message.ProductId, productItem);
                    Console.WriteLine("Updated StockQuantity for product {0} to {1}", message.ProductId, productItem.StockQuantity);

                    if (productItem.StockQuantity >= 0)
                    {
                        await client.PublishEventAsync("bookingStockConfirmed", new BookingStockConfirmation
                        {
                            BookingId = message.BookingId                    
                        });

                        Console.WriteLine("Sent confirmation for booking {0}", message.BookingId); 
                        return;
                    }                    
                }

                
                await client.PublishEventAsync("bookingStockRejected", new BookingStockRejection
                {
                    BookingId = message.BookingId                    
                });

                Console.WriteLine("Sent rejection for booking {0}", message.BookingId);                 
                                           
            }
        }               
    }
}
