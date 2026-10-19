using CinemaBooking.API.Extensions;
using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO.Admin;
using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api")]
    [AllowAnonymous]
    public class OpsController : ControllerBase
    {
        private readonly IAdminService _admin;
        public OpsController(IAdminService admin) => _admin = admin;

        [HttpGet("settings/public")]
        public IActionResult PublicSettings()
        {
            var all = _admin.GetSettings();
            return Ok(ApiResponse<object>.Ok(new
            {
                all.SeatPrice,
                all.OpenTime,
                all.CloseTime,
                all.WeekendSurcharge,
                all.LateShowSurcharge,
                all.LateShowAfter
            }));
        }

        [HttpGet("promotions")]
        public IActionResult PublicPromotions()
        {
            var items = _admin.GetPromotions().Where(p => p.IsActive).ToList();
            return Ok(ApiResponse<List<PromotionDto>>.Ok(items));
        }

        [HttpPost("promotions/quote")]
        public IActionResult Quote([FromBody] QuotePromotionRequest req)
        {
            req.UserId = User.GetUserId();
            return Ok(ApiResponse<QuotePromotionDto>.Ok(_admin.QuotePromotion(req)));
        }
    }
}
