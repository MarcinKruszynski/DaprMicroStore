using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using products.Model;

namespace products.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;        

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;            
        }

        [HttpGet]
        [Route("items")]
        public IEnumerable<ProductItem> Items()
        {
            return new List<ProductItem>()
            {
                new ProductItem { Name = "Zaraza", Price = 25m, StockQuantity = 10000 },
                new ProductItem { Name = "Melassa", Price = 20m, StockQuantity = 3000 }
            };
        }
    }
}
