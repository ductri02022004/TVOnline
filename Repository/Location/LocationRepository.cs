using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using static TVOnline.Models.Location;

namespace TVOnline.Repository.Location
{
    public class LocationRepository(AppDbContext context) : ILocationRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<List<Cities>> GetAllCities() => await _context.Cities.ToListAsync();
    }
}
