using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Sany3y.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks; // Explicitly using System.Threading.Tasks

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public PaymentController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("create-checkout-session")]
        public async System.Threading.Tasks.Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest req)
        {
            var domain = "https://localhost:7004"; // Frontend URL

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(req.Price * 100), // Amount in cents
                            Currency = "egp",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = req.ServiceName,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + "/Payment/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + $"/Payment/Cancel?taskId={req.TaskId}",
                Metadata = new Dictionary<string, string>
                {
                    { "TaskId", req.TaskId.ToString() },
                    { "CustomerId", req.CustomerId.ToString() }
                }
            };

            var service = new SessionService();
            try
            {
                Session session = await service.CreateAsync(options);
                return Ok(new { url = session.Url });
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        [HttpPost("webhook")]
        public async System.Threading.Tasks.Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"]
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    await FulfillOrder(session);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }

        private async System.Threading.Tasks.Task FulfillOrder(Session session)
        {
            if (session.Metadata.TryGetValue("TaskId", out var taskIdStr) && int.TryParse(taskIdStr, out var taskId))
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task != null)
                {
                    // Update task status or create payment record
                    var payment = new Payment
                    {
                        AmountAgreed = (decimal)session.AmountTotal / 100,
                        AmountPaid = (decimal)session.AmountTotal / 100,
                        PaymentDate = DateTime.Now,
                        PaymentStatus = "Completed",
                        PaymentMethodId = 2, // Assuming 2 is Online/Stripe
                        TaskId = taskId,
                        ClientId = task.ClientId
                    };

                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }

    public class CreateCheckoutSessionRequest
    {
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public int TaskId { get; set; }
        public long CustomerId { get; set; }
    }
}
