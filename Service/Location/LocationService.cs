using TVOnline.Repository.Location;
using TVOnline.Service.DTO;

namespace TVOnline.Service.Location
{
    public class LocationService(ILocationRepository locationRepository) : ILocationService
    {
        public async Task<List<CitiesResponse>> GetAllCities()
        {
            var cities = await locationRepository.GetAllCities();
            return cities.Select(c => c.ToCitiesResponse()).ToList();
        }
    }
}
