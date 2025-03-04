using TVOnline.Repository.Location;
using TVOnline.Service.DTO;

namespace TVOnline.Service.Location
{
    public interface ILocationService
    {
        Task<List<CitiesResponse>> GetAllCities();
    }
}
