using Microsoft.EntityFrameworkCore;

namespace Dora.OAuthServer
{
    public class OAuthDbContext: DbContext
    {
        public DbSet<DelegateConsentEntity> DelegateConsents { get; set; }
        public DbSet<OAuthGrantEntity> OAuthGrants { get; set; }
        public DbSet<ApplicationEntity> Applications { get; set; }
        public OAuthDbContext(DbContextOptions options) : base(options) {}
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OAuthGrantEntity>(b =>
            {
                b.HasKey("ClientId", "UserName");
                b.HasIndex(p => p.AuthorizationCode);
                b.HasIndex(p => p.RefreshToken);
                b.HasIndex(p => p.AccessToken);
            });

            builder.Entity<DelegateConsentEntity>(b =>
            {
                b.HasKey("ClientId", "UserName");
            });

            builder.Entity<ApplicationEntity>(b =>
            {
                b.HasKey(p => p.ClientId);
            });

        }
    }
}
