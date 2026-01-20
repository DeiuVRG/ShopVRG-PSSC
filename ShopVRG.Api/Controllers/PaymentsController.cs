namespace ShopVRG.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using ShopVRG.Api.Models;
using ShopVRG.Domain.Models.Commands;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Repositories;
using ShopVRG.Domain.Workflows;
using ShopVRG.Events;

/// <summary>
/// Controller for payment processing
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IEventSender _eventSender;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IEventSender eventSender,
        ILogger<PaymentsController> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _eventSender = eventSender;
        _logger = logger;
    }

    /// <summary>
    /// Process a payment for an order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        try
        {
            // Create command from request
            var command = new ProcessPaymentCommand
            {
                OrderId = request.OrderId,
                Amount = request.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CardNumber = request.CardNumber,
                CardHolderName = request.CardHolderName,
                ExpiryDate = request.ExpiryDate,
                Cvv = request.Cvv
            };

            // Execute workflow
            var workflow = new ProcessPaymentWorkflow();
            var paymentEvent = workflow.Execute(
                command,
                checkOrderExists: orderId => _orderRepository.ExistsAsync(orderId).GetAwaiter().GetResult(),
                getOrderTotal: orderId => _orderRepository.GetOrderTotalAsync(orderId).GetAwaiter().GetResult(),
                processPayment: payment => SimulatePaymentGateway(payment),
                persistPayment: (paymentId, orderId, amount, transRef) =>
                {
                    var saved = _paymentRepository.SavePaymentAsync(paymentId, orderId, amount, transRef).GetAwaiter().GetResult();
                    if (saved)
                    {
                        _orderRepository.MarkAsPaidAsync(orderId).GetAwaiter().GetResult();
                    }
                    return saved;
                }
            );

            // Handle result
            if (paymentEvent is PaymentProcessedEvent processed)
            {
                _logger.LogInformation("Payment {PaymentId} processed for order {OrderId}",
                    processed.PaymentId, processed.OrderId);

                // Send async event
                await _eventSender.SendAsync(EventTopics.PaymentProcessed, new
                {
                    PaymentId = processed.PaymentId.ToString(),
                    OrderId = processed.OrderId.ToString(),
                    Amount = processed.Amount.Value,
                    TransactionReference = processed.TransactionReference,
                    ProcessedAt = processed.ProcessedAt
                });

                var dto = new PaymentDto
                {
                    PaymentId = processed.PaymentId.ToString(),
                    OrderId = processed.OrderId.ToString(),
                    Amount = processed.Amount.Value,
                    MaskedCardNumber = processed.MaskedCardNumber,
                    TransactionReference = processed.TransactionReference,
                    ProcessedAt = processed.ProcessedAt
                };

                return Ok(new ApiResponse<PaymentDto>
                {
                    Success = true,
                    Data = dto,
                    Message = $"Payment processed successfully. Transaction: {processed.TransactionReference}"
                });
            }
            else if (paymentEvent is PaymentFailedEvent failed)
            {
                _logger.LogWarning("Payment failed for order {OrderId}: {Reasons}",
                    failed.OrderId, string.Join(", ", failed.Reasons));

                // Send async event
                await _eventSender.SendAsync(EventTopics.PaymentFailed, new
                {
                    OrderId = failed.OrderId,
                    Reasons = failed.Reasons,
                    FailedAt = DateTime.UtcNow
                });

                return BadRequest(new ApiResponse<PaymentDto>
                {
                    Success = false,
                    Errors = failed.Reasons.ToList(),
                    Message = "Payment processing failed"
                });
            }

            return StatusCode(500, new ApiResponse<PaymentDto>
            {
                Success = false,
                Errors = ["Unknown workflow result"]
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(500, new ApiResponse<PaymentDto>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }

    /// <summary>
    /// Simulates payment gateway processing
    /// In production, this would call a real payment provider (Stripe, PayPal, etc.)
    /// </summary>
    private static string? SimulatePaymentGateway(ValidatedPayment payment)
    {
        // Simulate some validation
        // In real scenario, this would call external payment API

        // For demo: cards ending in 0000 are declined
        if (payment.MaskedCardNumber.EndsWith("0000"))
        {
            return null;
        }

        // Generate a transaction reference
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }

    /// <summary>
    /// Confirm a payment through the simulated payment processor
    /// This endpoint is called after user confirms payment in the payment processor modal
    /// </summary>
    [HttpPost("confirm")]
    public async Task<ActionResult<ApiResponse<PaymentConfirmationDto>>> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            // Parse OrderId from string
            if (!OrderId.TryCreate(request.OrderId, out var orderId, out var error) || orderId == null)
            {
                return BadRequest(new ApiResponse<PaymentConfirmationDto>
                {
                    Success = false,
                    Errors = [error ?? "Invalid OrderId"],
                    Message = "The OrderId format is invalid"
                });
            }

            // Validate that the order exists
            if (!await _orderRepository.ExistsAsync(orderId))
            {
                return NotFound(new ApiResponse<PaymentConfirmationDto>
                {
                    Success = false,
                    Errors = ["Order not found"],
                    Message = "The specified order does not exist"
                });
            }

            _logger.LogInformation("Payment confirmation initiated for order {OrderId}", request.OrderId);

            // Return confirmation response
            var confirmationDto = new PaymentConfirmationDto
            {
                OrderId = request.OrderId,
                Status = "Confirmed",
                ConfirmedAt = DateTime.UtcNow,
                Message = "Payment processor has simulated the payment confirmation. Ready to proceed with shipment."
            };

            return Ok(new ApiResponse<PaymentConfirmationDto>
            {
                Success = true,
                Data = confirmationDto,
                Message = "Payment confirmed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment");
            return StatusCode(500, new ApiResponse<PaymentConfirmationDto>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }
}
