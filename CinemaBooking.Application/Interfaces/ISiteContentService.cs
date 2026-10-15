using CinemaBooking.Application.DTO;

namespace CinemaBooking.Application.Interfaces
{
    public interface ISiteContentService
    {
        SitePagesDto GetPublic();
        List<SiteArticleDto> GetAll();
        SiteArticleDto? Create(SaveSiteArticleRequest request);
        SiteArticleDto? Update(int id, SaveSiteArticleRequest request);
        bool Delete(int id);
    }
}
