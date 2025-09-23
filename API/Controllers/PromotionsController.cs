using Application.Interfaces;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Data;
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
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequestDto request)
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
            request.Image
            );
        return Ok(new { id });
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
            Image = p.Image
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
            Image = p.Image
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
            Image = p.Image
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
            Image = p.Image
        }).ToList();

        return Ok(result);
    }
}


