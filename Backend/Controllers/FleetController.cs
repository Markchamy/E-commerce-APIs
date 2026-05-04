using Backend.Hubs;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]

    public class FleetController : ControllerBase
    {
        private readonly FleetRunnrService _fleetRunnrService;
        private readonly IHubContext<OrderHub> _orderHubContext;

        public FleetController(FleetRunnrService fleetRunnrService, IHubContext<OrderHub> orderHubContext)
        {
            _fleetRunnrService = fleetRunnrService;
            _orderHubContext = orderHubContext;
        }

        [HttpGet("fleet/carriers")]
        public async Task<IActionResult> GetCarriers()
        {
            var result = await _fleetRunnrService.GetCarriersAsync();

            return Ok(result);
        }

        [HttpGet("fleet/customers")]
        public async Task<IActionResult> GetCustomer([FromQuery] string phone, [FromQuery] string email, [FromQuery] bool locations)
        {
            // Ensure at least one of the parameters (phone or email) is provided
            if (string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(email))
            {
                return BadRequest("Either phone or email must be provided.");
            }

            var result = await _fleetRunnrService.GetCustomerAsync(phone, email, locations);
            return Ok(result);
        }

        [HttpPost("fleet/customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] FleetCustomer customer)
        {
            if (string.IsNullOrEmpty(customer.email) && string.IsNullOrEmpty(customer.phone))
            {
                return BadRequest("Either phone or email must be provided.");
            }

            var result = await _fleetRunnrService.CreateCustomerAsync(customer);
            return Ok(result);
        }

        [HttpPut("fleet/customers/{customer_uuid}")]
        public async Task<IActionResult> UpdateCustomer(Guid customer_uuid, [FromBody] FleetCustomer customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer data is required.");
            }

            var result = await _fleetRunnrService.UpdateCustomerAsync(customer_uuid, customer);
            if (result == null)
            {
                return NotFound("Customer not found.");
            }

            return Ok(new { message = "success" });
        }

        [HttpGet("fleet/locations")]
        public async Task<IActionResult> GetLocations([FromQuery] string? externalId, [FromQuery] string? uuid)
        {
            var result = await _fleetRunnrService.GetLocationsAsync(externalId, uuid);
            return Ok(result);
        }

        [HttpPost("fleet/locations")]
        public async Task<IActionResult> CreateLocation([FromBody] FleetLocation location)
        {
            var result = await _fleetRunnrService.CreateLocationAsync(location);
            return Ok(result);
        }

        [HttpGet("fleet/orders")]
        public async Task<IActionResult> GetOrders([FromQuery] string orderNumber)
        {
            var result = await _fleetRunnrService.GetOrdersAsync(orderNumber);
            return Ok(result);
        }

        [HttpPost("fleet/orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest createOrderRequest)
        {
            try
            {
                var result = await _fleetRunnrService.CreateOrderAsync(createOrderRequest);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
        }

        [HttpPatch("fleet/orders/{order_uuid}")]
        public async Task<IActionResult> UpdateOrders(Guid order_uuid, [FromBody] FleetOrderUpdate update)
        {
            if (order_uuid == null)
            {
                return BadRequest("Order data is required.");
            }

            var result = await _fleetRunnrService.UpdateOrderAsync(order_uuid, update);
            if(result == null)
            {
                return NotFound("Order not found.");
            }

            return Ok(new { message = "success" });
        }

        [HttpPost("fleet/orders/{order_uuid}/assign")]
        public async Task<IActionResult> AssignCarrier(Guid order_uuid, [FromBody] AssignCarrierRequest carrier)
        {
            if (order_uuid == null)
            {
                return BadRequest("Order uuid is required.");
            }

            var result = await _fleetRunnrService.AssignCarrierAsync(order_uuid, carrier);
            if(result == null)
            {
                return NotFound("Order Not Found.");
            }

            return Ok(new {message = "success"});
        }

        [HttpPost("fleet/orders/{order_uuid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid order_uuid)
        {
            var result = await _fleetRunnrService.CancelOrderAsync(order_uuid);
            return Ok(result);
        }

        [HttpPost("fleet/orders/generate-labels")]
        public async Task<IActionResult> GenerateLabels([FromBody] GenerateLabelsRequest labelsRequest)
        {
            var result = await _fleetRunnrService.GenerateLabelsAsync(labelsRequest);
            return Ok(new {message = "success"});
        }

        [HttpPost("fleet/webhooks/subscribe")]
        public async Task<IActionResult> WebhooksSubscribe([FromBody] WebhookSubscribe subscribe)
        {
            var result = await _fleetRunnrService.WebhookSubscribeAsync(subscribe);
            return Ok(new { message = "success" });
        }

        [HttpPost("fleet/webhooks/unsubscribe")]
        public async Task<IActionResult> WebhooksUnsubscribe([FromBody] WebhookUnsubscribe unsubscribe)
        {
            var result = await _fleetRunnrService.WebhookUnsubscribeAsync(unsubscribe);
            return Ok(new { message = "success" });
        }

        [HttpPost("fleet/invoices/{invoice_number}/pay")]
        public async Task<IActionResult> PayInvoice(Guid invoice_number)
        {
            var result = await _fleetRunnrService.PayInvoiceAsync(invoice_number);
            return Ok(new { message = "success" });
        }

        [HttpPost("fleet/invoices/{invoice_number}/void")]
        public async Task<IActionResult> VoidInvoice(Guid invoice_number)
        {
            var result = await _fleetRunnrService.VoidInvoiceAsync(invoice_number);
            return Ok(new { message = "success" });
        }

        /// <summary>
        /// Inbound webhook receiver — FleetRunnr calls this URL when order/shipment events occur.
        /// Broadcasts the appropriate SignalR event to connected frontend clients.
        /// The frontend then updates the DB and refreshes the order details UI.
        /// Register this URL with FleetRunnr via POST /fleet/webhooks/subscribe.
        /// </summary>
        [HttpPost("fleet/webhooks/receive")]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveWebhook([FromBody] FleetRunnrWebhookPayload payload)
        {
            // Log raw payload to console so we can inspect what FleetRunnr actually sends
            Console.WriteLine($"[FleetRunnr Webhook] topic={payload?.topic} external_id={payload?.external_id} raw={System.Text.Json.JsonSerializer.Serialize(payload)}");

            if (payload == null || string.IsNullOrEmpty(payload.topic) || string.IsNullOrEmpty(payload.external_id))
                return BadRequest(new { message = "Invalid webhook payload." });

            switch (payload.topic)
            {
                case "shipment.assigned":
                    await _orderHubContext.Clients.All.SendAsync("ShipmentAssigned", new
                    {
                        orderId = payload.external_id,
                        driverName = payload.driver_name,
                        newShipmentStatus = payload.fulfillment_status ?? "ready_for_pickup"
                    });
                    break;

                case "shipment.attempted":
                    await _orderHubContext.Clients.All.SendAsync("ShipmentAttempted", new
                    {
                        orderId = payload.external_id
                    });
                    break;

                case "shipment.failed":
                    await _orderHubContext.Clients.All.SendAsync("ShipmentFailed", new
                    {
                        orderId = payload.external_id,
                        failureReason = payload.failure_reason,
                        newShipmentStatus = payload.fulfillment_status ?? "failed"
                    });
                    break;

                case "order.updated":
                case "order.cancelled":
                    await _orderHubContext.Clients.All.SendAsync("OrderStatusUpdated", new
                    {
                        orderId = payload.external_id,
                        newShipmentStatus = payload.fulfillment_status
                    });
                    break;

                case "accounting.merchant_invoice.paid":
                    await _orderHubContext.Clients.All.SendAsync("OrderPaid", new
                    {
                        orderId = payload.external_id,
                        amountPaid = payload.amount,
                        currency = payload.currency
                    });
                    break;

                case "items.picked":
                    await _orderHubContext.Clients.All.SendAsync("FulfillmentProgress", new
                    {
                        orderId = payload.external_id,
                        stage = "picked"
                    });
                    break;

                case "items.packed":
                    await _orderHubContext.Clients.All.SendAsync("FulfillmentProgress", new
                    {
                        orderId = payload.external_id,
                        stage = "packed"
                    });
                    break;

                default:
                    // Unknown topic — acknowledge receipt without broadcasting
                    break;
            }

            return Ok(new { message = "success" });
        }
    }
}
