using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using bookings.Model;
using Dapr;
using Dapr.Client;
using System.Threading;

namespace bookings.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingsController : ControllerBase
    {        
        [HttpPost("checkout")]
        public async Task Checkout(BookingCheckout bookingCheckout, [FromServices] DaprClient dapr, [FromServices] ILogger<BookingsController> logger)
        {            
            var bookingId = Guid.NewGuid().ToString();

            logger.LogInformation("Booking {BookingId} created for request {RequestId} with product {ProductId}", bookingId, bookingCheckout.RequestId, bookingCheckout.ProductId);

            var eventMessage = new BookingToCheck
            {
                BookingId = bookingId,
                ProductId = bookingCheckout.ProductId,
                Quantity = bookingCheckout.Quantity
            };

            await dapr.PublishEventAsync("bookingToCheck", eventMessage); 

            logger.LogInformation("Sent bookingToCheck message for booking {BookingId}", bookingId);          
        }

        [Topic("bookingStockConfirmed")]
        [HttpPost("bookingStockConfirmed")]
        public async Task BookingStockConfirmed(BookingStockConfirmation confirmation, [FromServices] DaprClient dapr, [FromServices] ILogger<BookingsController> logger)
        {
            logger.LogInformation("Booking {BookingId} stock confirmed", confirmation.BookingId);

            var notification = new NotificationData { BookingId = confirmation.BookingId };
            await dapr.InvokeBindingAsync("notifications-topic", "create", notification, null, default(CancellationToken));            

            logger.LogInformation($"Booking {confirmation.BookingId} confirmation notification was sent.");
        }

        [Topic("bookingStockRejected")]
        [HttpPost("bookingStockRejected")]
        public async Task BookingStockRejected(BookingStockRejection rejection, [FromServices] ILogger<BookingsController> logger)
        {
            logger.LogInformation("Booking {BookingId} stock rejected", rejection.BookingId);
        }
    }
}
