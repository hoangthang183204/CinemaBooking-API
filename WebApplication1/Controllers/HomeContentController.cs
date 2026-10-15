using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    public class HomeContentController : ControllerBase
    {
        private readonly IHomeContentService _content;

        public HomeContentController(IHomeContentService content) => _content = content;

        [HttpGet("api/home-content")]
        [AllowAnonymous]
        public IActionResult Public()
        {
            return Ok(ApiResponse<HomeContentDto>.Ok(_content.GetPublic()));
        }

        [HttpGet("api/admin/home-blocks")]
        [Authorize(Roles = "Admin")]
        public IActionResult All()
        {
            return Ok(ApiResponse<List<HomeBlockDto>>.Ok(_content.GetAll()));
        }

        [HttpPost("api/admin/home-blocks")]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] SaveHomeBlockRequest request)
        {
            var created = _content.Create(request);
            return created == null
                ? BadRequest(ApiResponse<string>.Fail("Nhập tiêu đề. Loại chỉ nhận banner hoặc bắp nước."))
                : Ok(ApiResponse<HomeBlockDto>.Ok(created, "Đã thêm nội dung trang chủ"));
        }

        [HttpPut("api/admin/home-blocks/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] SaveHomeBlockRequest request)
        {
            var updated = _content.Update(id, request);
            return updated == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy nội dung hoặc dữ liệu không hợp lệ"))
                : Ok(ApiResponse<HomeBlockDto>.Ok(updated, "Đã cập nhật"));
        }

        [HttpDelete("api/admin/home-blocks/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            return _content.Delete(id)
                ? Ok(ApiResponse<string>.Ok("Đã xóa"))
                : NotFound(ApiResponse<string>.Fail("Không tìm thấy nội dung"));
        }
    }
}
