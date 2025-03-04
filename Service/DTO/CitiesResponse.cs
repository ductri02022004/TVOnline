using static TVOnline.Models.Location;

namespace TVOnline.Service.DTO
{
    public class CitiesResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ZoneId { get; set; }
    }

    public static class CitiesResponseExtension
    {
        public static CitiesResponse ToCitiesResponse(this Cities city)
        {
            return new CitiesResponse
            {
                Id = city.CityId,
                Name = city.CityName,
                ZoneId = city.ZoneId,
            };
        }
    }
}
