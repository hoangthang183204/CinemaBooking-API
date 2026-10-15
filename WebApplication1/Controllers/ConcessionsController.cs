using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/concessions")]
    public class ConcessionsController : ControllerBase
    {
        private readonly CinemaDbContext _context;

        public ConcessionsController(CinemaDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.ConcessionItems
                .Where(c => c.IsActive && c.StockQuantity > 0)
                .OrderBy(c => c.SortOrder)
                .Select(c => new ConcessionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    Category = c.Category,
                    SortOrder = c.SortOrder,
                    StockQuantity = c.StockQuantity,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ConcessionDto>>.Ok(items));
        }

        [HttpGet("/api/admin/concessions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminList()
        {
            var items = await _context.ConcessionItems
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .Select(c => new ConcessionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    Category = c.Category,
                    SortOrder = c.SortOrder,
                    StockQuantity = c.StockQuantity,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ConcessionDto>>.Ok(items));
        }

        [HttpGet("/api/staff/concessions")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> StaffList()
        {
            var items = await _context.ConcessionItems
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .Select(c => new ConcessionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    Category = c.Category,
                    SortOrder = c.SortOrder,
                    StockQuantity = c.StockQuantity,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ConcessionDto>>.Ok(items));
        }

        [HttpPut("/api/staff/concessions/{id:int}/stock")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateConcessionStockRequest request)
        {
            if (request.StockQuantity < 0)
                return BadRequest(ApiResponse<string>.Fail("Số lượng không được âm."));

            var item = await _context.ConcessionItems.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy món"));

            item.StockQuantity = request.StockQuantity;
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<ConcessionDto>.Ok(Map(item), "Đã cập nhật số lượng"));
        }

        [HttpPost("/api/admin/concessions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] SaveConcessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0)
                return BadRequest(ApiResponse<string>.Fail("Nhập tên và giá hợp lệ."));

            var item = new ConcessionItem();
            Apply(item, request);
            _context.ConcessionItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<ConcessionDto>.Ok(Map(item), "Đã thêm món"));
        }

        [HttpPut("/api/admin/concessions/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveConcessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0)
                return BadRequest(ApiResponse<string>.Fail("Nhập tên và giá hợp lệ."));

            var item = await _context.ConcessionItems.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy món"));

            Apply(item, request);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<ConcessionDto>.Ok(Map(item), "Đã cập nhật món"));
        }

        [HttpDelete("/api/admin/concessions/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ConcessionItems.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy món"));

            var used = await _context.BookingConcessions.AnyAsync(c => c.ConcessionItemId == id);
            if (used)
            {
                item.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<string>.Ok("Món đã có trong đơn nên được ẩn khỏi quầy"));
            }

            _context.ConcessionItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("Đã xóa món"));
        }

        private static void Apply(ConcessionItem item, SaveConcessionRequest request)
        {
            item.Name = InputGuard.Plain(request.Name, 120);
            item.Description = InputGuard.Optional(request.Description, 500);
            item.Price = request.Price;
            item.ImageUrl = InputGuard.MediaUrl(request.ImageUrl);
            item.Category = InputGuard.Plain(string.IsNullOrWhiteSpace(request.Category) ? "Bắp" : request.Category, 40);
            item.SortOrder = request.SortOrder;
            item.StockQuantity = request.StockQuantity < 0 ? 0 : request.StockQuantity;
            item.IsActive = request.IsActive;
        }

        private static ConcessionDto Map(ConcessionItem c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Price = c.Price,
            ImageUrl = c.ImageUrl,
            Category = c.Category,
            SortOrder = c.SortOrder,
            StockQuantity = c.StockQuantity,
            IsActive = c.IsActive
        };
    }
}
