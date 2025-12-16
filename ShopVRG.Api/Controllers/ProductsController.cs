namespace ShopVRG.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using ShopVRG.Api.Models;
using ShopVRG.Domain.Repositories;

/// <summary>
/// Controller for managing PC components (products)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository productRepository,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            var dtos = products.Select(p => new ProductDto
            {
                Code = p.Code.Value,
                Name = p.Name.Value,
                Description = p.Description,
                Category = p.Category,
                Price = p.Price.Value,
                Stock = p.Stock.Value,
                IsActive = p.IsActive
            });

            return Ok(new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = true,
                Data = dtos,
                Message = $"Found {products.Count} products"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return StatusCode(500, new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }

    /// <summary>
    /// Get active products only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetActive()
    {
        try
        {
            var products = await _productRepository.GetActiveProductsAsync();
            var dtos = products.Select(p => new ProductDto
            {
                Code = p.Code.Value,
                Name = p.Name.Value,
                Description = p.Description,
                Category = p.Category,
                Price = p.Price.Value,
                Stock = p.Stock.Value,
                IsActive = p.IsActive
            });

            return Ok(new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = true,
                Data = dtos,
                Message = $"Found {products.Count} active products"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active products");
            return StatusCode(500, new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByCategory(string category)
    {
        try
        {
            var products = await _productRepository.GetByCategoryAsync(category);
            var dtos = products.Select(p => new ProductDto
            {
                Code = p.Code.Value,
                Name = p.Name.Value,
                Description = p.Description,
                Category = p.Category,
                Price = p.Price.Value,
                Stock = p.Stock.Value,
                IsActive = p.IsActive
            });

            return Ok(new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = true,
                Data = dtos,
                Message = $"Found {products.Count} products in category '{category}'"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category {Category}", category);
            return StatusCode(500, new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }

    /// <summary>
    /// Get product by code
    /// </summary>
    [HttpGet("{code}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetByCode(string code)
    {
        try
        {
            if (!Domain.Models.ValueObjects.ProductCode.TryCreate(code, out var productCode, out var error))
            {
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Errors = [error ?? "Invalid product code"]
                });
            }

            var product = await _productRepository.GetByCodeAsync(productCode!);
            if (product == null)
            {
                return NotFound(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Errors = [$"Product '{code}' not found"]
                });
            }

            var dto = new ProductDto
            {
                Code = product.Code.Value,
                Name = product.Name.Value,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price.Value,
                Stock = product.Stock.Value,
                IsActive = product.IsActive
            };

            return Ok(new ApiResponse<ProductDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {Code}", code);
            return StatusCode(500, new ApiResponse<ProductDto>
            {
                Success = false,
                Errors = [ex.Message]
            });
        }
    }
}
