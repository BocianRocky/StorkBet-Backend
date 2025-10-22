using Application.Interfaces;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionRepository _promotionRepository;

    public PromotionsController(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.DateEnd < request.DateStart)
        {
            return BadRequest(new { message = "DateEnd must be greater than or equal to DateStart" });
        }

        var id = await _promotionRepository.CreateAsync(
            request.PromotionName,
            request.DateStart,
            request.DateEnd,
            request.BonusType,
            request.BonusValue,
            request.PromoCode,
            request.MinDeposit,
            request.MaxDeposit,
            request.Image,
            request.Description
            );
        return Ok(new { id });
    }

    [HttpPost("with-image")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreatePromotionWithImage([FromForm] CreatePromotionWithImageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.DateEnd < request.DateStart)
        {
            return BadRequest(new { message = "DateEnd must be greater than or equal to DateStart" });
        }

        string imagePath = string.Empty;

        // Handle file upload
        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "promotions");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(request.ImageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.ImageFile.CopyToAsync(stream);
            }

            // Store only filename for database (shorter path)
            imagePath = fileName;
        }

        var id = await _promotionRepository.CreateAsync(
            request.PromotionName,
            request.DateStart,
            request.DateEnd,
            request.BonusType,
            request.BonusValue,
            request.PromoCode,
            request.MinDeposit,
            request.MaxDeposit,
            imagePath,
            request.Description
            );
        return Ok(new { id, imagePath });
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAll()
    {
        var promotions = await _promotionRepository.GetAllAsync();

        var result = promotions.Select(p => new PromotionDto
        {
            Id = p.Id,
            PromotionName = p.PromotionName,
            DateStart = p.DateStart,
            DateEnd = p.DateEnd,
            BonusType = p.BonusType,
            BonusValue = p.BonusValue,
            PromoCode = p.PromoCode,
            MinDeposit = p.MinDeposit,
            MaxDeposit = p.MaxDeposit,
            Image = !string.IsNullOrEmpty(p.Image) ? $"/api/promotions/image/{p.Image}" : string.Empty,
            Description = p.Description
        }).ToList();

        return Ok(result);
    }

    [HttpGet("today")]
    [AllowAnonymous]
    public async Task<IActionResult> GetToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var promotions = await _promotionRepository.GetTodayAsync(today);

        var result = promotions.Select(p => new PromotionDto
        {
            Id = p.Id,
            PromotionName = p.PromotionName,
            DateStart = p.DateStart,
            DateEnd = p.DateEnd,
            BonusType = p.BonusType,
            BonusValue = p.BonusValue,
            PromoCode = p.PromoCode,
            MinDeposit = p.MinDeposit,
            MaxDeposit = p.MaxDeposit,
            Image = !string.IsNullOrEmpty(p.Image) ? $"/api/promotions/image/{p.Image}" : string.Empty,
            Description = p.Description
        }).ToList();

        return Ok(result);
    }

    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailable()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var promotions = await _promotionRepository.GetAvailableAsync(today);

        var result = promotions.Select(p => new PromotionDto
        {
            Id = p.Id,
            PromotionName = p.PromotionName,
            DateStart = p.DateStart,
            DateEnd = p.DateEnd,
            BonusType = p.BonusType,
            BonusValue = p.BonusValue,
            PromoCode = p.PromoCode,
            MinDeposit = p.MinDeposit,
            MaxDeposit = p.MaxDeposit,
            Image = !string.IsNullOrEmpty(p.Image) ? $"/api/promotions/image/{p.Image}" : string.Empty,
            Description = p.Description
        }).ToList();

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetMyPromotions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var promos = await _promotionRepository.GetPlayerPromotionsAsync(userId);

        var result = promos.Select(p => new PlayerPromotionDto
        {
            Id = p.Id,
            PromotionName = p.PromotionName,
            DateStart = p.DateStart,
            DateEnd = p.DateEnd,
            BonusType = p.BonusType,
            BonusValue = p.BonusValue,
            PromoCode = p.PromoCode,
            MinDeposit = p.MinDeposit,
            MaxDeposit = p.MaxDeposit,
            Availability = p.Availability,
            Image = !string.IsNullOrEmpty(p.Image) ? $"/api/promotions/image/{p.Image}" : string.Empty,
            Description = p.Description
        }).ToList();

        return Ok(result);
    }

    public class RedeemPromotionRequest
    {
        public string Code { get; set; } = string.Empty;
    }
    
    [HttpPost("redeem")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> RedeemPromotion([FromBody] RedeemPromotionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { message = "Promotion code is required" });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        try
        {
            var assignmentId = await _promotionRepository.AssignPromotionToPlayerByCodeAsync(userId, request.Code);
            return Ok(new { id = assignmentId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var deleted=await _promotionRepository.DeletePromotionAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Promocja o ID {id} nie istnieje." });
        }
        return Ok(new { message = $"Promocja o ID {id} została usunięta." });
    }

    /// <summary>
    /// Pobiera obrazek promocji
    /// </summary>
    [HttpGet("image/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetPromotionImage(string fileName)
    {
        // Validate filename to prevent directory traversal attacks
        if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return BadRequest("Nieprawidłowa nazwa pliku.");
        }

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "promotions", fileName);
        
        if (!System.IO.File.Exists(imagePath))
        {
            return NotFound("Obrazek nie został znaleziony.");
        }

        var fileBytes = System.IO.File.ReadAllBytes(imagePath);
        var contentType = GetContentType(fileName);
        
        return File(fileBytes, contentType);
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}


