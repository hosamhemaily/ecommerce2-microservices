using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PaymentAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
     

        private readonly ILogger<PaymentController> _logger;
        private readonly IEventBus _eventBus;

        public PaymentController(ILogger<PaymentController> logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        [HttpGet("initiate")]
        public async Task<bool> Initiate([FromQuery] Guid orderId)
        {
            await Task.Delay(1000);
            // Simulate payment result
            return DateTime.Now.Minute % 2 == 0;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallBack([FromQuery] Guid orderId, [FromQuery] bool succeeded)
        {
            if (succeeded)
            {
                await _eventBus.Publish("PaymentSucceeded", new Domain.Events.PaymentSucceededEvent(orderId));
                _logger.LogInformation($"Payment succeeded for order {orderId}");
            }
            else
            {
                await _eventBus.Publish("PaymentFailed", new Domain.Events.PaymentFailedEvent(orderId));
                _logger.LogInformation($"Payment failed for order {orderId}");
            }
            return Ok(new { orderId, succeeded });
        }
    }
}
