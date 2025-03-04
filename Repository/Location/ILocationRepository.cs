using TVOnline.Data;
using static TVOnline.Models.Location;

namespace TVOnline.Repository.Location
{
    public interface ILocationRepository
    {
        Task<List<Cities>> GetAllCities();
    }
}
