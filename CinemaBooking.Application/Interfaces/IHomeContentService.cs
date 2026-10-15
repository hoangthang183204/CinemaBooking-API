using CinemaBooking.Application.DTO;

namespace CinemaBooking.Application.Interfaces
{
    public interface IHomeContentService
    {
        HomeContentDto GetPublic();
        List<HomeBlockDto> GetAll();
        HomeBlockDto? Create(SaveHomeBlockRequest request);
        HomeBlockDto? Update(int id, SaveHomeBlockRequest request);
        bool Delete(int id);
    }
}
