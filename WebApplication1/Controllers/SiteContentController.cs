using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    public class SiteContentController : ControllerBase
    {
        private readonly ISiteContentService _content;

        public SiteContentController(ISiteContentService content) => _content = content;

        [HttpGet("api/site-pages")]
        [AllowAnonymous]
        public IActionResult Public()
        {
            return Ok(ApiResponse<SitePagesDto>.Ok(_content.GetPublic()));
        }

        [HttpGet("api/admin/site-articles")]
        [Authorize(Roles = "Admin")]
        public IActionResult All()
        {
            return Ok(ApiResponse<List<SiteArticleDto>>.Ok(_content.GetAll()));
        }

        [HttpPost("api/admin/site-articles")]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] SaveSiteArticleRequest request)
        {
            var created = _content.Create(request);
            return created == null
                ? BadRequest(ApiResponse<string>.Fail("Nhập đủ tiêu đề hoặc đoạn mở đầu."))
                : Ok(ApiResponse<SiteArticleDto>.Ok(created, "Đã thêm nội dung"));
        }

        [HttpPut("api/admin/site-articles/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] SaveSiteArticleRequest request)
        {
            var updated = _content.Update(id, request);
            return updated == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy nội dung hoặc dữ liệu không hợp lệ"))
                : Ok(ApiResponse<SiteArticleDto>.Ok(updated, "Đã cập nhật"));
        }

        [HttpDelete("api/admin/site-articles/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            return _content.Delete(id)
                ? Ok(ApiResponse<string>.Ok("Đã xóa"))
                : NotFound(ApiResponse<string>.Fail("Không tìm thấy nội dung"));
        }
    }
}
