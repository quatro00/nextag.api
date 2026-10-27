using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SIGED.api.Data
{
    public class AuthDbContext : IdentityDbContext<IdentityUser, ApplicationRole, string>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>().ToTable("AspNetUsers", "dbo");
            builder.Entity<ApplicationRole>().ToTable("AspNetRoles", "dbo");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", "dbo");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "dbo");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", "dbo");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "dbo");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", "dbo");
        }
    }

    public class ApplicationRole : IdentityRole
    {
        public int SistemaId { get; set; }
    }
}
