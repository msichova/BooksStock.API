using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BooksStock.API.Services.Authentication
{
    public class AuthenticationApiDbContext(DbContextOptions<AuthenticationApiDbContext> options) : IdentityDbContext<ApiUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
