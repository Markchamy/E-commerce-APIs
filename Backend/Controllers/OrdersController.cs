using Backend.Data;
using Backend.Hubs;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersServices _ordersRepository;
        private readonly IUserServices _userRepository;
        private readonly IInventoryReservationService _inventoryReservationService;
        private readonly IHubContext<OrderHub> _orderHubContext;
        private readonly MyDbContext _context;

        public OrdersController(
            IOrdersServices ordersRepository,
            IUserServices userRepository,
            IInventoryReservationService inventoryReservationService,
            IHubContext<OrderHub> orderHubContext,
            MyDbContext context)
        {
            _ordersRepository = ordersRepository;
            _userRepository = userRepository;
            _inventoryReservationService = inventoryReservationService;
            _orderHubContext = orderHubContext;
            _context = context;
        }

        [HttpPost("create/orders")]
        public async Task<IActionResult> CreateOrder([FromBody] OrdersDTO ordersDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var nextOrderId = await _ordersRepository.GetNextOrderIdAsync();
                var nextOrderName = await _ordersRepository.GetNextOrderNameAsync();

                var orders = new OrdersModel
                {
                    // top-level
                    orderid = nextOrderId,
                    app_id = ordersDTO.app_id,
                    browser_ip = ordersDTO.browser_ip,
                    buyer_accepts_marketing = ordersDTO.buyer_accepts_marketing,
                    cancel_reason = ordersDTO.cancel_reason,
                    cancelled_at = ordersDTO.cancelled_at,
                    cart_token = ordersDTO.cart_token,
                    checkout_id = ordersDTO.checkout_id,
                    checkout_token = ordersDTO.checkout_token,
                    closed_at = ordersDTO.closed_at,
                    confirmation_number = ordersDTO.confirmation_number,
                    confirmed = ordersDTO.confirmed,
                    contact_email = ordersDTO.contact_email,
                    created_at = ordersDTO.created_at,
                    currency = ordersDTO.currency,
                    current_subtotal_price = ordersDTO.current_subtotal_price,
                    current_total_additional_fees_set = ordersDTO.current_total_additional_fees_set,
                    current_total_discounts = ordersDTO.current_total_discounts,
                    current_total_duties_set = ordersDTO.current_total_duties_set,
                    current_total_price = ordersDTO.current_total_price,
                    current_total_tax = ordersDTO.current_total_tax,
                    customer_local = ordersDTO.customer_local,
                    device_id = ordersDTO.device_id,
                    email = ordersDTO.email,
                    estimated_taxes = ordersDTO.estimated_taxes,
                    financial_status = ordersDTO.financial_status,
                    fulfillment_status = ordersDTO.fulfillment_status,
                    landing_site = ordersDTO.landing_site,
                    landing_site_ref = ordersDTO.landing_site_ref,
                    location_id = ordersDTO.location_id,
                    merchant_of_record_app_id = ordersDTO.merchant_of_record_app_id,

                    // If payload doesn't have a name, set a safe default.
                    name = !string.IsNullOrWhiteSpace(ordersDTO.name) ? ordersDTO.name : nextOrderName,

                    note = ordersDTO.note,
                    number = ordersDTO.number,
                    order_number = ordersDTO.order_number,
                    order_status = ordersDTO.order_status,
                    original_total_additional_fees_set = ordersDTO.original_total_additional_fees_set,
                    original_total_duties_set = ordersDTO.original_total_duties_set,
                    payment_gatewaynames = ordersDTO.payment_gatewaynames ?? new List<string>(),
                    phone = ordersDTO.phone,
                    po_number = ordersDTO.po_number,
                    presentment_currency = ordersDTO.presentment_currency,
                    processed_at = ordersDTO.processed_at,
                    reference = ordersDTO.reference,
                    referring_site = ordersDTO.referring_site,
                    source_identifier = ordersDTO.source_identifier,
                    source_name = ordersDTO.source_name,
                    source_url = ordersDTO.source_url,
                    subtotal_price = ordersDTO.subtotal_price,
                    tags = ordersDTO.tags,
                    tax_exempt = ordersDTO.tax_exempt,
                    taxes_included = ordersDTO.taxes_included,
                    test = ordersDTO.test,
                    token = ordersDTO.token,
                    total_discounts = ordersDTO.total_discounts,
                    total_line_items_price = ordersDTO.total_line_items_price,
                    total_outstanding = ordersDTO.total_outstanding,
                    total_price = ordersDTO.total_price,
                    total_taxe = ordersDTO.total_taxe,
                    total_tip_received = ordersDTO.total_tip_received,
                    total_weight = ordersDTO.total_weight,
                    updated_at = ordersDTO.updated_at,
                    user_id = ordersDTO.user_id,
                    payment_terms = ordersDTO.payment_terms,

                    //collections(each child gets orderid; id is NOT set so DB can auto - generate)
                    client_details = (ordersDTO.client_details ?? new List<ClientDetailsDTO>())
                        .Select(cd => new ClientDetailsModel
                        {
                            orderid = nextOrderId,
                            accept_language = cd.accept_language,
                            browser_height = cd.browser_height,
                            browser_width = cd.browser_width,
                            browser_ip = cd.browser_ip,
                            session_hash = cd.session_hash,
                            user_agent = cd.user_agent
                        })
                        .ToList(),

                    subtotal_price_set = (ordersDTO.currentSubtotalPrice ?? new List<CurrentSubTotalPriceDTO>())
                        .Select(sp => new SubTotalPriceModel
                        {
                            orderid = nextOrderId,
                            shop_amount = sp.shop_amount,
                            shop_currency_code = sp.shop_currency_code,
                            presentment_amount = sp.presentment_amount,
                            presentment_currency = sp.presentment_currency
                        })
                        .ToList(),

                    TotalDiscount = (ordersDTO.totalDiscount ?? new List<TotalDiscountDTO>())
                        .Select(d => new TotalDiscountModel
                         {
                             orderid = nextOrderId,
                             shop_amount = d.shop_amount,
                             shop_currency_code = d.shop_currency_code,
                             presentment_amount = d.presentment_amount,
                             presentment_currency = d.presentment_currency
                         })
                        .ToList(),

                    CurrentTotalPrice = (ordersDTO.currentTotalPrice ?? new List<CurrentTotalPriceDTO>())
                        .Select(tp => new CurrentTotalPriceModel
                        {
                            orderid = nextOrderId,
                            shop_amount = tp.shop_amount,
                            shop_currency_code = tp.shop_currency_code,
                            presentment_amount = tp.presentment_amount,
                            presentment_currency = tp.presentment_currency
                        })
                        .ToList(),

                    TotalTax = (ordersDTO.totalTax ?? new List<TotalTaxDTO>())
                        .Select(t => new TotalTaxModel
                        {
                            orderid = nextOrderId,
                            shop_amount = t.shop_amount,
                            shop_currency_code = t.shop_currency_code,
                            presentment_amount = t.presentment_amount,
                            presentment_currency = t.presentment_currency
                        })
                        .ToList(),

                    discount_code = (ordersDTO.discount_code ?? new List<DiscountCodeDTO>())
                        .Select(dc => new DiscountCodeModel
                        {
                            orderid = nextOrderId,
                            code = dc.code,
                            amount = dc.amount,
                            type = dc.type
                        })
                        .ToList(),

                    note_attributes = (ordersDTO.note_attributes ?? new List<NoteAttributesDTO>())
                        .Select(n => new NoteAttributesModel
                        {
                            orderid = nextOrderId,
                            name = n.name,
                            value = n.value
                        })
                        .ToList(),

                    taxLines = (ordersDTO.TaxLines ?? new List<TaxLinesDTO>())
                        .Select(tl => new TaxLinesModel
                        {
                            orderid = nextOrderId,
                            price = tl.price,
                            rate = tl.rate,
                            title = tl.title,
                            channel_liable = tl.channel_liable
                        })
                        .ToList(),

                    LineModels = (ordersDTO.totalLine ?? new List<TotalLineDTO>())
                        .Select(l => new TotalLineModel
                        {
                            orderid = nextOrderId,
                            presentment_amount = l.presentment_amount,
                            presentment_currency = l.presentment_currency,
                            shop_amount = l.shop_amount,
                            shop_currency_code = l.shop_currency_code
                        })
                        .ToList(),

                    //// LineItems: we DO set Shopify lineItemId (it's the PK in your model)
                    LineItems = (ordersDTO.LineItems ?? new List<LineItemsDTO>())
                        .Select(li => new LineItemsModel
                        {
                            lineItemId = li.lineItemId,           // keep Shopify id
                            orderid = nextOrderId,
                            fulfillable_quantity = li.fulfillable_quantity,
                            fulfillment_service = li.fulfillment_service,
                            fulfillment_status = li.fulfillment_status,
                            gift_card = li.gift_card,
                            grams = li.grams,
                            name = li.name,
                            price = li.price,
                            product_exists = li.product_exists,
                            product_id = li.product_id,
                            variant_id = li.variant_id,
                            quantity = li.quantity,
                            requires_shipping = li.requires_shipping,
                            sku = li.sku,
                            taxable = li.taxable,
                            title = li.title,
                            total_discount = li.total_discount,
                            variant_inventory_management = li.variant_inventory_management,
                            variant_title = li.variant_title,
                            vendor = li.vendor
                        })
                        .ToList(),

                    priceSet = (ordersDTO.priceSet ?? new List<PriceSetDTO>())
                        .Select(ps => new PriceSetModel
                        {
                            orderid = nextOrderId,
                            shop_amount = ps.shop_amount,
                            shop_currency_code = ps.shop_currency_code,
                            presentment_amount = ps.presentment_amount,
                            presentment_currency = ps.presentment_currency
                        })
                        .ToList(),

                    totalShipping = (ordersDTO.totalShipping ?? new List<TotalShippingDTO>())
                        .Select(ts => new TotalShippingModel
                        {
                            orderid = nextOrderId,
                            shop_amount = ts.shop_amount,
                            shop_currency_code = ts.shop_currency_code,
                            presentment_amount = ts.presentment_amount,
                            presentment_currency = ts.presentment_currency
                        })
                        .ToList(),

                    billing_address = (ordersDTO.billing_address ?? new List<BillingAddressDTO>())
                        .Select(b => new BillingAddressModel
                        {
                            orderid = nextOrderId,
                            first_name = b.first_name,
                            address1 = b.address1,
                            phone = b.phone,
                            city = b.city,
                            zip = b.zip,
                            province = b.province,
                            country = b.country,
                            last_name = b.last_name,
                            address2 = b.address2,
                            company = b.company,
                            latitude = b.latitude,
                            longitude = b.longitude,
                            name = b.name,
                            country_code = b.country_code,
                            province_code = b.province_code
                        })
                        .ToList(),

                    discount_applications = (ordersDTO.discount_applications ?? new List<DiscountApplicationsDTO>())
                        .Select(d => new DiscountApplicationsModel
                        {
                            orderid = nextOrderId,
                            target_type = d.target_type,
                            type = d.type,
                            value = d.value,
                            value_type = d.value_type,
                            allocation_method = d.allocation_method,
                            target_selection = d.target_selection,
                            code = d.code
                        })
                        .ToList(),

                    //// Fulfillments: copy Shopify id iff provided; otherwise let DB auto-generate
                    fulfillment = (ordersDTO.fulfillment ?? new List<FulfillmentDTO>())
                        .Select(fdto =>
                        {
                            var f = new FulfillmentsModel
                            {
                                orderid = nextOrderId,
                                location_id = fdto.location_id,
                                name = fdto.name,
                                service = fdto.service,
                                shipment_status = fdto.shipment_status,
                                status = fdto.status,
                                tracking_company = fdto.tracking_company,
                                tracking_number = fdto.tracking_number,
                                tracking_url = fdto.tracking_url,
                                created_at = fdto.created_at,
                                updated_at = fdto.updated_at
                            };
                            if (fdto.id.HasValue) f.id = fdto.id.Value; // take external ID when present
                            return f;
                        })
                        .ToList(),

                    ShippingAddress = (ordersDTO.ShippingAddress ?? new List<ShippingAddressDTO>())
                        .Select(s => new ShippingAddressModel
                        {
                            orderid = nextOrderId,
                            first_name = s.first_name,
                            address1 = s.address1,
                            city = s.city,
                            zip = s.zip,
                            province = s.province,
                            country = s.country,
                            last_name = s.last_name,
                            address2 = s.address2,
                            company = s.company,
                            latitude = s.latitude,
                            longitude = s.longitude,
                            name = s.name,
                            country_code = s.country_code,
                            province_code = s.province_code
                        })
                        .ToList(),

                    ShippingLines = (ordersDTO.ShippingLines ?? new List<ShippingLineDTO>())
                        .Select(sl => new ShippingLineModel
                        {
                            orderid = nextOrderId,
                            title = sl.title,
                            price = sl.price
                        })
                        .ToList()
                };

                await _ordersRepository.CreateOrder(orders);
                await _ordersRepository.SaveChangesAsync();

                await _orderHubContext.Clients.All.SendAsync("OrderCreated", new
                {
                    orderId = orders.orderid,
                    orderName = orders.name,
                    createdAt = orders.created_at
                });

                // Update customer's order count, last order info, and total spent
                if (orders.user_id.HasValue)
                {
                    Console.WriteLine($"📝 Order created with user_id: {orders.user_id.Value}");
                    var user = await _userRepository.GetUserByIdAsync(orders.user_id.Value);

                    if (user == null)
                    {
                        Console.WriteLine($"⚠️ Warning: User with ID {orders.user_id.Value} not found");
                    }
                    else if (user.Customer == null)
                    {
                        Console.WriteLine($"⚠️ Warning: User {orders.user_id.Value} exists but has no Customer record");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Updating customer data for user {orders.user_id.Value}");
                        user.Customer.OrdersCount++;
                        user.Customer.LastOrderId = (long)nextOrderId;
                        user.Customer.LastOrderName = orders.name;
                        user.Customer.TotalSpent += orders.total_price;
                        user.Customer.UpdatedAt = DateTime.Now;

                        await _userRepository.SaveChangesAsync();
                        Console.WriteLine($"✅ Customer data updated: OrdersCount={user.Customer.OrdersCount}, TotalSpent={user.Customer.TotalSpent}");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ Warning: Order created without user_id (guest order?)");
                }

                // Reserve inventory for the order
                var reservationResult = await _inventoryReservationService.ReserveInventoryForOrderAsync(orders.orderid);

                if (!reservationResult.Success)
                {
                    // Inventory reservation failed - log warning but order is already created
                    Console.WriteLine($"⚠️ Warning: Order {orders.orderid} created but inventory reservation failed: {reservationResult.Message}");
                    // You might want to add a flag to the order for manual review
                }

                return Ok(new ResponseBase(true, "orders created successfully.", orders));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                return StatusCode(500, ResponseBase.Failure(
                    $"An internal error occurred while creating the order: {ex.Message}; Inner exception: {innerMessage}"
                ));
            }
        }

        [HttpPost("close/order/{orderid}")]
        public async Task<IActionResult> CloseOrders(int orderid)
        {
            try
            {
                var order = await _ordersRepository.GetOrderById(orderid);
                if (order == null)
                {
                    return NotFound($"order with ID {orderid} is not found");
                }

                order.order_status = "Closed";
                order.closed_at = DateTime.Now;

                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, $"Order {orderid} closed successfully.", order));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while closing the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPost("cancel/order")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderDTO cancelOrderDto)
        {
            try
            {
                var order = await _ordersRepository.GetOrderById(cancelOrderDto.OrderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {cancelOrderDto.OrderId} is not found");
                }

                order.order_status = "Cancelled";
                order.cancelled_at = DateTime.Now;
                order.cancel_reason = cancelOrderDto.CancelReason;

                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                // Update customer's order count, last order info, and total spent
                if (order.user_id.HasValue)
                {
                    var user = await _userRepository.GetUserByIdAsync(order.user_id.Value);
                    if (user?.Customer != null)
                    {
                        // Decrement orders count (only if greater than 0)
                        if (user.Customer.OrdersCount > 0)
                        {
                            user.Customer.OrdersCount--;
                        }

                        // Subtract order total from customer's total spent
                        user.Customer.TotalSpent -= order.total_price;

                        // Ensure TotalSpent doesn't go negative
                        if (user.Customer.TotalSpent < 0)
                        {
                            user.Customer.TotalSpent = 0;
                        }

                        // Get previous order (most recent non-cancelled order)
                        var customerOrders = await _ordersRepository.GetOrdersByCustomer(order.user_id.Value, 10);
                        var previousOrder = customerOrders
                            .Where(o => o.orderid != order.orderid && o.order_status != "Cancelled")
                            .OrderByDescending(o => o.created_at)
                            .FirstOrDefault();

                        if (previousOrder != null)
                        {
                            user.Customer.LastOrderId = (long)previousOrder.orderid;
                            user.Customer.LastOrderName = previousOrder.name;
                        }
                        else
                        {
                            // No previous orders, reset to default
                            user.Customer.LastOrderId = 0;
                            user.Customer.LastOrderName = "";
                        }

                        user.Customer.UpdatedAt = DateTime.Now;
                        await _userRepository.SaveChangesAsync();
                    }
                }

                // Release inventory reservation
                var releaseResult = await _inventoryReservationService.CancelOrderInventoryReservationAsync(order.orderid);

                if (!releaseResult.Success)
                {
                    // Log warning but don't fail the cancellation
                    Console.WriteLine($"⚠️ Warning: Order {order.orderid} cancelled but inventory release failed: {releaseResult.Message}");
                }

                return Ok(new ResponseBase(true, $"Order {cancelOrderDto.OrderId} cancelled successfully.", order));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while cancelling the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }


        [HttpPost("open/orders/{orderid}")]
        public async Task<IActionResult> OpenOrders(long orderid)
        {
            try
            {
                var order = await _ordersRepository.GetOrderById(orderid);
                if (order == null)
                {
                    return NotFound($"order with ID {orderid} is not found.");
                }

                order.order_status = "Opened";
                order.closed_at = null;

                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, $"Order {orderid} opened successfully.", order));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An Internal error occured while openning the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("update-order-financial-status")]
        public async Task<IActionResult> UpdateOrderFinancialStatus([FromBody] financialDTO financial)
        {
            try
            {
                // Validate the input
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if the order exists
                var order = await _ordersRepository.GetOrderById(financial.orderid);
                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                // Update the financial status
                order.financial_status = financial.financial_status ?? "paid";
                order.updated_at = DateTime.Now;

                // Save changes to the database
                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Order financial status updated to 'paid'."));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An error occurred while updating the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("update-orders-fulfillment")]
        public async Task<IActionResult> UpdateOrderFulfillment([FromBody] fulfillmentDTO fulfillment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var order = await _ordersRepository.GetOrderById(fulfillment.orderid);
                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                order.fulfillment_status = fulfillment.fulfillment_status ?? "Fulfilled";
                order.updated_at = DateTime.Now;

                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                // Release inventory reservation when order is fulfilled
                var releaseResult = await _inventoryReservationService.ReleaseReservedInventoryForOrderAsync(order.orderid);

                if (!releaseResult.Success)
                {
                    // Log warning but don't fail the fulfillment update
                    Console.WriteLine($"⚠️ Warning: Order {order.orderid} fulfilled but inventory release failed: {releaseResult.Message}");
                }

                return Ok(new ResponseBase(true, "Order fulfillment status updated as fulfilled."));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An error occurred while updating the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("update-fulfillment-shipment-status")]
        public async Task<IActionResult> UpdateFulfillmentShipmentStatus([FromBody] ShipmentStatusDTO shipmentStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var order = await _ordersRepository.GetOrderById(shipmentStatus.orderid);
                if (order == null)
                    return NotFound("Order not found.");

                var fulfillment = _context.fulfillments.FirstOrDefault(f => f.orderid == shipmentStatus.orderid);

                if (fulfillment != null)
                {
                    // Update existing fulfillment record
                    fulfillment.shipment_status = shipmentStatus.shipment_status;
                    fulfillment.updated_at = DateTime.Now;
                }
                else
                {
                    // Create a new fulfillment record for this order (FleetRunnr delivery)
                    _context.fulfillments.Add(new FulfillmentsModel
                    {
                        orderid = shipmentStatus.orderid,
                        shipment_status = shipmentStatus.shipment_status,
                        tracking_company = "FleetRunnr",
                        service = "fleetrunnr",
                        status = "success",
                        name = $"#{order.name}.1",
                        location_id = 0,
                        tracking_number = "",
                        tracking_url = "",
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Shipment status updated."));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An error occurred: {ex.Message}; Inner: {innerMessage}"));
            }
        }

        [HttpPut("bulk-update-orders-fulfillment")]
        public async Task<IActionResult> BulkUpdateOrdersFulfillment([FromBody] BulkFulfillmentDTO bulkFulfillment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (bulkFulfillment.orderids == null || !bulkFulfillment.orderids.Any())
                {
                    return BadRequest("Order IDs list cannot be null or empty.");
                }

                var successfulUpdates = new List<long>();
                var failedUpdates = new List<object>();

                foreach (var orderId in bulkFulfillment.orderids)
                {
                    try
                    {
                        var order = await _ordersRepository.GetOrderById(orderId);
                        if (order == null)
                        {
                            failedUpdates.Add(new { OrderId = orderId, Reason = "Order not found" });
                            continue;
                        }

                        order.fulfillment_status = !string.IsNullOrEmpty(bulkFulfillment.fulfillment_status) 
                            ? bulkFulfillment.fulfillment_status 
                            : "Fulfilled";
                        order.updated_at = DateTime.Now;

                        await _ordersRepository.UpdateOrder(order);
                        successfulUpdates.Add(orderId);
                    }
                    catch (Exception orderEx)
                    {
                        failedUpdates.Add(new { OrderId = orderId, Reason = orderEx.Message });
                    }
                }

                await _ordersRepository.SaveChangesAsync();

                // Release inventory for fulfilled orders
                if (!string.IsNullOrEmpty(bulkFulfillment.fulfillment_status) &&
                    (bulkFulfillment.fulfillment_status.Equals("Fulfilled", StringComparison.OrdinalIgnoreCase) ||
                     bulkFulfillment.fulfillment_status.Equals("out_for_delivery", StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var orderId in successfulUpdates)
                    {
                        var releaseResult = await _inventoryReservationService.ReleaseReservedInventoryForOrderAsync(orderId);

                        if (!releaseResult.Success)
                        {
                            Console.WriteLine($"⚠️ Warning: Order {orderId} fulfilled but inventory release failed: {releaseResult.Message}");
                        }
                    }
                }

                var result = new
                {
                    TotalOrders = bulkFulfillment.orderids.Count,
                    SuccessfulUpdates = successfulUpdates.Count,
                    FailedUpdates = failedUpdates.Count,
                    UpdatedOrderIds = successfulUpdates,
                    FailedOrdersDetails = failedUpdates
                };

                string message = failedUpdates.Any() 
                    ? $"Bulk update completed with {successfulUpdates.Count} successes and {failedUpdates.Count} failures."
                    : $"All {successfulUpdates.Count} orders updated successfully.";

                return Ok(new ResponseBase(true, message, result));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An error occurred during bulk update: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("update-order-tags")]
        public async Task<IActionResult> UpdateOrderTags([FromBody] orderTagDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var order = await _ordersRepository.GetOrderById(dto.orderid);
                if (order == null)
                    return NotFound($"Order with Id {dto.orderid} not found.");

                // Capture old tags before overwriting
                var oldTags = (order.tags ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => t.ToLowerInvariant())
                    .ToHashSet();

                // Normalize: trim, dedupe, keep comma-separated
                var newTagsList = (dto.tags ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                order.tags = string.Join(", ", newTagsList);
                order.updated_at = DateTime.UtcNow;

                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                // Broadcast if "do" tag was newly added
                var doNewlyAdded = newTagsList.Any(t => t.Equals("do", StringComparison.OrdinalIgnoreCase))
                    && !oldTags.Contains("do");
                if (doNewlyAdded)
                {
                    await _orderHubContext.Clients.All.SendAsync("OrderTagsUpdated", new
                    {
                        orderId = order.orderid,
                        tags = order.tags,
                        addedTag = "do"
                    });
                }

                return Ok(new ResponseBase(true, $"Order {dto.orderid} tags updated.", new
                {
                    orderid = order.orderid,
                    tags = order.tags
                }));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                return StatusCode(500, ResponseBase.Failure(
                    $"An error occurred while updating order tags: {ex.Message}; Inner exception: {inner}"
                ));
            }
        }


        [HttpGet("export/orders")]
        public async Task<IActionResult> ExportOrders()
        {
            try
            {
                var orders = await _ordersRepository.ExportOrders();

                var csvBuilder = new StringBuilder();
                var headers = "Name, Email, Financial Status, Paid At, Fulfillment Status, Fulfilled at, Accepts Marketing, Currency, Subtotal, Shipping, Taxes, Total, Discount Code, Discount Amount," +
                    "Shipping Method, Created at, Lineitem quantity, lineitem name, lineitem price, lineitem sku, lineitem requires shipping, lineitem taxable, lineitem fulfillment status," +
                    "Billing Name, Billing Address1, Billing Address2, Billing Company, Billing City, Billing Zip, Billing Province, Billing Country, Billing Phone, Shipping Name," +
                    "Shipping Address1, Shipping Address2, Shipping Company, Shipping City, Shipping Zip, Shipping Province, Shipping Country, Notes, Note Attributes, Cancelled at," +
                    "Payment Method, Payment Terms, Vendor, Outstanding, Id, Tags, Source, LineItem Discount, Tax 1 Name, Tax 1 Value, Phone";

                csvBuilder.AppendLine(headers);

                foreach (var order in orders)
                {
                    foreach (var fulfillment in order.fulfillment)
                    {
                        foreach (var shipping in order.ShippingAddress)
                        {
                            foreach (var lineItem in order.LineItems)
                            {
                                foreach (var billing in order.billing_address)
                                {
                                    foreach (var tax in order.taxLines)
                                    {
                                            var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29}," +
                                                "{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53}",
                                                   order.name,
                                                   order.email,
                                                   order.financial_status,
                                                   order.closed_at,
                                                   order.fulfillment_status,
                                                   fulfillment.created_at,
                                                   order.buyer_accepts_marketing,
                                                   order.currency,
                                                   order.subtotal_price,
                                                   order.totalShipping,
                                                   order.total_taxe,
                                                   order.total_price,
                                                   order.discount_code,
                                                   order.total_discounts,
                                                   shipping.city,
                                                   order.created_at,
                                                   lineItem.current_quantity,
                                                   lineItem.name,
                                                   lineItem.price,
                                                   lineItem.sku,
                                                   lineItem.requires_shipping,
                                                   lineItem.taxable,
                                                   lineItem.fulfillment_status,
                                                   billing.name,
                                                   billing.address1,
                                                   billing.address2,
                                                   billing.company,
                                                   billing.city,
                                                   billing.zip,
                                                   billing.province,
                                                   billing.country,
                                                   billing.phone,
                                                   shipping.name,
                                                   shipping.address1,
                                                   shipping.address2,
                                                   shipping.company,
                                                   shipping.city,
                                                   shipping.zip,
                                                   shipping.province,
                                                   shipping.country,
                                                   order.note,
                                                   order.note_attributes,
                                                   order.cancelled_at,
                                                   order.payment_gatewaynames,
                                                   order.payment_terms,
                                                   // refund amount
                                                   lineItem.vendor,
                                                   order.total_outstanding,
                                                   order.orderid,
                                                   order.tags,
                                                   order.source_name,
                                                   lineItem.total_discount,
                                                   tax.title,
                                                   tax.price,
                                                   order.phone
                                                   );
                                            csvBuilder.AppendLine(newLine);
                                        
                                    }
                                }
                            }
                        }
                    }
                }

                var byteArray = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var stream = new MemoryStream(byteArray);

                return File(stream, "text/csv", "orders_export");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("Failed to export orders: " + ex.Message));
            }
        }


        [HttpGet("orders/cancel")]
        public async Task<IActionResult> GetCancelledOrders()
        {
            try
            {
                var cancelledOrders = await _ordersRepository.GetAllCancelledOrders();
                if (cancelledOrders == null || cancelledOrders.Count == 0)
                {
                    return NotFound("No cancelled orders found.");
                }

                return Ok(cancelledOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving cancelled orders: {ex.Message}");
            }
        }

        [HttpGet("orders/count")]
        public async Task<IActionResult> GetOrdersCount()
        {
            try
            {
                var count = await _ordersRepository.GetOrdersCount();
                return Ok(new ResponseBase(true, "Count retrieved successfully.", count));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An internal error occurred while fetching the orders count: " + ex.Message));
            }
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderById(long orderId)
        {
            try
            {
                var order = await _ordersRepository.GetOrderByIdWithDetails(orderId);
                if (order == null)
                {
                    return NotFound(new ResponseBase(false, $"Order with ID {orderId} not found.", null));
                }

                return Ok(new ResponseBase(true, "Order retrieved successfully.", order));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while fetching the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("order/latest")]
        public async Task<IActionResult> GetLatestOrder()
        {
            try
            {
                var order = await _ordersRepository.GetLatestOrderAsync();
                if (order == null)
                {
                    return NotFound(new ResponseBase(false, "No orders found.", null));
                }

                return Ok(new ResponseBase(true, "Latest order retrieved successfully.", order));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while fetching the latest order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("order/next-id")]
        public async Task<IActionResult> GetNextOrderId()
        {
            try
            {
                var nextId = await _ordersRepository.GetNextOrderIdAsync();
                return Ok(new ResponseBase(true, "Next order ID retrieved successfully.", new { nextOrderId = nextId }));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while getting next order ID: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("order/next-name")]
        public async Task<IActionResult> GetNextOrderName()
        {
            try
            {
                var nextName = await _ordersRepository.GetNextOrderNameAsync();
                return Ok(new ResponseBase(true, "Next order name retrieved successfully.", new { nextOrderName = nextName }));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while getting next order name: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders(
            int page = 1,                   // Pagination: Current page
            int pageSize = 50,              // Pagination: Number of orders per page
            string sortBy = "Date",         // Default sort column
            string sortDirection = "desc",  // Default sort direction
            string filter = "All",          // Filtering criteria
            string search = ""              // Search term
        )
        {
            try
            {
                // Validate pagination parameters
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new ResponseBase(false, "Page and pageSize must be greater than 0"));
                }

                // Fetch orders with sorting, filtering, and searching
                var orders = await _ordersRepository.GetAllOrders(page, pageSize, sortBy, sortDirection, filter, search);

                // Return orders or an empty list if none found
                return Ok(new ResponseBase(true, "Orders retrieved successfully", orders ?? new List<OrdersModel>()));
            }
            catch (Exception ex)
            {
                // Return 500 in case of an internal error
                return StatusCode(500, new ResponseBase(false, $"An internal error occurred: {ex.Message}"));
            }
        }

        [HttpGet("pmi/orders")]
        public async Task<IActionResult> GetPMIOrders(
            int page = 1,
            int pageSize = 50,
            string sortBy = "Date",
            string sortDirection = "desc",
            string filter = "All",
            string search = ""
        )
        {
            try
            {
                // Validate pagination parameters
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new ResponseBase(false, "Page and pageSize must be greater than 0"));
                }

                // Fetch PMI orders with sorting, filtering, and searching
                var orders = await _ordersRepository.GetPMIOrders(page, pageSize, sortBy, sortDirection, filter, search);

                // Return orders or an empty list if none found (matching the /orders endpoint response format)
                return Ok(new ResponseBase(true, "PMI orders retrieved successfully", orders ?? new List<OrdersModel>()));
            }
            catch (Exception ex)
            {
                // Return 500 in case of an internal error
                return StatusCode(500, new ResponseBase(false, $"An internal error occurred: {ex.Message}"));
            }
        }

        [HttpGet("orders/customer/{userId}")]
        public async Task<IActionResult> GetOrdersByCustomer(long userId, int limit = 50)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("User ID must be a positive number.");
                }

                var customerOrders = await _ordersRepository.GetOrdersByCustomer(userId, limit);

                if (customerOrders == null || customerOrders.Count == 0)
                {
                    return NotFound($"No orders found for user with ID: {userId}");
                }

                return Ok(customerOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving orders for user {userId}: {ex.Message}");
            }
        }


        [HttpPut("orders/{orderId}")]
        public async Task<IActionResult> UpdateOrder(long orderId, [FromBody] OrdersDTO orders)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingOrder = await _ordersRepository.GetOrderByIdWithDetails(orderId);
                if (existingOrder == null)
                {
                    return NotFound($"Order with Id {orderId} not found.");
                }

                existingOrder.app_id = orders.app_id;
                existingOrder.browser_ip = orders.browser_ip;
                existingOrder.buyer_accepts_marketing = orders.buyer_accepts_marketing;
                existingOrder.cancel_reason = orders.cancel_reason;
                existingOrder.cart_token = orders.cart_token;
                existingOrder.checkout_id = orders.checkout_id;

                existingOrder.updated_at = DateTime.Now;

                    foreach (var client in orders.client_details)
                    {
                        var existingClient = existingOrder.client_details.FirstOrDefault(clients => orders.orderid == orders.orderid);
                        if (existingClient != null)
                        {
                            existingClient.accept_language = client.accept_language;
                            existingClient.browser_height = client.browser_height;
                            existingClient.browser_ip = client.browser_ip;
                            existingClient.browser_width = client.browser_width;
                            existingClient.session_hash = client.session_hash;
                            existingClient.user_agent = client.user_agent;
                        }
                    }
                

                existingOrder.confirmation_number = orders.confirmation_number;
                existingOrder.confirmed = orders.confirmed;
                existingOrder.contact_email = orders.contact_email;
                existingOrder.currency = orders.currency;
                existingOrder.current_subtotal_price = orders.current_subtotal_price;


                    foreach (var price in orders.currentSubtotalPrice)
                    {
                        var existingPrice = existingOrder.subtotal_price_set.FirstOrDefault(subtotal => orders.orderid == orders.orderid);
                        if (existingPrice != null)
                        {
                            existingPrice.shop_amount = price.shop_amount;
                            existingPrice.shop_currency_code = price.shop_currency_code;
                            existingPrice.presentment_amount = price.presentment_amount;
                            existingPrice.presentment_currency = price.presentment_currency;
                        }
                    }
                

                existingOrder.current_total_additional_fees_set = orders.current_total_additional_fees_set;
                existingOrder.current_total_discounts = orders.current_total_discounts;



                foreach (var discount in orders.totalDiscount)
                {
                    var existingDiscount = existingOrder.TotalDiscount.FirstOrDefault(discounts => orders.orderid == orders.orderid);
                    if (existingDiscount != null)
                    {
                        existingDiscount.shop_amount = discount.shop_amount;
                        existingDiscount.shop_currency_code = discount.shop_currency_code;
                        existingDiscount.presentment_amount = discount.presentment_amount;
                        existingDiscount.presentment_currency = discount.presentment_currency;
                    }
                }


                existingOrder.current_total_duties_set = orders.current_total_duties_set;
                existingOrder.current_total_price = orders.current_total_price;


                foreach (var totalPrice in orders.currentTotalPrice)
                {
                    var existingCurrentPrice = existingOrder.CurrentTotalPrice.FirstOrDefault(totalPrices => orders.orderid == orders.orderid);
                    if (existingCurrentPrice != null)
                    {
                        existingCurrentPrice.shop_amount = totalPrice.shop_amount;
                        existingCurrentPrice.shop_currency_code = totalPrice.shop_currency_code;
                        existingCurrentPrice.presentment_amount = totalPrice.presentment_amount;
                        existingCurrentPrice.presentment_currency = totalPrice.presentment_currency;
                    }
                }


                existingOrder.current_total_tax = orders.current_total_tax;


                foreach (var tax in orders.totalTax)
                {
                    var existingTax = existingOrder.TotalTax.FirstOrDefault(tax => orders.orderid == orders.orderid);
                    if (existingTax != null)
                    {
                        existingTax.shop_amount = tax.shop_amount;
                        existingTax.shop_currency_code = tax.shop_currency_code;
                        existingTax.presentment_amount = tax.presentment_amount;
                        existingTax.presentment_currency = tax.presentment_currency;
                    }
                }


                existingOrder.customer_local = orders.customer_local;
                existingOrder.device_id = orders.device_id;


                foreach (var discountCode in orders.discount_code)
                {
                    var existingDiscountCode = existingOrder.discount_code.FirstOrDefault(discountCode => orders.orderid == orders.orderid);
                    if (existingDiscountCode != null)
                    {
                        existingDiscountCode.code = discountCode.code;
                        existingDiscountCode.amount = discountCode.amount;
                        existingDiscountCode.type = discountCode.type;
                    }
                }


                existingOrder.email = orders.email;
                existingOrder.estimated_taxes = orders.estimated_taxes;
                existingOrder.financial_status = orders.financial_status;
                existingOrder.fulfillment_status = orders.fulfillment_status;
                existingOrder.landing_site = orders.landing_site;
                existingOrder.landing_site_ref = orders.landing_site_ref;
                existingOrder.location_id = orders.location_id;
                existingOrder.merchant_of_record_app_id = orders.merchant_of_record_app_id;
                existingOrder.name = orders.name;
                existingOrder.note = orders.name;


                foreach (var note in orders.note_attributes)
                {
                    var existingNote = existingOrder.note_attributes.FirstOrDefault(note => orders.orderid == orders.orderid);
                    if (existingNote != null)
                    {
                        existingNote.name = note.name;
                        existingNote.value = note.value;
                    }
                }


                existingOrder.number = orders.number;
                existingOrder.order_number = orders.order_number;
                existingOrder.order_status = orders.order_status;
                existingOrder.original_total_additional_fees_set = orders.original_total_additional_fees_set;
                existingOrder.original_total_duties_set = orders.original_total_duties_set;
                existingOrder.payment_gatewaynames = orders.payment_gatewaynames;
                existingOrder.phone = orders.phone;
                existingOrder.po_number = orders.po_number;
                existingOrder.presentment_currency = orders.presentment_currency;
                existingOrder.reference = orders.reference;
                existingOrder.referring_site = orders.referring_site;
                existingOrder.source_identifier = orders.source_identifier;
                existingOrder.source_name = orders.source_name;
                existingOrder.source_url = orders.source_url;
                existingOrder.subtotal_price = orders.subtotal_price;
                existingOrder.tags = orders.tags;


                foreach (var taxLines in orders.TaxLines)
                {
                    var existingTaxLines = existingOrder.taxLines.FirstOrDefault(taxLines => orders.orderid == orders.orderid);
                    if (existingTaxLines != null)
                    {
                        existingTaxLines.price = taxLines.price;
                        existingTaxLines.rate = taxLines.rate;
                        existingTaxLines.title = taxLines.title;
                        existingTaxLines.channel_liable = taxLines.channel_liable;
                    }
                }


                existingOrder.taxes_included = orders.taxes_included;
                existingOrder.test = orders.test;
                existingOrder.token = orders.token;
                existingOrder.total_discounts = orders.total_discounts;
                existingOrder.total_line_items_price = orders.total_line_items_price;


                foreach (var lineModel in orders.totalLine)
                {
                    var existingLineModel = existingOrder.LineModels.FirstOrDefault(lineModel => orders.orderid == orders.orderid);
                    if (existingLineModel != null)
                    {
                        existingLineModel.shop_amount = lineModel.shop_amount;
                        existingLineModel.shop_currency_code = lineModel.shop_currency_code;
                        existingLineModel.presentment_amount = lineModel.presentment_amount;
                        existingLineModel.presentment_currency = lineModel.presentment_currency;
                    }
                }


                foreach (var lineItems in orders.LineItems)
                {
                    var existingLineItems = existingOrder.LineItems.FirstOrDefault(lineItems => orders.orderid == orders.orderid);
                    if (existingLineItems != null)
                    {
                        existingLineItems.fulfillable_quantity = lineItems.fulfillable_quantity;
                        existingLineItems.fulfillment_service = lineItems.fulfillment_service;
                        existingLineItems.fulfillment_status = lineItems.fulfillment_status;
                        existingLineItems.gift_card = lineItems.gift_card;
                        existingLineItems.grams = lineItems.grams;
                        existingLineItems.name = lineItems.name;
                        existingLineItems.price = lineItems.price;
                        existingLineItems.product_exists = lineItems.product_exists;
                        existingLineItems.product_id = lineItems.product_id;
                        existingLineItems.quantity = lineItems.quantity;
                        existingLineItems.requires_shipping = lineItems.requires_shipping;
                        existingLineItems.sku = lineItems.sku;
                        existingLineItems.taxable = lineItems.taxable;
                        existingLineItems.title = lineItems.title;
                        existingLineItems.total_discount = lineItems.total_discount;
                        existingLineItems.variant_inventory_management = lineItems.variant_inventory_management;
                        existingLineItems.variant_title = lineItems.variant_title;
                        existingLineItems.vendor = lineItems.vendor;
                    }
                }


                existingOrder.total_outstanding = orders.total_outstanding;
                existingOrder.total_price = orders.total_price;


                foreach (var priceSet in orders.priceSet)
                {
                    var existingPriceSet = existingOrder.priceSet.FirstOrDefault(priceSet => orders.orderid == orders.orderid);
                    if (existingPriceSet != null)
                    {
                        existingPriceSet.shop_amount = priceSet.shop_amount;
                        existingPriceSet.shop_currency_code = priceSet.shop_currency_code;
                        existingPriceSet.presentment_amount = priceSet.presentment_amount;
                        existingPriceSet.presentment_currency = priceSet.presentment_currency;
                    }
                }

                foreach (var shipping in orders.totalShipping)
                {
                    var existingShipping = existingOrder.totalShipping.FirstOrDefault(shipping => orders.orderid == orders.orderid);
                    if (existingShipping != null)
                    {
                        existingShipping.shop_amount = shipping.shop_amount;
                        existingShipping.shop_currency_code = shipping.shop_currency_code;
                        existingShipping.presentment_amount = shipping.presentment_amount;
                        existingShipping.presentment_currency = shipping.presentment_currency;
                    }
                }

                existingOrder.total_taxe = orders.total_taxe;
                existingOrder.total_tip_received = orders.total_tip_received;
                existingOrder.total_weight = orders.total_weight;
                existingOrder.user_id = orders.user_id;

                foreach (var billing in orders.billing_address)
                {
                    var existingBilling = existingOrder.billing_address.FirstOrDefault(billing => orders.orderid == orders.orderid);
                    if (existingBilling != null)
                    {
                        existingBilling.first_name = billing.first_name;
                        existingBilling.address1 = billing.address1;
                        existingBilling.phone = billing.phone;
                        existingBilling.city = billing.city;
                        existingBilling.zip = billing.zip;
                        existingBilling.province = billing.province;
                        existingBilling.country = billing.country;
                        existingBilling.last_name = billing.last_name;
                        existingBilling.address2 = billing.address2;
                        existingBilling.company = billing.company;
                        existingBilling.latitude = billing.latitude;
                        existingBilling.longitude = billing.longitude;
                        existingBilling.name = billing.name;
                        existingBilling.country_code = billing.country_code;
                        existingBilling.province_code = billing.province_code;
                    }
                }

                foreach (var discountApp in orders.discount_applications)
                {
                    var existingDiscountApp = existingOrder.discount_applications.FirstOrDefault(discountApp => orders.orderid == orders.orderid);
                    if (existingDiscountApp != null)
                    {
                        existingDiscountApp.target_type = discountApp.target_type;
                        existingDiscountApp.type = discountApp.type;
                        existingDiscountApp.value = discountApp.value;
                        existingDiscountApp.value_type = discountApp.value_type;
                        existingDiscountApp.allocation_method = discountApp.allocation_method;
                        existingDiscountApp.target_selection = discountApp.target_selection;
                        existingDiscountApp.code = discountApp.code;
                    }
                }

                foreach (var fulfillment in orders.fulfillment)
                {
                    var existingFulfillment = existingOrder.fulfillment.FirstOrDefault(fulfillment => orders.orderid == orders.orderid);
                    if (existingFulfillment != null)
                    {
                        existingFulfillment.location_id = fulfillment.location_id;
                        existingFulfillment.name = fulfillment.name;
                        existingFulfillment.service = fulfillment.service;
                        existingFulfillment.shipment_status = fulfillment.shipment_status;
                        existingFulfillment.status = fulfillment.status;
                        existingFulfillment.tracking_company = fulfillment.tracking_company;
                        existingFulfillment.tracking_number = fulfillment.tracking_number;
                        existingFulfillment.tracking_url = fulfillment.tracking_url;

                    }
                }

                existingOrder.payment_terms = orders.payment_terms;

                foreach (var shippingAddress in orders.ShippingAddress)
                {
                    var existingShippingAddress = existingOrder.ShippingAddress.FirstOrDefault(shippingAddress => orders.orderid == orders.orderid);
                    if (existingShippingAddress != null)
                    {
                        existingShippingAddress.first_name = shippingAddress.first_name;
                        existingShippingAddress.address1 = shippingAddress.address1;
                        existingShippingAddress.city = shippingAddress.city;
                        existingShippingAddress.zip = shippingAddress.zip;
                        existingShippingAddress.province = shippingAddress.province;
                        existingShippingAddress.country = shippingAddress.country;
                        existingShippingAddress.last_name = shippingAddress.last_name;
                        existingShippingAddress.address2 = shippingAddress.address2;
                        existingShippingAddress.company = shippingAddress.company;
                        existingShippingAddress.latitude = shippingAddress.latitude;
                        existingShippingAddress.longitude = shippingAddress.longitude;
                        existingShippingAddress.name = shippingAddress.name;
                        existingShippingAddress.country_code = shippingAddress.country_code;
                        existingShippingAddress.province_code = shippingAddress.province_code;
                    }
                }

                await _ordersRepository.UpdateOrder(existingOrder);

                return Ok(new ResponseBase(true, $"Order {orderId} updated successfully.", existingOrder));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while updating the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("update-product-fulfilled")]
        public async Task<IActionResult> UpdateProductFulfilled([FromBody] UpdateProductFulfilledDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var order = await _ordersRepository.GetOrderByIdWithDetails(request.OrderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {request.OrderId} not found.");
                }

                if (order.LineItems == null || !order.LineItems.Any())
                {
                    return NotFound($"No line items found for order {request.OrderId}.");
                }

                var updatedCount = 0;
                foreach (var lineItem in order.LineItems)
                {
                    if (string.IsNullOrEmpty(lineItem.product_fulfilled))
                    {
                        lineItem.product_fulfilled = "Fulfilled";
                        updatedCount++;
                    }
                }

                if (updatedCount == 0)
                {
                    return Ok(new ResponseBase(true, "No line items needed updating - all already have product_fulfilled values.", new { OrderId = request.OrderId, UpdatedCount = 0 }));
                }

                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, $"Successfully updated {updatedCount} line items to 'Fulfilled' status.", new { OrderId = request.OrderId, UpdatedCount = updatedCount }));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while updating product fulfilled status: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("update-specific-product-fulfilled")]
        public async Task<IActionResult> UpdateSpecificProductFulfilled([FromBody] UpdateSpecificProductFulfilledDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.LineItemIds == null || !request.LineItemIds.Any())
                {
                    return BadRequest("LineItemIds cannot be null or empty.");
                }

                var order = await _ordersRepository.GetOrderByIdWithDetails(request.OrderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {request.OrderId} not found.");
                }

                if (order.LineItems == null || !order.LineItems.Any())
                {
                    return NotFound($"No line items found for order {request.OrderId}.");
                }

                var updatedLineItems = new List<object>();
                var notFoundLineItems = new List<long>();
                var updatedCount = 0;

                foreach (var lineItemId in request.LineItemIds)
                {
                    var lineItem = order.LineItems.FirstOrDefault(li => li.lineItemId == lineItemId);
                    if (lineItem != null)
                    {
                        var previousStatus = lineItem.product_fulfilled;
                        lineItem.product_fulfilled = request.FulfillmentStatus;
                        updatedCount++;

                        updatedLineItems.Add(new
                        {
                            LineItemId = lineItemId,
                            ProductName = lineItem.name,
                            PreviousStatus = previousStatus,
                            NewStatus = request.FulfillmentStatus
                        });
                    }
                    else
                    {
                        notFoundLineItems.Add(lineItemId);
                    }
                }

                if (updatedCount == 0)
                {
                    return NotFound(new ResponseBase(false, "No matching line items found to update.", new
                    {
                        OrderId = request.OrderId,
                        RequestedLineItemIds = request.LineItemIds,
                        NotFoundLineItems = notFoundLineItems
                    }));
                }

                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                var result = new
                {
                    OrderId = request.OrderId,
                    UpdatedCount = updatedCount,
                    UpdatedLineItems = updatedLineItems,
                    NotFoundLineItems = notFoundLineItems.Any() ? notFoundLineItems : null
                };

                var message = notFoundLineItems.Any()
                    ? $"Successfully updated {updatedCount} line items. {notFoundLineItems.Count} line items were not found."
                    : $"Successfully updated all {updatedCount} requested line items to '{request.FulfillmentStatus}' status.";

                return Ok(new ResponseBase(true, message, result));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while updating specific product fulfilled status: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpDelete("delete/order/{orderId}")]
        public async Task<IActionResult> DeleteOrder(long orderId)
        {
            try
            {

                var order = await _ordersRepository.GetOrderByIdWithDetails(orderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {orderId} not found.");
                }

                await _ordersRepository.DeleteOrder(order);
                await _ordersRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Order deleted successfully.", null));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while deleting the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPatch("update-line-item-fulfillment")]
        public async Task<IActionResult> UpdateLineItemFulfillment([FromBody] UpdateLineItemFulfillmentDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.LineItemIds == null || !request.LineItemIds.Any())
                {
                    return BadRequest("LineItemIds cannot be null or empty.");
                }

                var order = await _ordersRepository.GetOrderByIdWithDetails(request.OrderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {request.OrderId} not found.");
                }

                if (order.LineItems == null || !order.LineItems.Any())
                {
                    return NotFound($"No line items found for order {request.OrderId}.");
                }

                var updatedLineItems = new List<object>();
                var notFoundLineItems = new List<long>();

                foreach (var lineItemId in request.LineItemIds)
                {
                    var lineItem = order.LineItems.FirstOrDefault(li => li.lineItemId == lineItemId);
                    if (lineItem != null)
                    {
                        if (lineItem.orderid != request.OrderId)
                        {
                            return BadRequest($"Line item {lineItemId} does not belong to order {request.OrderId}.");
                        }

                        var previousStatus = lineItem.fulfillment_status;
                        lineItem.fulfillment_status = request.FulfillmentStatus;

                        updatedLineItems.Add(new
                        {
                            LineItemId = lineItemId,
                            ProductName = lineItem.name,
                            PreviousStatus = previousStatus,
                            NewStatus = request.FulfillmentStatus
                        });
                    }
                    else
                    {
                        notFoundLineItems.Add(lineItemId);
                    }
                }

                if (updatedLineItems.Count == 0)
                {
                    return NotFound(new ResponseBase(false, "No matching line items found to update.", new
                    {
                        OrderId = request.OrderId,
                        RequestedLineItemIds = request.LineItemIds,
                        NotFoundLineItems = notFoundLineItems
                    }));
                }

                // Check if ALL line items are now fulfilled
                bool allLineItemsFulfilled = order.LineItems.All(li =>
                    li.fulfillment_status != null &&
                    li.fulfillment_status.Equals("Fulfilled", StringComparison.OrdinalIgnoreCase));

                string previousOrderStatus = order.fulfillment_status;
                if (allLineItemsFulfilled)
                {
                    order.fulfillment_status = "Fulfilled";
                }

                order.updated_at = DateTime.Now;

                await _ordersRepository.UpdateOrder(order);
                await _ordersRepository.SaveChangesAsync();

                var result = new
                {
                    OrderId = request.OrderId,
                    OrderFulfillmentStatus = order.fulfillment_status,
                    PreviousOrderFulfillmentStatus = previousOrderStatus,
                    AllLineItemsFulfilled = allLineItemsFulfilled,
                    UpdatedLineItemsCount = updatedLineItems.Count,
                    UpdatedLineItems = updatedLineItems,
                    NotFoundLineItems = notFoundLineItems.Any() ? notFoundLineItems : null,
                    AllLineItems = order.LineItems.Select(li => new
                    {
                        LineItemId = li.lineItemId,
                        ProductName = li.name,
                        FulfillmentStatus = li.fulfillment_status,
                        Quantity = li.quantity
                    }).ToList(),
                    UpdatedAt = order.updated_at
                };

                var message = allLineItemsFulfilled
                    ? $"Successfully updated {updatedLineItems.Count} line items. All line items are now fulfilled - Order fulfillment status automatically set to 'Fulfilled'."
                    : notFoundLineItems.Any()
                        ? $"Successfully updated {updatedLineItems.Count} line items. {notFoundLineItems.Count} line items were not found. Order has partially fulfilled items."
                        : $"Successfully updated {updatedLineItems.Count} line items to '{request.FulfillmentStatus}' status. Order has partially fulfilled items.";

                return Ok(new ResponseBase(true, message, result));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while updating line item fulfillment: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

    }
}
