using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using bookings.Model;
using Dapr.Client;

namespace bookings.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingsController : ControllerBase
    {
        [Route("checkout")]
        public async Task Checkout(BookingCheckout bookingCheckout, [FromServices] DaprClient dapr, [FromServices] ILogger<BookingsController> logger)
        {

        }
    }
}
