namespace ShopVRG.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using ShopVRG.Api.Models;
using ShopVRG.Domain.Models.Commands;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Repositories;
using ShopVRG.Domain.Workflows;
using ShopVRG.Events;

/// <summary>
/// Controller for order management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IEventSender _eventSender;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IEventSender eventSender,
        ILogger<OrdersController> logger)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _eventSender = eventSender;
        _logger = logger;
    }

    /// <summary>
    /// Place a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        try
        {
            // Create command from request
            var command = new PlaceOrderCommand
            {
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                ShippingStreet = request.ShippingStreet,
                ShippingCity = request.ShippingCity,
                ShippingPostalCode = request.ShippingPostalCode,
                ShippingCountry = request.ShippingCountry,
                OrderLines = request.OrderLines.Select(l => new OrderLineCommand
                {
                    ProductCode = l.ProductCode,
                    Quantity = l.Quantity.ToString()
                }).ToList()
            };

            // Execute workflow
            var workflow = new PlaceOrderWorkflow();
            var orderEvent = workflow.Execute(
                command,
                checkProductExists: code => _productRepository.ExistsAsync(code).GetAwaiter().GetResult(),
                getProductDetails: code =>
                {
                    var product = _productRepository.GetByCodeAsync(code).GetAwaiter().GetResult();
                    if (product == null) return null;
                    return (product.Name, product.Price, product.Stock);
                },
                reserveStock: (code, qty) => _productRepository.ReserveStockAsync(code, qty).GetAwaiter().GetResult(),
                persistOrder: (orderId, order) => _orderRepository.SaveOrderAsync(orderId, order).GetAwaiter().GetResult()
            );

            // Handle result
            if (orderEvent is OrderPlacedEvent placed)
            {
                _logger.LogInformation("Order {OrderId} placed successfully", placed.OrderId);

                // Send async event
                await _eventSender.SendAsync(EventTopics.OrderPlaced, new
                {
                    OrderId = placed.OrderId.ToString(),
                    CustomerEmail = placed.CustomerEmail.Value,
                    TotalPrice = placed.TotalPrice.Value,
                    PlacedAt = placed.PlacedAt
                });

                var dto = new OrderDto
                {
                    OrderId = placed.OrderId.ToString(),
                    CustomerName = placed.CustomerName.Value,
                    CustomerEmail = placed.CustomerEmail.Value,
                    ShippingAddress = new AddressDto
                    {
                        Street = placed.ShippingAddress.Street,
                        City = placed.ShippingAddress.City,
                        PostalCode = placed.ShippingAddress.PostalCode,
                        Country = placed.ShippingAddress.Country
                    },
                    OrderLines = placed.OrderLines.Select(l => new Models.OrderLineDto
                    {
                        ProductCode = l.ProductCode,
                        ProductName = l.ProductName,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        LineTotal = l.LineTotal
                    }).ToList(),
                    TotalPrice = placed.TotalPrice.Value,
                    Status = "Placed",
                    CreatedAt = placed.PlacedAt
                };

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = dto,
                    Message = $"Order {placed.OrderId} placed successfully"
                });
            }
            else if (orderEvent is OrderPlacementFailedEvent failed)
            {
                _logger.LogWarning("Order placement failed: {Reasons}", string.Join(", ", failed.Reasons));

                // Send async event
                await _eventSender.SendAsync(EventTopics.OrderFailed, new
                {
                    Reasons = failed.Reasons,
                    FailedAt = DateTime.UtcNow
                });

                return BadRequest(new ApiResponse<OrderDto>
                {
                    Success = false,
                    Errors = failed.Reasons.ToList(),
                    Message = "Order placement failed"
                });
            }

            return StatusCode(500, new ApiResponse<OrderDto>
            {
                Success = false,
                Errors = ["Unknown workflow result"]
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing order");
            return StatusCode(500, new ApiResponse<OrderDto>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }

    /// <summary>
    /// Check if order exists
    /// </summary>
    [HttpGet("{orderId}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckOrderExists(string orderId)
    {
        try
        {
            if (!OrderId.TryCreate(orderId, out var orderIdObj, out var error))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Errors = [error ?? "Invalid order ID"]
                });
            }

            var exists = await _orderRepository.ExistsAsync(orderIdObj!);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = exists
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking order {OrderId}", orderId);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }
}
