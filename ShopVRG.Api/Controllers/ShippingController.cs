namespace ShopVRG.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using ShopVRG.Api.Models;
using ShopVRG.Domain.Models.Commands;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Repositories;
using ShopVRG.Domain.Workflows;
using ShopVRG.Events;

/// <summary>
/// Controller for shipping management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ShippingController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventSender _eventSender;
    private readonly ILogger<ShippingController> _logger;

    public ShippingController(
        IOrderRepository orderRepository,
        IEventSender eventSender,
        ILogger<ShippingController> logger)
    {
        _orderRepository = orderRepository;
        _eventSender = eventSender;
        _logger = logger;
    }

    /// <summary>
    /// Ship an order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> ShipOrder([FromBody] ShipOrderRequest request)
    {
        try
        {
            // Create command from request
            var command = new ShipOrderCommand
            {
                OrderId = request.OrderId,
                Carrier = request.Carrier
            };

            // Execute workflow
            var workflow = new ShipOrderWorkflow();
            var shippingEvent = workflow.Execute(
                command,
                checkOrderExists: orderId => _orderRepository.ExistsAsync(orderId).GetAwaiter().GetResult(),
                checkOrderPaid: orderId => _orderRepository.IsPaidAsync(orderId).GetAwaiter().GetResult(),
                getOrderShippingAddress: orderId => _orderRepository.GetShippingAddressAsync(orderId).GetAwaiter().GetResult(),
                persistShipment: (orderId, trackingNumber, carrier) =>
                    _orderRepository.MarkAsShippedAsync(orderId, trackingNumber, carrier).GetAwaiter().GetResult(),
                getEstimatedDeliveryDays: carrier => GetEstimatedDeliveryDays(carrier)
            );

            // Handle result
            if (shippingEvent is OrderShippedEvent shipped)
            {
                _logger.LogInformation("Order {OrderId} shipped via {Carrier}. Tracking: {TrackingNumber}",
                    shipped.OrderId, shipped.Carrier, shipped.TrackingNumber);

                // Send async event
                await _eventSender.SendAsync(EventTopics.OrderShipped, new
                {
                    OrderId = shipped.OrderId.ToString(),
                    TrackingNumber = shipped.TrackingNumber,
                    Carrier = shipped.Carrier,
                    ShippedAt = shipped.ShippedAt,
                    EstimatedDelivery = shipped.EstimatedDelivery
                });

                var dto = new ShipmentDto
                {
                    OrderId = shipped.OrderId.ToString(),
                    TrackingNumber = shipped.TrackingNumber,
                    Carrier = shipped.Carrier,
                    Destination = new AddressDto
                    {
                        Street = shipped.Destination.Street,
                        City = shipped.Destination.City,
                        PostalCode = shipped.Destination.PostalCode,
                        Country = shipped.Destination.Country
                    },
                    ShippedAt = shipped.ShippedAt,
                    EstimatedDelivery = shipped.EstimatedDelivery
                };

                return Ok(new ApiResponse<ShipmentDto>
                {
                    Success = true,
                    Data = dto,
                    Message = $"Order shipped successfully. Tracking: {shipped.TrackingNumber}"
                });
            }
            else if (shippingEvent is ShippingFailedEvent failed)
            {
                _logger.LogWarning("Shipping failed for order {OrderId}: {Reasons}",
                    failed.OrderId, string.Join(", ", failed.Reasons));

                // Send async event
                await _eventSender.SendAsync(EventTopics.ShippingFailed, new
                {
                    OrderId = failed.OrderId,
                    Reasons = failed.Reasons,
                    FailedAt = DateTime.UtcNow
                });

                return BadRequest(new ApiResponse<ShipmentDto>
                {
                    Success = false,
                    Errors = failed.Reasons.ToList(),
                    Message = "Shipping failed"
                });
            }

            return StatusCode(500, new ApiResponse<ShipmentDto>
            {
                Success = false,
                Errors = ["Unknown workflow result"]
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error shipping order");
            return StatusCode(500, new ApiResponse<ShipmentDto>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }

    /// <summary>
    /// Get list of supported carriers
    /// </summary>
    [HttpGet("carriers")]
    public ActionResult<ApiResponse<IEnumerable<CarrierInfo>>> GetCarriers()
    {
        var carriers = new List<CarrierInfo>
        {
            new() { Code = "DHL", Name = "DHL Express", EstimatedDays = 3 },
            new() { Code = "FEDEX", Name = "FedEx", EstimatedDays = 2 },
            new() { Code = "UPS", Name = "UPS", EstimatedDays = 3 },
            new() { Code = "DPD", Name = "DPD", EstimatedDays = 4 },
            new() { Code = "GLS", Name = "GLS", EstimatedDays = 4 },
            new() { Code = "CARGUS", Name = "Cargus", EstimatedDays = 2 },
            new() { Code = "FAN_COURIER", Name = "Fan Courier", EstimatedDays = 1 },
            new() { Code = "SAMEDAY", Name = "Sameday", EstimatedDays = 1 }
        };

        return Ok(new ApiResponse<IEnumerable<CarrierInfo>>
        {
            Success = true,
            Data = carriers,
            Message = $"Found {carriers.Count} supported carriers"
        });
    }

    private static int GetEstimatedDeliveryDays(string carrier) => carrier.ToUpperInvariant() switch
    {
        "DHL" => 3,
        "FEDEX" => 2,
        "UPS" => 3,
        "DPD" => 4,
        "GLS" => 4,
        "CARGUS" => 2,
        "FAN_COURIER" => 1,
        "SAMEDAY" => 1,
        _ => 5
    };
}

/// <summary>
/// Carrier information
/// </summary>
public class CarrierInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int EstimatedDays { get; set; }
}
