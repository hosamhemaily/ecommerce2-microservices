using Application.Commands;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;

        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(decimal amount)
        {
            try
            {
                var id = await _mediator.Send(new CreateOrderCommand(amount));
                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "Internal server error");
            }
        }
        
    }
}
