using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TVOnline.Models;

namespace TVOnline.Data {
    public class AppDbContext : IdentityDbContext<Users> {
        public AppDbContext(DbContextOptions options) : base(options) {
        }
    }
}
